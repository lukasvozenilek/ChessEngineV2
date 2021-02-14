using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChessBoard : MonoBehaviour
{
    public BoardConfig boardConfig;
    public Tilemap whiteSquares;
    public Tilemap blackSquares;
    public Grid grid;

    public GameObject piecePrefab;

    public List<GameObject> pieces = new List<GameObject>();

    private AudioSource audioSource;
    
    void Start()
    {
        GameState.MainCamera = Camera.main;
        print(Board.Squares[0]);

        audioSource = GetComponent<AudioSource>();
        
        Board.LoadPositionFromFEN("8/8/pk6/8/3P4/1pn1N1R1/2r2R2/5K2 b - - 0 1");

        whiteSquares.color = boardConfig.whiteColor;
        blackSquares.color = boardConfig.blackColor;
        UpdateBoard();
    }


    public void UpdateBoard()
    {
        ClearBoard();
        int pos = 0;
        foreach (int piece in Board.Squares)
        {
            if (piece != Piece.None)
            {
                GameObject GO = Instantiate(piecePrefab);
                pieces.Add(GO);
                PieceGO pieceGO = GO.GetComponent<PieceGO>();
                
                //Calculate spawn position
                Vector3 spawnpos = grid.CellToWorld(new Vector3Int(pos % 8, pos / 8, 0));
                spawnpos += grid.cellSize / 2;
                spawnpos.z = -1;
                GO.transform.position = spawnpos;

                pieceGO.startPos = GO.transform.position;
                pieceGO.chessBoardComponent = this;
                
                //Set piece sprite

                switch (piece)
                {
                    case Piece.White + Piece.Pawn:
                        pieceGO.pieceImage.sprite = boardConfig.whitePawn;
                        break;
                    case Piece.White + Piece.King:
                        pieceGO.pieceImage.sprite = boardConfig.whiteKing;
                        break;
                    case Piece.White + Piece.Rook:
                        pieceGO.pieceImage.sprite = boardConfig.whiteRook;
                        break;
                    case Piece.White + Piece.Bishop:
                        pieceGO.pieceImage.sprite = boardConfig.whiteBishop;
                        break;
                    case Piece.White + Piece.Knight:
                        pieceGO.pieceImage.sprite = boardConfig.whiteKnight;
                        break;
                    case Piece.White + Piece.Queen:
                        pieceGO.pieceImage.sprite = boardConfig.whiteQueen;
                        break;
                    case Piece.Black + Piece.Pawn:
                        pieceGO.pieceImage.sprite = boardConfig.blackPawn;
                        break;
                    case Piece.Black + Piece.King:
                        pieceGO.pieceImage.sprite = boardConfig.blackKing;
                        break;
                    case Piece.Black + Piece.Rook:
                        pieceGO.pieceImage.sprite = boardConfig.blackRook;
                        break;
                    case Piece.Black + Piece.Bishop:
                        pieceGO.pieceImage.sprite = boardConfig.blackBishop;
                        break;
                    case Piece.Black + Piece.Knight:
                        pieceGO.pieceImage.sprite = boardConfig.blackKnight;
                        break;
                    case Piece.Black + Piece.Queen:
                        pieceGO.pieceImage.sprite = boardConfig.blackQueen;
                        break;
                }
            }
            pos++;
        }
    }

    public void ClearBoard()
    {
        foreach (GameObject GO in pieces)
        {
            Destroy(GO);
        }
        pieces.Clear();
    }
    
    void Update()
    {
        
    }
    
    private void OnMouseDrag()
    {
        
    }    

    private void OnMouseDown()
    {
        print(grid.WorldToCell(GameState.MainCamera.ScreenToWorldPoint(Input.mousePosition)));
    }

    private void OnMouseUp()
    {
        
    }
}
