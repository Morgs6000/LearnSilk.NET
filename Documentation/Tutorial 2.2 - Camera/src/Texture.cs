using Silk.NET.OpenGL;
using StbImageSharp;

namespace MySilkProgram;

public class Texture : IDisposable
{
    private uint _handle;
    private GL _gl;

    public unsafe Texture(GL gl, string path)
    {
        // Salvando a instância do gl.
        _gl = gl;

        // Gerando o identificador do OpenGL;
        _handle = _gl.GenTexture();
        Bind();
            
        // Carrega a imagem da memória.
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        
        fixed (byte* ptr = result.Data)
        {
            // Crie nossa textura e carregue os dados da imagem.
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) result.Width, 
                (uint) result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
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
