using System.Numerics;
using Silk.NET.OpenGL;

namespace MySilkProgram;

public class Shader : IDisposable
{
    private GL _gl = Game.GL;

    public uint Program;

    // O construtor gera o shader em tempo de execução.
    // ------------------------------------------------------------------------
    public Shader(string vertexPath, string fragmentPath)
    {
        // 1. recuperar o código-fonte do vértice/fragmento a partir de filePath

        string vShaderCode = string.Empty;
        string fShaderCode = string.Empty;

        try
        {
            // open files
            vShaderCode = File.ReadAllText(vertexPath);
            fShaderCode = File.ReadAllText(fragmentPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "ERROR::SHADER::FILE_NOT_SUCCESFULLY_READ" + "\n" +
                ex + "\n" + 
                " -- --------------------------------------------------- -- "
            );
        }

        // 2. compilar shaders

        uint vertex, fragment;

        // vertex Shader
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
        Program = _gl.CreateProgram();
        _gl.AttachShader(Program, vertex);
        _gl.AttachShader(Program, fragment);
        _gl.LinkProgram(Program);
        CheckCompileErrors(Program, "PROGRAM");

        // exclua os shaders, pois eles já estão vinculados ao nosso programa e não são mais necessários
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    // ativa o shader
    // ------------------------------------------------------------------------
    public void Use()
    {
        _gl.UseProgram(Program);
    }

    // funções utilitárias de uniformes
    // ------------------------------------------------------------------------

    public void SetBool(string name, bool value)
    {
        int location = _gl.GetUniformLocation(Program, name);
        _gl.Uniform1(location, value ? 1 : 0);
    }

    public void SetInt(string name, int value)
    {
        int location = _gl.GetUniformLocation(Program, name);
        _gl.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        int location = _gl.GetUniformLocation(Program, name);
        _gl.Uniform1(location, value);
    }
    
    public void SetVec3(string name, Vector3 value)
    {
        int location = _gl.GetUniformLocation(Program, name);
        if (location != -1)
            _gl.Uniform3(location, value.X, value.Y, value.Z);
    }

    public void SetMatrix4x4(string name, Matrix4x4 matrix)
    {
        int location = _gl.GetUniformLocation(Program, name);
        unsafe
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
    }

    //
    // ------------------------------------------------------------------------

    public void Dispose()
    {
        _gl.DeleteProgram(Program);
    }

    // função utilitária para verificar erros de compilação/vinculação de shaders.
    // ------------------------------------------------------------------------
    private void CheckCompileErrors(uint shader, string type)
    {
        int success;
        string infoLog;

        if (type != "PROGRAM")
        {
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out success);
            if (success == 0)
            {
                _gl.GetShaderInfoLog(shader, out infoLog);
                Console.WriteLine(
                    "ERROR::SHADER_COMPILATION_ERROR of type: " + type + "\n" + 
                    infoLog + "\n" + 
                    " -- --------------------------------------------------- -- "
                );
            }
        }
        else
        {
            _gl.GetProgram(shader, ProgramPropertyARB.LinkStatus, out success);
            if (success == 0)
            {
                _gl.GetProgramInfoLog(shader, out infoLog);
                Console.WriteLine(
                    "ERROR::PROGRAM_LINKING_ERROR of type: " + type + "\n" +
                    infoLog + "\n" + 
                    " -- --------------------------------------------------- -- "
                );
            }
        }
    }
}
