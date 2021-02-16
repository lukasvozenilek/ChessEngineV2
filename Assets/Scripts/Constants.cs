using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const ulong BB_ALL = 0xffffffffffffffff;
    public const ulong BB_NONE = 0;

    public static Dictionary<char, int> FENPieceNames = new Dictionary<char, int>
    {
        ['p'] = Piece.Pawn,
        ['k'] = Piece.King,
        ['r'] = Piece.Rook,
        ['q'] = Piece.Queen,
        ['n'] = Piece.Knight,
        ['b'] = Piece.Bishop
    };
    
    public static char[] boardCoordinates = new char[8]
    {
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H'
    };

    public static int[,] EdgeDistanceArray;
    public static ulong[] posToBBArray;
    
    
    static Constants()
    {
        EdgeDistanceArray = new int[64,8];
        posToBBArray = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            int file = i % 8;
            int rank = i / 8;

            posToBBArray[i] = (ulong) 1 << i;
            EdgeDistanceArray[i, 0] = 7 - rank;
            EdgeDistanceArray[i, 1] = Mathf.Min(7 - rank, 7 - file);
            EdgeDistanceArray[i, 2] = 7 - file;
            EdgeDistanceArray[i, 3] = Mathf.Min(7 - file,rank );
            EdgeDistanceArray[i, 4] = rank;
            EdgeDistanceArray[i, 5] = Mathf.Min(rank,file);
            EdgeDistanceArray[i, 6] = file;
            EdgeDistanceArray[i, 7] = Mathf.Min(file, 7 - rank);
        }
        Board.Restart();
    }
}
