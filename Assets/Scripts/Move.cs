public struct Move
{
    public int StartSquare;
    public int DestinationSquare;

    public Move (int startSqr, int destSqr)
    {
        StartSquare = startSqr;
        DestinationSquare = destSqr;
    }
}
public struct MoveResult
{
    public Move move;
    public bool legal;
    public bool capture;
    public int capturedPiece;
    public bool enpassant;
    public bool check;
    public bool castle;
    public bool forfeitedCastling;
}