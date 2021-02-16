public struct Move
{
    public int StartSquare;
    public int DestinationSquare;
    public int promotionID;

    public Move(int startSqr, int destSqr, int promotionID = 0)
    {
        StartSquare = startSqr;
        DestinationSquare = destSqr;
        this.promotionID = promotionID;
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