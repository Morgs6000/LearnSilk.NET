using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace LearnSilkNET;

public class Input
{
    public static CursorLockMode CursorLockMode
    {
        get
        {
            CursorMode inputMode = _mouse.Cursor.CursorMode;

            switch (inputMode)
            {
                case CursorMode.Normal:
                    return CursorLockMode.Normal;
                case CursorMode.Hidden:
                    return CursorLockMode.Hidden;
                case CursorMode.Disabled:
                    return CursorLockMode.Disabled;
                case CursorMode.Raw:
                    return CursorLockMode.Raw;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        set
        {
            CursorMode inputMode;

            switch (value)
            {
                case CursorLockMode.Normal:
                    inputMode = CursorMode.Normal;
                    break;
                case CursorLockMode.Hidden:
                    inputMode = CursorMode.Hidden;
                    break;
                case CursorLockMode.Disabled:
                    inputMode = CursorMode.Disabled;
                    break;
                case CursorLockMode.Raw:
                    inputMode = CursorMode.Raw;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _mouse.Cursor.CursorMode = inputMode;
        }
    }

    //
    // --------------------------------------------------

    public static Vector2 MousePosition => _mouse.Position;

    public static Vector2 MouseScrollDelta
    {
        get
        {
            var delta = _mouseScrollDelta;
            _mouseScrollDelta = Vector2.Zero;

            return delta;
        }
    }

    private static Vector2 _mouseScrollDelta;

    //
    // --------------------------------------------------

    private static IKeyboard _keyboard = null!;
    private static IMouse _mouse = null!;

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
        _mouse = input.Mice[0];

        _mouse.Scroll += (mouse, scrollWheel) =>
        {
            _mouseScrollDelta = new Vector2(scrollWheel.X, scrollWheel.Y);
        };
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
