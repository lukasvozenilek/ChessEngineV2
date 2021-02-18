public enum PlayerType
{
    Human,
    Minimax,
    MonteCarlo,
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
    
    public virtual MoveResult? GetMove()
    {
        return null;
    }
}
