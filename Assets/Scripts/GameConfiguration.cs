
public struct GameConfiguration
{
    public PlayerType player1type;
    public PlayerType player2type;
    public string startingFEN;
    
    public GameConfiguration(PlayerType player1type, PlayerType player2type, string startingFEN = Constants.startingFEN)
    {
        this.player1type = player1type;
        this.player2type = player2type;
        this.startingFEN = startingFEN;
    }
}
