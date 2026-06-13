using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LearnSilkNET;

public class Game : Engine
{
    private ShaderProgram _shader = null!;

    private float[] _vertices =
    {
        // positions           // colors
        -0.5f, -0.5f,  0.0f,   1.0f, 0.0f, 0.0f,
         0.5f, -0.5f,  0.0f,   0.0f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.0f,   0.0f, 0.0f, 1.0f,
        -0.5f,  0.5f,  0.0f,   1.0f, 1.0f, 0.0f
    };

    private uint[] _indices =
    {
        0, 1, 2, // primeiro triangulo
        0, 2, 3  // segundo triangulo
    };

    private uint _vertexArrayObject;
    private uint _vertexBufferObject;
    private uint _elementBufferObject;

    // 
    // --------------------------------------------------

    protected override void OnLoad()
    {
        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // shader
        // --------------------------------------------------

        _shader = new ShaderProgram(
            "shaders/vertex.glsl",
            "shaders/fragment.glsl"
        );

        // 
        // --------------------------------------------------

        _vertexArrayObject = _gl.GenVertexArray();
        _gl.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferObject);
        unsafe
        {
            fixed (float* buf = _vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _elementBufferObject = _gl.GenBuffer();
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
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        }
        _gl.EnableVertexAttribArray(0);

        // color attribute
        unsafe
        {
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        _gl.EnableVertexAttribArray(1);

        // 
        // --------------------------------------------------

        // _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
    }

    protected override void OnResize(Vector2D<int> newSize)
    {
        _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
    }

    protected override void OnUpdate(double deltaTime)
    {
        if (Input.GetKey(Key.Escape))
        {
            Close();
        }
    }

    protected override void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // shader
        // --------------------------------------------------

        _shader.Use();

        // 
        // --------------------------------------------------

        _gl.BindVertexArray(_vertexArrayObject);

        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }

        _gl.BindVertexArray(0);
    }

    protected override void OnClosing()
    {
        
    }
}
