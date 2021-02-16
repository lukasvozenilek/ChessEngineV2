using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Board
{
    //Board variables
    public int[] Squares;
    public bool turn;
    public List<MoveResult> moves = new List<MoveResult>();
    
    //Castling rights
    public bool castling_wk;
    public bool castling_wq;
    public bool castling_bk;
    public bool castling_bq;

    public List<int> whiteChecks = new List<int>();
    public List<int> blackChecks = new List<int>();
    public List<int> whitePins = new List<int>();
    public List<int> blackPins = new List<int>();
    
    public List<int> potentialWhitePins = new List<int>();
    public List<int> potentialBlackPins = new List<int>();
    
    
    public Board(string FEN = Constants.startingFEN)
    {
        Squares = new int[64];
        LoadPositionFromFEN(FEN);
    }

    public void ResetCastlingRights()
    {
        castling_wk = true;
        castling_wq = true;
        castling_bk = true;
        castling_bq = true;
    }
    
    public void ClearBoard()
    {
        for (int i = 0; i < 64; i++) Squares[i] = Piece.None; 
        moves.Clear();
    }

    public void LoadPositionFromFEN(string fen)
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
                    pieceid += Constants.FENPieceNames[char.ToLower(item)];
                    Squares[boardpos] = pieceid;
                    boardpos++;
                }
            }
        }
        GameState.UpdateBoard();
    }

    public List<Move> GetAllLegalMoves()
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

    public List<Move> GetLegalMovesFromSquare(int square)
    {
        int piece = Squares[square];
        bool myColor = GetPieceColor(square);
        
        List<Move> legalmoves = new List<Move>();
        ulong legalMovesBB = Constants.BB_NONE;
        List<int> attackedLine = new List<int>();
        
        if (Piece.IsType(piece, Piece.Knight))
        {
            //Iterate through only longitudinal directions to find board edges
            for (int i = 0; i < 8; i = i + 2)
            {
                //Continue if theres room
                if (Constants.EdgeDistanceArray[square, i] >= 2)
                {
                    //Calculate intermediate square location
                    int intermedSquare = square + (2 * Constants.DirectionToOffset(i));
                
                    //Now check both perpendicular directions
                    for (int j = 0; j < 2; j++)
                    {
                        int i_perp = j == 0 ? i + 2 : i - 2;
                        if (i_perp > 6) i_perp = 0;
                        if (i_perp < 0) i_perp = 6;

                        if (Constants.EdgeDistanceArray[intermedSquare, i_perp] >= 1)
                        {
                            int destSquare = intermedSquare + Constants.DirectionToOffset(i_perp);
                            if (Squares[destSquare] == Piece.None || (Squares[destSquare] != Piece.None && GetPieceColor(destSquare) != myColor))
                            {
                                legalmoves.Add(new Move(square, destSquare));
                            }
                        }
                    }
                }
            }
        }
        else if (Piece.IsType(piece, Piece.Pawn))
        {
            int destSquare;
            //Pawn moving
            destSquare = GetPieceColor(square) ? square - 8 : square + 8;
            //Ensure still in bounds of board
            if (destSquare < 63 && destSquare > 0)
            {
                if (Squares[destSquare] == Piece.None)
                {
                    legalmoves.Add(new Move(square, destSquare));
                    //Check if pawn on original square
                    if ((square / 8 == 1 && !myColor) || square / 8 == 6 && myColor)
                    {
                        destSquare = myColor ? square - 16 : square + 16;
                        if (Squares[destSquare] == Piece.None) legalmoves.Add(new Move(square, destSquare));
                    }
                } 
            }
            
            int passentSquare = -1;
            //En passant, checks if last move was a pawn push
            if (moves.Count > 0)
            {
                Move previousMove = moves[moves.Count - 1].move;
                //Check if last move was a pawn move
                if (Piece.IsType(Squares[previousMove.DestinationSquare], Piece.Pawn))
                {
                    //Next see if that pawn moved more than 1 square
                    if (Mathf.Abs(previousMove.StartSquare / 8 - previousMove.DestinationSquare / 8) > 1)
                    {
                        //If so, this is a legal en passent move.
                        int offset = GetPieceColor(previousMove.DestinationSquare) ? -8 : 8;
                        passentSquare = previousMove.StartSquare + offset;
                    }
                }
            }
            
            //Pawn Capturing, checks both diagonals
            for (int i = 0; i < 2; i++)
            {
                if (i == 0 && Constants.EdgeDistanceArray[square, 7] == 0) continue;
                if (i == 1 && Constants.EdgeDistanceArray[square, 2] == 0) continue;
                int offset = i == 0 ? 7 : 9;
                destSquare = myColor ? square - offset : square + offset;
                if (destSquare < 63 && destSquare > 0)
                {
                    if (passentSquare == destSquare)
                    {
                        legalmoves.Add(new Move(square, destSquare));
                    } else if (((Squares[destSquare] != Piece.None) && GetPieceColor(destSquare) != myColor))
                    {
                        legalmoves.Add(new Move(square, destSquare));
                    }
                }
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
                    attackedLine.Clear();
                    //Iterate along a direction here
                    int hits = 0;
                    for (int j = 1; j < Mathf.Min(Constants.EdgeDistanceArray[square, i], maxDist) + 1; j++)
                    {
                        int boardPos = square + (Constants.DirectionToOffset(i) * j);
                        int destpiece = Squares[boardPos];

                        //First, check if a piece occupies this square
                        if (destpiece != Piece.None)
                        {
                            if (GetPieceColor(boardPos) == turn)
                            {
                                //Our own piece, cant get past no matter what.
                                if (hits > 1)
                                {
                                    break;
                                }
                            }
                            //If already added capture move, skip this and start looking for another piece behind this piece
                            if (hits == 0 && GetPieceColor(boardPos) != turn)
                            {
                                //Add capture move
                                legalmoves.Add(new Move(square, boardPos));
                                
                                //Look for Check
                                if (Piece.IsType(destpiece, Piece.King))
                                {
                                    if (turn)
                                    {
                                        blackChecks.AddRange(attackedLine);
                                        blackChecks.Add(square);
                                    }
                                    else
                                    {
                                        whiteChecks.AddRange(attackedLine);
                                        whiteChecks.Add(square);
                                    }
                                }
                            }
                            else if (hits <= 2)
                            {
                                //If piece after last piece is a king, we found our pin.
                                if (Piece.IsType(destpiece, Piece.King) && (GetPieceColor(boardPos) != turn))
                                {
                                    if (turn)
                                    {
                                        switch (hits)
                                        {
                                            case 1:
                                                blackPins.AddRange(attackedLine);
                                                blackPins.Add(square);
                                                break;
                                            case 2:
                                                potentialBlackPins.Add(square);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (hits)
                                        {
                                            case 1:
                                                whitePins.AddRange(attackedLine);
                                                whitePins.Add(square);
                                                break;
                                            case 2:
                                                potentialWhitePins.Add(square);
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                            hits++;
                        }
                        //Empty square, add this move
                        else
                        {
                            //Make sure we arent now looking for pins, as those aren't legal moves.
                            if (hits == 0) legalmoves.Add(new Move(square, boardPos));
                        }
                        attackedLine.Add(boardPos);
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
                            legalmoves.Add(new Move(square,square + 2));
                        }
                    }
                    if (castling_bq)
                    {
                        if (Squares[square - 1] == Piece.None && Squares[square - 2] == Piece.None && Squares[square - 3] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square - 2));
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
                            legalmoves.Add(new Move(square,square + 2));
                        }
                    }
                    if (castling_wq)
                    {
                        if (Squares[square - 1] == Piece.None && Squares[square - 2] == Piece.None && Squares[square - 3] == Piece.None)
                        {
                            legalmoves.Add(new Move(square,square - 2));
                        }
                    }
                }
            }
        }

        List<Move> filteredMoves = new List<Move>();
        //Finally check for pins
        if (turn && whitePins.Count > 0)
        {
            foreach (Move move in legalmoves)
            {
                if (!whitePins.Contains(move.StartSquare) || (whitePins.Contains(move.StartSquare) && whitePins.Contains(move.DestinationSquare)))
                {
                    filteredMoves.Add(move);
                }
            } 
            legalmoves = filteredMoves.ToList();
        }
        if (!turn && blackPins.Count > 0)
        {
            foreach (Move move in legalmoves)
            {
                if (!blackPins.Contains(move.StartSquare) || (blackPins.Contains(move.StartSquare) && blackPins.Contains(move.DestinationSquare)))
                {
                    filteredMoves.Add(move);
                }
            }
            legalmoves = filteredMoves.ToList();
        }
        
        //Checks
        if (turn && whiteChecks.Count > 0)
        {
            foreach (Move move in legalmoves)
            {
                if ((!Piece.IsType(Squares[move.StartSquare], Piece.King) && whiteChecks.Contains(move.DestinationSquare)) || (Piece.IsType(Squares[move.StartSquare], Piece.King) && !whiteChecks.Contains(move.DestinationSquare)))
                {
                    filteredMoves.Add(move);
                }
            }
            legalmoves = filteredMoves.ToList();
        }
        if (!turn && blackChecks.Count > 0)
        {
            foreach (Move move in legalmoves)
            {
                if ((!Piece.IsType(Squares[move.StartSquare], Piece.King) && blackChecks.Contains(move.DestinationSquare)) || (Piece.IsType(Squares[move.StartSquare], Piece.King) && !blackChecks.Contains(move.DestinationSquare)))
                {
                    filteredMoves.Add(move);
                }
            }
            legalmoves = filteredMoves.ToList();
        }


        return legalmoves;
    }

    

    public MoveResult MakeMove(Move move, bool sendEvent = true)
    {
        MoveResult result = new MoveResult();
        result.move = move;
        
        //Castling rights
        if (move.StartSquare == 0 || move.StartSquare == 4) castling_wk = false;
        if (move.StartSquare == 7 || move.StartSquare == 4) castling_wq = false;
        if (move.StartSquare == 56 || move.StartSquare == 60) castling_bk = false;
        if (move.StartSquare == 63 || move.StartSquare == 60) castling_bq = false;

        result.capturedPiece = Squares[move.DestinationSquare];
        result.capture = Squares[move.DestinationSquare] != Piece.None;
        
        //Execute enpassent move
        if (Piece.IsType(Squares[move.StartSquare], Piece.Pawn) &&  (move.StartSquare%8 != move.DestinationSquare%8 )&& Squares[move.DestinationSquare] == Piece.None)
        {
            int offset = GetPieceColor(move.StartSquare) ? 8 : -8;
            Squares[move.DestinationSquare + offset] = Piece.None;
            result.capturedPiece = Piece.Pawn + (GetPieceColor(move.StartSquare) ? Piece.White : Piece.Black);
            result.enpassant = true;
            result.capture = true;
        }
        
        Squares[move.DestinationSquare] = Squares[move.StartSquare];
        Squares[move.StartSquare] = Piece.None;
        
        //Execute castling
        if (Piece.IsType(Squares[move.DestinationSquare], Piece.King) && (Mathf.Abs(move.StartSquare%8 - move.DestinationSquare%8) > 1) )
        {
            
            if (GetPieceColor(move.DestinationSquare))
            {
                //Black
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    Squares[61] = Squares[63];
                    Squares[63] = Piece.None;
                    result.castle = true;
                }
                else
                {
                    Squares[59] = Squares[56];
                    Squares[56] = Piece.None;
                    result.castle = true;
                }
            }
            else
            {
                //White
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    Squares[5] = Squares[7];
                    Squares[7] = Piece.None;
                    result.castle = true;
                }
                else
                {
                    Squares[3] = Squares[0];
                    Squares[0] = Piece.None;
                    result.castle = true;
                }
            }
        }
        
        //Evaluate all possible new attacks
        if (turn)
        {
            foreach (int sqr in potentialBlackPins.ToList())
            {
                GetLegalMovesFromSquare(sqr);
            }
            //Clear opponents pins
            whitePins.Clear();
            potentialWhitePins.Clear();
            whiteChecks.Clear();
        }
        else
        {
            foreach (int sqr in potentialWhitePins.ToList())
            {
                GetLegalMovesFromSquare(sqr);
            }
            //Clear opponents pins
            potentialBlackPins.Clear();
            blackPins.Clear();
            blackChecks.Clear();
        }
        
        //Now evaluate new attacks
        GetLegalMovesFromSquare(move.DestinationSquare);

        //Finally, add move to records and update turn.
        moves.Add(result);
        if (sendEvent) GameState.UpdateBoard();
        turn = !turn;
        GetAllLegalMoves();
        return result;
    }

    public void UnmakeMove(bool sendEvent=true)
    {
        if (moves.Count == 0) return;
        MoveResult lastMove = moves[moves.Count-1];
        Squares[lastMove.move.StartSquare] = Squares[lastMove.move.DestinationSquare];
        if (lastMove.enpassant)
        {
            Squares[lastMove.move.DestinationSquare] = Piece.None;
            int offset = GetPieceColor(lastMove.move.StartSquare) ? 8 : -8;
            Squares[lastMove.move.DestinationSquare + offset] = lastMove.capturedPiece;
        }
        else if (lastMove.castle)
        {
            if (GetPieceColor(lastMove.move.DestinationSquare))
            {
                //Black
                if (Squares[lastMove.move.DestinationSquare + 1] == Piece.None)
                {
                    Squares[63] = Squares[61];
                    Squares[61] = Piece.None;
                }
                else
                {
                    Squares[56] = Squares[59];
                    Squares[59] = Piece.None;
                }
                //Re-enable castling rights
                castling_bk = true;
                castling_bq = true;
            }
            else
            {
                //White
                if (Squares[lastMove.move.DestinationSquare + 1] == Piece.None)
                {
                    Squares[7] = Squares[5];
                    Squares[5] = Piece.None;
                    
                }
                else
                {
                    Squares[0] = Squares[3];
                    Squares[3] = Piece.None;
                }
                //Re-enable castling rights
                castling_wk = true;
                castling_wq = true;
            }
            Squares[lastMove.move.DestinationSquare] = Piece.None;
        }
        else
        {
            Squares[lastMove.move.DestinationSquare] = lastMove.capturedPiece;
        }
        
        blackPins.Clear();
        whitePins.Clear();
        blackChecks.Clear();
        whiteChecks.Clear();
        
        moves.RemoveAt(moves.Count-1);
        turn = !turn;
        if (sendEvent) GameState.UpdateBoard();
    }

    public MoveResult RequestMove(Move move)
    {
        MoveResult result = new MoveResult();
        result.move = move;
        if (GetPieceColor(move.StartSquare) == turn && GetLegalMovesFromSquare(move.StartSquare).Contains(move))
        {
            result = MakeMove(move);
            result.legal = true;
        }
        else
        {
            result.legal = false;
        }
        return result;
    }

    public bool GetPieceColor(int square)
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
}