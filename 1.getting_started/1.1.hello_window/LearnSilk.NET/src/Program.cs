using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LearnSilk_NET;

public class Program
{
    private static IWindow _window;
    private static GL _gl;

    private static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "My first Silk.NET application!";

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;

        // _window.Center();
        
        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();

        _window.Center();
    }

    private static void OnRender(double deltaTime)
    {
        // _window.SwapBuffers();
    }

    private static void OnFramebufferResize(Vector2D<int> size)
    {
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }
}
