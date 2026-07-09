using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MySilkProgram;

public class Texture : IDisposable
{
    private uint _handle;
    private GL _gl;

    public string Path { get; set; }
    public TextureType Type { get; }

    public unsafe Texture(GL gl, string path, TextureType type = TextureType.None)
    {
        // Salvando a instância do gl.
        _gl = gl;

        Path = path;
        Type = type;

        // Gerando o identificador do OpenGL;
        _handle = _gl.GenTexture();
        Bind();
            
        // Carrega a imagem da memória.
        using (var img = Image.Load<Rgba32>(path))
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

            img.ProcessPixelRows(acessor =>
            {
                for (int y = 0; y < acessor.Height; y++)
                {
                    fixed(void* data = acessor.GetRowSpan(y))
                    {
                        _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)acessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                }
            });
        }

        SetParameters();
    }

    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        // Salvando a instância do gl.
        _gl = gl;

        // Gerando o identificador do OpenGL;
        _handle = _gl.GenTexture();
        Bind();

        // Queremos também a capacidade de criar uma textura usando dados gerados por código.
        fixed (void* d = &data[0])
        {
            // Definindo os dados de uma textura.
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        // Definindo alguns parâmetros de textura para que a textura se comporte conforme o esperado.
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        
        // Gerando mipmaps.
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        // Ao vincular uma textura, podemos escolher a qual slot de textura ela será vinculada.
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Dispose()
    {
        // Para realizar o descarte, precisamos excluir o identificador OpenGL da textura.
        _gl.DeleteTexture(_handle);
    }
}
