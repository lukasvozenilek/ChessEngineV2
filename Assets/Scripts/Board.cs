using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public struct Move
{
    public int StartSquare;
    public int DestinationSquare;
    public int capturedID;

    public Move (int startSqr, int destSqr, int cappturedid)
    {
        StartSquare = startSqr;
        DestinationSquare = destSqr;
        capturedID = cappturedid;
    }
}

public struct MoveResult
{
    public bool legal;
    public bool capture;
    public bool enpassant;
    public bool check;
}

public static class Board
{
    //Board variables
    public static int[] Squares;
    public static bool turn;
    public static List<Move> moves = new List<Move>();

    //Constants
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

    public static int[,] EdgeDistanceArray;  

    static Board()
    {
        Squares = new int[64];
        EdgeDistanceArray = new int[64,8];

        for (int i = 0; i < 64; i++)
        {
            int file = i % 8;
            int rank = i / 8;

            EdgeDistanceArray[i, 0] = 7 - rank;
            EdgeDistanceArray[i, 1] = Mathf.Min(7 - rank, 7 - file);
            EdgeDistanceArray[i, 2] = 7 - file;
            EdgeDistanceArray[i, 3] = Mathf.Min(7 - file,rank );
            EdgeDistanceArray[i, 4] = rank;
            EdgeDistanceArray[i, 5] = Mathf.Min(rank,file);
            EdgeDistanceArray[i, 6] = file;
            EdgeDistanceArray[i, 7] = Mathf.Min(file, 7 - rank);
        }
        
        LoadPositionFromFEN(startingFEN);
    }

    public static void ClearBoard()
    {
        for (int i = 0; i < 64; i++) Squares[i] = Piece.None; 
        moves.Clear();
    }

    public static void LoadPositionFromFEN(string fen)
    {
        //First, clear board
        ClearBoard();
        turn = false;
        
        int boardpos = 56;
        foreach (char item in fen)
        {
            if (item == ' ') break;
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
                }
            }
        }
    }

    public static List<Move> GetLegalMovesFromSquare(int square)
    {
        int piece = Squares[square];
        List<Move> legalmoves = new List<Move>();

        if (Piece.IsType(piece, Piece.Knight))
        {
            Debug.Log("Knight!");
        }
        else if (Piece.IsType(piece, Piece.Pawn))
        {
            int destSquare;
            bool myColor = GetPieceColor(square);
            //Pawn moving
            destSquare= GetPieceColor(square) ? square - 8 : square + 8;
            if (Squares[destSquare] == Piece.None)
            {
                legalmoves.Add(new Move(square, destSquare, Squares[destSquare]));
                //Check if pawn on original square
                if ((square / 8 == 1 && !myColor) || square / 8 == 6 && myColor)
                {
                    destSquare = myColor ? square - 16 : square + 16;
                    if (Squares[destSquare] == Piece.None) legalmoves.Add(new Move(square, destSquare, Squares[destSquare]));
                }
            }
            //Pawn Capturing
            //TODO: Combine these
            destSquare= myColor ? square - 9 : square + 9;
            if (Squares[destSquare] != Piece.None && GetPieceColor(destSquare) != myColor)
            {
                legalmoves.Add(new Move(square, destSquare, Squares[destSquare]));
            }
            destSquare= myColor ? square - 7 : square + 7;
            if (Squares[destSquare] != Piece.None && GetPieceColor(destSquare) != myColor)
            {
                legalmoves.Add(new Move(square, destSquare, Squares[destSquare]));
            }
        }
        else
        {
            //Sliding type move behavior
            //Iterate through directions
            for (int i = 0; i < 8; i++)
            {
                bool lat = i % 2 == 0;
                if (Piece.IsType(piece, Piece.Queen) || Piece.IsType(piece, Piece.King) || (lat && Piece.IsType(piece, Piece.Rook)) || (!lat && Piece.IsType(piece, Piece.Bishop)))
                {
                    int maxDist = Piece.IsType(piece, Piece.King) ? 1 : 8;
                    for (int j = 1; j < Mathf.Min(EdgeDistanceArray[square, i], maxDist) + 1; j++)
                    {
                        int boardPos = square + (DirectionToOffset(i) * j);
                        
                        int destpiece = Squares[boardPos];

                        //First, check if a piece occupies this square
                        if (destpiece != Piece.None)
                        {
                            if (GetPieceColor(boardPos) == turn)
                            {
                                //Our own piece, cant get past no matter what.
                                break;
                            }
                            else
                            {
                                legalmoves.Add(new Move(square, boardPos, destpiece));
                                //We break here as we cant go past a capture
                                break;
                            }
                        }

                        legalmoves.Add(new Move(square, boardPos, destpiece));
                    }
                }
            }
        }
        return legalmoves;
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

    public static void MakeMove(Move move)
    {
        Squares[move.DestinationSquare] = Squares[move.StartSquare];
        Squares[move.StartSquare] = Piece.None;
        moves.Add(move);
        turn = !turn;
    }

    public static void UnmakeMove()
    {
        Move lastMove = moves[moves.Count-1];
        Squares[lastMove.StartSquare] = Squares[lastMove.DestinationSquare];
        Squares[lastMove.DestinationSquare] = lastMove.capturedID;
        moves.RemoveAt(moves.Count-1);
        turn = !turn;
        GameState.UpdateBoard();
    }

    public static MoveResult RequestMove(Move move)
    {
        MoveResult result = new MoveResult();
        if (GetPieceColor(move.StartSquare) == turn && GetLegalMovesFromSquare(move.StartSquare).Contains(move))
        {
            if (Squares[move.DestinationSquare] != Piece.None && GetPieceColor(move.DestinationSquare) != turn) result.capture = true;
            MakeMove(move);
            result.legal = true;
        }
        else
        {
            result.legal = false;
        }

        return result;
    }

    public static bool GetPieceColor(int square)
    {
        int piece = Squares[square];
        if ((piece & Piece.White) > 0)
        {
            return false;
        } else if ((piece & Piece.Black) > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public static ulong PositionToBB(int pos)
    {
        return (ulong)1 << pos;
    }
}