
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MoveGenerator
{
    static List<Move> legalMoves = new List<Move>();
    static List<int> attackedLine = new List<int>();
    
    public static List<Move> GetAllLegalMoves(Board board)
    {
        legalMoves.Clear();
        for (int square = 0; square < 64; square++)
        {
            if (!Piece.IsType(board.Squares[square], Piece.None) && board.GetPieceColor(square) == board.turn)
            {
                int piece = board.Squares[square];
                bool myColor = board.GetPieceColor(square);

                attackedLine.Clear();

                if (Piece.IsType(piece, Piece.Knight))
                {
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
                                    int destSquare = intermedSquare + Constants.DirectionToOffset(i_perp);
                                    if (board.Squares[destSquare] == Piece.None || (board.Squares[destSquare] != Piece.None && board.GetPieceColor(destSquare) != myColor))
                                    {
                                        legalMoves.Add(new Move(square, destSquare));
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
                                if (board.Squares[destSquare] == Piece.None) legalMoves.Add(new Move(square, destSquare));
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
                            else if (((board.Squares[destSquare] != Piece.None) && board.GetPieceColor(destSquare) != myColor))
                            {
                                legalMoves.Add(new Move(square, destSquare));
                            }
                        }
                    }
                }
                else
                {
                    //Sliding type move behavior
                    //Iterate through directions

                    for (int direction = 0; direction < 8; direction++)
                    {
                        bool lat = direction % 2 == 0;
                        if (Piece.IsType(piece, Piece.Queen) || Piece.IsType(piece, Piece.King) ||
                            (lat && Piece.IsType(piece, Piece.Rook)) || (!lat && Piece.IsType(piece, Piece.Bishop)))
                        {
                            int maxDist = Piece.IsType(piece, Piece.King) ? 1 : 8;
                            attackedLine.Clear();
                            //Iterate along a direction here
                            int hits = 0;
                            for (int offset = 1; offset < Mathf.Min(Constants.EdgeDistanceArray[square, direction], maxDist) + 1; offset++)
                            {
                                int boardPos = square + (Constants.DirectionToOffset(direction) * offset);
                                int destpiece = board.Squares[boardPos];

                                //First, check if a piece occupies this square
                                if (destpiece != Piece.None)
                                {
                                    if (board.GetPieceColor(boardPos) == board.turn)
                                    {
                                        //Our own piece, cant get past no matter what.
                                        if (hits > 1)
                                        {
                                            break;
                                        }
                                    }

                                    //If already added capture move, skip this and start looking for another piece behind this piece
                                    if (hits == 0 && board.GetPieceColor(boardPos) != board.turn)
                                    {
                                        //Add capture move
                                        legalMoves.Add(new Move(square, boardPos));

                                        //Look for Check
                                        if (Piece.IsType(destpiece, Piece.King))
                                        {
                                            if (board.turn)
                                            {
                                                board.blackChecks.AddRange(attackedLine);
                                                board.blackChecks.Add(square);
                                            }
                                            else
                                            {
                                                board.whiteChecks.AddRange(attackedLine);
                                                board.whiteChecks.Add(square);
                                            }
                                        }
                                    }
                                    else if (hits <= 2)
                                    {
                                        //If piece after last piece is a king, we found our pin.
                                        if (Piece.IsType(destpiece, Piece.King) && (board.GetPieceColor(boardPos) != board.turn))
                                        {
                                            if (board.turn)
                                            {
                                                switch (hits)
                                                {
                                                    case 1:
                                                        board.blackPins.AddRange(attackedLine);
                                                        board.blackPins.Add(square);
                                                        break;
                                                    case 2:
                                                        board.potentialBlackPins.Add(square);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                switch (hits)
                                                {
                                                    case 1:
                                                        board.whitePins.AddRange(attackedLine);
                                                        board.whitePins.Add(square);
                                                        break;
                                                    case 2:
                                                        board.potentialWhitePins.Add(square);
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
                                    if (hits == 0) legalMoves.Add(new Move(square, boardPos));
                                }

                                attackedLine.Add(boardPos);
                            }
                        }
                    }

                    //Evaluate castling
                    if (Piece.IsType(piece, Piece.King))
                    {
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
                    }
                }

                List<Move> filteredMoves = new List<Move>();
                //Finally check for pins
                if (board.turn && board.whitePins.Count > 0)
                {
                    foreach (Move move in legalMoves)
                    {
                        if (!board.whitePins.Contains(move.StartSquare) || (board.whitePins.Contains(move.StartSquare) && board.whitePins.Contains(move.DestinationSquare)))
                        {
                            filteredMoves.Add(move);
                        }
                    }

                    legalMoves = filteredMoves.ToList();
                }

                if (!board.turn && board.blackPins.Count > 0)
                {
                    foreach (Move move in legalMoves)
                    {
                        if (!board.blackPins.Contains(move.StartSquare) || 
                            board.blackPins.Contains(move.StartSquare) && 
                             board.blackPins.Contains(move.DestinationSquare))
                        {
                            filteredMoves.Add(move);
                        }
                    }

                    legalMoves = filteredMoves.ToList();
                }

                //Checks
                if (board.turn && board.whiteChecks.Count > 0)
                {
                    foreach (Move move in legalMoves)
                    {
                        if (!Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                             board.whiteChecks.Contains(move.DestinationSquare) ||
                            Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                             !board.whiteChecks.Contains(move.DestinationSquare))
                        {
                            filteredMoves.Add(move);
                        }
                    }

                    legalMoves = filteredMoves.ToList();
                }

                if (!board.turn && board.blackChecks.Count > 0)
                {
                    foreach (Move move in legalMoves)
                    {
                        if (!Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                             board.blackChecks.Contains(move.DestinationSquare) ||
                            Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                             !board.blackChecks.Contains(move.DestinationSquare))
                        {
                            filteredMoves.Add(move);
                        }
                    }

                    legalMoves = filteredMoves.ToList();
                }
            }
        }
        return legalMoves;
    }
}