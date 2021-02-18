using System;
using System.Collections.Generic;
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
    
    public bool starting_castling_wk;
    public bool starting_castling_wq;
    public bool starting_castling_bk;
    public bool starting_castling_bq;

    public event Action boardUpdatedEvent;
    public event Action gameOverEvent;

    public List<int> whitePieces = new List<int>();
    public List<int> blackPieces = new List<int>();

    private MoveGenerator moveGenerator;
    
    public Board(string FEN = Constants.startingFEN)
    {
        Squares = new int[64];
        LoadPositionFromFEN(FEN);
        moveGenerator = new MoveGenerator();
    }
    
    public Board(Board copyfrom)
    {
        //Copy current position
        Squares = copyfrom.Squares;
        
        //Copy current and starting castling rights
        castling_bk = copyfrom.castling_bk;
        castling_wq = copyfrom.castling_wq;
        castling_bq = copyfrom.castling_bq;
        castling_wk = copyfrom.castling_wk;
        starting_castling_wk = copyfrom.starting_castling_wk;
        starting_castling_wq = copyfrom.starting_castling_wq;
        starting_castling_bk = copyfrom.starting_castling_bk;
        starting_castling_bq = copyfrom.starting_castling_bq;
        
        //Add move history
        moves.AddRange(copyfrom.moves);
    
        //Copy turn
        turn = copyfrom.turn;
            
        //Create move generator
        moveGenerator = new MoveGenerator();   
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
        string[] data = fen.Split(' ');
        
        ClearBoard();
        turn = false;
        
        //First evaluate piece positions
        int boardpos = 56;
        foreach (char item in data[0])
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
        
        //Next, who's move?
        turn = data[1] != "w";
        
        //Next, castling rights
        string castleString = data[2];
        foreach (char item in castleString)
        {
            switch (item)
            {
                case '-':
                    castling_bk = false;
                    starting_castling_bk = false;
                    castling_bq = false;
                    starting_castling_bq = false;
                    castling_wk = false;
                    starting_castling_wk = false;
                    castling_wq = false;
                    starting_castling_wq = false;
                    break;
                case 'k':
                    castling_bk = true;
                    starting_castling_bk = true;
                    break;
                case 'K':
                    castling_wk = true;
                    starting_castling_wk = true;
                    break;
                case 'q':
                    castling_bq = true;
                    starting_castling_bq = true;
                    break;
                case 'Q':
                    castling_wq = true;
                    starting_castling_wq = true;
                    break;
            }
        }
        
        //Next, En passant target square
        //string passantString = data[3];
        
        //Next, Halfmove clock (for fifty move rule)
        //string halfmoveString = data[4];
        
        //Finally, fullmove number
        //string moveString = data[5];
        
        GameState.UpdateBoard();
    }
    

    public MoveResult MakeMove(Move move, bool sendEvent = true)
    {
        MoveResult result = new MoveResult();
        result.castlingRights = new CastlingRights();
        
        result.castlingRights.b_ks = castling_bk;
        result.castlingRights.b_qs = castling_bq;
        result.castlingRights.w_ks = castling_wk;
        result.castlingRights.w_qs = castling_wq; 
        
        
        result.move = move;
        
        //Losing castling rights due to king and rook moves.
        if (castling_wq && (move.StartSquare == 0 || move.StartSquare == 4))
        {
            castling_wq = false;
            result.castlingRights.w_qs = false;
        }

        if (castling_wk && (move.StartSquare == 7 || move.StartSquare == 4))
        {
            castling_wk = false;
            result.castlingRights.w_ks = false;
        }

        if (castling_bq && (move.StartSquare == 56 || move.StartSquare == 60))
        {
            castling_bq = false;
            result.castlingRights.b_qs = false;
        }

        if (castling_bk && (move.StartSquare == 63 || move.StartSquare == 60))
        {
            castling_bk = false;
            result.castlingRights.b_ks = false;
        }

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
        
        //Detect 2 move
        if (Piece.IsType(Squares[move.DestinationSquare], Piece.Pawn) &&
            (Mathf.Abs(move.StartSquare / 8 - move.DestinationSquare / 8) > 1))
        {
            result.pawn2squares = true;
        }


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

        if (move.promotionID != Piece.None)
        {
            Squares[move.DestinationSquare] = move.promotionID + (turn? Piece.Black : Piece.White);
        }
        
        //Finally, add move to records and update turn.
        moves.Add(result);
        turn = !turn;
        return result;
    }

    public void UnmakeMove(bool sendEvent=true)
    {
        if (moves.Count == 0) return;
        MoveResult lastMove = moves[moves.Count-1];

        if (lastMove.move.promotionID != Piece.None)
        {
            Squares[lastMove.move.StartSquare] = Piece.Pawn + (turn ? Piece.White : Piece.Black);
        }
        else
        {
            Squares[lastMove.move.StartSquare] = Squares[lastMove.move.DestinationSquare];
        }
        
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
            }
            Squares[lastMove.move.DestinationSquare] = Piece.None;
        }
        else
        {
            Squares[lastMove.move.DestinationSquare] = lastMove.capturedPiece;
        }
        
        //Restore previous castling rights
        if (moves.Count > 1)
        {
            MoveResult lastMoveResult = moves[moves.Count - 2];
            //Black
            castling_bk = lastMoveResult.castlingRights.b_ks;
            castling_bq = lastMoveResult.castlingRights.b_qs;
      
            //White
            castling_wk = lastMoveResult.castlingRights.w_ks;
            castling_wq = lastMoveResult.castlingRights.w_qs;
            
        } else if (moves.Count == 1)
        {
            castling_bk = starting_castling_bk;
            castling_wk = starting_castling_wk;
            castling_bq = starting_castling_bq;
            castling_wq = starting_castling_wq;
        }
        
        moves.RemoveAt(moves.Count-1);
        turn = !turn;
    }

    public MoveResult RequestMove(Move move)
    {
        MoveResult result = new MoveResult();
        result.move = move;
        if (GetPieceColor(move.StartSquare) == turn && moveGenerator.GetAllLegalMoves(this).Contains(move))
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
        return (piece & Piece.Black) > 0;
    }
}