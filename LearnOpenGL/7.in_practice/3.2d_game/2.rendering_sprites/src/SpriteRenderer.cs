#region Licence
/*

*******************************************************************
** This code is part of Breakout.
**
** Breakout is free software: you can redistribute it and/or modify
** it under the terms of the CC BY 4.0 license as published by
** Creative Commons, either version 4 of the License, or (at your
** option) any later version.
******************************************************************

[pt-BR]

*******************************************************************
** Este código faz parte do Breakout.
**
** O Breakout é um software livre: você pode redistribuí-lo e/ou modificá-lo
** sob os termos da licença CC BY 4.0, conforme publicada pela
** Creative Commons, seja a versão 4 da Licença ou (a seu
** critério) qualquer versão posterior.
******************************************************************

*/
#endregion

using System.Numerics;
using MySilkProgram;
using Silk.NET.OpenGL;

namespace Breakout;

public class SpriteRenderer
{
    private GL _gl = Program.GL;

    // Estado de renderização
    private Shader _shader;
    private uint _quadVAO;

    // Construtor (inicializa shaders/formas)
    public SpriteRenderer(Shader shader)
    {
        _shader = shader;

        InitRenderData();
    }

    // Destrutor
    ~SpriteRenderer()
    {
        _gl.DeleteVertexArrays(1, ref _quadVAO);
    }

    // Renderiza um quadrilátero definido texturizado com o sprite fornecido
    public void DrawSprite(Texture2D texture, Vector2 position, Vector2? size, float rotate = 0.0f, Vector3? color = null)
    {
        Vector2 finalSize = size ?? new Vector2(10.0f, 10.0f);
        Vector3 finalColor = color ?? new Vector3(1.0f);

        // preparar transformações
        _shader.Use();

        Matrix4x4 model = Matrix4x4.Identity;

        model *= Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));

        model *= Matrix4x4.CreateTranslation( // mover a origem da rotação para o centro do quadrilátero
            new Vector3(0.5f * finalSize.X, 0.5f * finalSize.Y, 0.0f)
        );
        model *= Matrix4x4.CreateFromAxisAngle( // depois rotacione
            Vector3.Normalize(new Vector3(0.0f, 0.0f, 1.0f)),
            MathHelper.DegressToRadians(rotate)
        );
        model *= Matrix4x4.CreateTranslation( // mover a origem de volta
            new Vector3(-0.5f * finalSize.X, -0.5f * finalSize.Y, 0.0f)
        );

        model *= Matrix4x4.CreateScale(new Vector3(finalSize, 1.0f));

        _shader.SetMatrix4("model", model);

        // renderizar quadrilátero texturizado
        _shader.SetVector3f("spriteColor", finalColor);

        _gl.ActiveTexture(TextureUnit.Texture0);
        texture.Bind();

        _gl.BindVertexArray(_quadVAO);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _gl.BindVertexArray(0);
    }

    // Inicializa e configura o buffer e os atributos de vértice do quad
    private void InitRenderData()
    {
        // configurar VAO/VBO
        uint VBO;

        float[] vertices =
        {
            // pos        // tex
            0.0f, 1.0f, 0.0f, 1.0f,
            1.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 

            0.0f, 1.0f, 0.0f, 1.0f,
            1.0f, 1.0f, 1.0f, 1.0f,
            1.0f, 0.0f, 1.0f, 0.0f
        };

        _gl.GenVertexArrays(1, out _quadVAO);
        _gl.GenBuffers(1, out VBO);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        unsafe
        {
            fixed (float* buf = vertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindVertexArray(_quadVAO);
        _gl.EnableVertexAttribArray(0);
        unsafe
        {
            _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
    }
}
