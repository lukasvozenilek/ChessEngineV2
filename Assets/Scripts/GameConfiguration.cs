
public struct GameConfiguration
{
    public Player.PlayerType player1type;
    public Player.PlayerType player2type;
    public int player1Diff;
    public int player2Diff;
    public string startingFEN;
    
    public GameConfiguration(Player.PlayerType player1type, Player.PlayerType player2type, int player1Diff = 1, int player2Diff = 1, string startingFEN = Constants.startingFEN)
    {
        this.player1type = player1type;
        this.player2type = player2type;
        this.player1Diff = player1Diff;
        this.player2Diff = player2Diff;
        this.startingFEN = startingFEN;
    }
}
