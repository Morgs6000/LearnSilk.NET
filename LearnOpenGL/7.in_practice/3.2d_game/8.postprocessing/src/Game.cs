using System.Numerics;
using Breakout.Utilities;
using MySilkProgram.Inputs;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace Breakout;

using Collision = (bool collision, Direction direction, Vector2 difference);

// Representa o estado atual do jogo
public enum GameState
{
    GAME_ACTIVE,
    GAME_MENU,
    GAME_WIN
}

public enum Direction
{
    UP,
	RIGHT,
	DOWN,
	LEFT
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

    // Game-related State data
    private SpriteRenderer Renderer = null!;
    private GameObject Player = null!;
    private BallObject Ball = null!;
    private ParticleGenerator Particles = null!;
    private PostProcessor Effects = null!;

    private List<GameLevel> Levels = new List<GameLevel>();
    private int Level;

    // Tamanho inicial da raquete do jogador
    private Vector2 PLAYER_SIZE = new Vector2(100.0f, 20.0f);

    // Velocidade inicial da raquete do jogador
    private float PLAYER_VELOCITY = 500.0f;

    // Velocidade inicial da bola
    private Vector2 INITIAL_BALL_VELOCITY = new Vector2(100.0f, -350.0f);

    // Raio do objeto bola
    private const float BALL_RADIUS = 12.5f;

    private float ShakeTime = 0.0f;

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

        ResourceManager.LoadShader(
            vShaderFile: "res/Shaders/particle/vertex.glsl",
            fShaderFile: "res/Shaders/particle/fragment.glsl",
            gShaderFile: null,
            name:        "particle"
        );

        ResourceManager.LoadShader(
            vShaderFile: "res/Shaders/post_processing/vertex.glsl",
            fShaderFile: "res/Shaders/post_processing/fragment.glsl",
            gShaderFile: null,
            name:        "postprocessing"
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

        ResourceManager.GetShader("particle").Use().SetInteger("sprite", 0);
        ResourceManager.GetShader("particle").SetMatrix4("projection", projection);  

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

        ResourceManager.LoadTexture(
            file:  "res/Textures/particle.png", 
            alpha: true, 
            name:  "particle"
        );

        // definir controles específicos de renderização
        Renderer = new SpriteRenderer(ResourceManager.GetShader("sprite")); 

        Particles = new ParticleGenerator(
            ResourceManager.GetShader("particle"),
            ResourceManager.GetTexture("particle"),
            500
        );

        Effects = new PostProcessor(
            ResourceManager.GetShader("postprocessing"),
            Width, 
            Height
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

        // configure game objects

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
        // update objects
        Ball.Move(deltaTime, Width);

        // check for collisions
        DoCollisions();

        // update particles
        Particles.Update(deltaTime, Ball, 2, new Vector2(Ball.Radius / 2.0f));

        // reduzir o tempo de vibração
        if (ShakeTime > 0.0f)
        {
            ShakeTime -= deltaTime;

            if (ShakeTime <= 0.0f)
            {
                Effects.Shake = false;
            }
        }

        // verifica a condição de derrota
        if (Ball.Position.Y >= Height) // a bola chegou à borda inferior?
        {
            ResetLevel();
            ResetPlayer();
        }
    }

    public void Render()
    {
        if (State == GameState.GAME_ACTIVE)
        {
            // iniciar a renderização para o framebuffer de pós-processamento
            Effects.BeginRender();

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

                // draw player
                Player.Draw(Renderer);

                // draw particles
                Particles.Draw();

                // draw ball
                Ball.Draw(Renderer);
            }

            // finalizar a renderização para o framebuffer de pós-processamento
            Effects.EndRender();

            // renderizar quad de pós-processamento
            Effects.Render(Time.ElapsedTime);
        }
    }

    public bool CheckCollision(GameObject one, GameObject two) // AABB - AABB collision
    {
        // collision x-axis?
        bool collisionX = one.Position.X + one.Size.X >= two.Position.X &&
            two.Position.X + two.Size.X >= one.Position.X;
        
        // collision y-axis?
        bool collisionY = one.Position.Y + one.Size.Y >= two.Position.Y &&
            two.Position.Y + two.Size.Y >= one.Position.Y;
        
        return collisionX && collisionY;
    }

    public Collision CheckCollision(BallObject one, GameObject two) // AABB - Circle collision
    {
        // obter primeiro o ponto central do círculo
        Vector2 center = new Vector2(
            one.Position.X + one.Radius,
            one.Position.Y + one.Radius
        );

        // calcular informações da AABB (centro, semi-extensões)
        Vector2 aabb_half_extents = new Vector2(
            two.Size.X / 2.0f,
            two.Size.Y / 2.0f
        );
        Vector2 aabb_center = new Vector2(
            two.Position.X + aabb_half_extents.X,
            two.Position.Y + aabb_half_extents.Y
        );

        // obtém o vetor diferença entre os dois centros
        Vector2 difference = center - aabb_center;
        Vector2 clamped = Vector2.Clamp(difference, -aabb_half_extents, aabb_half_extents);

        // adicione o valor limitado a AABB_center para obter o ponto da caixa mais próximo do círculo
        Vector2 closet = aabb_center + clamped;

        // obtém o vetor entre o centro do círculo e o ponto mais próximo da AABB e verifica se o comprimento é menor ou igual ao raio
        difference = closet - center;

        if (difference.Length() <= one.Radius)
        {
            return (true, VectorDirection(difference), difference);
        }
        else
        {
            return (false, Direction.UP, new Vector2(0.0f, 0.0f));
        }

        // return difference.Length() < one.Radius;
    }

    public void DoCollisions()
    {
        foreach (GameObject box in Levels[Level].Bricks)
        {
            if (!box.Destroyed) 
            {
                Collision collision = CheckCollision(Ball, box);

                if (collision.collision) // se a colisão for verdadeira
                {
                    // destrói o bloco se não for sólido
                    if (!box.IsSolid)
                    {
                        box.Destroyed = true;
                    }
                    else
                    {
                        // se o bloco for sólido, habilite o efeito de tremor
                        ShakeTime = 0.05f;
                        Effects.Shake = true;
                    }

                    // resolução de colisões
                    Direction dir = collision.direction;
                    Vector2 diff_vector = collision.difference;

                    if (dir == Direction.LEFT || dir == Direction.RIGHT) // horizontal collision
                    {
                        Ball.Velocity.X = -Ball.Velocity.X; // reverse horizontal velocity

                        // relocate
                        float penetration = Ball.Radius - MathF.Abs(diff_vector.X);

                        if (dir == Direction.LEFT)
                        {
                            Ball.Position.X += penetration; // move ball to right
                        }
                        else
                        {
                            Ball.Position.X -= penetration; // move ball to left;
                        }
                    }
                    else // vertical collision
                    {
                        Ball.Velocity.Y = -Ball.Velocity.Y; // reverse vertical velocity

                        // relocate
                        float penetration = Ball.Radius - MathF.Abs(diff_vector.Y);

                        if (dir == Direction.UP)
                        {
                            Ball.Position.Y -= penetration; // move ball back up
                        }
                        else
                        {
                            Ball.Position.Y += penetration; // move ball back down
                        }
                    }
                }
            }
        }

        // verificar colisões para a raquete do jogador (a menos que esteja travada)
        Collision result = CheckCollision(Ball, Player);

        if (!Ball.Stuck && result.collision)
        {
            // verifica onde atingiu a plataforma e altera a velocidade com base no ponto de impacto
            float centerBorad = Player.Position.X + Player.Size.X / 2.0f;
            float distance = (Ball.Position.X + Ball.Radius) - centerBorad;
            float percentage = distance / (Player.Size.X / 2.0f);

            // então, mova-se de acordo
            float strenght = 2.0f;
            Vector2 oldVelocity = Ball.Velocity;
            
            Ball.Velocity.X = INITIAL_BALL_VELOCITY.X * percentage * strenght;
            Ball.Velocity.Y = -1.0f * MathF.Abs(Ball.Velocity.Y);

            Ball.Velocity = Vector2.Normalize(Ball.Velocity) * oldVelocity.Length();
        }
    }

    public Direction VectorDirection(Vector2 target)
    {
        Vector2[] compass =
        {
            new Vector2( 0.0f,  1.0f), // up  
            new Vector2( 1.0f,  0.0f), // right  
            new Vector2( 0.0f, -1.0f), // down  
            new Vector2(-1.0f,  0.0f), // left  
        };

        float max = 0.0f;
        int best_match = -1;

        for (int i = 0; i < 4; i++)
        {
            float dot_product = Vector2.Dot(Vector2.Normalize(target), compass[i]);

            if (dot_product > max)
            {
                max = dot_product;
                best_match = i;
            }
        }

        return (Direction)best_match;
    }

    public void ResetLevel()
    {
        if (Level == 0)
        {
            Levels[0].Load("levels/one.lvl", Width, Height / 2);
        }
        else if (Level == 1)
        {
            Levels[1].Load("levels/two.lvl", Width, Height / 2);
        }
        else if (Level == 2)
        {
            Levels[2].Load("levels/three.lvl", Width, Height / 2);
        }
        else if (Level == 3)
        {
            Levels[3].Load("levels/four.lvl", Width, Height / 2);
        }
    }

    public void ResetPlayer()
    {
        // redefinir estatísticas do jogador/bola
        Player.Size = PLAYER_SIZE;
        Player.Position = new Vector2(
            Width / 2.0f - PLAYER_SIZE.X / 2.0f,
            Height - PLAYER_SIZE.Y
        );

        Ball.Reset(
            Player.Position + new Vector2(
                PLAYER_SIZE.X / 2.0f - BALL_RADIUS,
                -(BALL_RADIUS * 2.0f)
            ),
            INITIAL_BALL_VELOCITY
        );
    }
}
