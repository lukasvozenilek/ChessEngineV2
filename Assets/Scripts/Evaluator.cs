using System.Collections.Generic;
using UnityEngine;

public class Evaluator
{
    private Board board;
    public Evaluator(Board board)
    {
        this.board = board;
    }

    private int GetMaterialScore(int piece)
    {
        switch (piece)
        {
            case Piece.Pawn:
                return Constants.PawnValue;
            case Piece.Knight:
                return Constants.KnightValue;
            case Piece.Bishop:
                return Constants.BishopValue;
            case Piece.Rook:
                return Constants.RookValue;
            case Piece.Queen:
                return Constants.QueenValue;
            default:
                return 0;
            
        }
    }

    private float GetLocationScore(int piece, bool pieceColor, int square)
    {
        bool endgame = board.whiteMat + board.blackMat < 16;
        switch (piece)
        {
            case Piece.Pawn:
                return Constants.pawnTableScale * Constants.pawnTable[pieceColor? 63-square:square];
            case Piece.Knight:
                return Constants.knightTableScale * Constants.knightTable[pieceColor? 63-square:square];
            case Piece.Bishop:
                return Constants.bishopTableScale * Constants.bishopTable[pieceColor? 63-square:square];
            case Piece.Rook:
                return Constants.rookTableScale * Constants.rookTable[pieceColor? 63-square:square];
            case Piece.Queen:
                return Constants.queenTableScale * Constants.queenTable[pieceColor? 63-square:square];
            case Piece.King:
                int index = pieceColor ? 63 - square : square;
                return Constants.kingTableScale * (endgame? Constants.kingTable_endgame[index]: Constants.kingTable[index]);
            default:
                return 0;
        }
    }
    
    public float EvaluateBoard()
    {
        int whiteMaterial = 0;
        float whitePosition = 0;
        int blackMaterial = 0;
        float blackPosition = 0;
        foreach (KeyValuePair<int, int> piece in board.whitePieces)
        {
            bool pieceColor = board.GetPieceColor(piece.Key);
            int pieceType = Piece.GetType(piece.Value);
            whiteMaterial += GetMaterialScore(pieceType);
            whitePosition += GetLocationScore(pieceType, pieceColor, piece.Key);
        }
        foreach (KeyValuePair<int, int> piece in board.blackPieces)
        {
            bool pieceColor = board.GetPieceColor(piece.Key);
            int pieceType = Piece.GetType(piece.Value);
            blackMaterial += GetMaterialScore(pieceType);
            blackPosition += GetLocationScore(pieceType, pieceColor, piece.Key);
        }

        board.whiteMat = whiteMaterial;
        board.blackMat = blackMaterial;
        return (whiteMaterial + whitePosition) - (blackMaterial + blackPosition);
    }
}
