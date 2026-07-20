using System.Numerics;
using MySilkProgram.Inputs;
using MySilkProgram.Utilities;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace MySilkProgram;

public class Game
{
    private IWindow _window = null!;
    
    private static GL _gl = null!;
    public static GL GL = null!;

    private Shader _shader = null!;

    private uint _texture1, _texture2;

    private Camera _camera = null!;

    // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
    // ---------------------------------------------------------------------------
    private readonly float[] _vertices =
    {
        // positions           // colors           // texture coords
        -0.5f, -0.5f, -0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 0
        -0.5f, -0.5f,  0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 1
        -0.5f,  0.5f,  0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 2
        -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f, // 3
        
         0.5f, -0.5f,  0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 4
         0.5f, -0.5f, -0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 5
         0.5f,  0.5f, -0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 6
         0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f, // 7
        
        -0.5f, -0.5f, -0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 8
         0.5f, -0.5f, -0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 9
         0.5f, -0.5f,  0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 10
        -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f, // 11
        
        -0.5f,  0.5f,  0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 12
         0.5f,  0.5f,  0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 13
         0.5f,  0.5f, -0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 14
        -0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f, // 15
        
         0.5f, -0.5f, -0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 16
        -0.5f, -0.5f, -0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 17
        -0.5f,  0.5f, -0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 18
         0.5f,  0.5f, -0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f, // 19
        
        -0.5f, -0.5f,  0.5f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 20
         0.5f, -0.5f,  0.5f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 21
         0.5f,  0.5f,  0.5f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 22
        -0.5f,  0.5f,  0.5f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f  // 23
    };

    private readonly uint[] _indices = // observe que começamos do 0!
    {
        0, 1, 2, // primeiro triangulo
        0, 2, 3, // segundo triangulo

        4, 5, 6,
        4, 6, 7,

        8, 9, 10,
        8, 10, 11,

        12, 13, 14,
        12, 14, 15,

        16, 17, 18,
        16, 18, 19,

        20, 21, 22,
        20, 22, 23
    };

    private uint _vertexArrayObject;
    private uint _vertexBufferObject;
    private uint _elementBufferObject;

    private Vector3[] _cubePositions = {
        new Vector3( 0.0f,  0.0f,  0.0f), 
        new Vector3( 2.0f,  5.0f, -15.0f), 
        new Vector3(-1.5f, -2.2f, -2.5f),  
        new Vector3(-3.8f, -2.0f, -12.3f),  
        new Vector3( 2.4f, -0.4f, -3.5f),  
        new Vector3(-1.7f,  3.0f, -7.5f),  
        new Vector3( 1.3f, -2.0f, -2.5f),  
        new Vector3( 1.5f,  2.0f, -2.5f), 
        new Vector3( 1.5f,  0.2f, -1.5f), 
        new Vector3(-1.3f,  1.0f, -1.5f)  
    };

    public static GLEnum glCheckError(string file, int line)
    {
        GLEnum errorCode;

        while ((errorCode = _gl.GetError()) != GLEnum.NoError)
        {
            string error = string.Empty;

            switch (errorCode)
            {
                case GLEnum.InvalidEnum:
                    error = "INVALID_ENUM";
                    break;
                case GLEnum.InvalidValue:
                    error = "INVALID_VALUE";
                    break;
                case GLEnum.InvalidOperation:
                    error = "INVALID_OPERATION";
                    break;
                case GLEnum.StackOverflow:
                    error = "STACK_OVERFLOW";
                    break;
                case GLEnum.StackUnderflow:
                    error = "STACK_UNDERFLOW";
                    break;
                case GLEnum.OutOfMemory:
                    error = "OUT_OF_MEMORY";
                    break;
                case GLEnum.InvalidFramebufferOperation:
                    error = "GL_INVALID_FRAMEBUFFER_OPERATION";
                    break;
            }

            Console.WriteLine($"{error} | {file} ({line})");
        }

        return errorCode;
    }

    private static void glDebugOutput(
        GLEnum souce, 
        GLEnum type, 
        uint id, 
        GLEnum severity, 
        int length, 
        nint message, 
        nint userParam
    )
    {
        // ignorar códigos de erro/aviso não significativos
        if (id == 131169 || id == 131185 || id == 131218 || id == 131204)
        {
            return;
        }

        Console.WriteLine("---------------");
        Console.WriteLine($"Debug message ({id}): {message}");

        switch (souce)
        {
            case GLEnum.DebugSourceApi:
                Console.Write("Source: API");
                break;
            case GLEnum.DebugSourceWindowSystem:
                Console.Write("Source: Window System");
                break;
            case GLEnum.DebugSourceShaderCompiler:
                Console.Write("Source: Shader Compiler");
                break;
            case GLEnum.DebugSourceThirdParty:
                Console.Write("Source: Third Party");
                break;
            case GLEnum.DebugSourceApplication:
                Console.Write("Source: Application");
                break;
            case GLEnum.DebugSourceOther:
                Console.Write("Source: Other");
                break;
        }

        Console.WriteLine();

        switch (type)
        {
            case GLEnum.DebugTypeError:
                Console.Write("Type: Error");
                break;
            case GLEnum.DebugTypeDeprecatedBehavior:
                Console.Write("Type: Deprecated Behaviour");
                break;
            case GLEnum.DebugTypeUndefinedBehavior:
                Console.Write("Type: Undefined Behaviour");
                break;
            case GLEnum.DebugTypePortability:
                Console.Write("Type: Portability");
                break;
            case GLEnum.DebugTypePerformance:
                Console.Write("Type: Performance");
                break;
            case GLEnum.DebugTypeMarker:
                Console.WriteLine("Type: Marker");
                break;
            case GLEnum.DebugTypePushGroup:
                Console.Write("Type: Push Group");
                break;
            case GLEnum.DebugTypePopGroup:
                Console.Write("Type: Pop Group");
                break;
            case GLEnum.DebugTypeOther:
                Console.Write("Type: Other");
                break;
        }

        Console.WriteLine();

        switch (severity)
        {
            case GLEnum.DebugSeverityHigh:
                Console.Write("Severity: high");
                break;
            case GLEnum.DebugSeverityMedium:
                Console.Write("Severity: medium");
                break;
            case GLEnum.DebugSeverityLow:
                Console.Write("Severity: low");
                break;
            case GLEnum.DebugSeverityNotification:
                Console.Write("Severity: notification");
                break;
        }

        Console.WriteLine();
        Console.WriteLine();
    }

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
                "Falha ao criar a janela Silk.NET" + "\n" +
                ex + "\n" + 
                " -- --------------------------------------------------- -- "
            );
        }
    }

    private void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        Input.Initialize(_window);

        _gl = _window.CreateOpenGL();
        GL = _gl;

        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        unsafe
        {
            // habilita o contexto de depuração do OpenGL se o contexto permitir um contexto de depuração

            int flags;
            _gl.GetInteger(GLEnum.ContextFlags, out flags);

            if ((flags & (int)GLEnum.ContextFlagDebugBit) != 0)
            {
                // initialize debug output
                _gl.Enable(EnableCap.DebugOutput);
                _gl.Enable(EnableCap.DebugOutputSynchronous); // garante que os erros sejam exibidos de forma síncrona
                _gl.DebugMessageCallback(glDebugOutput, null);
                _gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }
        }

        // construir e compilar nosso programa de shader
        // ------------------------------------
        _shader = new Shader( // você pode nomear seus arquivos de shader como quiser
            "res/Shaders/base/vertex.glsl",
            "res/Shaders/base/fragment.glsl"
        );

        // carregar e criar uma textura
        // ----------------------------

        // texture 1
        // ---------
        _gl.GenTextures(1, out _texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _texture1);

        // definir os parâmetros de wrapping da textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // define o modo de repetição da textura como GL_REPEAT (método padrão)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // carregar imagem, criar textura e gerar mipmaps
        int width, height;
        byte[] data;

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (Stream stream = File.OpenRead("res/Textures/container.jpg"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            width  = image.Width;
            height = image.Height;
            data   = image.Data;
        }

        if (data != null) 
        {
            unsafe 
            {
                fixed (byte* ptr = data) 
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                }
            }
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura.");
        }

        // texture 2
        // ---------
        _gl.GenTextures(1, out _texture2);
        _gl.BindTexture(TextureTarget.Texture2D, _texture2);

        // definir os parâmetros de wrapping da textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // define o modo de repetição da textura como GL_REPEAT (método padrão)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        using (Stream stream = File.OpenRead("res/Textures/awesomeface.png"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            width  = image.Width;
            height = image.Height;
            data   = image.Data;
        }

        if (data != null) 
        {
            unsafe 
            {
                fixed (byte* ptr = data) 
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                }
            }
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura.");
        }

        // informar ao OpenGL a qual unidade de textura cada sampler pertence (precisa ser feito apenas uma vez)
        // -----------------------------------------------------------------------------------------------------
        _shader.Use(); // não se esqueça de ativar/usar o shader antes de definir os uniforms!
        _shader.SetInt("texture1", 0);
        _shader.SetInt("texture2", 1);

        _gl.GenVertexArrays(1, out _vertexArrayObject);
        _gl.GenBuffers(1, out _vertexBufferObject);
        _gl.GenBuffers(1, out _elementBufferObject);

        // primeiro vincule o Vertex Array Object, depois vincule e configure o(s) buffer(s) de vértices e, em seguida, configure o(s) atributo(s) de vértice.
        _gl.BindVertexArray(_vertexArrayObject);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferObject);
        unsafe
        {
            fixed (float* buf = _vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _elementBufferObject);
        unsafe
        {
            fixed (uint* buf = _indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
            }
        }

        // position attribute
        unsafe
        {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // color attribute
        unsafe
        {
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        // texture coords attribute
        unsafe
        {
            _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(2);

        // observe que isso é permitido; a chamada para glVertexAttribPointer registrou o VBO como o objeto de buffer de vértices vinculado ao atributo de vértice, portanto, podemos desvinculá-lo com segurança logo em seguida
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        // lembre-se: NÃO desvincule o EBO enquanto um VAO estiver ativo, pois o objeto de buffer de elementos vinculado ESTÁ armazenado no VAO; mantenha o EBO vinculado.
        // _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // Você pode desvincular o VAO posteriormente para que outras chamadas de VAO não modifiquem acidentalmente este VAO, mas isso raramente acontece. Modificar outros VAOs exige uma chamada para glBindVertexArray de qualquer forma, então geralmente não desvinculamos VAOs (nem VBOs) quando não é diretamente necessário.
        _gl.BindVertexArray(0);

        // descomente esta chamada para desenhar polígonos em wireframe.
        // _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        _gl.Enable(EnableCap.DepthTest);

        _camera = new Camera();

        Input.CursorLockMode = CursorLockMode.Raw;
    }

    private void OnResize(Vector2D<int> newSize)
    {
        // certifique-se de que a viewport corresponda às novas dimensões da janela; observe que largura e a altura será significativamente maior do que a especificada em telas retina.
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    private void OnUpdate(double deltaTime)
    {
        Time.Update(deltaTime);
        Input.NewFrame();

        if (Input.GetKey(Key.Escape))
        {
            _window.Close();
        }

        _camera.ProcessKeyboad();
        _camera.ProcessMouseMovement();
        _camera.ProcessMouseScroll();
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        Matrix4x4 model = Matrix4x4.Identity;
        _shader.SetMatrix4x4("model", model);

        Matrix4x4 view = _camera.GetViewMatrix();
        _shader.SetMatrix4x4("view", view);

        Matrix4x4 projection = _camera.GetProjectionMatrix(_window);
        _shader.SetMatrix4x4("projection", projection);

        // vincular texturas às unidades de textura correspondentes
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture1);
        _gl.ActiveTexture(TextureUnit.Texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _texture2);

        _gl.BindVertexArray(_vertexArrayObject);

        for (int i = 0; i < 10; i++)
        {
            model = Matrix4x4.Identity;

            float angle = 20.0f * i;
            model *= Matrix4x4.CreateFromAxisAngle(
                Vector3.Normalize(new Vector3(1.0f, 0.3f, 0.5f)),
                MathHelper.DegressToRadians(angle)
            );

            model *= Matrix4x4.CreateTranslation(_cubePositions[i]);

            _shader.SetMatrix4x4("model", model);

            unsafe
            {
                // renderiza o triângulo
                _gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, (void*)0);
            }
        }
    }

    private void OnClosing()
    {
        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // ---------------------------------------------------------------------------
        _gl.DeleteVertexArrays(1, ref _vertexArrayObject);
        _gl.DeleteBuffers(1, ref _vertexBufferObject);
        _gl.DeleteBuffers(1, ref _elementBufferObject);
        
        _shader.Dispose();
    }
}
