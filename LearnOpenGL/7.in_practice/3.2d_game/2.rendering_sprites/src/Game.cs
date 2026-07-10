using System.Numerics;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Breakout;

// Representa o estado atual do jogo
public enum GameState
{
    GAME_ACTIVE,
    GAME_MENU,
    GAME_WIN
}

// A classe Game encapsula todo o estado e a funcionalidade relacionados ao jogo.
// Ela reúne todos os dados do jogo em uma única classe para
// facilitar o acesso a cada um dos componentes e o gerenciamento.
public class Game
{
    private GL _gl = Program.GL;

    // game state
    public GameState State;
    public uint Width, Height;

    private SpriteRenderer Renderer = null!;

    // constructor
    public Game(uint width, uint height)
    {
        State = GameState.GAME_ACTIVE;
        Width = width;
        Height = height;
    }

    // destructor (finalizer in C#)
    ~Game()
    {
        // Limpar recursos, se necessário
    }

    // inicializar o estado do jogo (carregar todos os shaders/texturas/níveis)
    public void Init()
    {
        // carregar shaders
        ResourceManager.LoadShader(
            vShaderFile: "res/Shaders/base/vertex.glsl",
            fShaderFile: "res/Shaders/base/fragment.glsl",
            gShaderFile: null,
            name:        "sprite"
        );

        // configure shaders
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
            left:        0.0f, 
            right:       Width, 
            bottom:      Height, 
            top:         0.0f,
            zNearPlane: -1.0f, 
            zFarPlane:   1.0f
        );

        ResourceManager.GetShader("sprite").Use().SetInteger("image", 0);
        ResourceManager.GetShader("sprite").SetMatrix4("projection", projection);

        // definir controles específicos de renderização
        Renderer = new SpriteRenderer(ResourceManager.GetShader("sprite"));   

        // carregar texturas
        ResourceManager.LoadTexture(
            file:  "res/Textures/awesomeface.png", 
            alpha: true, 
            name:  "face"
        );     
    }

    public void ProcessInput(float deltaTime)
    {
        
    }

    public void Update(float deltaTime)
    {
        
    }

    public void Render()
    {
        Renderer.DrawSprite(
            texture:  ResourceManager.GetTexture("face"),
            position: new Vector2(200.0f, 200.0f),
            size:     new Vector2(300.0f, 400.0f),
            rotate:   45.0f,
            color:    new Vector3(0.0f, 1.0f, 0.0f)
        );
    }
}
