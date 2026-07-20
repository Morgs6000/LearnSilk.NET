using System.Numerics;
using Silk.NET.Input;

namespace Breakout;

// PowerUp herda seu estado e suas funções de renderização de
// GameObject, mas também armazena informações adicionais para
// indicar sua duração de atividade e se está ativado ou não.
// O tipo de PowerUp é armazenado como uma string.
public class PowerUp : GameObject
{
    // O tamanho de um bloco PowerUp
    private static Vector2 POWERUP_SIZE = new(60.0f, 20.0f);

    // Velocidade que um bloco PowerUp possui ao ser gerado
    private static Vector2 VELOCITY = new(0.0f, 150.0f);

    // powerup state
    public string Type;
    public float Duration;
    public bool Activated;

    // constructor
    public PowerUp(string type, Vector3 color, float duration, Vector2 position, Texture2D texture) : base(position, POWERUP_SIZE, texture, color, VELOCITY)
    {
        Type = type;
        Duration = duration;
        // Activated
    }
}
