// Especificando a versão, assim como no nosso vertex shader.
#version 330 core

// As variáveis ​​de entrada, novamente com o prefixo 'f', pois são as variáveis ​​de entrada do nosso shader de fragmento.
// Por enquanto, elas precisam compartilhar o mesmo nome, embora exista uma maneira de contornar isso mais adiante.
in vec2 fUv;
  
// A saída do nosso fragment shader; ela precisa ser apenas um vec3 ou um vec4, contendo as informações de cor de cada "fragmento" ou pixel da nossa geometria.
out vec4 FragColor;

// É assim que declaramos uma uniform; elas podem ser usadas em todos os nossos shaders e compartilhar o mesmo nome.
// O nome começa com o prefixo "u", pois se trata de uma uniform nossa.
uniform sampler2D uTexture;

void main()
{
    // Aqui estamos definindo nossa variável de saída, cujo nome não é importante.
    FragColor = texture(uTexture, fUv);
}
