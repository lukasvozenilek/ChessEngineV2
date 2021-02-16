using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceGO : MonoBehaviour
{
    public SpriteRenderer pieceImage;
    public ChessBoard chessBoardComponent;
    public int startSquare;
    public int pieceID;
    
    public void OnMouseDrag()
    {
        Vector3 newPos = GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = -1;
        transform.position = newPos;
    }
    public void OnMouseDown()
    {
        if (chessBoardComponent.board.GetPieceColor(startSquare) == chessBoardComponent.board.turn)
        {
            List<Move> legalMoves = MoveGenerator.GetAllLegalMoves(chessBoardComponent.board).Where(move =>
            {
                return move.StartSquare == startSquare; 
            }).ToList();
            chessBoardComponent.CreateOverlayFromMoves(legalMoves);
        }
    }

    public void OnMouseUp()
    {
        Vector3 destinationPos = chessBoardComponent.grid.WorldToCell(GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition));
        int destinationSquare = (int) (destinationPos.x) + ((int) destinationPos.y * 8);
        
        //Check bounds of chess board
        if (destinationPos.x >= 0 && destinationPos.x <= 7 && destinationPos.y >= 0 && destinationPos.y <= 7 && destinationSquare != startSquare)
        {
            //Promotion
            if (Piece.IsType(pieceID, Piece.Pawn))
            {
                if ((chessBoardComponent.board.turn && destinationPos.y == 0) || (!chessBoardComponent.board.turn && destinationPos.y == 7) )
                {
                    Debug.Log("Promotion!");
                }
            }
            
            MoveResult moveResult = chessBoardComponent.board.RequestMove(new Move(startSquare, destinationSquare));
            if (moveResult.legal)
            {
                //Kinda nasty way to find if a check took place but meh
                MoveGenerator.CalculateAttacks(chessBoardComponent.board,!chessBoardComponent.board.turn);
                if (MoveGenerator.checkedSquares.Count > 0)
                {
                    chessBoardComponent.audioSource.PlayOneShot(chessBoardComponent.boardConfig.checkSound);
                } else if (moveResult.castle)
                {
                    chessBoardComponent.audioSource.PlayOneShot(chessBoardComponent.boardConfig.castleSound);
                }
                else if (moveResult.capture)
                {
                    chessBoardComponent.audioSource.PlayOneShot(chessBoardComponent.boardConfig.captureSound);
                }
                else
                {
                    chessBoardComponent.audioSource.PlayOneShot(chessBoardComponent.boardConfig.moveSound);
                }
            }
            chessBoardComponent.UpdateBoard();
            
        }
        else
        {
            //If target square is not valid, skip board back.
            chessBoardComponent.UpdateBoard();
        }
        
    }
}
