using System;
using System.Collections.Generic;

/// <summary>
/// Script originally created by Sebastian Lague and modified for my engine.
/// https://github.com/SebLague/Chess-AI/blob/main/Assets/Scripts/Other/PGNCreator.cs
/// </summary>

public static class PGNCreator {

	public static string CreatePGN (List<MoveResult> moves) {
			string pgn = "";
			Board board = new Board ();

			for (int plyCount = 0; plyCount < moves.Count; plyCount++) {
				string moveString = NotationFromMove (board, moves[plyCount]);
				board.MakeMove (moves[plyCount].move);
				if (plyCount % 2 == 0) {
					pgn += ((plyCount / 2) + 1) + ". ";
				}
				pgn += moveString + " ";
			}
			return pgn;
		}

		public static string NotationFromMove (string currentFen, MoveResult move) {
			Board board = new Board ();
			board.LoadPositionFromFEN(currentFen);
			return NotationFromMove (board, move);
		}

		static string NotationFromMove (Board board, MoveResult move) {

			MoveGenerator moveGen = new MoveGenerator ();

			int movePieceType = Piece.GetType(board.Squares[move.move.StartSquare]);
			int capturedPieceType = Piece.GetType(board.Squares[move.move.DestinationSquare]);

			if (move.castle) {
				int delta = move.move.DestinationSquare - move.move.StartSquare;
				if (delta == 2) {
					return "O-O";
				} else if (delta == -2) {
					return "O-O-O";
				}
			}

			string moveNotation = GetSymbolFromPieceType (movePieceType);

			// check if any ambiguity exists in notation (e.g if e2 can be reached via Nfe2 and Nbe2)
			if (movePieceType != Piece.Pawn && movePieceType != Piece.King) {
				var allMoves = moveGen.GetAllLegalMoves(board);

				foreach (Move altMove in allMoves) {

					if (altMove.StartSquare != move.move.StartSquare && altMove.DestinationSquare == move.move.DestinationSquare) { // if moving to same square from different square
						if (Piece.GetType(board.Squares[altMove.StartSquare]) == movePieceType) { // same piece type

							int fromFileIndex = move.move.StartSquare % 8;
							int alternateFromFileIndex = altMove.StartSquare % 8;
							int fromRankIndex = move.move.StartSquare / 8;
							int alternateFromRankIndex = altMove.StartSquare / 8;

							if (fromFileIndex != alternateFromFileIndex) { // pieces on different files, thus ambiguity can be resolved by specifying file
								moveNotation += Char.ToLower(Constants.boardCoordinates[fromFileIndex]);
								break; // ambiguity resolved
							} else if (fromRankIndex != alternateFromRankIndex)
							{
								moveNotation += (1 + fromRankIndex);
								break; // ambiguity resolved
							}
						}
					}

				}
			}
			
			if (capturedPieceType != Piece.None) { // add 'x' to indicate capture
				if (movePieceType == Piece.Pawn)
				{
					moveNotation += Char.ToLower(Constants.boardCoordinates[move.move.StartSquare % 8]);
				}
				moveNotation += "x";
			} else { // check if capturing ep
				if (move.enpassant) {
					moveNotation += Char.ToLower(Constants.boardCoordinates[move.move.StartSquare % 8]) + "x";
				}
			}
			
			//Add file name
			moveNotation += Char.ToLower(Constants.boardCoordinates[move.move.DestinationSquare % 8]);
			//Add rank name
			moveNotation += (1 + (move.move.DestinationSquare / 8));

			// add promotion piece
			if (move.move.promotionID != Piece.None) {
				int promotionPieceType = move.move.promotionID;
				moveNotation += "=" + GetSymbolFromPieceType (promotionPieceType);
			}

			board.MakeMove (move.move);
			var legalResponses = moveGen.GetAllLegalMoves(board);
			// add check/mate symbol if applicable
			if (moveGen.checkSquaresBB > 0) {
				if (legalResponses.Count == 0) {
					moveNotation += "#";
				} else {
					moveNotation += "+";
				}
			}
			board.UnmakeMove();
			return moveNotation;
		}

		static string GetSymbolFromPieceType (int pieceType) {
			switch (pieceType) {
				case Piece.Rook:
					return "R";
				case Piece.Knight:
					return "N";
				case Piece.Bishop:
					return "B";
				case Piece.Queen:
					return "Q";
				case Piece.King:
					return "K";
				default:
					return "";
			}
		}

	}