using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace MySilkProgram;

public class Game
{
    private static IWindow _window = null!;
    private static GL _gl = null!;

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
    }

    private void OnResize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    private void OnUpdate(double deltaTime)
    {
        
    }

    private void OnRender(double deltaTime)
    {
        
    }

    private void OnClosing()
    {
        
    }
}
