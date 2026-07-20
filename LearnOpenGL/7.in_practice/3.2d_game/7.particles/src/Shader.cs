using System.Numerics;
using Silk.NET.OpenGL;

namespace Breakout;

public class Shader
{
    private GL _gl = Program.GL;

    // state
    public uint ID;

    // constructor
    public Shader()
    {
        
    }

    // compila o shader a partir do código-fonte fornecido
    public void Compile(string vertexSource, string fragmentSource, string? geometrySource)
    {
        uint sVertex, sFragment, gShader = 0;

        // vertex Shader
        sVertex = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(sVertex, vertexSource);
        _gl.CompileShader(sVertex);
        CheckCompileErrors(sVertex, "VERTEX");

        // fragment Shader
        sFragment = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(sFragment, fragmentSource);
        _gl.CompileShader(sFragment);
        CheckCompileErrors(sFragment, "FRAGMENT");

        // se o código-fonte do shader de geometria for fornecido, compile também o shader de geometria
        if (geometrySource != null)
        {
            gShader = _gl.CreateShader(ShaderType.GeometryShader);
            _gl.ShaderSource(gShader, geometrySource);
            _gl.CompileShader(gShader);
            CheckCompileErrors(gShader, "GEOMETRY");
        }

        // shader Program
        ID = _gl.CreateProgram();
        _gl.AttachShader(ID, sVertex);
        _gl.AttachShader(ID, sFragment);
        if (geometrySource != null)
        {
            _gl.AttachShader(ID, gShader);
        }
        _gl.LinkProgram(ID);
        CheckCompileErrors(ID, "PROGRAM");

        // exclua os shaders, pois eles já estão vinculados ao nosso programa e não são mais necessários
        _gl.DeleteShader(sVertex);
        _gl.DeleteShader(sFragment);
        if (geometrySource != null)
        {
            _gl.DeleteShader(gShader);
        }
    }

    // define o shader atual como ativo
    public Shader Use()
    {
        _gl.UseProgram(ID);

        return this;
    }

    public void SetFloat(string name, float value, bool useShader = false) 
    {
        if (useShader)
        {
            Use();
        }

        _gl.Uniform1(_gl.GetUniformLocation(ID, name), value);
    }

    public void SetInteger(string name, int value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }

        _gl.Uniform1(_gl.GetUniformLocation(ID, name), value);
    }

    public void SetVector2f(string name, float x, float y, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }

        _gl.Uniform2(_gl.GetUniformLocation(ID, name), x, y);
    }

    public void SetVector2f(string name, Vector2 value, bool useShader = false) 
    {
        if (useShader)
        {
            Use();            
        }

        _gl.Uniform2(_gl.GetUniformLocation(ID, name), value);
    }

    public void SetVector3f(string name, float x, float y, float z, bool useShader = false)
    {
        if (useShader)
        {
            Use();            
        }

        _gl.Uniform3(_gl.GetUniformLocation(ID, name), x, y, z);
    }

    public void SetVector3f(string name, Vector3 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();            
        }

        _gl.Uniform3(_gl.GetUniformLocation(ID, name), value);
    }

    public void SetVector4f(string name, float x, float y, float z, float w, bool useShader = false)
    {
        if (useShader)
        {
            Use();            
        }

        _gl.Uniform4(_gl.GetUniformLocation(ID, name), x, y, z, w);
    }

    public void SetVector4f(string name, Vector4 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();            
        }

        _gl.Uniform4(_gl.GetUniformLocation(ID, name), value);
    }

    public void SetMatrix4(string name, Matrix4x4 matrix, bool useShader = false)
    {
        if (useShader)
        {
            Use();   
        }

        unsafe
        {
            _gl.UniformMatrix4(_gl.GetUniformLocation(ID, name), 1, false, (float*)&matrix);
        }
    }

    // verifica se a compilação ou a vinculação falharam e, em caso afirmativo, imprime os logs de erro
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
