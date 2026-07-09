# region
/*

https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.4%20-%20Abstractions/Program.cs

*/
# endregion

using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window;
    private static GL _gl;

    private static Shader _shader;
    private static Texture _texture;
    
    private static readonly float[] _vertices =
    {
    //  aPosition            | aTexCoords
        -0.5f, -0.5f, -0.5f,   0.0f, 1.0f, // 0
        -0.5f, -0.5f,  0.5f,   1.0f, 1.0f, // 1
        -0.5f,  0.5f,  0.5f,   1.0f, 0.0f, // 2
        -0.5f,  0.5f, -0.5f,   0.0f, 0.0f, // 3
        
         0.5f, -0.5f,  0.5f,   0.0f, 1.0f, // 4
         0.5f, -0.5f, -0.5f,   1.0f, 1.0f, // 5
         0.5f,  0.5f, -0.5f,   1.0f, 0.0f, // 6
         0.5f,  0.5f,  0.5f,   0.0f, 0.0f, // 7
        
        -0.5f, -0.5f, -0.5f,   0.0f, 1.0f, // 8
         0.5f, -0.5f, -0.5f,   1.0f, 1.0f, // 9
         0.5f, -0.5f,  0.5f,   1.0f, 0.0f, // 10
        -0.5f, -0.5f,  0.5f,   0.0f, 0.0f, // 11
        
        -0.5f,  0.5f,  0.5f,   0.0f, 1.0f, // 12
         0.5f,  0.5f,  0.5f,   1.0f, 1.0f, // 13
         0.5f,  0.5f, -0.5f,   1.0f, 0.0f, // 14
        -0.5f,  0.5f, -0.5f,   0.0f, 0.0f, // 15
        
         0.5f, -0.5f, -0.5f,   0.0f, 1.0f, // 16
        -0.5f, -0.5f, -0.5f,   1.0f, 1.0f, // 17
        -0.5f,  0.5f, -0.5f,   1.0f, 0.0f, // 18
         0.5f,  0.5f, -0.5f,   0.0f, 0.0f, // 19
        
        -0.5f, -0.5f,  0.5f,   0.0f, 1.0f, // 20
         0.5f, -0.5f,  0.5f,   1.0f, 1.0f, // 21
         0.5f,  0.5f,  0.5f,   1.0f, 0.0f, // 22
        -0.5f,  0.5f,  0.5f,   0.0f, 0.0f  // 23
    };

    private static readonly uint[] _indices =
    {
        0, 1, 2,
        0, 2, 3,

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

    // Nossos novos objetos abstraídos; aqui especificamos quais são os tipos.
    private static VertexArrayObject<float, uint> _vao;
    private static BufferObject<float> _vbo;
    private static BufferObject<uint> _ebo;

    // Define a posição da câmera e as direções relativas de "cima" e "direita"
    private static Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
    private static Vector3 _cameraTarget = Vector3.Zero;
    private static Vector3 _cameraDirection = Vector3.Normalize(_cameraPosition - _cameraTarget);
    private static Vector3 _cameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, _cameraDirection));
    private static Vector3 _cameraUp = Vector3.Cross(_cameraDirection, _cameraRight);

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

    private static void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        IInputContext input = _window.CreateInput();        
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.CornflowerBlue);

        _shader = new Shader(_gl, "shader.vert", "shader.frag");

        _texture = new Texture(_gl, "silk.png");

        // Instanciando nossas novas abstrações
        _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        // Informando ao objeto VAO como organizar os ponteiros de atributos
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        // _gl.Enable(EnableCap.Blend);
        // _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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
        _gl.Enable(EnableCap.DepthTest);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Vinculando e utilizando nosso VAO e shader.
        _shader.Use();

        // Definindo um uniforme.
        _shader.SetUniform("uTexture", 0);

        // Use o tempo decorrido para converter em radianos e permitir que nosso cubo gire ao longo do tempo
        var difference = (float)(_window.Time * 100);

        var size = _window.FramebufferSize;

        var model = Matrix4x4.CreateRotationY(MathHelper.DegressToRadians(difference)) *
            Matrix4x4.CreateRotationX(MathHelper.DegressToRadians(difference));
        var view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.DegressToRadians(45.0f),
            (float)size.X / (float)size.Y,
            0.1f,
            100.0f
        );

        _shader.SetUniform("uModel", model);
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);

        _texture.Bind(TextureUnit.Texture0);

        _vao.Bind();

        _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static void OnClose()
    {
        // Lembre-se de descartar todas as instâncias.
        _shader.Dispose();
        _texture.Dispose();

        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}
