using System.Numerics;
using Silk.NET.Input;

namespace LearnSilkNET;

public class Camera
{
    // camera Attributes
    public Vector3 Position   = new Vector3( 0.0f,  0.0f,  3.0f);
    public Vector3 Front = new Vector3( 0.0f,  0.0f, -1.0f);
    public Vector3 Up    = new Vector3( 0.0f,  1.0f,  0.0f);

    // euler Angles
    public float Yaw = -90.0f;
    public float Pitch = 0.0f;

    // camera options
    public float MovementSpeed = 2.5f;
    public float MouseSensitivity = 0.1f;
    public float Zoom = 45.0f;

    //
    private bool _firstMouse = true;
    private Vector2 _lastPos;

    // Construtor
    // --------------------------------------------------

    public Camera()
    {
        Input.CursorLockMode = CursorLockMode.Raw;
    }

    // 
    // --------------------------------------------------

    public void Update()
    {
        ProcessKeyboard();
        ProcessMouseMovement();
        ProcessMouseScroll();
    }

    // Retorna a matriz de visualização calculada usando os ângulos de Euler e a matriz LookAt.
    // --------------------------------------------------
    
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(
            cameraPosition: Position,
            cameraTarget:   Position + Front,
            cameraUpVector: Up
        );
    }

    // Retorna a matriz de projeção
    // --------------------------------------------------

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            fieldOfView: Mathf.Radians(Zoom),
            aspectRatio: (float)Screen.Widht / (float)Screen.Height,
            nearPlaneDistance: 0.1f,
            farPlaneDistance: 100.0f
        );
    }

    // Processa a entrada recebida de qualquer sistema de entrada semelhante a um teclado. Aceita parâmetros de entrada na forma de um ENUM definido pela câmera (para abstraí-lo dos sistemas de janelas).
    // --------------------------------------------------

    private void ProcessKeyboard()
    {
        float velocity = MovementSpeed * Time.DeltaTime;

        if (Input.GetKey(Key.W))
        {
            Position += velocity * Vector3.Normalize(new Vector3(Front.X, 0.0f, Front.Z));
        }
        if (Input.GetKey(Key.S))
        {
            Position -= velocity * Vector3.Normalize(new Vector3(Front.X, 0.0f, Front.Z));
        }
        if (Input.GetKey(Key.A))
        {
            Position -= velocity * Vector3.Normalize(Vector3.Cross(Front, Up));
        }
        if (Input.GetKey(Key.D))
        {
            Position += velocity * Vector3.Normalize(Vector3.Cross(Front, Up));
        }
        if (Input.GetKey(Key.Space))
        {
            Position += velocity * Up;
        }
        if (Input.GetKey(Key.ShiftLeft))
        {
            Position -= velocity * Up;
        }
    }

    // Processa a entrada recebida de um sistema de entrada de mouse. Espera o valor de deslocamento nas direções x e y.
    // --------------------------------------------------

    private void ProcessMouseMovement(bool constrainPitch = true)
    {
        if (_firstMouse)
        {
            _lastPos = Input.MousePosition;
            _firstMouse = false;
        }

        float xoffset = Input.MousePosition.X - _lastPos.X;
        float yoffset = _lastPos.Y - Input.MousePosition.Y;
        _lastPos = Input.MousePosition;

        xoffset *= MouseSensitivity;
        yoffset *= MouseSensitivity;

        Yaw += xoffset;
        Pitch += yoffset;

        // Certifique-se de que, quando o campo estiver fora dos limites, a tela não seja invertida.
        // --------------------------------------------------

        if (constrainPitch)
        {
            Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
        }

        // Atualizar vetores Frontal, Direito e Superior usando os ângulos de Euler atualizados
        // --------------------------------------------------

        UpdateCameraVectors();
    }

    // Processa a entrada recebida de um evento de rolagem do mouse. Requer entrada apenas no eixo vertical da roda.
    // --------------------------------------------------

    private void ProcessMouseScroll()
    {
        Zoom -= Input.MouseScrollDelta.Y;
        Zoom = Math.Clamp(Zoom, 1.0f, 45.0f);
    }

    // Calcula o vetor frontal a partir dos ângulos de Euler (atualizados) da câmera.
    // --------------------------------------------------
    
    private void UpdateCameraVectors()
    {
        // calcular o novo vetor Front
        // --------------------------------------------------

        Vector3 front;

        front.X = MathF.Cos(Mathf.Radians(Pitch)) * MathF.Cos(Mathf.Radians(Yaw));
        front.Y = MathF.Sin(Mathf.Radians(Pitch));
        front.Z = MathF.Cos(Mathf.Radians(Pitch)) * MathF.Sin(Mathf.Radians(Yaw));

        Front = Vector3.Normalize(front);
    }
}
