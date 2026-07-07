# region
/*

https://dotnet.github.io/Silk.NET/docs/opengl/c1/1-hello-window

https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.1%20-%20Hello%20Window/Program.cs

*/
# endregion

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window;

    private static void Main(string[] args)
    {
        // Crie uma janela.
        WindowOptions options = WindowOptions.Default;        
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "My first Silk.NET application!";

        // Para não ver a janela antes dela ser centralizada no monitor.
        options.IsVisible = false;

        _window = Window.Create(options);

        // Atribua eventos.
        _window.Load += OnLoad;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        // Executar a janela.
        _window.Run();

        // window.Run() é um método BLOQUEANTE — isso significa que ele interromperá a execução de qualquer código no método atual até que a janela termine de ser executada. Portanto, este método dispose não será chamado até que você feche a janela.
        _window.Dispose();
    }

    private static void OnLoad()
    {
        // Centraliza a janela no monitor, e depois exibe a janela novamente.
        // Sem isso a janela é gerada em uma posição aleatória do monitor, e isso me causava incomodo.
        // Alguns usuarios de Linux relatam mal funcionamento com esta linha.
        _window.Center();
        _window.IsVisible = true;

        // Configurar contexto de entrada.
        IInputContext input = _window.CreateInput();        
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    private static void OnFramebufferResize(Vector2D<int> newSize)
    {
        // Atualizar proporções de aspecto, regiões de recorte, viewports, etc.
    }

    private static void OnUpdate(double deltaTime)
    {
        // Todas as atualizações do programa devem ser feitas aqui.
    }

    private static void OnRender(double deltaTime)
    {
        // Todo o processamento de renderização deve ser feito aqui.
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        // Verifica se deve fechar a janela ao pressionar Esc.
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}
