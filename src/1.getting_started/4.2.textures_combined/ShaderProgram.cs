using Silk.NET.OpenGL;

namespace LearnSilkNET;

public class ShaderProgram
{
    public uint ID;

    private GL _gl = Engine.GL;

    // O construtor gera o shader dinamicamente.
    // --------------------------------------------------

    public ShaderProgram(string vertexPath, string fragmentPath)
    {
        string vShaderCode = string.Empty;
        string fShaderCode = string.Empty;

        try
        {
            vShaderCode = File.ReadAllText(vertexPath);
            fShaderCode = File.ReadAllText(fragmentPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "ERROR::SHADER::FILE_NOT_SUCCESSFULLY_READ:"
                + "\n\n" + ex
                + "\n\n" + " -- --------------------------------------------------- -- "
            );
        }

        uint vertex, fragment;

        // vertex shader
        vertex = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertex, vShaderCode);
        _gl.CompileShader(vertex);
        CheckCompileErrors(vertex, "VERTEX");

        // fragment Shader
        fragment = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragment, fShaderCode);
        _gl.CompileShader(fragment);
        CheckCompileErrors(fragment, "FRAGMENT");

        // shader Program
        ID = _gl.CreateProgram();
        _gl.AttachShader(ID, vertex);
        _gl.AttachShader(ID, fragment);
        _gl.LinkProgram(ID);
        CheckCompileErrors(ID, "PROGRAM");

        // Exclua os shaders, pois agora estão integrados ao nosso programa e não são mais necessários.
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    // ativar o shader
    // --------------------------------------------------

    public void Use()
    {
        _gl.UseProgram(ID);
    }

    // funções uniformes de utilidade
    // --------------------------------------------------

    public void SetBool(string name, bool value)
    {
        int location = _gl.GetUniformLocation(ID, name);
        _gl.Uniform1(location, value ? 1 : 0);
    }

    public void SetInt(string name, int value)
    {
        int location = _gl.GetUniformLocation(ID, name);
        _gl.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        int location = _gl.GetUniformLocation(ID, name);
        _gl.Uniform1(location, value);
    }

    // Função utilitária para verificar erros de compilação/vinculação de shaders.
    // --------------------------------------------------

    private void CheckCompileErrors(uint shader, string type)
    {
        int success;
        string infoLog;

        if (type != "PROGRAM")
        {
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out success);
            if (success == 0)
            {
                infoLog = _gl.GetShaderInfoLog(shader);
                Console.WriteLine(
                    "ERROR::SHADER_COMPILATION_ERROR of type: " + type
                    + "\n\n" + infoLog
                    + "\n\n" + " -- --------------------------------------------------- -- "
                );
            }
        }
        else
        {
            _gl.GetProgram(shader, ProgramPropertyARB.LinkStatus, out success);
            if (success == 0)
            {
                infoLog = _gl.GetProgramInfoLog(shader);
                Console.WriteLine(
                    "ERROR::PROGRAM_LINKING_ERROR of type: " + type
                    + "\n\n" + infoLog
                    + "\n\n" + " -- --------------------------------------------------- -- "
                );
            }
        }
    }
}
