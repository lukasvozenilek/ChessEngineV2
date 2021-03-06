﻿using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class Board
{
    //Board variablesm
    public int[] Squares;
    public bool turn;
    public List<MoveResult> moves = new List<MoveResult>();
    
    public int halfMoveClock;
    
    //Castling rights
    public bool castling_wk;
    public bool castling_wq;
    public bool castling_bk;
    public bool castling_bq;
    
    public bool starting_castling_wk;
    public bool starting_castling_wq;
    public bool starting_castling_bk;
    public bool starting_castling_bq;

    public Dictionary<int, int> whitePieces = new Dictionary<int, int>();
    public int whiteMat;
    
    public Dictionary<int, int> blackPieces = new Dictionary<int, int>();
    public int blackMat;
    
    private MoveGenerator moveGenerator;

    public Zobrist zobrist;
    public ulong currentHash;
    
    public int BoardResult = BOARD_PROGR;
    
    public const int BOARD_PROGR = 0;
    public const int BOARD_WHITEWON = 1;
    public const int BOARD_BLACKWON = 2;
    public const int BOARD_DRAW = 3;
    
    
    //New board from FEN
    public Board(string FEN = Constants.startingFEN)
    {
        Squares = new int[64];
        LoadPositionFromFEN(FEN);
        zobrist = new Zobrist();
        currentHash = zobrist.HashPosition(this);
        InitBoard();
    }
    
    //New board from another board
    public Board(Board copyfrom)
    {
        Squares = new int[64];
        
        //Copy current position
        Array.Copy(copyfrom.Squares, 0, Squares, 0, 64);

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
        
        //Material
        whitePieces = new Dictionary<int, int>(copyfrom.whitePieces);
        whiteMat = copyfrom.whiteMat;
        blackPieces = new Dictionary<int, int>(copyfrom.blackPieces);
        blackMat = copyfrom.blackMat;
    
        //Copy turn
        turn = copyfrom.turn;
        
        //Copy zobrist information
        currentHash = copyfrom.currentHash;
        zobrist = copyfrom.zobrist;
        InitBoard();
    }

    private void InitBoard()
    {
        BoardResult = BOARD_PROGR;
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
        whitePieces.Clear();
        blackPieces.Clear();
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
                    bool color = char.IsUpper(item);
                    int pieceid = color ? Piece.White : Piece.Black;
                    pieceid += Constants.FENPieceNames[char.ToLower(item)];
                    Squares[boardpos] = pieceid;
                    if (color)
                    {
                        whitePieces.Add(boardpos, pieceid);
                    }
                    else
                    {
                        blackPieces.Add(boardpos, pieceid);
                    }
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
    
    public string GenerateFEN () {
			string fen = "";
			for (int rank = 7; rank >= 0; rank--) {
				int numEmptyFiles = 0;
				for (int file = 0; file < 8; file++) {
					int i = rank * 8 + file;
					int piece = Squares[i];
					if (piece != 0) {
						if (numEmptyFiles != 0) {
							fen += numEmptyFiles;
							numEmptyFiles = 0;
						}
						bool isBlack = GetPieceColor(i);
						int pieceType = Piece.GetType(piece);
						char pieceChar = ' ';
						switch (pieceType) {
							case Piece.Rook:
								pieceChar = 'R';
								break;
							case Piece.Knight:
								pieceChar = 'N';
								break;
							case Piece.Bishop:
								pieceChar = 'B';
								break;
							case Piece.Queen:
								pieceChar = 'Q';
								break;
							case Piece.King:
								pieceChar = 'K';
								break;
							case Piece.Pawn:
								pieceChar = 'P';
								break;
						}
						fen += (isBlack) ? pieceChar.ToString ().ToLower () : pieceChar.ToString ();
					} else {
						numEmptyFiles++;
					}

				}
				if (numEmptyFiles != 0) {
					fen += numEmptyFiles;
				}
				if (rank != 0) {
					fen += '/';
				}
			}

			// Side to move
			fen += ' ';
			fen += (!turn) ? 'w' : 'b';

			// Castling
            bool whiteKingside = castling_wk;
            bool whiteQueenside = castling_wq;
            bool blackKingside = castling_bk;
            bool blackQueenside = castling_bq;
			fen += ' ';
			fen += (whiteKingside) ? "K" : "";
			fen += (whiteQueenside) ? "Q" : "";
			fen += (blackKingside) ? "k" : "";
			fen += (blackQueenside) ? "q" : "";

            fen += "-";
			//fen += ((Board.currentGameState & 15) == 0) ? "-" : "";
            fen += "-";
            fen += ' ';
			// En-passant
            fen += '-';
            /*
			fen += ' ';
			int epFile = (int) (board.currentGameState >> 4) & 15;
			if (epFile == 0) {
				fen += '-';
			} else {
				string fileName = BoardRepresentation.fileNames[epFile - 1].ToString ();
				int epRank = (board.WhiteToMove) ? 6 : 3;
				fen += fileName + epRank;
			}
			*/

			// 50 move counter
			fen += ' ';
			fen += halfMoveClock;

			// Full-move count (should be one at start, and increase after each move by black)
			fen += ' ';
			fen += (moves.Count / 2) + 1;

			return fen;
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

        int movedPiece = Squares[move.StartSquare];
        result.movedPiece = movedPiece;
        
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
        if (Piece.IsType(movedPiece, Piece.Pawn) &&  (move.StartSquare%8 != move.DestinationSquare%8 ) && Squares[move.DestinationSquare] == Piece.None)
        {
            int offset = GetPieceColor(move.StartSquare) ? 8 : -8;
            result.capturedPiece = Squares[move.DestinationSquare + offset];
            Squares[move.DestinationSquare + offset] = Piece.None;
            currentHash ^= zobrist.HashPiece(move.DestinationSquare + offset, result.capturedPiece);

            if (turn)
            {
                whitePieces.Remove(move.DestinationSquare + offset);
            }
            else
            {
                blackPieces.Remove(move.DestinationSquare + offset);
            }
            
            result.enpassant = true;
            result.capture = true;
        }
        
        //Detect 2 move
        if (Piece.IsType(Squares[move.DestinationSquare], Piece.Pawn) &&
            (Mathf.Abs(move.StartSquare / 8 - move.DestinationSquare / 8) > 1))
        {
            result.pawn2squares = true;
        }
        
        
        //ACTUAL MOVING LOGIC STARTS HERE
        
        //Set destination square to piece on start square
        Squares[move.DestinationSquare] = movedPiece;
        //Only hash out original piece of not enpassant, as we did this earlier
        if (!result.enpassant) currentHash ^= zobrist.HashPiece(move.DestinationSquare,result.capturedPiece);
        currentHash ^= zobrist.HashPiece(move.DestinationSquare,movedPiece);

        //Remove piece at start square
        Squares[move.StartSquare] = Piece.None;
        currentHash ^= zobrist.HashPiece(move.StartSquare, movedPiece);
        
        
        //Execute castling
        if (Piece.IsType(Squares[move.DestinationSquare], Piece.King) && (Mathf.Abs(move.StartSquare%8 - move.DestinationSquare%8) > 1) )
        {
            
            if (GetPieceColor(move.DestinationSquare))
            {
                //Black
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    //Kingside
                    //Move the rook
                    Squares[61] = Squares[63];
                    blackPieces.Add(61,Squares[63]);
                    currentHash ^= zobrist.HashPiece(61, Squares[63]);
                    
                    //Remove rook
                    currentHash ^= zobrist.HashPiece(63, Squares[63]);
                    Squares[63] = Piece.None;
                    blackPieces.Remove(63);
                    result.castle = true;
                }
                else
                {
                    //Queenside
                    //Move rook
                    Squares[59] = Squares[56];
                    blackPieces.Add(59,Squares[56]);
                    currentHash ^= zobrist.HashPiece(59, Squares[56]);
                    
                    //Remove rook
                    currentHash ^= zobrist.HashPiece(56, Squares[56]);
                    Squares[56] = Piece.None;
                    blackPieces.Remove(56);
                    result.castle = true;
                }
            }
            else
            {
                //White
                if (Squares[move.DestinationSquare + 1] != Piece.None)
                {
                    Squares[5] = Squares[7];
                    whitePieces.Add(5,Squares[7]);
                    currentHash ^= zobrist.HashPiece(5, Squares[7]);
                    
                    currentHash ^= zobrist.HashPiece(7, Squares[7]);
                    Squares[7] = Piece.None;
                    whitePieces.Remove(7);
                    result.castle = true;
                }
                else
                {
                    Squares[3] = Squares[0];
                    whitePieces.Add(3, Squares[0]);
                    currentHash ^= zobrist.HashPiece(3, Squares[0]);
                    
                    currentHash ^= zobrist.HashPiece(0, Squares[0]);
                    Squares[0] = Piece.None;
                    whitePieces.Remove(0);
                    result.castle = true;
                }
            }
        }
        
        //Update piece dictionary
        if (turn)
        {
            blackPieces.Remove(move.StartSquare);
            blackPieces.Add(move.DestinationSquare,movedPiece);
            if (result.capture)
            {
                whitePieces.Remove(move.DestinationSquare);
            }
        }
        else
        {
            whitePieces.Remove(move.StartSquare);
            whitePieces.Add(move.DestinationSquare, movedPiece);
            if (result.capture)
            {
                blackPieces.Remove(move.DestinationSquare);
            }
        }

        //Promotion
        if (move.promotionID != Piece.None)
        {
            int promotedPiece = move.promotionID + (turn ? Piece.Black : Piece.White);

            currentHash ^= zobrist.HashPiece(move.DestinationSquare, Squares[move.DestinationSquare]);
            currentHash ^= zobrist.HashPiece(move.DestinationSquare, promotedPiece);
            
            Squares[move.DestinationSquare] = promotedPiece;
            
            if (turn)
            {
                blackPieces[move.DestinationSquare] = move.promotionID + Piece.Black;
            }
            else
            {
                whitePieces[move.DestinationSquare] = move.promotionID + Piece.White;
            }
        }
        
        result.resultingHash = currentHash;
        
        //Check for draw
        if (moves.Count > 5)
        {
            if (moves[moves.Count - 4].resultingHash == currentHash && moves[moves.Count - 6].resultingHash == moves[moves.Count - 2].resultingHash)
            {
                BoardResult = BOARD_DRAW;
            }
        }
        
        //Finally, add move to records and update turn.
        moves.Add(result);
        turn = !turn;
        result.legal = true;
        return result;
    }

    public void UnmakeMove(bool sendEvent=true)
    {
        if (moves.Count == 0) return;
        MoveResult lastMove = moves[moves.Count-1];

        int movedPiece = lastMove.movedPiece;
        
        if (turn)
        {
            whitePieces.Remove(lastMove.move.DestinationSquare);
        }
        else
        {
            blackPieces.Remove(lastMove.move.DestinationSquare);
        }


        if (lastMove.move.promotionID != Piece.None)
        {
            if (turn)
            {
                whitePieces.Add(lastMove.move.StartSquare,movedPiece);
            }
            else
            {
                blackPieces.Add(lastMove.move.StartSquare,movedPiece);
            }

            int originalPawn = Piece.Pawn + (turn ? Piece.White : Piece.Black);
            Squares[lastMove.move.StartSquare] = originalPawn;
            currentHash ^= zobrist.HashPiece(lastMove.move.StartSquare, originalPawn);
            currentHash ^= zobrist.HashPiece(lastMove.move.DestinationSquare, lastMove.move.promotionID + (turn ? Piece.White : Piece.Black));
        }
        else
        {
            if (turn)
            {
                whitePieces.Add(lastMove.move.StartSquare,movedPiece);
            }
            else
            {
                blackPieces.Add(lastMove.move.StartSquare, movedPiece);
            }
            currentHash ^= zobrist.HashPiece(lastMove.move.DestinationSquare, movedPiece);
            currentHash ^= zobrist.HashPiece(lastMove.move.StartSquare, movedPiece);
            Squares[lastMove.move.StartSquare] = movedPiece;
        }
        
        if (lastMove.enpassant)
        {
            Squares[lastMove.move.DestinationSquare] = Piece.None;
            int offset = GetPieceColor(lastMove.move.StartSquare) ? 8 : -8;
            currentHash ^= zobrist.HashPiece(lastMove.move.DestinationSquare + offset, lastMove.capturedPiece);
            Squares[lastMove.move.DestinationSquare + offset] = lastMove.capturedPiece;
            if (turn)
            {
                blackPieces.Add(lastMove.move.DestinationSquare + offset, lastMove.capturedPiece);
            }
            else
            {
                whitePieces.Add(lastMove.move.DestinationSquare + offset, lastMove.capturedPiece);
            }
        }
        else if (lastMove.castle)
        {
            if (GetPieceColor(lastMove.move.DestinationSquare))
            {
                //Black
                if (Squares[lastMove.move.DestinationSquare + 1] == Piece.None)
                {
                    Squares[63] = Squares[61];
                    blackPieces.Add(63, Squares[61]);
                    currentHash ^= zobrist.HashPiece(63, Squares[61]);
                    
                    currentHash ^= zobrist.HashPiece(61, Squares[61]);
                    Squares[61] = Piece.None;
                    blackPieces.Remove(61);
                }
                else
                {
                    Squares[56] = Squares[59];
                    blackPieces.Add(56, Squares[59]);
                    currentHash ^= zobrist.HashPiece(56, Squares[59]);
                    
                    currentHash ^= zobrist.HashPiece(59, Squares[59]);
                    Squares[59] = Piece.None;
                    blackPieces.Remove(59);
                }

            }
            else
            {
                //White
                if (Squares[lastMove.move.DestinationSquare + 1] == Piece.None)
                {
                    Squares[7] = Squares[5];
                    whitePieces.Add(7, Squares[5]);
                    currentHash ^= zobrist.HashPiece(7, Squares[5]);
                    
                    currentHash ^= zobrist.HashPiece(5, Squares[5]);
                    Squares[5] = Piece.None;
                    whitePieces.Remove(5);
                }
                else
                {
                    Squares[0] = Squares[3];
                    whitePieces.Add(0, Squares[3]);
                    currentHash ^= zobrist.HashPiece(0, Squares[3]);
                    
                    currentHash ^= zobrist.HashPiece(3, Squares[3]);
                    Squares[3] = Piece.None;
                    whitePieces.Remove(3);
                }
            }
            Squares[lastMove.move.DestinationSquare] = Piece.None;
        }
        else //Capture or just simply a move
        {
            Squares[lastMove.move.DestinationSquare] = lastMove.capturedPiece;
            currentHash ^= zobrist.HashPiece(lastMove.move.DestinationSquare, lastMove.capturedPiece);

            if (lastMove.capture)
            {
                if (turn)
                {
                    blackPieces.Add(lastMove.move.DestinationSquare, lastMove.capturedPiece);
                }
                else
                {
                    whitePieces.Add(lastMove.move.DestinationSquare, lastMove.capturedPiece);
                }
            }
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

        BoardResult = BOARD_PROGR;
        
        moves.RemoveAt(moves.Count-1);
        turn = !turn;
    }

    public MoveResult RequestMove(Move move)
    {
        MoveResult result = new MoveResult();
        result.move = move;
        if (moveGenerator.GetAllLegalMoves(this).Contains(move))
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