public static class Evaluation
{
    public const int QueenValue = 8;
    public const int PawnValue = 1;
    public const int KnightValue = 3;
    public const int BishopValue = 3;
    public const int RookValue = 5;

    public static int EvaluateBoard(Board board)
    {
        return EvaluateMaterial(board);
    }
    
    public static int EvaluateMaterial(Board board)
    {
        int materialPoints = 0;
        //First, material
        for (int i = 0; i < 64; i++)
        {
            int sign = board.GetPieceColor(i) ? -1 : 1;
            switch (Piece.GetType(board.Squares[i]))
            {
                case Piece.Pawn:
                    materialPoints += sign * PawnValue;
                    break;
                case Piece.Knight:
                    materialPoints += sign * KnightValue;
                    break;
                case Piece.Bishop:
                    materialPoints += sign * BishopValue;
                    break;
                case Piece.Rook:
                    materialPoints += sign * RookValue;
                    break;
                case Piece.Queen:
                    materialPoints += sign * QueenValue;
                    break;
            }
        }
        return materialPoints;
    }
}
