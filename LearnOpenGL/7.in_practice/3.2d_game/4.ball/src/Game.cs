using System.Numerics;
using MySilkProgram.Inputs;
using Silk.NET.Input;
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

    private List<GameLevel> Levels = new List<GameLevel>();
    private int Level;

    // Tamanho inicial da raquete do jogador
    private Vector2 PLAYER_SIZE = new Vector2(100.0f, 20.0f);

    // Velocidade inicial da raquete do jogador
    private float PLAYER_VELOCITY = 500.0f;

    private GameObject Player = null!;

    // Velocidade inicial da bola
    private Vector2 INITIAL_BALL_VELOCITY = new Vector2(100.0f, -350.0f);

    // Raio do objeto bola
    private const float BALL_RADIUS = 12.5f;

    private BallObject Ball = null!;

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
            file:  "res/Textures/background.jpg", 
            alpha: false, 
            name:  "background"
        );   

        ResourceManager.LoadTexture(
            file:  "res/Textures/awesomeface.png", 
            alpha: true, 
            name:  "face"
        );     

        ResourceManager.LoadTexture(
            file:  "res/Textures/block.png", 
            alpha: false, 
            name:  "block"
        );      

        ResourceManager.LoadTexture(
            file:  "res/Textures/block_solid.png", 
            alpha: false, 
            name:  "block_solid"
        );    

        ResourceManager.LoadTexture(
            file:  "res/Textures/paddle.png", 
            alpha: true, 
            name:  "paddle"
        );

        // load levels

        GameLevel one = new GameLevel(); one.Load(
            file:        "levels/one.lvl", 
            levelWidth:  Width,  
            levelHeight: Height / 2
        );

        GameLevel two = new GameLevel(); two.Load(
            file:        "levels/two.lvl", 
            levelWidth:  Width,  
            levelHeight: Height / 2
        );

        GameLevel three = new GameLevel(); three.Load(
            file:        "levels/three.lvl", 
            levelWidth:  Width,  
            levelHeight: Height / 2
        );

        GameLevel four = new GameLevel(); four.Load(
            file:        "levels/four.lvl", 
            levelWidth:  Width,  
            levelHeight: Height / 2
        );

        Levels.Add(one);
        Levels.Add(two);
        Levels.Add(three);
        Levels.Add(four);

        Level = 0;

        Vector2 playerPos = new Vector2(
            Width / 2.0f - PLAYER_SIZE.X / 2.0f,
            Height - PLAYER_SIZE.Y
        );

        Player = new GameObject(
            playerPos,
            PLAYER_SIZE,
            ResourceManager.GetTexture("paddle")
        );

        Vector2 ballPos = playerPos + new Vector2(
            PLAYER_SIZE.X / 2.0f - BALL_RADIUS,
            -BALL_RADIUS * 2.0f
        );

        Ball = new BallObject(
            pos:      ballPos, 
            radius:   BALL_RADIUS, 
            velocity: INITIAL_BALL_VELOCITY,
            sprite:   ResourceManager.GetTexture("face")
        );
    }

    public void ProcessInput(float deltaTime)
    {
        if (State == GameState.GAME_ACTIVE)
        {
            float velocity = PLAYER_VELOCITY * deltaTime;

            // move playerboard
            if (Input.GetKey(Key.A))
            {
                if (Player.Position.X >= 0.0f)
                {
                    Player.Position.X -= velocity;

                    if (Ball.Stuck)
                    {
                        Ball.Position.X -= velocity;
                    }
                }
            }
            if (Input.GetKey(Key.D))
            {
                if (Player.Position.X <= Width - Player.Size.X)
                {
                    Player.Position.X += velocity;

                    if (Ball.Stuck)
                    {
                        Ball.Position.X += velocity;
                    }
                }
            }
            if (Input.GetKeyDown(Key.Space))
            {
                Ball.Stuck = false;
            }
        }
    }

    public void Update(float deltaTime)
    {
        Ball.Move(deltaTime, Width);
    }

    public void Render()
    {
        if (State == GameState.GAME_ACTIVE)
        {
            // draw background
            Renderer.DrawSprite(
                texture:  ResourceManager.GetTexture("background"),
                position: new Vector2(0.0f, 0.0f),
                size:     new Vector2(Width, Height),
                rotate:   0.0f
            );

            // draw level
            Levels[Level].Draw(Renderer);

            Ball.Draw(Renderer);
        }

        Player.Draw(Renderer);
    }
}
