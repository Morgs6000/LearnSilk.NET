using Silk.NET.OpenGL;

namespace MySilkProgram;

// Nossa abstração de objeto de buffer.
public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    // Nosso identificador (handle), o tipo de buffer e a instância do GL que esta classe utilizará; eles são privados porque não há motivo para serem públicos. 
    // Na maioria das vezes, você vai querer abstrair elementos para tornar detalhes como esses invisíveis.
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        // Definindo a instância do GL e armazenando nosso tipo de buffer.
        _gl = gl;
        _bufferType = bufferType;

        // Obtendo o identificador e, em seguida, carregando os dados para esse identificador.
        _handle = _gl.GenBuffer();
        Bind();

        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        // Vinculando o objeto de buffer com o tipo de buffer correto.
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        // Lembre-se de excluir nosso buffer.
        _gl.DeleteBuffer(_handle);
    }
}
