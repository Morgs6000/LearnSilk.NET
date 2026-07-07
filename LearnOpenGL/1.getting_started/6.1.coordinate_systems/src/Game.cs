using System.Numerics;
using MySilkProgram.Inputs;
using MySilkProgram.Utilities;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace MySilkProgram;

public class Game
{
    private IWindow _window = null!;
    private GL _gl = null!;

    public static GL GL = null!;

    private Shader _shader = null!;

    private uint _texture1, _texture2;

    // configurar dados de vértice (e buffer(s)) e configurar atributos de vértice
    // ---------------------------------------------------------------------------
    private readonly float[] _vertices =
    {
        // positions           // colors           // texture coords
        -0.5f, -0.5f,  0.0f,   1.0f, 0.0f, 0.0f,   0.0f, 0.0f, // 0 // inferior esquerdo
         0.5f, -0.5f,  0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // 1 // inferior direito
         0.5f,  0.5f,  0.0f,   0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // 2 // superior direito
        -0.5f,  0.5f,  0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f  // 3 // superior esquerdo
    };

    private readonly uint[] _indices = // observe que começamos do 0!
    {
        0, 1, 2, // primeiro triangulo
        0, 2, 3  // segundo triangulo
    };

    private uint _vertexArrayObject;
    private uint _vertexBufferObject;
    private uint _elementBufferObject;

    public Game()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "LearnOpenGL with Silk.NET";
        options.IsVisible = false;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Resize += OnResize;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "Falha ao criar a janela Silk.NET" + "\n" +
                ex + "\n" + 
                " -- --------------------------------------------------- -- "
            );
        }
    }

    private void OnLoad()
    {
        _window.Center();
        _window.IsVisible = true;

        _gl = _window.CreateOpenGL();
        GL = _gl;

        Input.Initialize(_window);

        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // construir e compilar nosso programa de shader
        // ------------------------------------
        _shader = new Shader( // você pode nomear seus arquivos de shader como quiser
            "res/Shaders/base/vertex.glsl",
            "res/Shaders/base/fragment.glsl"
        );

        // carregar e criar uma textura
        // ----------------------------

        // texture 1
        // ---------
        _gl.GenTextures(1, out _texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _texture1);

        // definir os parâmetros de wrapping da textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // define o modo de repetição da textura como GL_REPEAT (método padrão)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // carregar imagem, criar textura e gerar mipmaps
        int width, height;
        byte[] data;

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (Stream stream = File.OpenRead("res/Textures/container.jpg"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            width  = image.Width;
            height = image.Height;
            data   = image.Data;
        }

        if (data != null) 
        {
            unsafe 
            {
                fixed (byte* ptr = data) 
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                }
            }
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura.");
        }

        // texture 2
        // ---------
        _gl.GenTextures(1, out _texture2);
        _gl.BindTexture(TextureTarget.Texture2D, _texture2);

        // definir os parâmetros de wrapping da textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // define o modo de repetição da textura como GL_REPEAT (método padrão)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        // definir parâmetros de filtragem de textura
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        using (Stream stream = File.OpenRead("res/Textures/awesomeface.png"))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            width  = image.Width;
            height = image.Height;
            data   = image.Data;
        }

        if (data != null) 
        {
            unsafe 
            {
                fixed (byte* ptr = data) 
                {
                    _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                }
            }
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }
        else
        {
            Console.WriteLine("Falha ao carregar a textura.");
        }

        // informar ao OpenGL a qual unidade de textura cada sampler pertence (precisa ser feito apenas uma vez)
        // -----------------------------------------------------------------------------------------------------
        _shader.Use(); // não se esqueça de ativar/usar o shader antes de definir os uniforms!
        _shader.SetInt("texture1", 0);
        _shader.SetInt("texture2", 1);

        _gl.GenVertexArrays(1, out _vertexArrayObject);
        _gl.GenBuffers(1, out _vertexBufferObject);
        _gl.GenBuffers(1, out _elementBufferObject);

        // primeiro vincule o Vertex Array Object, depois vincule e configure o(s) buffer(s) de vértices e, em seguida, configure o(s) atributo(s) de vértice.
        _gl.BindVertexArray(_vertexArrayObject);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferObject);
        unsafe
        {
            fixed (float* buf = _vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _elementBufferObject);
        unsafe
        {
            fixed (uint* buf = _indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
            }
        }

        // position attribute
        unsafe
        {
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // color attribute
        unsafe
        {
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        // texture coords attribute
        unsafe
        {
            _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(2);

        // observe que isso é permitido; a chamada para glVertexAttribPointer registrou o VBO como o objeto de buffer de vértices vinculado ao atributo de vértice, portanto, podemos desvinculá-lo com segurança logo em seguida
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        // lembre-se: NÃO desvincule o EBO enquanto um VAO estiver ativo, pois o objeto de buffer de elementos vinculado ESTÁ armazenado no VAO; mantenha o EBO vinculado.
        // _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // Você pode desvincular o VAO posteriormente para que outras chamadas de VAO não modifiquem acidentalmente este VAO, mas isso raramente acontece. Modificar outros VAOs exige uma chamada para glBindVertexArray de qualquer forma, então geralmente não desvinculamos VAOs (nem VBOs) quando não é diretamente necessário.
        _gl.BindVertexArray(0);

        // descomente esta chamada para desenhar polígonos em wireframe.
        // _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        // certifique-se de que a viewport corresponda às novas dimensões da janela; observe que largura e a altura será significativamente maior do que a especificada em telas retina.
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    private void OnUpdate(double deltaTime)
    {
        Time.Update(deltaTime);
        Input.NewFrame();

        if (Input.GetKey(Key.Escape))
        {
            _window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _shader.Use();

        Matrix4x4 model = Matrix4x4.Identity;
        model *= Matrix4x4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegressToRadians(-55.0f));

        Matrix4x4 view = Matrix4x4.Identity;
        view *= Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, -3.0f));

        Matrix4x4 projection = Matrix4x4.Identity;
        projection *= Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegressToRadians(45.0f), (float)_window.Size.X / (float)_window.Size.Y, 0.1f, 100.0f);

        _shader.SetMatrix4x4("model", model);
        _shader.SetMatrix4x4("view", view);
        _shader.SetMatrix4x4("projection", projection);

        // vincular texturas às unidades de textura correspondentes
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture1);
        _gl.ActiveTexture(TextureUnit.Texture1);
        _gl.BindTexture(TextureTarget.Texture2D, _texture2);

        _gl.BindVertexArray(_vertexArrayObject);
        unsafe
        {
            // renderiza o triângulo
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    private void OnClosing()
    {
        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // ---------------------------------------------------------------------------
        _gl.DeleteVertexArrays(1, ref _vertexArrayObject);
        _gl.DeleteBuffers(1, ref _vertexBufferObject);
        _gl.DeleteBuffers(1, ref _elementBufferObject);
        
        _shader.Dispose();
    }
}
