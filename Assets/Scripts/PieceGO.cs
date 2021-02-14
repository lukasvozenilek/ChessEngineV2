using System;
using UnityEngine;

public class PieceGO : MonoBehaviour
{
    public SpriteRenderer pieceImage;
    public ChessBoard chessBoardComponent;

    public Vector3 startPos;

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
        if (transform.position != startPos) chessBoardComponent.UpdateBoard();
    }
}
