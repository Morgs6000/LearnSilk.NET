# region
/*

https://dotnet.github.io/Silk.NET/docs/opengl/c1/2-hello-quad

https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.2%20-%20Hello%20quad/Program.cs

*/
# endregion

using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window;
    private static GL _gl;

    // Os shaders de vértice são executados em cada vértice.
    private static readonly string _vertexShaderSource = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0f);
        }
    ";

    // Os fragment shaders são executados em cada fragmento/pixel da geometria.
    private static readonly string _fragmentShaderSource = @"
        #version 330 core

        out vec4 out_color;

        void main()
        {
            out_color = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
    ";

    private static uint _program;

    // Dados de vértice, carregados no VBO.
    private static readonly float[] _vertices =
    {
        -0.5f, -0.5f,  0.0f,
         0.5f, -0.5f,  0.0f,
         0.5f,  0.5f,  0.0f,
        -0.5f,  0.5f,  0.0f
    };

    // Dados de índice, carregados no EBO.
    private static readonly uint[] indices =
    {
        0u, 1u, 2u,
        0u, 2u, 3u
    };

    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;

    private static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;        
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "LearnOpenGL with Silk.NET";
        options.IsVisible = false;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;

        _window.Run();

        _window.Dispose();
    }

    private static unsafe void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        IInputContext input = _window.CreateInput();        
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        // Obtendo a API OpenGL para desenhar na tela.
        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.CornflowerBlue);

        // Criando um shader de vértice.
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, _vertexShaderSource);
        _gl.CompileShader(vertexShader);

        // Verificando o shader em busca de erros de compilação.
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        // Criando um shader de fragmento.
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, _fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        // Verificando o shader em busca de erros de compilação.
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
        }

        // Combinando os shaders em um único programa de shader.
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        _gl.LinkProgram(_program);

        // Verificando a vinculação em busca de erros.
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
        }

        // Exclua os shaders individuais que não são mais úteis;
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Criando um array de vértices.
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Inicializando um buffer de vértices que armazena os dados dos vértices.
        _vbo = _gl.GenBuffer(); // Criando o buffer.
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo); // Vinculando o buffer.
        fixed (float* buf = _vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw); // Definindo dados do buffer.
        }

        // Inicializando um buffer de elementos que armazena os dados de índices.
        _ebo = _gl.GenBuffer(); // Criando o buffer.
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo); // Vinculando o buffer.
        fixed (uint* buf = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw); // Definindo dados do buffer.
        }

        // Informe ao OpenGL como fornecer os dados aos shaders.
        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private static void OnFramebufferResize(Vector2D<int> newSize)
    {
        _gl.Viewport(newSize);
    }

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static unsafe void OnRender(double deltaTime)
    {
        // Limpa o canal de cor.
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        //Vincula a geometria e o shader.
        _gl.UseProgram(_program);
        _gl.BindVertexArray(_vao);

        // Desenhe a geometria.
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static void OnClose()
    {
        // Lembre-se de excluir os buffers.
        _gl.DeleteProgram(_program);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}
