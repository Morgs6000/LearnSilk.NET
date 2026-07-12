using System.Numerics;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using MySilkProgram.Inputs;
using MySilkProgram.Utilities;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace MySilkProgram;

public struct Character
{
    public uint TextureID;  // Identificador da textura do glifo
    public Vector2 Size;    // Tamanho do glifo
    public Vector2 Bearing; // Deslocamento da linha de base até a esquerda/topo do glifo
    public uint Advance;    // Deslocamento para avançar para o próximo glifo
}

public class Game
{
    private IWindow _window = null!;
    private GL _gl = null!;

    public static GL GL = null!;

    private Shader _shader = null!;
    
    private Dictionary<uint, Character> Characters = [];

    private uint VAO, VBO;

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
            "res/Shaders/text/vertex.glsl",
            "res/Shaders/text/fragment.glsl"
        );

        // FreeType
        // --------
        unsafe
        {
            /*
            FT_LibraryRec_* lib;
            FT_FaceRec_* face;
            var error = FT_Init_FreeType(&lib);

            error = FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi("some_font_name.ttf"), 0, &face);
            error = FT_Set_Char_Size(face, 0, 16 * 64, 300, 300);
            var glyphIndex = FT_Get_Char_Index(face, 'F');
            error = FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
            error = FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL);
            */

            FT_LibraryRec_* ft;

            if (FT_Init_FreeType(&ft) != 0)
            {
                Console.WriteLine("ERROR::FREETYPE: Could not init FreeType Library");
                return;
            }

            FT_FaceRec_* face;
            if (FT_New_Face(ft, (byte*)Marshal.StringToHGlobalAnsi("res/Fonts/arial.ttf"), 0, &face) != 0)
            {
                Console.WriteLine("ERROR::FREETYPE: Failed to load font");
                return;
            }

            FT_Set_Pixel_Sizes(face, 0, 48);

            /*
            if (FT_Load_Char(face, 'X', FT_LOAD_RENDER) != 0)
            {
                Console.WriteLine("ERROR::FREETYTPE: Failed to load Glyph");
                return;
            }
            //*/

            _gl.PixelStore(GLEnum.UnpackAlignment, 1); // desativar a restrição de alinhamento de bytes

            for (uint c = 0; c < 128; c++)
            {
                // carregar glifo de caractere
                if (FT_Load_Char(face, c, FT_LOAD_RENDER) != 0)
                {
                    Console.WriteLine("ERROR::FREETYTPE: Failed to load Glyph");
                    return;
                }

                // generate texture
                uint texture;
                _gl.GenTextures(1, out texture);
                _gl.BindTexture(TextureTarget.Texture2D, texture);
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Red,
                    face->glyph->bitmap.width,
                    face->glyph->bitmap.rows,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedByte,
                    face->glyph->bitmap.buffer
                );

                // set texture options
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                // agora armazene o caractere para uso posterior
                Character character = new Character
                {
                    TextureID = texture,
                    Size = new Vector2(face->glyph->bitmap.width, face->glyph->bitmap.rows),
                    Bearing = new Vector2(face->glyph->bitmap_left, face->glyph->bitmap_top),
                    Advance = (uint)face->glyph->advance.x,
                };

                Characters.Add(c, character);
            }

            FT_Done_Face(face);
            FT_Done_FreeType(ft);
        }

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _gl.GenVertexArrays(1, out VAO);
        _gl.GenBuffers(1, out VBO);
        _gl.BindVertexArray(VAO);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        unsafe
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(float) * 6 * 4), null, BufferUsageARB.DynamicDraw);
        }

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
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
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        Matrix4x4 projection = GetProjectionMatrix();
        _shader.SetMatrix4x4("projection", projection);

        RenderText(_shader, "This is sample text", 25.0f, 25.0f, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
        RenderText(_shader, "(C) LearnOpenGL.com", 540.0f, 540.0f, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));
    }

    private void OnClosing()
    {
        // opcional: desalocar todos os recursos assim que não forem mais necessários:
        // ---------------------------------------------------------------------------
        
        _shader.Dispose();
    }

    private Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographicOffCenter(
            left: 0.0f,
            right: _window.Size.X,
            bottom: 0.0f,
            top: _window.Size.Y,
            -1.0f,
            1.0f
        );
    }

    private void RenderText(Shader shader, string text, float x, float y, float scale, Vector3 color)
    {
        // ativar o estado de renderização correspondente
        shader.Use();
        _gl.Uniform3(_gl.GetUniformLocation(shader.Program, "textColor"), color);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindVertexArray(VAO);

        // percorrer todos os caracteres
        foreach (var c in text)
        {
            if (!Characters.TryGetValue(c, out Character ch))
            {
                continue;
            }

            float xpos = x + ch.Bearing.X * scale;
            float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

            float w = ch.Size.X * scale;
            float h = ch.Size.Y * scale;

            // atualizar o VBO para cada caractere
            float[] vertices =
            {
                xpos,     ypos + h, 0.0f, 0.0f,
                xpos,     ypos,     0.0f, 1.0f,
                xpos + w, ypos,     1.0f, 1.0f,

                xpos,     ypos + h, 0.0f, 0.0f,
                xpos + w, ypos,     1.0f, 1.0f,
                xpos + w, ypos + h, 1.0f, 0.0f,
            };

            // renderizar textura de glifo sobre quadrilátero
            _gl.BindTexture(TextureTarget.Texture2D, ch.TextureID);

            // atualizar o conteúdo da memória VBO
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

            // render quad
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // agora avance os cursores para o próximo glifo (note que o avanço é em unidades de 1/64 de pixel)
            x += (ch.Advance >> 6) * scale; // deslocamento de bits de 6 posições para obter o valor em pixels (2^6 = 64)
        }

        _gl.BindVertexArray(0);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}
