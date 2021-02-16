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

    public List<int> whitePieces = new List<int>();
    public List<int> blackPieces = new List<int>();
    
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
                    castling_bq = false;
                    castling_wk = false;
                    castling_wq = false;
                    break;
                case 'k':
                    castling_bk = true;
                    break;
                case 'K':
                    castling_wk = true;
                    break;
                case 'q':
                    castling_bq = true;
                    break;
                case 'Q':
                    castling_wq = true;
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

        //Finally, add move to records and update turn.
        moves.Add(result);
        turn = !turn;
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

        moves.RemoveAt(moves.Count-1);
        turn = !turn;
    }

    public MoveResult RequestMove(Move move)
    {
        MoveResult result = new MoveResult();
        result.move = move;
        if (GetPieceColor(move.StartSquare) == turn && MoveGenerator.GetAllLegalMoves(this).Contains(move))
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