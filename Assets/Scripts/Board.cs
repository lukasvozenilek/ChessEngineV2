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
    public bool castle;
}

public static class Board
{
    //Board variables
    public static int[] Squares;
    public static bool turn;
    public static List<Move> moves = new List<Move>();
    
    //Castling rights
    public static bool castling_wk;
    public static bool castling_wq;
    public static bool castling_bk;
    public static bool castling_bq;

    //Constants
    public const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const ulong BB_ALL = 0xffffffffffffffff;
    public const ulong BB_NONE = 0;
    public const int CASTLE_NONE = 0;
    public const int CASTLE_KS = 1;
    public const int CASTLE_QS = 2;
    
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
        Board.Restart();
    }

    public static void Restart()
    {
        LoadPositionFromFEN(startingFEN);
        ResetCastlingRights();
    }

    public static void ResetCastlingRights()
    {
        castling_wk = true;
        castling_wq = true;
        castling_bk = true;
        castling_bq = true;
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
        GameState.UpdateBoard();
    }

    public static List<Move> GetAllLegalMoves()
    {
        List<Move> legalMoves = new List<Move>();
        for (int i = 0; i < 64; i++)
        {
            if (GetPieceColor(i) == turn)
            {
                legalMoves.AddRange(GetLegalMovesFromSquare(i));
            }
        }
        return legalMoves;
    }

    public static List<Move> GetLegalMovesFromSquare(int square)
    {
        int piece = Squares[square];
        bool myColor = GetPieceColor(square);
        List<Move> legalmoves = new List<Move>();

        if (Piece.IsType(piece, Piece.Knight))
        {
            //Iterate through only longitudinal directions to find board edges
            for (int i = 0; i < 8; i = i + 2)
            {
                //Continue if theres room
                if (EdgeDistanceArray[square, i] >= 2)
                {
                    //Calculate intermediate square location
                    int intermedSquare = square + (2 * DirectionToOffset(i));
                
                    //Now check both perpendicular directions
                    for (int j = 0; j < 2; j++)
                    {
                        int i_perp = j == 0 ? i + 2 : i - 2;
                        if (i_perp > 6) i_perp = 0;
                        if (i_perp < 0) i_perp = 6;

                        if (EdgeDistanceArray[intermedSquare, i_perp] >= 1)
                        {
                            int destSquare = intermedSquare + DirectionToOffset(i_perp);
                            if (Squares[destSquare] == Piece.None || (Squares[destSquare] != Piece.None && Board.GetPieceColor(destSquare) != myColor)) legalmoves.Add(new Move(square, destSquare, Squares[destSquare]));
                        }
                    }
                }
            }
        }
        else if (Piece.IsType(piece, Piece.Pawn))
        {
            int destSquare;
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
            //Evaluate castling
            if (Piece.IsType(piece, Piece.King))
            {
                //Black castling
                if (myColor && (castling_bk || castling_bq))
                {
                    if (castling_bk)
                    {
                        if (Squares[square + 1] == Piece.None && Squares[square + 2] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square + 2 , Piece.None));
                        }
                    }
                    if (castling_bq)
                    {
                        if (Squares[square - 1] == Piece.None && Squares[square - 2] == Piece.None && Squares[square - 3] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square - 2 , Piece.None));
                        }
                    }
                }
                //White Castling
                else if (!myColor && (castling_wk || castling_wq))
                {
                    if (castling_wk)
                    {
                        if (Squares[square + 1] == Piece.None && Squares[square + 2] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square + 2 , Piece.None));
                        }
                    }
                    if (castling_wq)
                    {
                        if (Squares[square - 1] == Piece.None && Squares[square - 2] == Piece.None && Squares[square - 3] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square - 2 , Piece.None));
                        }
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
        //Castling rights
        if (move.StartSquare == 0 || move.StartSquare == 4) castling_wk = false;
        if (move.StartSquare == 7 || move.StartSquare == 4) castling_wq = false;
        if (move.StartSquare == 56 || move.StartSquare == 60) castling_bk = false;
        if (move.StartSquare == 63 || move.StartSquare == 60) castling_bq = false;

        Squares[move.DestinationSquare] = Squares[move.StartSquare];
        Squares[move.StartSquare] = Piece.None;
        
        //Check if castle move
        if (Piece.IsType(Squares[move.DestinationSquare], Piece.King) && (Mathf.Abs(move.StartSquare%8 - move.DestinationSquare%8) > 1) )
        {
            
            if (GetPieceColor(move.DestinationSquare))
            {
                //Black
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    Squares[61] = Squares[63];
                    Squares[63] = Piece.None;
                }
                else
                {
                    Squares[59] = Squares[56];
                    Squares[56] = Piece.None;
                }
                
            }
            else
            {
                //White
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    Squares[5] = Squares[7];
                    Squares[7] = Piece.None;
                }
                else
                {
                    Squares[3] = Squares[0];
                    Squares[0] = Piece.None;
                }
            }
            
        }

        moves.Add(move);
        GameState.UpdateBoard();
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
            //if (move.castle > 0) result.castle = true;
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