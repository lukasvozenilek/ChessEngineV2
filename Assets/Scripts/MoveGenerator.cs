
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class MoveGenerator
{
    public static List<Move> legalMoves = new List<Move>();
    public static List<int> attackedSquares = new List<int>();
    public static List<int> checkedSquares = new List<int>();
    public static List<int> pinnedSquares = new List<int>();
    public static List<int> attackedLine = new List<int>();

    public static void CalculateAttacks(Board board, bool player)
    {
        attackedSquares.Clear();
        checkedSquares.Clear();
        pinnedSquares.Clear();

        int destSquare;

        for (int square = 0; square < 64; square++)
        {
            if (!Piece.IsType(board.Squares[square], Piece.None) && board.GetPieceColor(square) == player)
            {
                int piece = board.Squares[square];
                switch (Piece.GetType(piece))
                {
                    case Piece.Knight:
                        for (int direction = 0; direction < 8; direction += 2)
                        {
                            //Continue if theres room
                            if (Constants.EdgeDistanceArray[square, direction] >= 2)
                            {
                                //Calculate intermediate square location
                                int intermedSquare = square + (2 * Constants.DirectionToOffset(direction));

                                //Now check both perpendicular directions
                                for (int j = 0; j < 2; j++)
                                {
                                    int i_perp = j == 0 ? direction + 2 : direction - 2;
                                    if (i_perp > 6) i_perp = 0;
                                    if (i_perp < 0) i_perp = 6;

                                    if (Constants.EdgeDistanceArray[intermedSquare, i_perp] >= 1)
                                    {
                                        destSquare = intermedSquare + Constants.DirectionToOffset(i_perp);
                                        int attackedPiece = board.Squares[destSquare];
                                        attackedSquares.Add(destSquare);
                                        if (Piece.IsType(attackedPiece, Piece.King) && (board.GetPieceColor(destSquare) != player))
                                        {
                                            checkedSquares.Add(square);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Piece.Pawn:
                        //Pawn Attacks, checks both diagonals
                        for (int pd = 0; pd < 2; pd++)
                        {
                            int direction1 = player ? 2 : 7;
                            int direction2 = player ? 7 : 2;
                            if (pd == 0 && Constants.EdgeDistanceArray[square, direction1] == 0) continue;
                            if (pd == 1 && Constants.EdgeDistanceArray[square, direction2] == 0) continue;
                            int offset = pd == 0 ? 7 : 9;
                            destSquare = player ? square - offset : square + offset;
                            if (destSquare > 63 || destSquare < 0) continue;
                            if (Piece.IsType(board.Squares[destSquare], Piece.King) && board.GetPieceColor(destSquare) != player)
                            {
                                checkedSquares.Add(destSquare);
                            }
                            attackedSquares.Add(destSquare);
                        }
                        break;
                    case Piece.Rook:
                    case Piece.Queen:
                    case Piece.Bishop:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            bool lat = direction % 2 == 0;
                            attackedLine.Clear();
                            bool hit = false;
                            bool kinghit = false;
                            
                            if (Piece.IsType(piece, Piece.Queen) ||
                                (lat && Piece.IsType(piece, Piece.Rook)) || 
                                (!lat && Piece.IsType(piece, Piece.Bishop)))
                            {
                                //Iterate along a direction here
                                for (int offset = 1; offset < Constants.EdgeDistanceArray[square, direction] + 1; offset++)
                                {
                                    int boardPos = square + (Constants.DirectionToOffset(direction) * offset);
                                    if (boardPos < 0 || boardPos > 63) continue;
                                    int destpiece = board.Squares[boardPos];
                                    
                                    
                                    //First, check if a piece occupies this square
                                    if (destpiece != Piece.None)
                                    {
                                        if (!hit) attackedSquares.Add(boardPos);
                                        
                                        //If our piece, stop here as no checks or pins can occur.
                                        if (board.GetPieceColor(boardPos) == player) break;
                                        
                                        if (Piece.IsType(destpiece, Piece.King))
                                        {
                                            if (!hit)
                                            {
                                                checkedSquares.AddRange(attackedLine);
                                                checkedSquares.Add(square);
                                                if (kinghit)
                                                {
                                                    attackedSquares.Add(boardPos);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                pinnedSquares.AddRange(attackedLine);
                                                pinnedSquares.Add(square);
                                                break;
                                            }
                                            kinghit = true;
                                        }
                                        else
                                        {
                                            if (hit)
                                            {
                                                break;
                                            }
                                            hit = true;
                                        }
                                    }
                                    else
                                    {
                                        if (!hit) attackedSquares.Add(boardPos); 
                                    }
                                    
                                    attackedLine.Add(boardPos);
                                }
                            }
                        }
                        break;
                    case Piece.King:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            int boardPos = square + Constants.DirectionToOffset(direction);
                            if (attackedSquares.Contains(boardPos)) continue;
                            if (boardPos < 0 || boardPos > 63) continue;
                            int destpiece = board.Squares[boardPos];
                            attackedSquares.Add(boardPos);
                        }
                        break;
                }
            }
        }
    }
    
    public static List<Move> GetAllLegalMoves(Board board)
    {
        legalMoves.Clear();
        CalculateAttacks(board, !board.turn);
        
        for (int square = 0; square < 64; square++)
        {
            //Only iterate through squares that contain my own piece
            //TODO: Cache piece locations so we don't have to check this statement 64 times.
            if (!Piece.IsType(board.Squares[square], Piece.None) && board.GetPieceColor(square) == board.turn)
            {
                int piece = board.Squares[square];
                bool myColor = board.GetPieceColor(square);
                int destSquare;

                switch (Piece.GetType(piece))
                {
                    case Piece.Knight:
                        //Iterate through only longitudinal directions to find board edges
                        for (int direction = 0; direction < 8; direction += 2)
                        {
                            //Continue if theres room
                            if (Constants.EdgeDistanceArray[square, direction] >= 2)
                            {
                                //Calculate intermediate square location
                                int intermedSquare = square + (2 * Constants.DirectionToOffset(direction));

                                //Now check both perpendicular directions
                                for (int j = 0; j < 2; j++)
                                {
                                    int i_perp = j == 0 ? direction + 2 : direction - 2;
                                    if (i_perp > 6) i_perp = 0;
                                    if (i_perp < 0) i_perp = 6;

                                    if (Constants.EdgeDistanceArray[intermedSquare, i_perp] >= 1)
                                    {
                                        destSquare = intermedSquare + Constants.DirectionToOffset(i_perp);
                                        if (board.Squares[destSquare] == Piece.None ||
                                            (board.Squares[destSquare] != Piece.None &&
                                             board.GetPieceColor(destSquare) != myColor))
                                        {
                                            legalMoves.Add(new Move(square, destSquare));
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Piece.Pawn:
                        //Pawn moving
                        destSquare = board.GetPieceColor(square) ? square - 8 : square + 8;
                        //Ensure still in bounds of board
                        if (destSquare < 63 && destSquare > 0)
                        {
                            if (board.Squares[destSquare] == Piece.None)
                            {
                                legalMoves.Add(new Move(square, destSquare));
                                //Check if pawn on original square
                                if ((square / 8 == 1 && !myColor) || square / 8 == 6 && myColor)
                                {
                                    destSquare = myColor ? square - 16 : square + 16;
                                    if (board.Squares[destSquare] == Piece.None)
                                        legalMoves.Add(new Move(square, destSquare));
                                }
                            }
                        }

                        int passentSquare = -1;
                        //En passant, checks if last move was a pawn push
                        if (board.moves.Count > 0)
                        {
                            Move previousMove = board.moves[board.moves.Count - 1].move;
                            //Check if last move was a pawn move
                            if (Piece.IsType(board.Squares[previousMove.DestinationSquare], Piece.Pawn))
                            {
                                //Next see if that pawn moved more than 1 square
                                if (Mathf.Abs(previousMove.StartSquare / 8 - previousMove.DestinationSquare / 8) > 1)
                                {
                                    //If so, this is a legal en passent move.
                                    int offset = board.GetPieceColor(previousMove.DestinationSquare) ? -8 : 8;
                                    passentSquare = previousMove.StartSquare + offset;
                                }
                            }
                        }

                        //Pawn Capturing, checks both diagonals
                        for (int pd = 0; pd < 2; pd++)
                        {
                            int direction1 = board.turn ? 2 : 7;
                            int direction2 = board.turn ? 7 : 2;
                            if (pd == 0 && Constants.EdgeDistanceArray[square, direction1] == 0) continue;
                            if (pd == 1 && Constants.EdgeDistanceArray[square, direction2] == 0) continue;
                            int offset = pd == 0 ? 7 : 9;
                            destSquare = myColor ? square - offset : square + offset;
                            if (destSquare < 63 && destSquare > 0)
                            {
                                if (passentSquare == destSquare)
                                {
                                    legalMoves.Add(new Move(square, destSquare));
                                }
                                else if (((board.Squares[destSquare] != Piece.None) &&
                                          board.GetPieceColor(destSquare) != myColor))
                                {
                                    legalMoves.Add(new Move(square, destSquare));
                                }
                            }
                        }
                        break;
                    case Piece.Rook:
                    case Piece.Queen:
                    case Piece.Bishop:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            bool lat = direction % 2 == 0;
                            if (Piece.IsType(piece, Piece.Queen) ||
                                (lat && Piece.IsType(piece, Piece.Rook)) || 
                                (!lat && Piece.IsType(piece, Piece.Bishop)))
                            {
                                //Iterate along a direction here
                                for (int offset = 1; offset < Constants.EdgeDistanceArray[square, direction] + 1; offset++)
                                {
                                    int boardPos = square + (Constants.DirectionToOffset(direction) * offset);
                                    if (boardPos < 0 || boardPos > 63) continue;
                                    int destpiece = board.Squares[boardPos];

                                    //First, check if a piece occupies this square
                                    if (destpiece != Piece.None)
                                    {
                                        if (board.GetPieceColor(boardPos) == board.turn)
                                        {
                                            //Our own piece, cant get past no matter what.
                                            break;
                                        }

                                        //Enemy piece, so capture
                                        legalMoves.Add(new Move(square, boardPos));
                                        //We break here, as no legal moves past a capture
                                        break;
                                    }

                                    legalMoves.Add(new Move(square, boardPos));
                                }
                            }
                        }
                        break;
                    case Piece.King:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            int boardPos = square + Constants.DirectionToOffset(direction);
                            if (attackedSquares.Contains(boardPos)) continue;
                            if (boardPos < 0 || boardPos > 63) continue;
                            int destpiece = board.Squares[boardPos];

                            //First, check if a piece occupies this square
                            if (destpiece != Piece.None)
                            {
                                if (board.GetPieceColor(boardPos) == board.turn)
                                {
                                    //Our own piece, cant get past no matter what.
                                    continue;
                                }

                                //Enemy piece, so capture
                                legalMoves.Add(new Move(square, boardPos));
                                //We break here, as no legal moves past a capture
                                continue;
                            }
                            legalMoves.Add(new Move(square, boardPos));
                        }
                        //Black castling
                        if (myColor && (board.castling_bk || board.castling_bq))
                        {
                            if (board.castling_bk)
                            {
                                if (board.Squares[square + 1] == Piece.None && board.Squares[square + 2] == Piece.None)
                                {
                                    legalMoves.Add(new Move(square, square + 2));
                                }
                            }

                            if (board.castling_bq)
                            {
                                if (board.Squares[square - 1] == Piece.None && board.Squares[square - 2] == Piece.None && board.Squares[square - 3] == Piece.None)
                                {
                                    legalMoves.Add(new Move(square, square - 2));
                                }
                            }
                        }
                        //White Castling
                        else if (!myColor && (board.castling_wk || board.castling_wq))
                        {
                            if (board.castling_wk)
                            {
                                if (board.Squares[square + 1] == Piece.None && board.Squares[square + 2] == Piece.None)
                                {
                                    legalMoves.Add(new Move(square, square + 2));
                                }
                            }

                            if (board.castling_wq)
                            {
                                if (board.Squares[square - 1] == Piece.None && board.Squares[square - 2] == Piece.None && board.Squares[square - 3] == Piece.None)
                                {
                                    legalMoves.Add(new Move(square, square - 2));
                                }
                            }
                        }
                        break;
                    }
            }
            
            List<Move> filteredMoves = new List<Move>();
            //Pins
            if (pinnedSquares.Count > 0)
            {
                foreach (Move move in legalMoves)
                {
                    //King can always move behind a pin
                    if (Piece.IsType(board.Squares[move.StartSquare], Piece.King) || (!pinnedSquares.Contains(move.StartSquare) || (pinnedSquares.Contains(move.StartSquare) && pinnedSquares.Contains(move.DestinationSquare))))
                    {
                        filteredMoves.Add(move);
                    }
                }
                legalMoves = filteredMoves.ToList();
            }

            //Checks
            if (checkedSquares.Count > 0)
            {
                foreach (Move move in legalMoves)
                {
                    if (!Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                         checkedSquares.Contains(move.DestinationSquare) ||
                        Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                         !checkedSquares.Contains(move.DestinationSquare))
                    {
                        filteredMoves.Add(move);
                    }
                }

                legalMoves = filteredMoves.ToList();
            }

        }
        return legalMoves;
    }
}