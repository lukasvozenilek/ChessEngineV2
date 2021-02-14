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

    public void OnMouseClick()
    {
        
    }

    public void OnMouseUp()
    {
        Vector3 destinationPos = chessBoardComponent.grid.WorldToCell(GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition));
        int destinationSquare = (int) (destinationPos.x) + ((int) destinationPos.y * 8);

        if (destinationPos.x >= 0 && destinationPos.x <= 7 && destinationPos.y >= 0 && destinationPos.y <= 7 && destinationSquare != startSquare)
        {
            Board.Squares[destinationSquare] = Board.Squares[startSquare];
            Board.Squares[startSquare] = Piece.None;
            chessBoardComponent.UpdateBoard();
            chessBoardComponent.audioSource.PlayOneShot(chessBoardComponent.boardConfig.moveSound);
        }
        else
        {
            //If target square is not valid, skip board back.
            chessBoardComponent.UpdateBoard();
        }
    }
}
