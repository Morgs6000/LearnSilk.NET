using System.Numerics;
using MySilkProgram;
using Silk.NET.OpenGL;

namespace Breakout;

public class SpriteRenderer
{
    private GL _gl = Program.GL;

    // Estado de renderização
    private Shader _shader;
    private uint _quadVAO;

    // Construtor (inicializa shaders/formas)
    public SpriteRenderer(Shader shader)
    {
        _shader = shader;

        InitRenderData();
    }

    // Destrutor
    ~SpriteRenderer()
    {
        
    }

    // Renderiza um quadrilátero definido texturizado com o sprite fornecido
    public void DrawSprite(Texture2D texture, Vector2 position, Vector2 size, float rotate, Vector3 color)
    {        
        // preparar transformações
        _shader.Use();

        Matrix4x4 model = Matrix4x4.Identity;

        model *= Matrix4x4.CreateScale(new Vector3(size, 1.0f));

        model *= Matrix4x4.CreateTranslation( // mover a origem de volta
            new Vector3(-0.5f * size.X, -0.5f * size.Y, 0.0f)
        );
        model *= Matrix4x4.CreateFromAxisAngle( // depois rotacione
            Vector3.Normalize(new Vector3(0.0f, 0.0f, 1.0f)),
            MathHelper.DegressToRadians(rotate)
        );
        model *= Matrix4x4.CreateTranslation( // mover a origem da rotação para o centro do quadrilátero
            new Vector3(0.5f * size.X, 0.5f * size.Y, 0.0f)
        );

        model *= Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));

        _shader.SetMatrix4("model", model);

        // renderizar quadrilátero texturizado
        _shader.SetVector3f("spriteColor", color);

        _gl.ActiveTexture(TextureUnit.Texture0);
        texture.Bind();

        _gl.BindVertexArray(_quadVAO);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    // Inicializa e configura o buffer e os atributos de vértice do quad
    private void InitRenderData()
    {
        // configurar VAO/VBO
        uint VBO;

        float[] vertices =
        {
            // pos        // tex
            0.0f, 0.0f,   0.0f, 0.0f, // 0
            1.0f, 0.0f,   1.0f, 0.0f, // 1
            1.0f, 1.0f,   1.0f, 1.0f, // 2
            
            0.0f, 0.0f,   0.0f, 0.0f, // 0
            1.0f, 1.0f,   1.0f, 1.0f, // 2
            0.0f, 1.0f,   0.0f, 1.0f  // 3
        };

        _gl.GenVertexArrays(1, out _quadVAO);
        _gl.GenBuffers(1, out VBO);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        unsafe
        {
            fixed (float* buf = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindVertexArray(_quadVAO);
        _gl.EnableVertexAttribArray(0);
        unsafe
        {
            _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }
}
