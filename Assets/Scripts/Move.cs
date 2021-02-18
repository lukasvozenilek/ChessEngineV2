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
    public bool pawn2squares;
    public CastlingRights castlingRights;
}

public struct CastlingRights
{
    public bool w_qs;
    public bool w_ks;
    public bool b_qs;
    public bool b_ks;
}