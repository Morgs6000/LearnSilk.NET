using Silk.NET.Input;
using Silk.NET.Windowing;

namespace LearnSilkNET;

public class Input
{
    private static IKeyboard _keyboard = null!;

    private static HashSet<Key> _keys = [];
    private static HashSet<Key> _keysPrevious = [];

    private static readonly HashSet<Key> _keysValid = Enum.GetValues<Key>()
        .Where(key => key != Key.Unknown)
        .ToHashSet();

    //
    // --------------------------------------------------

    public static void Initialize(IWindow window)
    {
        IInputContext input = window.CreateInput();

        _keyboard = input.Keyboards[0];
    }

    //
    // --------------------------------------------------

    public static void NewFrame()
    {
        // --- _keysPrevious ---
        _keysPrevious.Clear();

        foreach (Key key in _keys)
        {
            _keysPrevious.Add(key);
        }

        // --- _keys ---
        _keys.Clear();

        if (_keyboard != null)
        {
            foreach (Key key in _keysValid)
            {
                if (_keyboard.IsKeyPressed(key))
                {
                    _keys.Add(key);
                }
            }
        }
    }

    //
    // --------------------------------------------------

    public static bool GetKey(Key key)
    {
        return _keys.Contains(key);
    }

    public static bool GetKeyDown(Key key)
    {
        return _keys.Contains(key) && !_keysPrevious.Contains(key);
    }

    public static bool GetKeyUp(Key key)
    {
        return !_keys.Contains(key) && _keysPrevious.Contains(key);
    }
}
