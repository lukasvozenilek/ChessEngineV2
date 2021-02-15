using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ChessBoard.cs is responsible for the graphical representation of the board. It manages updating the piece objects from the static board
/// state as well as flipping pieces for black's perspective.
/// </summary>
public class ChessBoard : MonoBehaviour
{
    public BoardConfig boardConfig;
    public Tilemap whiteSquares;
    public Tilemap blackSquares;
    public Grid grid;

    public GameObject piecePrefab;
    public GameObject overlayPrefab;

    public List<GameObject> pieces = new List<GameObject>();
    public List<GameObject> overlays = new List<GameObject>();
    
    public AudioSource audioSource;

    void Start()
    {
        GameState.MainCamera = Camera.main;
        
        audioSource = GetComponent<AudioSource>();

        whiteSquares.color = boardConfig.whiteColor;
        blackSquares.color = boardConfig.blackColor;
        
        UpdateBoard();

        GameState.UpdateBoardEvent += UpdateBoard;
    }

    private void OnDestroy()
    {
        GameState.UpdateBoardEvent -= UpdateBoard;
    }


    public void UpdateBoard()
    {
        ClearBoard();
        ClearOverlays();
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

                pieceGO.startSquare = pos;
                pieceGO.chessBoardComponent = this;
                
                //Flip piece if in black perspective
                pieceGO.transform.localScale = GameState.BlackPerspective ? new Vector3(-1, -1, 1) : new Vector3(1, 1, 1);
                
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
    
    public void CreateOverlay(int pos)
    {
        GameObject GO = Instantiate(overlayPrefab);
        overlays.Add(GO);
        Vector3 spawnpos = grid.CellToWorld(new Vector3Int(pos % 8, pos / 8, 0));
        spawnpos += grid.cellSize / 2;
        spawnpos.z = -1;
        GO.transform.position = spawnpos;
    }

    public void CreateOverlayFromBB(ulong bb)
    {
        ClearOverlays();
        //Debug.Log( Convert.ToString((long)bb, 2));
        for (int i = 0; i < 64; i++)
        {
            ulong mask = (ulong)1 << i;
            if ((mask & bb) >= 1)
            {
                CreateOverlay(i);
            }
        }
    }
    
    public void CreateOverlayFromMoves(List<Move> moves)
    {
        ClearOverlays();
        foreach (Move move in moves) {
            CreateOverlay(move.DestinationSquare);
        }
    }

    public void ClearOverlays()
    {
        foreach (GameObject GO in overlays)
        {
            Destroy(GO);
        }
        overlays.Clear();
    }


    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            GameState.BlackPerspective = !GameState.BlackPerspective;
            UpdateBoard();
        }
        if (Input.GetButtonDown("Undo Move"))
        {
            Board.UnmakeMove();
        }

        if (Input.GetButtonDown("Legal Moves"))
        {
            CreateOverlayFromMoves(Board.GetAllLegalMoves());
        }
    }
}
