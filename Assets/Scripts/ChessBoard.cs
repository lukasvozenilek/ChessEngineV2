using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ChessBoard.cs is the monobehaviour responsible for the graphical representation of the board
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

    public Board board;

    public bool canMoveWhitePieces;
    public bool canMoveBlackPieces;

    public UserInterface userInterfaceComponent;
    
    void Start()
    {
        GameState.MainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        whiteSquares.color = boardConfig.whiteColor;
        blackSquares.color = boardConfig.blackColor;
    }
    
    private void OnDestroy()
    {
        GameState.UpdateBoardEvent -= UpdateBoard;
    }

    private bool PlayingGame;
    private bool HumanPlayer;
    private bool AIPlayer;

    private Player whitePlayer;
    private Player blackPlayer;
    
    private MoveGenerator moveGenerator;
    public void StartNewGame(GameConfiguration gameconfig)
    {
        moveGenerator = new MoveGenerator();
        board = new Board(gameconfig.startingFEN);
        
        userInterfaceComponent.CloseGameResultWindow();

        switch (gameconfig.player1type)
        {
            case PlayerType.Human:
                whitePlayer = new Players.HumanPlayer(board, this);
                break;
            case PlayerType.Minimax:
                whitePlayer = new Minimax(board, 6);
                break;
            case PlayerType.MonteCarlo:
                whitePlayer = new MonteCarlo(board);
                break;
            case PlayerType.Random:
                whitePlayer = new Players.Random(board);
                break;
            case PlayerType.Stockfish:
                whitePlayer = new Players.Stockfish(board);
                break;
        }
        
        switch (gameconfig.player2type)
        {
            case PlayerType.Human:
                blackPlayer = new Players.HumanPlayer(board, this);
                break;
            case PlayerType.Minimax:
                blackPlayer = new Minimax(board, 6);
                break;
            case PlayerType.MonteCarlo:
                 whitePlayer = new MonteCarlo(board);
                 break;
            case PlayerType.Random:
                blackPlayer = new Players.Random(board);
                break;
            case PlayerType.Stockfish:
                blackPlayer = new Players.Stockfish(board);
                break;
            
        }

        whitePlayer.MoveCompleteEvent += MoveCompletedCallback;
        blackPlayer.MoveCompleteEvent += MoveCompletedCallback;

        PlayingGame = true;
        UpdateBoard();
        whitePlayer.PlayMove();
    }

    //This callback may be executed from a thread, so it must be seperate and trigger an update from Unity update loop.
    public MoveResult? PlayerMoveResult = null;
    public bool PlayerMoveComplete = false;
    public bool ReadyForNextMove = false;
    public void MoveCompletedCallback(MoveResult? result)
    {
        PlayerMoveResult = result;
        PlayerMoveComplete = true;
    }

    public void PlayerMoveCompleted(MoveResult? result)
    {
        if (result == null)
        {
            GameOver(board.BoardResult);
            PlayingGame = false;
        }
        else
        {
            UpdateBoard();
            PlayAudioFromMove((MoveResult)result);
            //Debug.Log("Current Evaluation: " + Evaluation.EvaluateBoard(board));
            ReadyForNextMove = true;
            
        }
    }

    public void GameOver(int result)
    {
        ReadyForNextMove = false;
        PlayerMoveComplete = false;
        PlayerMoveResult = null;
        userInterfaceComponent.GameOver(result);
    }

    private void Update()
    {
        if (PlayerMoveComplete)
        {
            PlayerMoveComplete = false;
            PlayerMoveCompleted(PlayerMoveResult);
        }

        if (blackPlayer != null && whitePlayer != null)
        {
            if (blackPlayer.moveEvaluation.Count > 0)
            {
                userInterfaceComponent.UpdateUIEval(true,new Dictionary<Move, float>(blackPlayer.moveEvaluation));
            } 
            else if (whitePlayer.moveEvaluation.Count > 0)
            {
                userInterfaceComponent.UpdateUIEval(false, new Dictionary<Move, float>(whitePlayer.moveEvaluation));
            } 
        }

        if (ReadyForNextMove)
        {
            if (!board.turn) whitePlayer.PlayMove();
            else blackPlayer.PlayMove();
            ReadyForNextMove = false;
        }
        
        if ((canMoveWhitePieces || canMoveBlackPieces) && Input.GetKeyDown(KeyCode.Mouse1))
        {
            UpdateBoard();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameState.BlackPerspective = !GameState.BlackPerspective;
            UpdateBoard();
        }
        
        if (Input.GetKey(KeyCode.Space))
        {
            UpdateBoard();
        }
        
        if (Input.GetButtonDown("Undo Move"))
        {
            board.UnmakeMove();
            UpdateBoard();
        }

        if (Input.GetButtonDown("Legal Moves"))
        {
            List<Move> moves = moveGenerator.GetAllLegalMoves(board);
            Debug.Log("Legal moves: " + moves.Count);
            CreateOverlayFromMoves(moves);
        }

        if (Input.GetButtonDown("White Attacking Moves"))
        {
            moveGenerator.CalculateAttacks(board, false);
            CreateOverlayFromBB(moveGenerator.attackSquaresBB);
        }
        
        if (Input.GetButtonDown("Black Attacking Moves"))
        {
            moveGenerator.CalculateAttacks(board, true);
            CreateOverlayFromBB(moveGenerator.attackSquaresBB);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            moveGenerator.CalculateAttacks(board, true);
            CreateOverlayFromBB(moveGenerator.checkSquaresBB);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            moveGenerator.CalculateAttacks(board, false);
            CreateOverlayFromBB(moveGenerator.checkSquaresBB);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            moveGenerator.CalculateAttacks(board, false);
            CreateOverlayFromBB(moveGenerator.pinnedSquaresBB);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            moveGenerator.CalculateAttacks(board, true);
            CreateOverlayFromBB(moveGenerator.pinnedSquaresBB);
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            List<int> squareList = new List<int>();
            squareList.Clear();
            foreach (KeyValuePair<int, int> piece in board.blackPieces)
            {
                squareList.Add(piece.Key);
            }
            foreach (KeyValuePair<int, int> piece in board.whitePieces)
            {
                squareList.Add(piece.Key);
            }
            CreateOverlayFromSquares(squareList);
        }
    }


    public void UpdateBoard()
    {
        ClearBoard();
        ClearOverlays();
        int pos = 0;
       
        for (int i = 0; i < 64; i++)
        {
            int piece = board.Squares[i];
            if (piece != Piece.None)
            {
                GameObject GO = Instantiate(piecePrefab);
                pieces.Add(GO);
                
                PieceGO pieceGO = GO.GetComponent<PieceGO>();
                pieceGO.pieceID = piece;
                pieceGO.myColor = board.GetPieceColor(i);
                pieceGO.chessBoardComponent = this;
                
                //Calculate spawn position
                Vector3 spawnpos = grid.CellToWorld(new Vector3Int(pos % 8, pos / 8, 0));
                spawnpos += grid.cellSize / 2;
                spawnpos.z = -1;
                GO.transform.position = spawnpos;
                pieceGO.startSquare = pos;

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
    
    public void CreateOverlayFromSquares(List<int> moves)
    {
        ClearOverlays();
        foreach (int square in moves) {
            CreateOverlay(square);
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


    public void PlayAudioFromMove(MoveResult move)
    {
        if (move.legal)
        {
            //Kinda nasty way to find if a check took place but meh
            moveGenerator.CalculateAttacks(board,!board.turn);
            if (moveGenerator.checkSquaresBB > 0)
            {
                audioSource.PlayOneShot(boardConfig.checkSound);
            } else if (move.castle)
            {
                audioSource.PlayOneShot(boardConfig.castleSound);
            }
            else if (move.capture)
            {
                audioSource.PlayOneShot(boardConfig.captureSound);
            }
            else
            {
                audioSource.PlayOneShot(boardConfig.moveSound);
            }
        }
    }
}
