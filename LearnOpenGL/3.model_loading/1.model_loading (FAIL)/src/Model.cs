using System.Numerics;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace MySilkProgram;

public class Model
{
    private GL _gl = Game.GL;

    private Assimp _assimp =  null!;

    public Model(string path)
    {
        LoadModel(path);
    }

    public void Draw(Shader shader)
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].Draw(shader);
        }
    }

    // model data
    private List<Texture> textures_loaded = new List<Texture>();
    private List<Mesh> meshes = new List<Mesh>();
    private string directory = string.Empty;

    private void LoadModel(string path)
    {
        _assimp = Assimp.GetApi();
        unsafe
        {
            Scene* scene = _assimp.ImportFile(
                path,
                (uint)(PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs)
            );

            if (scene == null ||
                (scene->MFlags & Assimp.SceneFlagsIncomplete) != 0 ||
                scene->MRootNode == null)
            {
                Console.WriteLine("ERROR::ASSIMP::" + _assimp.GetErrorStringS());

                return;
            } 

            directory = path.Remove(path.LastIndexOf('/'));

            ProcessNode(scene->MRootNode, scene);
        }
    }

    private unsafe void ProcessNode(Node* node, Scene* scene)
    {
        // processa todas as malhas do nó (se houver)
        for (int i = 0; i < node->MNumMeshes; i++)
        {
            Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[node->MMeshes[i]];

            meshes.Add(ProcessMesh(mesh, scene));
        }

        // então, faça o mesmo para cada um de seus filhos
        for (int i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene);
        }
    }

    private unsafe Mesh ProcessMesh(Silk.NET.Assimp.Mesh* mesh, Scene* scene)
    {
        List<Vertex> vertices = new List<Vertex>();
        List<uint> indices = new List<uint>();
        List<Texture> textures = new List<Texture>();

        for (int i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex;

            // processar posições de vértices, normais e coordenadas de textura
            Vector3 vector;
            vector.X = mesh->MVertices[i].X;
            vector.Y = mesh->MVertices[i].Y;
            vector.Z = mesh->MVertices[i].Z;
            vertex.Position = vector;

            vector.X = mesh->MNormals[i].X;
            vector.Y = mesh->MNormals[i].Y;
            vector.Z = mesh->MNormals[i].Z;
            vertex.Normal = vector;

            if (mesh->MTextureCoords[0] != null) // a malha contém coordenadas de textura?
            {
                Vector2 vec;
                vec.X = mesh->MTextureCoords[0][i].X;
                vec.Y = mesh->MTextureCoords[0][i].Y;
                vertex.TexCoords = vec;
            }
            else
            {
                vertex.TexCoords = new Vector2(0.0f, 0.0f);
            }
            
            vertices.Add(vertex);
        }

        // process indices
        for (int i = 0; i < mesh->MNumFaces; i++)
        {
            Face face = mesh->MFaces[i];

            for (int j = 0; j < face.MNumIndices; j++)
            {
                indices.Add(face.MIndices[j]);
            }
        }

        // process material
        if (mesh->MMaterialIndex >= 0)
        {
            if (mesh->MMaterialIndex >= 0)
            {
                Material* material = scene->MMaterials[mesh->MMaterialIndex];

                List<Texture> diffuseMaps = LoadMaterialTextures(
                    material, 
                    TextureType.Diffuse, 
                    "texture_diffuse"
                );
                textures.AddRange(diffuseMaps);

                List<Texture> specularMaps = LoadMaterialTextures(
                    material,
                    TextureType.Specular,
                    "texture_specular"
                );
                textures.AddRange(specularMaps);
            }
        }

        return new Mesh(vertices, indices, textures);
    }
    
    private unsafe List<Texture> LoadMaterialTextures(Material* mat, TextureType type, string typeName)
    {
        var textures = new List<Texture>();
        var assimp = Silk.NET.Assimp.Assimp.GetApi();

        uint textureCount = assimp.GetMaterialTextureCount(mat, type);

        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            var result = assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);

            if (result == Return.Success)
            {
                string texturePath = path.ToString();
                bool skip = false;

                // Verifica se a textura já foi carregada anteriormente
                foreach (var loadedTexture in textures_loaded)
                {
                    if (loadedTexture.path == texturePath)
                    {
                        textures.Add(loadedTexture);
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    // Se não foi carregada, carrega agora
                    Texture texture = new Texture();
                    texture.id = TextureFromFile(texturePath, directory);
                    texture.type = typeName;
                    texture.path = texturePath;
                    
                    textures.Add(texture);
                    textures_loaded.Add(texture); // Adiciona ao cache global
                }
            }
        }
        
        return textures;
    }

    private uint TextureFromFile(string path, string directory, bool gamma = false)
    {
        string filename = Path.Combine(directory, path);

        uint textureID = _gl.GenTexture();
        
        // Carrega a imagem
        using (FileStream stream = System.IO.File.OpenRead(filename))
        {
            // StbImageSharp carrega a imagem
            ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            _gl.BindTexture(TextureTarget.Texture2D, textureID);

            // Define o formato interno com base no número de componentes
            // StbImageSharp com RedGreenBlueAlpha sempre retorna 4 componentes (RGBA)
            PixelFormat format = PixelFormat.Rgba;
            InternalFormat internalFormat = InternalFormat.Rgba;

            unsafe
            {
                fixed (byte* p = result.Data)
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, 
                        (uint)result.Width, (uint)result.Height, 0, 
                        format, PixelType.UnsignedByte, p);
                }
            }
            _gl.GenerateMipmap(TextureTarget.Texture2D);

            // Parâmetros de filtragem e wrap
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        }

        return textureID;
    }
}
