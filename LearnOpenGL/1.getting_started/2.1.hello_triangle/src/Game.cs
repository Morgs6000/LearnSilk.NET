using MySilkProgram.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace MySilkProgram;

public class Game
{
    private static IWindow _window = null!;
    private static GL _gl = null!;

    private const string _vertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 aPos;

        void main()
        {
            gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
        }
    ";

    private const string _fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
    ";

    private uint _shaderProgram;

    // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
    // ---------------------------------------------------------------------------
    private readonly float[] _vertices =
    {
        -0.5f, -0.5f,  0.0f, // esqueda
         0.5f, -0.5f,  0.0f, // direita
         0.0f,  0.5f,  0.0f  // topo
    };

    private uint _vertexArrayObject;
    private uint _vertexBufferObject;

    public Game()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "LearnOpenGL with Silk.NET";
        options.IsVisible = false;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Resize += OnResize;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "Falha ao criar a janela Silk.NET" + "\n\n" +
                ex
            );
        }
    }

    private void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        _gl = _window.CreateOpenGL();

        Input.Initialize(_window);

        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // construir e compilar nosso programa de shader
        // ---------------------------------------------

        // vertex shader
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, _vertexShaderSource);
        _gl.CompileShader(vertexShader);

        int success;
        string infoLog;

        // verificar erros de compilação de shaders
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            _gl.GetShaderInfoLog(vertexShader, out infoLog);
            Console.WriteLine("ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" + infoLog);
        }

        // fragment shader
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, _fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        // verificar erros de compilação de shaders
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            _gl.GetShaderInfoLog(fragmentShader, out infoLog);
            Console.WriteLine("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" + infoLog);
        }

         // link shaders
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);

        // check for linking errors
        _gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out success);
        if (success == 0)
        {
            _gl.GetProgramInfoLog(_shaderProgram, out infoLog);
            Console.WriteLine("ERROR::SHADER::PROGRAM::LINKING_FAILED\n" + infoLog);
        }

        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        _gl.GenVertexArrays(1, out _vertexArrayObject);

        // primeiro vincule o Vertex Array Object, depois vincule e configure o(s) buffer(s) de vértices e, em seguida, configure o(s) atributo(s) de vértice.
        _gl.BindVertexArray(_vertexArrayObject);

        _gl.GenBuffers(1, out _vertexBufferObject);

        // observe que isso é permitido; a chamada para glVertexAttribPointer registrou o VBO como o objeto de buffer de vértices vinculado ao atributo de vértice, portanto, podemos desvinculá-lo com segurança logo em seguida
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferObject);
        unsafe {
            fixed (float* buf = _vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        unsafe {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // Você pode desvincular o VAO posteriormente para que outras chamadas de VAO não modifiquem acidentalmente este VAO, mas isso raramente acontece. Modificar outros VAOs exige uma chamada para glBindVertexArray de qualquer forma, então geralmente não desvinculamos VAOs (nem VBOs) quando não é diretamente necessário.
        _gl.BindVertexArray(0);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        // certifique-se de que a viewport corresponda às novas dimensões da janela; observe que largura e a altura será significativamente maior do que a especificada em telas retina.
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    private void OnUpdate(double deltaTime)
    {
        Input.NewFrame();

        if (Input.GetKey(Key.Escape))
        {
            _window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // desenhar nosso primeiro triângulo
        _gl.UseProgram(_shaderProgram);
        _gl.BindVertexArray(_vertexArrayObject); // como temos apenas um único VAO, não há necessidade de vinculá-lo todas as vezes, mas faremos isso para manter as coisas um pouco mais organizadas
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        // _gl.BindVertexArray(0); // não é necessário desvinculá-lo toda vez.
    }

    private void OnClosing()
    {
        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // ---------------------------------------------------------------------------
        _gl.DeleteVertexArrays(1, ref _vertexArrayObject);
        _gl.DeleteBuffers(1, ref _vertexBufferObject);
        _gl.DeleteProgram(_shaderProgram);
    }
}
