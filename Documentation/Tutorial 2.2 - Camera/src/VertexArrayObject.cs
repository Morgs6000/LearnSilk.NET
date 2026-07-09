using Silk.NET.OpenGL;

namespace MySilkProgram;

// A abstração do objeto de array de vértices.
public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
{
    // Nosso identificador e a instância do GL que esta classe utilizará; eles são privados porque não há motivo para serem públicos. 
    // Na maioria das vezes, você vai querer abstrair elementos para tornar coisas assim invisíveis.
    private uint _handle;
    private GL _gl;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        // Salvando a instância do GL.
        _gl = gl;

        // Definindo o identificador e vinculando o VBO e o EBO a este VAO.
        _handle = _gl.GenVertexArray();
        Bind();

        vbo.Bind();
        ebo.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        // Configurando um ponteiro de atributo de vértice
        _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint) sizeof(TVertexType), (void*) (offSet * sizeof(TVertexType)));
        _gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        // Vinculando o array de vértices.
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        // Lembre-se de descartar este objeto para que os dados na GPU sejam liberados. 
        // Não excluímos o VBO e o EBO aqui, pois um mesmo VBO pode estar armazenado em múltiplos VAOs.
        _gl.DeleteVertexArray(_handle);
    }
}
