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
    public bool myColor;
    
    private MoveGenerator moveGenerator;
    private void Start()
    {
        moveGenerator = new MoveGenerator();
    }

    private bool CheckIfCanMove()
    {
        return (chessBoardComponent.canMoveWhitePieces && !myColor) ||
               (chessBoardComponent.canMoveBlackPieces && myColor);
    }
    
    public void OnMouseDrag()
    {
        if (!CheckIfCanMove()) return;
        Vector3 newPos = GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = -1;
        transform.position = newPos;
    }
    public void OnMouseDown()
    {
        if (!CheckIfCanMove()) return;
        if (chessBoardComponent.board.GetPieceColor(startSquare) == chessBoardComponent.board.turn)
        {
            List<Move> legalMoves = moveGenerator.GetAllLegalMoves(chessBoardComponent.board).Where(move =>
            {
                return move.StartSquare == startSquare; 
            }).ToList();
            chessBoardComponent.CreateOverlayFromMoves(legalMoves);
        }
    }

    public void OnMouseUp()
    {
        if (!CheckIfCanMove()) return;
        Vector3 destinationPos = chessBoardComponent.grid.WorldToCell(GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition));
        int destinationSquare = (int) (destinationPos.x) + ((int) destinationPos.y * 8);
        
        //Check bounds of chess board
        if (destinationPos.x >= 0 && destinationPos.x <= 7 && destinationPos.y >= 0 && destinationPos.y <= 7 && destinationSquare != startSquare)
        {
            int promotionID = 0;
            
            //Promotion
            if (Piece.IsType(pieceID, Piece.Pawn))
            {
                if ((chessBoardComponent.board.turn && destinationPos.y == 0) || (!chessBoardComponent.board.turn && destinationPos.y == 7) )
                {
                    //Here we would query the user for promotion piece.
                    //For now, forcing queen promotion.
                    promotionID = Piece.Queen;
                }
            }
            MoveResult moveResult = chessBoardComponent.board.RequestMove(new Move(startSquare, destinationSquare, promotionID));
            chessBoardComponent.canMoveBlackPieces = false;
            chessBoardComponent.canMoveWhitePieces = false;
            chessBoardComponent.MoveCompletedCallback(moveResult);
        }
        else
        {
            //If target square is not valid, skip board back.
            chessBoardComponent.UpdateBoard();
        }
        
    }
}
