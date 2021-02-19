using UnityEngine;

public static class Evaluation
{
    public const int QueenValue = 8;
    public const int PawnValue = 1;
    public const int KnightValue = 3;
    public const int BishopValue = 3;
    public const int RookValue = 5;


    public static float[] kingTable;
    public static float[] pawnTable;
    public static float[] bishopTable;
    public static float[] knightTable;
    public static float[] queenTable;
    public static float[] rookTable;

    public const float kingTableScale = 0.1f;
    public const float pawnTableScale = 0.1f;
    public const float bishopTableScale = 0.1f;
    public const float knightTableScale = 0.1f;
    public const float queenTableScale = 0.1f;
    public const float rookTableScale = 0.1f;

    static Evaluation ()
    {
        kingTable = Resources.Load<PositionWeightTable>("Position Tables/King").table;
        pawnTable = Resources.Load<PositionWeightTable>("Position Tables/Pawn").table; 
        bishopTable = Resources.Load<PositionWeightTable>("Position Tables/Bishop").table; 
        knightTable = Resources.Load<PositionWeightTable>("Position Tables/Knight").table; 
        queenTable = Resources.Load<PositionWeightTable>("Position Tables/Queen").table; 
        rookTable = Resources.Load<PositionWeightTable>("Position Tables/Rook").table; 
    }
    
    public static float EvaluateBoard(Board board)
    {
        float evaluation = 0;
        //First, material
        for (int i = 0; i < 64; i++)
        {
            bool pieceColor = board.GetPieceColor(i);
            int sign =  pieceColor? -1 : 1;
            switch (Piece.GetType(board.Squares[i]))
            {
                case Piece.Pawn:
                    evaluation += sign * PawnValue;
                    evaluation += sign * pawnTableScale * pawnTable[pieceColor? 63-i:i];
                    break;
                case Piece.Knight:
                    evaluation += sign * KnightValue;
                    evaluation += sign * knightTableScale * knightTable[pieceColor? 63-i:i];
                    break;
                case Piece.Bishop:
                    evaluation += sign * BishopValue;
                    evaluation += sign * bishopTableScale * bishopTable[pieceColor? 63-i:i];
                    break;
                case Piece.Rook:
                    evaluation += sign * RookValue;
                    evaluation += sign * rookTableScale * rookTable[pieceColor? 63-i:i];
                    break;
                case Piece.Queen:
                    evaluation += sign * QueenValue;
                    evaluation += sign * queenTableScale * queenTable[pieceColor? 63-i:i];
                    break;
                case Piece.King:
                    evaluation += sign * kingTableScale * kingTable[pieceColor? 63-i:i];
                    break;
            }
        }
        return evaluation;
    }
}
