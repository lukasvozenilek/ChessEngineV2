using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceGO : MonoBehaviour
{
    public SpriteRenderer pieceImage;
    public ChessBoard chessBoardComponent;
    private Board boardRef;

    public int startSquare;

    public void OnMouseDrag()
    {
        Vector3 newPos = GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = -1;
        transform.position = newPos;
        
    }
    public void OnMouseDown()
    {
        if (boardRef.GetPieceColor(startSquare) == boardRef.turn)
        {
            List<Move> legalMoves = boardRef.GetLegalMovesFromSquare(startSquare);
            Debug.Log(legalMoves.Count);
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
            MoveResult moveResult = boardRef.RequestMove(new Move(startSquare, destinationSquare));
            if (moveResult.legal)
            {
                if (moveResult.capture)
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
