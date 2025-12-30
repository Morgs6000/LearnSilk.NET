using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace MySilkProgram;

public class Program
{
    private static IWindow _window = null!;
    private static GL _gl = null!;

    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;

    private static uint _program;
    private static uint _texture;

    private static void Main(string[] args)
    {
        // Console.WriteLine("Hello, World!");

        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "My first Silk.NET application!";

        _window = Window.Create(options);

        _window.Load   += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
    }

    private static unsafe void OnLoad()
    {
        IInputContext input = _window.CreateInput();

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.CornflowerBlue);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        float[] vertices =
        {
            // aPosition          // aTexCoords
            -0.5f, -0.5f, 0.0f,   0.0f, 0.0f,
             0.5f, -0.5f, 0.0f,   1.0f, 0.0f,
             0.5f,  0.5f, 0.0f,   1.0f, 1.0f,
            -0.5f,  0.5f, 0.0f,   0.0f, 1.0f
        };

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }

        uint[] indices =
        {
            0u, 1u, 2u,
            0u, 2u, 3u
        };

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }

        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);

        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        const string vertexCode = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec2 aTextureCoord;

            out vec2 frag_texCoords;

            void main()
            {
                gl_Position = vec4(aPosition, 1.0);

                frag_texCoords = aTextureCoord;
            }
        ";

        const string fragmentCode = @"
            #version 330 core
            out vec4 out_color;

            in vec2 frag_texCoords;

            uniform sampler2D uTexture;

            void main()
            {
                //out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
                out_color = texture(uTexture, frag_texCoords);
            }
        ";

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
        {
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
        }

        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
        }

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        _texture = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes("silk.png"), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* ptr = result.Data)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _gl.GenerateMipmap(TextureTarget.Texture2D);

        _gl.BindTexture(TextureTarget.Texture2D, 0);

        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }    

    private static void OnUpdate(double deltaTime)
    {
        
    }

    private static unsafe void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.BindVertexArray(_vao);

        _gl.UseProgram(_program);

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
    
    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if(key == Key.Escape)
        {
            _window.Close();
        }
    }
}
