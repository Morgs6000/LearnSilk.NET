// Aqui especificamos a versão do nosso shader.
#version 330 core

// Estas linhas especificam a localização e o tipo dos nossos atributos; os atributos aqui recebem o prefixo "v", pois são as entradas para o vertex shader; embora isso não seja estritamente necessário, é um bom hábito.
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;

// Esta é a nossa variável de saída; observe que ela possui o prefixo 'f', pois é a entrada do nosso shader de fragmentos.
out vec2 fUv;

void main()
{
    // gl_Position é uma variável embutida em todos os vertex shaders que especifica a posição do nosso vértice.
    gl_Position = vec4(vPos, 1.0);

    // O restante deste código parece C puro (quase C#)
    fUv = vUv;
}
