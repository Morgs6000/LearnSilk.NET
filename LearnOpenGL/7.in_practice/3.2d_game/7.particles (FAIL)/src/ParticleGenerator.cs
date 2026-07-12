using System.Numerics;
using Breakout;
using Silk.NET.OpenGL;

namespace Breakout;

// Representa uma única partícula e seu estado
public struct Particle
{
    public Vector2 Position, Velocity;
    public Vector4 Color;
    public float Life;

    public Particle()
    {
        Position = new Vector2(0.0f);
        Velocity = new Vector2(0.0f);
        Color = new Vector4(1.0f);
        Life = 0.0f;
    }
}

// O ParticleGenerator atua como um contêiner para a renderização de um grande número de partículas, gerando e atualizando partículas repetidamente e eliminando-as após um determinado período de tempo.
public class ParticleGenerator
{
    private GL _gl = Program.GL;

    // state
    private List<Particle> _particles = [];
    private uint _amount;

    // render state
    private Shader _shader = null!;
    private Texture2D _texture = null!;
    private uint VAO;

    private int _lastUsedParticle = 0;

    // constructor
    public ParticleGenerator(Shader shader, Texture2D texture, uint amount)
    {
        _shader = shader;
        _texture = texture;
        _amount = amount;

        Init();
    }

    // update all particles
    public void Update(float dt, GameObject obj, uint newParticles, Vector2 offset)
    {        
        // add new particles 
        for (uint i = 0; i < newParticles; i++)
        {
            int unusedParticle = FirstUnusedParticle();
            RespawnParticle(_particles[unusedParticle], obj, offset);
        }

        // update all particles
        for (int i = 0; i < _amount; i++)
        {
            Particle p = _particles[i];
            p.Life -= dt; // reduce life

            if (p.Life > 0.0f)
            {
                // a partícula está ativa, portanto, atualize-a
                p.Position -= p.Velocity * dt;
                p.Color.W -= dt * 2.5f;
            }
        }
    }   

    // render all particles
    public void Draw()
    {
        // use a mesclagem aditiva para criar um efeito de brilho
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        _shader.Use();

        foreach (Particle particle in _particles)
        {
            if (particle.Life > 0.0f)
            {
                _shader.SetVector2f("offset", particle.Position);
                _shader.SetVector4f("color", particle.Color);

                _texture.Bind();

                _gl.BindVertexArray(VAO);
                _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
                _gl.BindVertexArray(0);
            }
        }
        
        // não se esqueça de redefinir para o modo de mesclagem padrão
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    // inicializa o buffer e os atributos de vértice
    private void Init()
    {
        uint VBO;

        float[] particle_quad =
        {
            0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f,
            
            0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
        };

        _gl.GenVertexArrays(1, out VAO);
        _gl.GenBuffers(1, out VBO);
        _gl.BindVertexArray(VAO);

        // fill mesh buffer
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(particle_quad.Length * sizeof(float)), particle_quad, BufferUsageARB.StaticDraw);

        // set mesh attributes
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        _gl.BindVertexArray(0);

        // cria this->amount instâncias de partícula padrão
        for (uint i = 0; i < _amount; i++)
        {
            _particles.Add(new Particle());
        }
    }

    // retorna o índice da primeira Partícula que não está sendo usada atualmente (por exemplo, Life <= 0.0f) ou 0 se nenhuma partícula estiver inativa no momento
    private int FirstUnusedParticle()
    {
        // pesquisa a partir da última partícula utilizada; isso geralmente retorna quase instantaneamente
        for (int i = _lastUsedParticle; i < _amount; i++)
        {
            if (_particles[i].Life <= 0.0f)
            {
                _lastUsedParticle = i;
                return i;
            }
        }

        // caso contrário, realize uma busca linear
        for (int i = 0; i < _lastUsedParticle; i++)
        {
            if (_particles[i].Life <= 0.0f)
            {
                _lastUsedParticle = i;
                return i;
            }
        }

        // sobrescreve a primeira partícula se todas as outras estiverem vivas
        _lastUsedParticle = 0;
        return 0;
    }

    // respawns particle
    private void RespawnParticle(Particle particle, GameObject obj, Vector2 offset)
    {
        Random rand = new Random();

        float random = ((rand.Next() % 100) - 50) / 10.0f;
        float rColor = 0.5f + ((rand.Next() % 100) / 100.0f);
        particle.Position = obj.Position + new Vector2(random) + offset;
        particle.Color = new Vector4(rColor, rColor, rColor, 1.0f);
        particle.Life = 1.0f;
        particle.Velocity = obj.Velocity * 1.0f;
    }
}
