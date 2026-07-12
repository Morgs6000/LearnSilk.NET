using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace MySilkProgram;

public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;
}

public struct Texture
{
    public uint id;
    public string type;
    public string path;
}

public class Mesh
{
    private GL _gl = Game.GL;
        
    // mesh data
    public List<Vertex> Vertices;
    public List<uint> Indices;
    public List<Texture> Textures;

    public Mesh(List<Vertex> vertices, List<uint> indices, List<Texture> textures)
    {
        Vertices = vertices;
        Indices = indices;
        Textures = textures;

        SetupMesh();
    }

    public void Draw(Shader shader)
    {
        uint diffeseNr = 1;
        uint specularNr = 1;

        for (int i = 0; i < Textures.Count; i++)
        {
            _gl.ActiveTexture(TextureUnit.Texture0 + i); // ativar a unidade de textura correta antes de vincular

            // obtém o número da textura (o N em diffuse_textureN)
            string number = string.Empty;
            string name = Textures[i].type;

            if (name == "texture_diffuse")
            {
                number = (diffeseNr++).ToString();
            }
            else if (name == "texture_specular")
            {
                number = (specularNr++).ToString();
            }

            shader.SetInt(("material." + name + number), i);

            _gl.BindTexture(TextureTarget.Texture2D, Textures[i].id);
        }

        _gl.ActiveTexture(TextureUnit.Texture0);

        // draw mesh
        _gl.BindVertexArray(VAO);
        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Count, DrawElementsType.UnsignedInt, (void*)0);
        }
        _gl.BindVertexArray(0);
    }

    //  render data
    private uint VAO, VBO, EBO;

    private void SetupMesh()
    {
        _gl.GenVertexArrays(1, out VAO);
        _gl.GenBuffers(1, out VBO);
        _gl.GenBuffers(1, out EBO);

        _gl.BindVertexArray(VAO);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

        Span<Vertex> vertexSpan = CollectionsMarshal.AsSpan(Vertices);
        Span<byte> byteSpan = MemoryMarshal.AsBytes(vertexSpan);

        unsafe
        {
            fixed (byte* buf = byteSpan)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Count * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
        unsafe
        {
            fixed (uint* buf = Indices.ToArray())
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Count * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
            }
        }

        // vertex positions
        _gl.EnableVertexAttribArray(0);
        unsafe
        {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
        }

        // vertex normals
        _gl.EnableVertexAttribArray(1);
        unsafe
        {
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)sizeof(Vector3));
        }

        // vertex texture coords
        _gl.EnableVertexAttribArray(2);
        unsafe
        {
            _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(sizeof(Vector3) * 2));
        }

        _gl.BindVertexArray(0);
    }
}
