using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MoveGenerator
{
    public List<Move> legalMoves = new List<Move>();
    
    public ulong attackSquaresBB = 0;
    public ulong checkSquaresBB = 0;
    public ulong pinnedSquaresBB = 0;
    public ulong attackedLineBB = 0;
    public int pinnedKingSquare = 0;

    public void CalculateAttacks(Board board, bool player)
    {
        attackSquaresBB = 0;
        checkSquaresBB = 0;
        pinnedSquaresBB = 0;
        attackedLineBB = 0;
        pinnedKingSquare = -1;
        

        //TODO: Cache piece locations so we don't have to iterate 64 times.
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

                                        attackSquaresBB |= Constants.posToBBArray[destSquare];
                                        if (Piece.IsType(attackedPiece, Piece.King) && (board.GetPieceColor(destSquare) != player))
                                        {
                                            checkSquaresBB |= Constants.posToBBArray[destSquare];
                                            checkSquaresBB |= Constants.posToBBArray[square];
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
                                checkSquaresBB |= Constants.posToBBArray[destSquare];
                                checkSquaresBB |= Constants.posToBBArray[square];
                            }
                            attackSquaresBB |= Constants.posToBBArray[destSquare];
                        }
                        break;
                    case Piece.Rook:
                    case Piece.Queen:
                    case Piece.Bishop:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            bool lat = direction % 2 == 0;
                            attackedLineBB = 0;
                            bool hit = false;
                            bool kinghit = false;
                            
                            if (Piece.IsType(piece, Piece.Queen) ||
                                (lat && Piece.IsType(piece, Piece.Rook)) || 
                                (!lat && Piece.IsType(piece, Piece.Bishop)))
                            {
                                //Iterate along a direction here
                                for (int offset = 1; offset < (Constants.EdgeDistanceArray[square, direction] + 1); offset++)
                                {
                                    int boardPos = square + (Constants.DirectionToOffset(direction) * offset);
                                    if (boardPos < 0 || boardPos > 63) continue;
                                    int destpiece = board.Squares[boardPos];
                                    
                                    //First, check if a piece occupies this square
                                    if (destpiece != Piece.None)
                                    {
                                        if (!hit) attackSquaresBB |= Constants.posToBBArray[boardPos];
                                        //If our piece, stop here as no checks or pins can occur.
                                        if (board.GetPieceColor(boardPos) == player) break;
                                        
                                        if (Piece.IsType(destpiece, Piece.King))
                                        {
                                            if (!hit)
                                            {
                                                checkSquaresBB |= attackedLineBB;
                                                checkSquaresBB |= Constants.posToBBArray[square];
                                                if (kinghit)
                                                {
                                                    attackSquaresBB |= Constants.posToBBArray[boardPos];
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                pinnedSquaresBB |= attackedLineBB;
                                                pinnedSquaresBB |= Constants.posToBBArray[square];
                                                pinnedKingSquare = boardPos;
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
                                        if (!hit) attackSquaresBB |= Constants.posToBBArray[boardPos];
                                    }
                                    attackedLineBB |= Constants.posToBBArray[boardPos];
                                }
                            }
                        }
                        break;
                    case Piece.King:
                        for (int direction = 0; direction < 8; direction++)
                        {
                            if (Constants.EdgeDistanceArray[square, direction] >= 1)
                            {
                                int boardPos = square + Constants.DirectionToOffset(direction);
                                //if (attackedSquares.Contains(boardPos)) continue;
                                if (boardPos < 0 || boardPos > 63) continue;
                                int destpiece = board.Squares[boardPos];
                                attackSquaresBB |= Constants.posToBBArray[boardPos];
                            }
                        }
                        break;
                }
            }
        }
    }
    
    public List<Move> GetAllLegalMoves(Board board)
    {
        legalMoves.Clear();
        CalculateAttacks(board, !board.turn);

        //TODO: Cache piece locations so we don't have to iterate 64 times.
        for (int square = 0; square < 64; square++)
        {
            //Only iterate through squares that contain my own piece
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
                        if (destSquare <= 63 && destSquare >= 0)
                        {
                            if (board.Squares[destSquare] == Piece.None)
                            {
                                AddPawnPromotion(board, new Move(square, destSquare));
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
                            if (destSquare <= 63 && destSquare >= 0)
                            {
                                if (passentSquare == destSquare)
                                {
                                    legalMoves.Add(new Move(square, destSquare));
                                }
                                else if (((board.Squares[destSquare] != Piece.None) &&
                                          board.GetPieceColor(destSquare) != myColor))
                                {
                                    AddPawnPromotion(board, new Move(square, destSquare));
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
                            if (Constants.EdgeDistanceArray[square, direction] >= 1)
                            {
                                int boardPos = square + Constants.DirectionToOffset(direction);
                                if ((Constants.posToBBArray[boardPos] & attackSquaresBB) > 0) continue;
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
                        }

                        //Only check castling if no checks are present
                        if (checkSquaresBB == 0)
                        {
                            //Black castling
                            if (myColor && (board.castling_bk || board.castling_bq))
                            {
                                if (board.castling_bk)
                                {
                                    if (((Constants.posToBBArray[square + 1] & attackSquaresBB) == 0) &&
                                    ((Constants.posToBBArray[square + 2] & attackSquaresBB) == 0))
                                    {
                                        if (board.Squares[square + 1] == Piece.None &&
                                            board.Squares[square + 2] == Piece.None &&
                                            Piece.IsType(board.Squares[square + 3], Piece.Rook))
                                        {
                                            legalMoves.Add(new Move(square, square + 2));
                                        }
                                    }
                                }

                                if (board.castling_bq)
                                {
                                    if (((Constants.posToBBArray[square - 1] & attackSquaresBB) == 0) &&
                                        ((Constants.posToBBArray[square - 2] & attackSquaresBB) == 0))
                                    {
                                        if (board.Squares[square - 1] == Piece.None &&
                                            board.Squares[square - 2] == Piece.None &&
                                            board.Squares[square - 3] == Piece.None &&
                                            Piece.IsType(board.Squares[square - 4], Piece.Rook))
                                        {
                                            legalMoves.Add(new Move(square, square - 2));
                                        }
                                    }
                                }
                            }
                            
                            //White Castling
                            else if (!myColor && (board.castling_wk || board.castling_wq))
                            {
                                if (board.castling_wk)
                                {
                                    if (((Constants.posToBBArray[square + 1] & attackSquaresBB) == 0) &&
                                        ((Constants.posToBBArray[square + 2] & attackSquaresBB) == 0))
                                    {
                                        if (board.Squares[square + 1] == Piece.None &&
                                            board.Squares[square + 2] == Piece.None &&
                                            Piece.IsType(board.Squares[square + 3], Piece.Rook))
                                        {
                                            legalMoves.Add(new Move(square, square + 2));
                                        }
                                    }
                                }

                                if (board.castling_wq)
                                {
                                    if (((Constants.posToBBArray[square - 1] & attackSquaresBB) == 0) &&
                                        ((Constants.posToBBArray[square - 2] & attackSquaresBB) == 0))
                                    {
                                        if (board.Squares[square - 1] == Piece.None &&
                                            board.Squares[square - 2] == Piece.None &&
                                            board.Squares[square - 3] == Piece.None &&
                                            Piece.IsType(board.Squares[square - 4], Piece.Rook))
                                        {
                                            legalMoves.Add(new Move(square, square - 2));
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }
        
        List<Move> filteredMoves = new List<Move>();
        //Pins
        if (pinnedSquaresBB > 0)
        {
            filteredMoves.Clear();
            foreach (Move move in legalMoves)
            {
                if (Piece.IsType(board.Squares[move.StartSquare], Piece.King))
                {
                    filteredMoves.Add(move);
                }
                else 
                {
                    bool startSquareInPin = (Constants.posToBBArray[move.StartSquare] & pinnedSquaresBB) > 0;
                    bool endSquareInPin = (Constants.posToBBArray[move.DestinationSquare] & pinnedSquaresBB) > 0;
                    int moveDirection = Mathf.Abs(move.DestinationSquare - move.StartSquare);
                    int pinDirection = Mathf.Abs(move.StartSquare - pinnedKingSquare);
                    bool moveWithinPin = ((moveDirection==pinDirection) || moveDirection%pinDirection==0 || pinDirection%moveDirection==0) && ((startSquareInPin) && endSquareInPin);
                    if (((!startSquareInPin) || moveWithinPin))
                    {
                        filteredMoves.Add(move); 
                    }
                }
            }
            legalMoves = filteredMoves.ToList();
        }

        //Checks
        if (checkSquaresBB > 0)
        {
            filteredMoves.Clear();
            foreach (Move move in legalMoves)
            {
                bool destinationUnderCheck = (Constants.posToBBArray[move.DestinationSquare] & checkSquaresBB) > 0;
                if ((!Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                     destinationUnderCheck) ||
                    (Piece.IsType(board.Squares[move.StartSquare], Piece.King) &&
                     (!destinationUnderCheck || board.Squares[move.DestinationSquare] != Piece.None)))
                {
                    filteredMoves.Add(move);
                }
            }
            legalMoves = filteredMoves.ToList();
        }

        return legalMoves;
    }

    public void AddPawnPromotion(Board board, Move move)
    {
        if ((board.turn && (move.DestinationSquare / 8 == 0)) || !board.turn && (move.DestinationSquare / 8 == 7))
        {
            legalMoves.Add(new Move(move.StartSquare, move.DestinationSquare, Piece.Knight));
            legalMoves.Add(new Move(move.StartSquare, move.DestinationSquare, Piece.Bishop));
            legalMoves.Add(new Move(move.StartSquare, move.DestinationSquare, Piece.Queen));
            legalMoves.Add(new Move(move.StartSquare, move.DestinationSquare, Piece.Rook));
        }
        else
        {
            legalMoves.Add(move);
        }
    }
}