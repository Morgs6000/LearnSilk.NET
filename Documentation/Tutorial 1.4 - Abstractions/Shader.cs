using Silk.NET.OpenGL;

namespace MySilkProgram;

public class Shader : IDisposable
{
    // Nosso identificador e a instância do GL que esta classe utilizará; eles são privados porque não há motivo para serem públicos. 
    // Na maioria das vezes, você vai querer abstrair elementos para tornar coisas assim invisíveis.
    private uint _handle;
    private GL _gl;

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;

        // Carrega os shaders individuais.
        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        // Cria o programa de shader.
        _handle = _gl.CreateProgram();

        // Anexe os shaders individuais.
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);

        // Verificar erros de vinculação.
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
        }

        // Desanexar e excluir os shaders
        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use()
    {
        // Usando o programa
        _gl.UseProgram(_handle);
    }

    // Uniforms são propriedades que se aplicam a toda a geometria
    public void SetUniform(string name, int value)
    {
        // Definindo um uniform em um shader usando um nome.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1) // Se GetUniformLocation retornar -1, o uniform não é encontrado.
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        // Lembre-se de excluir o programa quando terminarmos.
        _gl.DeleteProgram(_handle);
    }

    private uint LoadShader(ShaderType type, string path)
    {
        // Para carregar um único shader, precisamos:
        // 1) Carregar o shader de um arquivo. 
        // 2) Criar o identificador (handle). 
        // 3) Enviar o código-fonte para o OpenGL. 
        // 4) Compilar o shader. 
        // 5) Verificar se há erros.
        string src = File.ReadAllText(path);
        uint handle = _gl.CreateShader(type);

        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);

        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}
