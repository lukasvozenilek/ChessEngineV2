﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{ 
    public ChessBoard chessBoardRef;
    private PERFT perft;
    private SpeedTest speedTest;
    
    [Header("Left Pane")] 
    [Header("Testing")]
    public Button runPerftButton;
    public Button runSpeedButton;
    
    [Header("New Game")] public Button newGameButton;
    public TMP_Dropdown player1dropdown;
    public TMP_Dropdown player2dropdown;
    public TMP_InputField FENInput;
    

    [Header("Right pane")] 
    [Header("Evaluation")]
    public TMP_Text evaluationText;

    [Header("References")]
    public GameOverWindow gameOverWindowRef;
    public GameObject canvasComponent;
    public void Start()
    {
        canvasComponent.SetActive(true);
        gameOverWindowRef.gameObject.SetActive(false);
        runPerftButton.onClick.AddListener(RunPERFT);
        runSpeedButton.onClick.AddListener(StartSpeedTest);
        newGameButton.onClick.AddListener(StartNewGame);
        gameOverWindowRef.closeButton.onClick.AddListener(CloseGameResultWindow);
        
        perft = new PERFT();
        speedTest = new SpeedTest();
    }

    public void StartSpeedTest()
    {
        speedTest.RunSpeedtest();
    }

    public void StartNewGame()
    {
        GameConfiguration config = new GameConfiguration((PlayerType)player1dropdown.value, (PlayerType)player2dropdown.value);
        config.startingFEN = String.IsNullOrEmpty(FENInput.text)? Constants.startingFEN : FENInput.text ;
        chessBoardRef.StartNewGame(config);
    }
    
    public void UpdateUIEval(bool color, Dictionary<Move,float> evals)
    {
        string text = (color ? "Black" : "White") + ":\n";
        foreach (KeyValuePair<Move, float> eval in evals)
        {
            text += Constants.MoveToString(eval.Key) + ": " + Math.Round(eval.Value, 3) + "\n";
        }
        evaluationText.text = text;
    }

    public void GameOver(int result)
    {
        gameOverWindowRef.gameObject.SetActive(true);
        string resultText = "Unknown Result!";
        switch (result)
        {
            case Board.BOARD_DRAW:
                resultText = "Draw!";
                break;
            case Board.BOARD_WHITEWON:
                resultText = "White wins!";
                break;
            case Board.BOARD_BLACKWON:
                resultText = "Black wins!";
                break;
        }
        gameOverWindowRef.outcomeText.text = resultText;
    }


    public void CloseGameResultWindow()
    {
        gameOverWindowRef.gameObject.SetActive(false);
    }
    
    public void RunPERFT()
    {
        PERFTConfig config1 = new PERFTConfig();
        config1.FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        config1.requirements = new List<long> {1, 20, 400, 8902, 197281, 4865609};

        PERFTConfig config2 = new PERFTConfig();
        config2.FEN = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        config2.requirements = new List<long> {1, 48, 2039, 97862, 4085603, 193690690};

        PERFTConfig config3 = new PERFTConfig();
        config3.FEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
        config3.requirements = new List<long> {1, 14, 191, 2812, 43238, 674624};


        PERFTConfig[] configs = new[]
        {
            config1,
            config2,
            config3
        };

        perft.RUN_PERFT(configs[0]);
    }
}
