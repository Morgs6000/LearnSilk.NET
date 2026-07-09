using System.Numerics;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace MySilkProgram;

public class Model : IDisposable
{
    private readonly GL _gl;
    private Assimp _assimp;
    private List<Texture> _textureLoaded = new List<Texture>();
    public string Directory { get; protected set; } = string.Empty;
    public List<Mesh> Meshes { get; protected set; } = new List<Mesh>();

    public Model(GL gl, string path, bool name = false)
    {
        var assimp = Assimp.GetApi();
        _assimp = assimp;
        _gl = gl;

        LoadModel(path);
    }

    private unsafe void LoadModel(string path)
    {
        var scene = _assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate);

        if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
        {
            var error = _assimp.GetErrorStringS();
            throw new Exception(error);
        }

        Directory = path;

        ProcessNode(scene->MRootNode, scene);
    }

    private unsafe void ProcessNode(Node* node, Scene* scene)
    {
        for (int i = 0; i < node->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[node->MMeshes[i]];
            Meshes.Add(ProcessMesh(mesh, scene));
        }

        for (int i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene);
        }
    }

    private unsafe Mesh ProcessMesh(AssimpMesh* mesh, Scene* scene)
    {
        // dados a preencher
        List<Vertex> vertices = new List<Vertex>();
        List<uint> indices = new List<uint>();
        List<Texture> textures = new List<Texture>();

        // percorre cada um dos vértices da malha
        for (uint i = 0; i < mesh->MNumVertices; i++)
        {
            Vertex vertex = new Vertex();
            vertex.BondeIds = new int[Vertex.MAX_BONE_INFLUENCE];
            vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];

            vertex.Position = mesh->MVertices[i];

            // normals
            if (mesh->MNormals != null)
            {
                vertex.Normal = mesh->MNormals[i];
            }

            // tangent
            if (mesh->MTangents != null)
            {
                vertex.Tangent = mesh->MTangents[i];
            }

            // bitangent
            if (mesh->MBitangents != null)
            {
                vertex.Bitangent = mesh->MBitangents[i];
            }

            // texture coordinates
            if (mesh->MTextureCoords[0] != null) // a malha contém coordenadas de textura?
            {
                // Um ​​vértice pode conter até 8 coordenadas de textura diferentes. Portanto, assumimos que não utilizaremos modelos nos quais um vértice possa ter múltiplas coordenadas de textura; assim, sempre utilizamos o primeiro conjunto (0).
                Vector3 texcoord3 = mesh->MTextureCoords[0][i];

                vertex.TexCoords = new Vector2(texcoord3.X, texcoord3.Y);
            }

            vertices.Add(vertex);
        }

        // agora, percorra cada uma das faces da malha (uma face é um triângulo da malha) e obtenha os índices de vértice correspondentes.
        for (uint i = 0; i < mesh->MNumFaces; i++)
        {
            Face face = mesh->MFaces[i];

            // recupera todos os índices da face e os armazena no vetor de índices
            for (uint j = 0; j < face.MNumIndices; j++)
            {
                indices.Add(face.MIndices[j]);
            }
        }

        // process materials
        Material* material = scene->MMaterials[mesh->MMaterialIndex];

        // Adotamos uma convenção para os nomes dos samplers nos shaders. Cada textura difusa deve ser nomeada como 'texture_diffuseN', onde N é um número sequencial variando de 1 a MAX_SAMPLER_NUMBER. 
        // O mesmo se aplica a outras texturas, conforme resumido na lista a seguir:
        // diffuse: texture_diffuseN
        // specular: texture_specularN
        // normal: texture_normalN

        // 1. diffuse maps
        var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
        if (diffuseMaps.Any()) 
        {
            textures.AddRange(diffuseMaps);
        }

        // 2. specular maps
        var specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
        if (specularMaps.Any())
        {
            textures.AddRange(specularMaps);
        }

        // 3. normal maps
        var normalMaps = LoadMaterialTextures(material, TextureType.Height, "texture_normal");
        if (normalMaps.Any())
        {
            textures.AddRange(normalMaps);
        }

        // 4. height maps
        var heightMaps = LoadMaterialTextures(material, TextureType.Ambient, "texture_height");
        if (heightMaps.Any()) 
        {
            textures.AddRange(heightMaps);
        }

        // retorna um objeto de malha criado a partir dos dados de malha extraídos
        var result = new Mesh(_gl, BuildVertices(vertices), BuildIndices(indices), textures);
        return result;
    }

    private unsafe List<Texture> LoadMaterialTextures(Material* mat, TextureType type, string typeName)
    {
        var textureCount = _assimp.GetMaterialTextureCount(mat, type);

        List<Texture> textures = new List<Texture>();

        for (uint i = 0; i < textureCount; i++)
        {
            AssimpString path;
            _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);

            bool skip = false;

            for (int j = 0; j < _textureLoaded.Count; j++)
            {
                if (_textureLoaded[j].Path == path)
                {
                    textures.Add(_textureLoaded[j]);
                    skip = true;
                    break;
                }
            }
            if(!skip)
            {
                var texture = new Texture(_gl, Directory, type);
                texture.Path = path;
                textures.Add(texture);
                _textureLoaded.Add(texture);
            }
        }

        return textures;
    }

    private float[] BuildVertices(List<Vertex> vertexCollection)
    {
        var vertices = new List<float>();

        foreach (var vertex in vertexCollection)
        {
            vertices.Add(vertex.Position.X);
            vertices.Add(vertex.Position.Y);
            vertices.Add(vertex.Position.Z);
            vertices.Add(vertex.TexCoords.X);
            vertices.Add(vertex.TexCoords.Y);
        }

        return vertices.ToArray();
    }

    private uint[] BuildIndices(List<uint> indices)
    {
        return indices.ToArray();
    }

    public void Dispose()
    {
        foreach (var mesh in Meshes)
        {
            mesh.Dispose();
        }

        _textureLoaded = null;
    }
}
