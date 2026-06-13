using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LearnSilkNET;

public class Game
{
    protected IWindow _window = null!;
    protected GL _gl = null!;

    // Construtor
    // --------------------------------------------------

    public Game()
    {
        WindowOptions options = WindowOptions.Default;

        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Minha primeira aplicação Silk.NET!";
        options.IsVisible = false;

        _window = Window.Create(options);
    }

    // Run
    // --------------------------------------------------

    public void Run()
    {
        // --- Load ---
        _window.Load += () =>
        {
            _window.Center();
            _window.IsVisible = true;

            _gl = _window.CreateOpenGL();

            Input.Initialize(_window);

            OnLoad();
        };

        // --- Resize ---
        _window.Resize += newSize =>
        {
            OnResize(newSize);
        };

        // --- Update ---
        _window.Update += deltaTime =>
        {
            Input.NewFrame();

            OnUpdate(deltaTime);
        };

        // --- Render ---
        _window.Render += deltaTime =>
        {
            OnRender(deltaTime);
        };

        // --- Closing ---
        _window.Closing += () =>
        {
            OnClosing();
        };

        // --- Run ---
        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "Falha ao criar a janela Silk.NET"
                + "\n\n" + ex
                + "\n\n" + " -- --------------------------------------------------- -- "
            );
        }
    }
    
    public void Close()
    {
        _window.Close();
    }

    //
    // --------------------------------------------------

    protected virtual void OnLoad()
    {
        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }
    
    protected virtual void OnResize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }
    
    protected virtual void OnUpdate(double deltaTime)
    {
        if (Input.GetKey(Key.Escape))
        {
            Close();
        }
    }
    
    protected virtual void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }
    
    protected virtual void OnClosing()
    {
        
    }
}
