using System;
using System.Collections.Generic;

public enum PlayerType
{
    Human,
    Minimax,
    MonteCarlo,
    Random
}

public class Player
{
    public Board board;
    public MoveGenerator moveGenerator;
    public event Action<MoveResult?> MoveCompleteEvent;
    public Dictionary<Move, float> moveEvaluation = new Dictionary<Move, float>();
    
    public Player (Board board)
    {
        this.board = board;
        moveGenerator = new MoveGenerator();
    }
    
    public virtual void PlayMove()
    {
        return;
    }

    public void InvokeMoveComplete(MoveResult? moveResult)
    {
        MoveCompleteEvent?.Invoke(moveResult);
    }
}
