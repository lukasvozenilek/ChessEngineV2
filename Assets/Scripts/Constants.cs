using System;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const ulong BB_ALL = 0xffffffffffffffff;
    public const ulong BB_NONE = 0;
    
    
    public const int QueenValue = 8;
    public const int PawnValue = 1;
    public const int KnightValue = 3;
    public const int BishopValue = 3;
    public const int RookValue = 5;


    public static float[] kingTable;
    public static float[] kingTable_endgame;
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
        
        kingTable = Resources.Load<PositionWeightTable>("Position Tables/King").table;
        kingTable_endgame = Resources.Load<PositionWeightTable>("Position Tables/King_Endgame").table;
        
        pawnTable = Resources.Load<PositionWeightTable>("Position Tables/Pawn").table; 
        
        bishopTable = Resources.Load<PositionWeightTable>("Position Tables/Bishop").table; 
        
        knightTable = Resources.Load<PositionWeightTable>("Position Tables/Knight").table; 
        
        queenTable = Resources.Load<PositionWeightTable>("Position Tables/Queen").table; 
        
        rookTable = Resources.Load<PositionWeightTable>("Position Tables/Rook").table;
    }
    
    public static int DirectionToOffset(int dir)
    {
        switch (dir)
        {
            case 0:
                return 8;
            case 1:
                return 9;
            case 2:
                return 1;
            case 3:
                return -7;
            case 4:
                return -8;
            case 5:
                return -9;
            case 6:
                return -1;
            case 7:
                return 7;
            default:
                return 0;
        }
    }
    
    public static string ConvertToCoord(int pos)
    {
        int rank = (pos / 8) + 1;
        int file = pos % 8;

        return boardCoordinates[file].ToString().ToLower() + rank.ToString();
    }
    
    public static string MoveToString(Move move)
    {
        return ConvertToCoord(move.StartSquare) + ConvertToCoord(move.DestinationSquare);
    }
    
    public static Move StringToMove(string moveString)
    {
        Move move = new Move();
        
        int file = FileToNum(moveString[0]);
        int rank = int.Parse(moveString[1].ToString()) - 1;
        move.StartSquare = (rank * 8) + file;
        
        file = FileToNum(moveString[2]);
        rank = int.Parse(moveString[3].ToString()) - 1;
        move.DestinationSquare = (rank * 8) + file;
        
        return move;
    }
    
    public static int FileToNum(char file)
    {
        switch (Char.ToLower(file))
        {
            case 'a':
                return 0;
            case 'b':
                return 1;
            case 'c':
                return 2;
            case 'd':
                return 3;
            case 'e':
                return 4;
            case 'f':
                return 5;
            case 'g':
                return 6;
            case 'h':
                return 7;
            default:
                return -1;
        }
    }
}