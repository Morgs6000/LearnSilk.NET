using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LearnSilk_NET;

public class Program
{
    private static IWindow _window = null!;
    private static GL _gl = null!;

    private static float[] vertices =
    {
        -0.5f, -0.5f, 0.0f,
         0.5f, -0.5f, 0.0f,
         0.0f,  0.5f, 0.0f
    };

    private static uint VBO;

    private static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "My first Silk.NET application!";

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;
        
        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();

        _window.Center();

        IInputContext input = _window.CreateInput();

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _gl.GenBuffers(1, out VBO);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageARB.StaticDraw);
    }

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    private static void OnFramebufferResize(Vector2D<int> size)
    {
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }
    
    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if(key == Key.Escape)
        {
            _window.Close();
        }
    }
}
