using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LearnSilkNET;

public class Engine
{
    protected IWindow _window = null!;
    protected GL _gl = null!;

    // Construtor
    // --------------------------------------------------

    public Engine()
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
        
    }
    
    protected virtual void OnResize(Vector2D<int> newSize)
    {
        
    }
    
    protected virtual void OnUpdate(double deltaTime)
    {
        
    }
    
    protected virtual void OnRender(double deltaTime)
    {
        
    }
    
    protected virtual void OnClosing()
    {
        
    }
}
