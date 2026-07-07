# region
/*

https://dotnet.github.io/Silk.NET/docs/opengl/c1/3-hello-texture

https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.3%20-%20Textures/Program.cs

*/
# endregion

using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window;
    private static GL _gl;

    // O código do shader de vértices.
    private static readonly string _vertexShaderSource = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        // Além do atributo aPosition, criamos agora um atributo aTexCoords para as coordenadas de textura.
        layout (location = 1) in vec2 aTextureCoord;

        // Da mesma forma, também atribuímos um atributo out para ser passado para o shader de fragmentos.
        out vec2 frag_texCoords;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0f);

            // Este shader de vértices básico não processa as coordenadas de textura, então podemos passá-las diretamente para o shader de fragmentos.
            frag_texCoords = aTextureCoord;
        }
    ";

    // O código do shader de fragmentos.
    private static readonly string _fragmentShaderSource = @"
        #version 330 core

        // Este atributo de entrada corresponde ao atributo de saída que definimos no shader de vértices.
        in vec2 frag_texCoords;

        out vec4 out_color;

        // Agora definimos um valor uniforme!
        // Um ​​valor uniforme em OpenGL é um valor que pode ser alterado fora do shader, modificando seu valor.
        // Um ​​sampler2D contém uma textura e informações sobre como amostrá-la.
        // Amostrar uma textura é basicamente calcular a cor de um pixel em uma textura em qualquer ponto.
        uniform sampler2D uTexture;

        void main()
        {
            // Usamos a função texture do GLSL para amostrar da textura nas coordenadas de textura de entrada fornecidas.
            out_color = texture(uTexture, frag_texCoords);
        }
    ";

    private static uint _program;

    private static uint _texture;

    // Os dados dos vértices do quadrilátero. 
    // Você deve ter notado uma adição: coordenadas de textura! 
    // Coordenadas de textura são valores entre 0 e 1 (veremos mais sobre isso adiante) que indicam à GPU qual parte
    // da textura deve ser usada para cada vértice.
    private static readonly float[] _vertices =
    {
    //  aPosition            | aTexCoords
        -0.5f, -0.5f,  0.0f,   0.0f, 1.0f,
         0.5f, -0.5f,  0.0f,   1.0f, 1.0f,
         0.5f,  0.5f,  0.0f,   1.0f, 0.0f,
        -0.5f,  0.5f,  0.0f,   0.0f, 0.0f
    };

    // Os dados de índices dos quads.
    private static readonly uint[] indices =
    {
        0u, 1u, 2u,
        0u, 2u, 3u
    };

    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;

    private static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;        
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "LearnOpenGL with Silk.NET";
        options.IsVisible = false;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;

        _window.Run();

        _window.Dispose();
    }

    private static unsafe void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        IInputContext input = _window.CreateInput();        
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.CornflowerBlue);

        // Crie nosso shader de vértices e forneça a ele o código-fonte do shader de vértices.
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, _vertexShaderSource);

        // Tenta compilar o shader.
        _gl.CompileShader(vertexShader);

        // Verifique se o shader foi compilado com sucesso.
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        // Repita este processo para o shader de fragmento.
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, _fragmentShaderSource);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
        }

        // Crie seu programa de shader e anexe os shaders de vértice e de fragmento.
        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        // Tenta "vincular" o programa.
        _gl.LinkProgram(_program);

        // Assim como na compilação de shaders, verifique se o programa de shader foi vinculado corretamente.
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
        }

        // Desvincula e exclui nossos shaders. Uma vez que um programa é vinculado, não precisamos mais dos objetos de shader individuais.
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Agora criamos nossa textura! 
        // Primeiro, criamos a textura em si. Em seguida, devemos definir uma unidade de textura ativa. Cada unidade de textura é uma textura vinculável independente que podemos utilizar em um shader. As GPUs possuem um número máximo de unidades de textura que podem utilizar; no entanto, a especificação do OpenGL determina que DEVE haver pelo menos 32 unidades disponíveis. 
        // Assim como fazemos com buffers, vinculamos então a textura a um alvo Texture2D.
        _texture = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        // Use o StbImageSharp para carregar uma imagem do nosso arquivo PNG. 
        // Isso carregará e descompactará o resultado em um array de bytes brutos que podemos passar diretamente para o OpenGL.
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("silk.png"), ColorComponents.RedGreenBlueAlpha);

        // Define um ponteiro para os dados da imagem
        fixed(byte* ptr = result.Data)
        {
            // Carregar nossos dados de textura para a GPU. 
            // Vamos analisar cada parâmetro utilizado aqui:
            // 1. Informar ao OpenGL que queremos carregar dados na textura vinculada ao alvo Texture2D. 
            // 2. Estamos carregando o nível "base" da textura; portanto, este valor deve ser 0. Você não precisa se preocupar com níveis de textura por enquanto. 
            // 3. Informar ao OpenGL que queremos que a GPU armazene esses dados no formato RGBA na própria GPU. 
            // 4. A largura da imagem. 
            // 5. A altura da imagem. 
            // 6. Esta é a borda da imagem. Este valor DEVE ser 0. É um componente remanescente do OpenGL legado e não tem nenhuma utilidade. 
            // 7. Nossos dados de imagem estão formatados como RGBA; portanto, devemos informar ao OpenGL que estamos carregando dados RGBA. 
            // 8. A biblioteca StbImageSharp retorna esses dados como um array de bytes (byte[]); portanto, devemos informar ao OpenGL que estamos carregando dados no formato de byte sem sinal (unsigned byte). 
            // 9. O ponteiro real para os nossos dados!
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        // Vamos definir alguns parâmetros da textura! 
        // Isso instrui a GPU sobre como ela deve amostrar a textura. 

        // Define o modo de repetição (wrap mode) da textura como "repeat". 
        // O modo de repetição define o que deve acontecer quando as coordenadas da textura saem do intervalo de 0 a 1.
        // Neste caso, definimos como "repeat". A textura será simplesmente repetida em um padrão de mosaico contínuo. 
        // Observe que estamos usando os modos de repetição S e T aqui. Essa é a versão do OpenGL para o mapeamento UV padrão com o qual você talvez esteja mais familiarizado, onde S corresponde ao eixo X e T ao eixo Y.
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);

        // Os filtros de minificação (min) e magnificação (mag) definem como a textura deve ser amostrada ao ser redimensionada. 
        // O filtro "min" (minificação) é usado quando a textura é reduzida de tamanho. 
        // O filtro "mag" (magnificação) é usado quando a textura é aumentada de tamanho. 
        // Estamos utilizando filtragem bilinear aqui, pois ela geralmente produz um bom resultado. 
        // Você também pode usar filtragem "nearest" (ou "point") ou filtragem anisotrópica; esta última está disponível apenas
        // para o filtro de minificação. 
        // Você pode notar que o filtro de minificação também define um filtro de "mipmap". Abordaremos os mipmaps mais adiante.
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Gere mipmaps para esta textura.
        // Observação: Precisamos fazer isso ou a textura aparecerá preta (esta é uma opção que você pode alterar, mas está fora do escopo deste tutorial).
        // O que é um mipmap?
        // Um ​​mipmap é essencialmente uma versão menor da textura existente. Ao gerar mipmaps, o tamanho da textura é continuamente reduzido pela metade, geralmente parando quando atinge um tamanho de 1x1 pixel. (Observação: existem exceções a isso, por exemplo, se a GPU atingir seu nível máximo de mipmaps, que é tanto uma limitação de hardware quanto um valor definido pelo usuário. Você não precisa se preocupar com isso por enquanto, então apenas assuma que os mipmaps serão gerados até 1x1 pixel).
        // Os mipmaps são usados ​​quando a textura é reduzida em tamanho, para produzir um resultado muito melhor e para reduzir os padrões de efeito moiré.
        _gl.GenerateMipmap(TextureTarget.Texture2D);

        // Desvincule a textura, pois não precisamos mais atualizá-la.
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        // Obtenha o uniform da textura e defina-o como 0.
        // Podemos fazer isso facilmente usando glGetUniformLocation e fornecendo um nome. 
        // Definir o valor como 0 indica que você deseja utilizar a unidade de textura de índice 0.
        // Geralmente, o OpenGL deve inicializar automaticamente todos os valores uniform com seus valores padrão (que são quase sempre 0); no entanto, é recomendável adquirir o hábito de inicializar todos os valores uniform com um valor conhecido antes de utilizá-los no shader.
        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        // Crie o VAO.
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Cria o VBO.
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Carrega os dados dos vértices para o VBO.
        fixed (float* buf = _vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }

        // Crie o EBO.
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        // Carregue os dados dos índices para o EBO.
        fixed (uint* buf = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }

        // Configurar nossos atributos de vértice! Eles informam ao array de vértices (VAO) como processar os dados de vértice que definimos  anteriormente. Cada array de vértices contém atributos.

        // Nossa constante de stride (passo). O stride deve ser especificado em bytes; portanto, pegamos o primeiro atributo (um vec3), multiplicamos pelo tamanho em bytes de um float e, em seguida, pegamos o segundo atributo (um vec2) e fazemos o mesmo.
        const uint stride = (3 * sizeof(float)) + (2 * sizeof(float));

        // Habilite o atributo "aPosition" em nosso array de vértices, fornecendo também seu tamanho e stride.
        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

        // Agora precisamos habilitar nossas coordenadas de textura! Definimos isso como a localização 1, então é isso que usaremos aqui. O código é muito semelhante ao anterior, mas você deve garantir que o deslocamento (offset) seja definido como o **tamanho em bytes** do atributo anterior.
        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        // Desvincule tudo, pois não precisamos disso.
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // Finalmente, um pouco de blending! 
        // Se você desativar o blending, notará uma borda preta ao redor da textura. 
        // A textura é parcialmente transparente, mas o OpenGL não sabe como lidar com isso por padrão. 
        // Ao ativar o blending e definir uma função de blend, você instrui o OpenGL sobre como lidar com a transparência. 
        // Nesse caso, isso remove o fundo preto e deixa apenas a textura. 
        // A função de blend foge do escopo deste tutorial, então não se preocupe se não a compreender totalmente. 
        // O programa funcionará perfeitamente sem o blending!
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static void OnFramebufferResize(Vector2D<int> newSize)
    {
        _gl.Viewport(newSize);
    }

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static unsafe void OnRender(double deltaTime)
    {
        // Limpa a janela com a cor definida anteriormente.
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Vincule nosso VAO e, em seguida, o programa.
        _gl.UseProgram(_program);

        // Assim como fizemos anteriormente com a criação de texturas, primeiro  precisamos definir a unidade de textura ativa e, em seguida, vincular a textura para utilizá-la durante a renderização!
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        _gl.BindVertexArray(_vao);

        // Desenhe nosso quadrilátero! Usamos uma contagem de 6 aqui porque temos um total de 6 vértices que compõem um quadrilátero.
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static void OnClose()
    {
        
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }
}
