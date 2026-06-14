using Silk.NET.Windowing;

namespace LearnSilkNET;

public class Screen
{
    public static int Widht => _window.Size.X;
    public static int Height => _window.Size.Y;

    private static IWindow _window = null!;

    public static void Initialize(IWindow window)
    {
        _window = window;
    }
}
