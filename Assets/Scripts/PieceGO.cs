using System;
using UnityEngine;

public class PieceGO : MonoBehaviour
{
    public SpriteRenderer pieceImage;
    public ChessBoard chessBoardComponent;

    public int startSquare;

    public void OnMouseDrag()
    {
        Vector3 newPos = GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = -1;
        transform.position = newPos;
        
    }
    public void OnMouseDown()
    {
        if (Board.GetPieceColor(startSquare) == Board.turn) chessBoardComponent.CreateOverlayFromMoves(Board.GetLegalMovesFromSquare(startSquare));
    }

    public void OnMouseUp()
    {
        Vector3 destinationPos = chessBoardComponent.grid.WorldToCell(GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition));
        int destinationSquare = (int) (destinationPos.x) + ((int) destinationPos.y * 8);

        if (destinationPos.x >= 0 && destinationPos.x <= 7 && destinationPos.y >= 0 && destinationPos.y <= 7 && destinationSquare != startSquare)
        {
            MoveResult moveResult = Board.RequestMove(new Move(startSquare, destinationSquare, Board.Squares[destinationSquare]));
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
