using System.Numerics;

namespace MySilkProgram;

public class Transform
{
    // Uma abstração de transformação. 
    // Para uma transformação, precisamos de posição, escala e rotação; dependendo da aplicação que você está criando, os tipos para esses elementos podem variar. 

    // Aqui, escolhemos vec3 para a posição, float para a escala e quatérnion para a rotação, pois essa é a abordagem mais comum. 
    // Outro exemplo poderia utilizar vec3, vec3 e vec4, de modo que a rotação fosse representada por eixo e ângulo em vez de um quatérnion.

    public Vector3 Position { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);

    public float Scale { get; set; } = 1.0f;

    public Quaternion Rotation { get; set; } = Quaternion.Identity;

    // Nota: A ordem aqui importa.
    public Matrix4x4 ViewMatrix => 
        Matrix4x4.Identity * 
        Matrix4x4.CreateFromQuaternion(Rotation) *
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateTranslation(Position);
}
