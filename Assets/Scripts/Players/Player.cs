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
    
    public Player (Board board)
    {
        this.board = board;
        moveGenerator = new MoveGenerator();
    }
    
    public virtual MoveResult? PlayMove()
    {
        return null;
    }
}
