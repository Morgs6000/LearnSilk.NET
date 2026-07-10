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

namespace Breakout;

// Representa o estado atual do jogo
public enum GameState
{
    GAME_ACTIVE,
    GAME_MENU,
    GAME_WIN
}

// A classe Game encapsula todo o estado e a funcionalidade relacionados ao jogo.
// Ela reúne todos os dados do jogo em uma única classe para
// facilitar o acesso a cada um dos componentes e o gerenciamento.
public class Game
{
    // game state
    public GameState State;
    public uint Width, Height;

    // constructor
    public Game(uint width, uint height)
    {
        State = GameState.GAME_ACTIVE;
        Width = width;
        Height = height;
    }

    // destructor (finalizer in C#)
    ~Game()
    {
        // Limpar recursos, se necessário
    }

    // inicializar o estado do jogo (carregar todos os shaders/texturas/níveis)
    public void Init()
    {
        
    }

    public void ProcessInput(float deltaTime)
    {
        
    }

    public void Update(float deltaTime)
    {
        
    }

    public void Render()
    {
        
    }
}
