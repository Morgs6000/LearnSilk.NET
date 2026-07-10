using MySilkProgram.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Breakout;

public class Program
{
    private static IWindow _window = null!;

    private static GL _gl = null!;
    public static GL GL = null!;

    private static uint SCREEN_WIDTH => (uint)_window.Size.X;
    private static uint SCREEN_HEIGHT => (uint)_window.Size.Y;

    private static Game Breakout = null!;

    private static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Breakout";
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

    private static void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        Input.Initialize(_window);

        _gl = _window.CreateOpenGL();
        GL = _gl;

        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        // initialize game
        // ---------------
        Breakout = new Game(SCREEN_WIDTH, SCREEN_HEIGHT);
        Breakout.Init();

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static void OnResize(Vector2D<int> newSize)
    {
        // certifique-se de que a viewport corresponda às novas dimensões da janela; observe que a largura e a altura serão significativamente maiores do que as especificadas em telas Retina.
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    private static void OnUpdate(double deltaTime)
    {
        Input.NewFrame();

        // gerenciar a entrada do usuário
        // ------------------------------
        Breakout.ProcessInput((float)deltaTime);

        // atualizar estado do jogo
        // ------------------------
        Breakout.Update((float)deltaTime);

        // quando o usuário pressiona a tecla Esc, definimos a propriedade WindowShouldClose como true, fechando o aplicativo
        if (Input.GetKey(Key.Escape))
        {
            _window.Close();
        }
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        Breakout.Render();
    }

    private static void OnClosing()
    {
        // excluir todos os recursos carregados usando o gerenciador de recursos
        // ---------------------------------------------------------------------
        ResourceManager.Clear();
    }
}
