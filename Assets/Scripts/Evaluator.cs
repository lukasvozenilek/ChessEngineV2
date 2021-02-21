using UnityEngine;

public class Evaluator
{
    private Board board;
    public Evaluator(Board board)
    {
        this.board = board;
    }
    
    public float EvaluateBoard()
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
                    evaluation += sign * Constants.PawnValue;
                    evaluation += sign * Constants.pawnTableScale * Constants.pawnTable[pieceColor? 63-i:i];
                    break;
                case Piece.Knight:
                    evaluation += sign * Constants.KnightValue;
                    evaluation += sign * Constants.knightTableScale * Constants.knightTable[pieceColor? 63-i:i];
                    break;
                case Piece.Bishop:
                    evaluation += sign * Constants.BishopValue;
                    evaluation += sign * Constants.bishopTableScale * Constants.bishopTable[pieceColor? 63-i:i];
                    break;
                case Piece.Rook:
                    evaluation += sign * Constants.RookValue;
                    evaluation += sign * Constants.rookTableScale * Constants.rookTable[pieceColor? 63-i:i];
                    break;
                case Piece.Queen:
                    evaluation += sign * Constants.QueenValue;
                    evaluation += sign * Constants.queenTableScale * Constants.queenTable[pieceColor? 63-i:i];
                    break;
                case Piece.King:
                    evaluation += sign * Constants.kingTableScale * Constants.kingTable[pieceColor? 63-i:i];
                    break;
            }
        }
        return evaluation;
    }
}
