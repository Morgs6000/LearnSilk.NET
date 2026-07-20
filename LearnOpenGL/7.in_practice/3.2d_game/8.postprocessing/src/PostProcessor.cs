using System.Numerics;
using Silk.NET.OpenGL;

namespace Breakout;

// A classe PostProcessor gerencia todos os efeitos de pós-processamento para o jogo Breakout.
// Ela renderiza o jogo em um quadrilátero texturizado, permitindo ativar efeitos específicos por meio das variáveis ​​booleanas Confuse, Chaos ou Shake.
// Para que a classe funcione, é necessário chamar BeginRender() antes de renderizar o jogo e EndRender() após a renderização.
public class PostProcessor
{
    private GL _gl = Program.GL;

    // state
    public Shader PostProcessingShader = null!;
    public Texture2D Texture = null!;
    public uint Widht, Height;

    // options
    public bool Confuse, Chaos, Shake;

    // constructor
    public PostProcessor(Shader shader, uint width, uint height)
    {
        PostProcessingShader = shader;

        Texture = new Texture2D();
        
        Widht = width;
        Height = height;

        Confuse = false;
        Chaos = false;
        Shake = false;

        // inicializar objeto renderbuffer/framebuffer
        _gl.GenFramebuffers(1, out MSFBO);
        _gl.GenFramebuffers(1, out FBO);
        _gl.GenRenderbuffers(1, out RBO);

        // inicializa o armazenamento do renderbuffer com um buffer de cor multisampled (não é necessário um buffer de profundidade/stencil)
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
        _gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 4, InternalFormat.Rgb, width, height); // alocar armazenamento para o objeto de buffer de renderização
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Renderbuffer, RBO); // anexa o objeto de buffer de renderização MS ao framebuffer

        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("ERROR::POSTPROCESSOR: Failed to initialize MSFBO");
        }

        // inicializa também o FBO/textura para o qual o buffer de cor com multisampling será copiado (blit); usado para operações de shader (para efeitos de pós-processamento)
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        Texture.Generate(width, height, null!);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Texture2D, Texture.ID, 0); // anexa a textura ao framebuffer como seu anexo de cor

        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("ERROR::POSTPROCESSOR: Failed to initialize FBO");
        }

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // inicializar dados de renderização e uniforms
        InitRenderData();
        PostProcessingShader.SetInteger("scene", 0, true);

        float offset = 1.0f / 300.0f;
        float[,] offsets =
        {
            { -offset,  offset  },  // top-left
            {  0.0f,    offset  },  // top-center
            {  offset,  offset  },  // top-right
            { -offset,  0.0f    },  // center-left
            {  0.0f,    0.0f    },  // center-center
            {  offset,  0.0f    },  // center - right
            { -offset, -offset  },  // bottom-left
            {  0.0f,   -offset  },  // bottom-center
            {  offset, -offset  }   // bottom-right  
        };

        unsafe
        {
            fixed (float* offsetsPtr = offsets)
            {
                _gl.Uniform2(_gl.GetUniformLocation(PostProcessingShader.ID, "offsets"), 9, (float*)offsetsPtr);
            }
        }

        int[] edge_kernel =
        {
            -1, -1, -1,
            -1,  8, -1,
            -1, -1, -1
        };

        _gl.Uniform1(_gl.GetUniformLocation(PostProcessingShader.ID, "edge_kernel"), 9, edge_kernel);

        float[] blur_kernel =
        {
            1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f,
            2.0f / 16.0f, 4.0f / 16.0f, 2.0f / 16.0f,
            1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f
        };

        _gl.Uniform1(_gl.GetUniformLocation(PostProcessingShader.ID, "blur_kernel"), 9, blur_kernel);
    }

    // prepara as operações de framebuffer do pós-processador antes de renderizar o jogo
    public void BeginRender()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    // deve ser chamado após a renderização do jogo, para armazenar todos os dados renderizados em um objeto de textura
    public void EndRender()
    {
        // agora resolve o buffer de cor com multisampling para um FBO intermediário, para armazená-lo em uma textura
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MSFBO);
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
        _gl.BlitFramebuffer(0, 0, (int)Widht, (int)Height, 0, 0, (int)Widht, (int)Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // associa os framebuffers de LEITURA e ESCRITA ao framebuffer padrão
    }

    // renderiza o quad de textura do PostProcessor (como um sprite grande que cobre a tela inteira)
    public void Render(float time)
    {
        // set uniforms/options
        PostProcessingShader.Use();
        PostProcessingShader.SetFloat("time", time);
        PostProcessingShader.SetInteger("confuse", Confuse ? 1 : 0);
        PostProcessingShader.SetInteger("chaos", Chaos ? 1 : 0);
        PostProcessingShader.SetInteger("shake", Shake ? 1 : 0);

        // render textured quad
        _gl.ActiveTexture(TextureUnit.Texture0);
        Texture.Bind();
        _gl.BindVertexArray(VAO);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    // render state
    private uint MSFBO, FBO; // MSFBO = FBO com multisampling. O FBO é padrão, usado para copiar (blit) o ​​buffer de cor MS para uma textura.
    private uint RBO; // O RBO é usado para buffer de cor com multisampling
    private uint VAO;

    // inicializa o quad para renderizar a textura de pós-processamento
    private void InitRenderData()
    {
        // configure VAO/VBO
        uint VBO;

        float[] vertices =
        {
            // pos        // tex
            -1.0f, -1.0f, 0.0f, 0.0f,
            1.0f,  1.0f, 1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f, 1.0f,

            -1.0f, -1.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 1.0f, 0.0f,
            1.0f,  1.0f, 1.0f, 1.0f
        };

        _gl.GenVertexArrays(1, out VAO);
        _gl.GenBuffers(1, out VBO);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);

        _gl.BindVertexArray(VAO);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }
}
