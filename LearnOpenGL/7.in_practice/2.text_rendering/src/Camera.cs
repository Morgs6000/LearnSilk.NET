using System.Numerics;
using MySilkProgram.Inputs;
using MySilkProgram.Utilities;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace MySilkProgram;

// Uma classe de câmera abstrata que processa a entrada e calcula os ângulos de Euler, vetores e matrizes correspondentes para uso no OpenGL
public class Camera
{
    // camera Attributes
    public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f);
    public Vector3 Front = new Vector3(0.0f, 0.0f, -1.0f);
    public Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);

    // euler Angles
    public float Yaw = -90.0f;
    public float Pitch = 0.0f;

    // camera options
    public float MovementSpeed = 2.5f;
    public float MouseSensitivity = 0.1f;
    public float Zoom = 45.0f;

    private bool _firstMouse = true;
    private Vector2 _lastPos;

    // constructor
    public Camera()
    {
        
    }

    // retorna a matriz de visualização calculada usando ângulos de Euler e a matriz LookAt
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(
            Position,
            Position + Front,
            Up
        );
    }

    // retorna a matriz de projeção
    public Matrix4x4 GetProjectionMatrix(IWindow window)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.DegressToRadians(Zoom), 
            (float)window.Size.X / (float)window.Size.Y, 
            0.1f, 
            100.0f
        );
    }

    // processa a entrada recebida de qualquer sistema de entrada do tipo teclado. Aceita um parâmetro de entrada na forma de um ENUM definido pela câmera (para abstraí-lo de sistemas de janelas)
    public void ProcessKeyboad()
    {
        float cameraSpeed = MovementSpeed * Time.DeltaTime;

        if (Input.GetKey(Key.W))
        {
            Position += cameraSpeed * Vector3.Normalize(new Vector3(Front.X, 0.0f, Front.Z));
        }
        if (Input.GetKey(Key.S))
        {
            Position -= cameraSpeed * Vector3.Normalize(new Vector3(Front.X, 0.0f, Front.Z));
        }
        if (Input.GetKey(Key.A))
        {
            Position -= cameraSpeed * Vector3.Normalize(Vector3.Cross(Front, Up));
        }
        if (Input.GetKey(Key.D))
        {
            Position += cameraSpeed * Vector3.Normalize(Vector3.Cross(Front, Up));
        }

        if (Input.GetKey(Key.Space))
        {
            Position += cameraSpeed * Up;
        }
        if (Input.GetKey(Key.ShiftLeft))
        {
            Position -= cameraSpeed * Up;
        }
    }

    // processa a entrada recebida de um sistema de entrada de mouse. Espera o valor de deslocamento nas direções x e y.
    public void ProcessMouseMovement()
    {
        if (_firstMouse)
        {
            _lastPos = Input.MousePositon;
            _firstMouse = false;
        }

        float xoffset = Input.MousePositon.X - _lastPos.X;
        float yoffset = _lastPos.Y - Input.MousePositon.Y;
        _lastPos = Input.MousePositon;

        xoffset *= MouseSensitivity;
        yoffset *= MouseSensitivity;

        Yaw   += xoffset;
        Pitch += yoffset;

        // certifique-se de que a tela não seja invertida quando o pitch estiver fora dos limites
        Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);

        // atualiza os vetores Front, Right e Up usando os ângulos de Euler atualizados
        UpdateCameraVectors();
    }

    // processa a entrada recebida de um evento de roda de rolagem do mouse. Requer entrada apenas no eixo vertical da roda.
    public void ProcessMouseScroll()
    {
        Zoom -= Input.MouseScrollDelta.Y;
        Zoom = Math.Clamp(Zoom, 1.0f, 45.0f);
    }

    // calcula o vetor frontal a partir dos ângulos de Euler (atualizados) da câmera
    private void UpdateCameraVectors()
    {
        // calcula o novo vetor Front
        Vector3 direction;

        direction.X = MathF.Cos(MathHelper.DegressToRadians(Pitch)) * MathF.Cos(MathHelper.DegressToRadians(Yaw));
        direction.Y = MathF.Sin(MathHelper.DegressToRadians(Pitch));
        direction.Z = MathF.Cos(MathHelper.DegressToRadians(Pitch)) * MathF.Sin(MathHelper.DegressToRadians(Yaw));

        Front = Vector3.Normalize(direction);
    }
}
