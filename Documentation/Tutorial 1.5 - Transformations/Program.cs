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
        -0.5f, -0.5f,  0.0f,   0.0f, 1.0f,
         0.5f, -0.5f,  0.0f,   1.0f, 1.0f,
         0.5f,  0.5f,  0.0f,   1.0f, 0.0f,
        -0.5f,  0.5f,  0.0f,   0.0f, 0.0f
    };

    private static readonly uint[] _indices =
    {
        0u, 1u, 2u,
        0u, 2u, 3u
    };

    // Nossos novos objetos abstraídos; aqui especificamos quais são os tipos.
    private static VertexArrayObject<float, uint> _vao;
    private static BufferObject<float> _vbo;
    private static BufferObject<uint> _ebo;

    // Criando transforms para as transformações
    private static Transform[] Transforms = new Transform[4];

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

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Ao contrário do que ocorre na transformação, devido à nossa abstração, a ordem não importa aqui.

        // Translation.
        Transforms[0] = new Transform();
        Transforms[0].Position = new Vector3(0.5f, 0.5f, 0f);

        // Rotation.
        Transforms[1] = new Transform();
        Transforms[1].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1.0f);

        // Scaling.
        Transforms[2] = new Transform();
        Transforms[2].Scale = 0.5f;

        // Mixed transformation.
        Transforms[3] = new Transform();
        Transforms[3].Position = new Vector3(-0.5f, 0.5f, 0f);
        Transforms[3].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1.0f);
        Transforms[3].Scale = 0.5f;
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
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Vinculando e utilizando nosso VAO e shader.
        _shader.Use();

        // Definindo um uniforme.
        _shader.SetUniform("uTexture", 0);

        _texture.Bind(TextureUnit.Texture0);

        _vao.Bind();

        for (int i = 0; i < Transforms.Length; i++)
        {
            // Utilizando as transformações.
            _shader.SetUniform("uModel", Transforms[i].ViewMatrix);

            _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        }        
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
