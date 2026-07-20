using System.Numerics;

namespace Breakout;

// Objeto contêiner para armazenar todo o estado relevante para uma única entidade de objeto de jogo. É provável que cada objeto no jogo precise, no mínimo, do estado descrito em GameObject.
public class GameObject
{
    // object state
    public Vector2 Position, Size, Velocity;
    public Vector3 Color;
    public float Rotation;
    public bool IsSolid;
    public bool Destroyed;

    // render state
    public Texture2D Sprite = null!;

    // constructor(s)
    public GameObject()
    {
        Position = new Vector2(0.0f, 0.0f);
        Size = new Vector2(1.0f, 1.0f);
        Velocity = new Vector2(0.0f);
        Color = new Vector3(1.0f);
        Rotation = 0.0f;
        // Sprite = ;
        IsSolid = false;
        Destroyed = false;
    }

    public GameObject(Vector2 pos, Vector2 size, Texture2D sprite, Vector3? color = null, Vector2? velocity = null)
    {
        Position = pos;
        Size = size;
        Velocity = velocity ?? new Vector2(0.0f, 0.0f);
        Color = color ?? new Vector3(1.0f);
        Rotation = 0.0f;
        Sprite = sprite;
        IsSolid = false;
        Destroyed = false;
    }

    // draw sprite
    public virtual void Draw(SpriteRenderer renderer)
    {
        renderer.DrawSprite(Sprite, Position, Size, Rotation, Color);
    }
}
