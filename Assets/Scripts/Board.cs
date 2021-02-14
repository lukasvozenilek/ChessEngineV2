using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Board
{
    public static int[] Squares;

    public static string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    
    public static Dictionary<char, int> FENPieceNames = new Dictionary<char, int>
    {
        ['p'] = Piece.Pawn,
        ['k'] = Piece.King,
        ['r'] = Piece.Rook,
        ['q'] = Piece.Queen,
        ['n'] = Piece.Knight,
        ['b'] = Piece.Bishop
    };

    static Board()
    {
        Squares = new int[64];
        LoadPositionFromFEN(startingFEN);
    }

    public static void LoadPositionFromFEN(string fen)
    {
        int boardpos = 56;
        foreach (char item in fen)
        {
            if (item == '/')
            {
                boardpos = 8 * (((int)boardpos/8)-2);
            }
            else
            {
                if (char.IsDigit(item))
                {
                    Squares[boardpos] = Piece.None;
                    boardpos += (int) char.GetNumericValue(item);
                }
                else
                {
                    int pieceid = char.IsUpper(item) ? Piece.White : Piece.Black;
                    pieceid += FENPieceNames[char.ToLower(item)];
                    Squares[boardpos] = pieceid;
                    boardpos++;
                    if (boardpos == 8) break;
                }
            }
        }
    }
}