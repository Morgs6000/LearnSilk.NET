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

    private List<GameLevel> Levels = new List<GameLevel>();
    private List<PowerUp> PowerUps = new List<PowerUp>();

    private int Level;
    private int Lives;

    // Game-related State data
    private SpriteRenderer Renderer = null!;
    private GameObject Player = null!;
    private BallObject Ball = null!;
    private ParticleGenerator Particles = null!;
    private PostProcessor Effects = null!;
    private TextRenderer Text = null!;

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
        State = GameState.GAME_MENU;
        Width = width;
        Height = height;
        Lives = 3;
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

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_speed.png", 
            alpha: true, 
            name:  "powerup_speed"
        );

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_sticky.png", 
            alpha: true, 
            name:  "powerup_sticky"
        );

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_increase.png", 
            alpha: true, 
            name:  "powerup_increase"
        );

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_confuse.png", 
            alpha: true, 
            name:  "powerup_confuse"
        );

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_chaos.png", 
            alpha: true, 
            name:  "powerup_chaos"
        );

        ResourceManager.LoadTexture(
            file:  "res/Textures/powerup_passthrough.png", 
            alpha: true, 
            name:  "powerup_passthrough"
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

        Text = new TextRenderer(Width, Height);
        Text.Load("res/Fonts/OCRAEXT.TTF", 24);

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
        if (State == GameState.GAME_MENU)
        {
            if (Input.GetKeyDown(Key.Enter))
            {
                State = GameState.GAME_ACTIVE;                
            }
        }
        if (Input.GetKeyDown(Key.W))
        {
            Level = (Level + 1) % 4;
        }
        if (Input.GetKeyDown(Key.S))
        {
            if (Level > 0)
            {
                Level--;
            }
            else
            {
                Level = 3;
            }

            // Level = (Level - 1) % 4;
        }
        if (State == GameState.GAME_WIN)
        {
            if (Input.GetKeyDown(Key.Enter))
            {
                Effects.Chaos = false;

                State = GameState.GAME_MENU;
            }
        }
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

        // update PowerUps
        UpdatePowerUps(deltaTime);

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
            Lives--;

            // o jogador perdeu todas as suas vidas? : fim de jogo
            if (Lives == 0)
            {
                ResetLevel();
                State = GameState.GAME_MENU;
            }
            
            ResetPlayer();
        }

        // verifica a condição de vitória
        if (State == GameState.GAME_ACTIVE && Levels[Level].IsCompleted())
        {
            ResetLevel();
            ResetPlayer();

            Effects.Chaos = true;

            State = GameState.GAME_WIN;
        }
    }

    public void Render()
    {
        if (State == GameState.GAME_ACTIVE || 
            State == GameState.GAME_MENU
        )
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

                // draw PowerUps
                foreach (PowerUp powerUp in PowerUps)
                {
                    if (!powerUp.Destroyed)
                    {
                        powerUp.Draw(Renderer);
                    }
                }

                // draw particles
                Particles.Draw();

                // draw ball
                Ball.Draw(Renderer);
            }

            // finalizar a renderização para o framebuffer de pós-processamento
            Effects.EndRender();

            // renderizar quad de pós-processamento
            Effects.Render(Time.ElapsedTime);

            // renderizar texto (não incluir no pós-processamento)
            Text.RenderText($"Lives: {Lives}", 5.0f, 5.0f, 1.0f);
        }
        if (State == GameState.GAME_MENU)
        {
            Text.RenderText("Press ENTER to start", 250.0f, Height / 2, 1.0f);
            Text.RenderText("Press W or S to select level", 245.0f, Height / 2 + 20.0f, 0.75f);
        }
        if (State == GameState.GAME_WIN)
        {
            Text.RenderText("You WON!!!", 320.0f, Height / 2.0f - 20.0f, 1.0f, new Vector3(0.0f, 1.0f, 0.0f));
            Text.RenderText("Press ENTER to retry or ESC to quit", 130.0f, Height / 2.0f, 1.0f, new Vector3(1.0f, 1.0f, 0.0f));
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

                        SpawnPowerUps(box);
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

                    if (!(Ball.PassThrough && !box.IsSolid)) // não realize a resolução de colisão em blocos não sólidos se a travessia estiver ativada
                    {
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
        }

        // verifique também colisões com PowerUps e, se houver, ative-os
        foreach (PowerUp powerUp in PowerUps)
        {
            if (!powerUp.Destroyed)
            {
                // primeiro, verifique se o power-up ultrapassou a borda inferior; se sim: mantenha-o inativo e destrua-o
                if (powerUp.Position.Y >= Height)
                {
                    powerUp.Destroyed = true;
                }
                if (CheckCollision(Player, powerUp))
                {
                    // colidiu com o jogador, agora ativa o power-up
                    ActivatePowerUp(powerUp);
                    powerUp.Destroyed = true;
                    powerUp.Activated = true;
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

            // se o power-up "Sticky" estiver ativado, também grude a bola na raquete após o cálculo dos novos vetores de velocidade
            Ball.Stuck = Ball.Sticky;
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

        Lives = 3;
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

    private bool ShouldSpawn(uint chance)
    {
        Random r = new Random();
        uint random = (uint)r.Next() % chance;

        return random == 0;
    }

    private void SpawnPowerUps(GameObject block)
    {
        if (ShouldSpawn(75)) // 1 in 75 chance
        {
            PowerUps.Add(new PowerUp(
                "speed", 
                new Vector3(0.5f, 0.5f, 1.0f),
                0.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_speed")
            ));
        }
        if (ShouldSpawn(75))
        {
            PowerUps.Add(new PowerUp(
                "sticky", 
                new Vector3(1.0f, 0.5f, 1.0f),
                20.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_sticky")
            ));
        }
        if (ShouldSpawn(75))
        {
            PowerUps.Add(new PowerUp(
                "pass-through", 
                new Vector3(0.5f, 1.0f, 0.5f),
                10.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_passthrough")
            ));
        }
        if (ShouldSpawn(75))
        {
            PowerUps.Add(new PowerUp(
                "pad-size-increase", 
                new Vector3(1.0f, 0.6f, 0.4f),
                0.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_increase")
            ));
        }
        if (ShouldSpawn(15)) // power-ups negativos devem surgir com mais frequência
        {
            PowerUps.Add(new PowerUp(
                "confuse", 
                new Vector3(1.0f, 0.3f, 0.3f),
                15.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_confuse")
            ));
        }
        if (ShouldSpawn(15))
        {
            PowerUps.Add(new PowerUp(
                "chaos", 
                new Vector3(0.9f, 0.25f, 0.25f),
                15.0f,
                block.Position,
                ResourceManager.GetTexture("powerup_chaos")
            ));
        }
    }

    private bool IsOtherPowerUpActive(List<PowerUp> powerUps, string type)
    {
        // Verifica se outro PowerUp do mesmo tipo ainda está ativo caso em que não desativamos seu efeito (ainda)
        foreach (PowerUp powerUp in PowerUps)
        {
            if (powerUp.Activated)
            {
                if (powerUp.Type == type)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdatePowerUps(float dt)
    {
        foreach (PowerUp powerUp in PowerUps)
        {
            powerUp.Position += powerUp.Velocity * dt;

            if (powerUp.Activated)
            {
                powerUp.Duration -= dt;

                if (powerUp.Duration <= 0.0f)
                {
                    // remove o power-up da lista (será removido posteriormente)
                    powerUp.Activated = false;

                    // desativar efeitos
                    if (powerUp.Type == "sticky")
                    {
                        if (!IsOtherPowerUpActive(PowerUps, "sticky"))
                        {
                            // redefinir apenas se nenhum outro PowerUp do tipo "sticky" estiver ativo
                            Ball.Sticky = false;
                            Player.Color = new Vector3(1.0f);
                        }
                    }
                    else if (powerUp.Type == "pass-through")
                    {
                        if (!IsOtherPowerUpActive(PowerUps, "pass-through"))
                        {
                            // redefinir apenas se nenhum outro PowerUp do tipo "pass-through" estiver ativo
                            Ball.PassThrough = false;
                            Ball.Color = new Vector3(1.0f);
                        }
                    }
                    else if (powerUp.Type == "confuse")
                    {
                        if (!IsOtherPowerUpActive(PowerUps, "confuse"))
                        {
                            // redefinir apenas se nenhum outro PowerUp do tipo "confusão" estiver ativo
                            Effects.Confuse = false;
                        }
                    }
                    else if (powerUp.Type == "chaos")
                    {
                        if (!IsOtherPowerUpActive(PowerUps, "chaos"))
                        {
                            // reinicia apenas se nenhum outro PowerUp do tipo caos estiver ativo
                            Effects.Chaos = false;
                        }
                    }
                }
            }
        }

        // Remove do vetor todos os PowerUps que estejam destruídos E NÃO ativados (ou seja, fora do mapa ou já encerrados)
        // Nota: utilizamos uma expressão lambda para remover cada PowerUp que esteja destruído e não ativado
        PowerUps.RemoveAll(powerUp => powerUp.Destroyed && !powerUp.Activated);
    }

    private void ActivatePowerUp(PowerUp powerUp)
    {
        if (powerUp.Type == "speed")
        {
            Ball.Velocity *= 1.2f;
        }
        else if (powerUp.Type == "sticky")
        {
            Ball.Sticky = true;
            Player.Color = new Vector3(1.0f, 0.5f, 1.0f);
        }
        else if (powerUp.Type == "pass-throug")
        {
            Ball.PassThrough = true;
            Ball.Color = new Vector3(1.0f, 0.5f, 0.5f);
        }
        else if (powerUp.Type == "pad-size-increase")
        {
            Player.Size.X += 50.0f;
        }
        else if (powerUp.Type == "confuse")
        {
            if (!Effects.Chaos)
            {
                Effects.Confuse = true; // ativar apenas se o caos ainda não estivesse ativo
            }
        }
        else if (powerUp.Type == "chaos")
        {
            if (!Effects.Confuse)
            {
                Effects.Chaos = true;
            }
        }
    }
}
