
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MinimaxSpeedTestResult
{
    public int EvaluatedMoves;
    public int AlphaPrunedMoves;
    public int BetaPrunedMoves;
    public float ElapsedTime;

    

    public MinimaxSpeedTestResult(int evaluatedMoves, int alphaPrunedMoves, int betaPrunedMoves, float elapsedTime)
    {
        this.EvaluatedMoves = evaluatedMoves;
        this.AlphaPrunedMoves = alphaPrunedMoves;
        this.BetaPrunedMoves = betaPrunedMoves;
        this.ElapsedTime = elapsedTime;
    }
}

public class SpeedTest
{
    private Board board;
    private MoveGenerator moveGenerator;
    private Minimax minimax;
    private const int numberOfTests = 3;
    private int currentIteration = 0;
    private float startTime;
    public event Action<List<MinimaxSpeedTestResult>> testCompleteCallback;

    private List<MinimaxSpeedTestResult> testResults = new List<MinimaxSpeedTestResult>();
    
    public SpeedTest()
    {
        board = new Board();
        moveGenerator = new MoveGenerator();
        minimax = new Minimax(board, 5);
        minimax.MoveCompleteEvent += MinimaxDoneCallback;
    }

    public void RunSpeedtest()
    {
        board = new Board();
        Debug.Log("Running speed test");
        currentIteration = 0;
        startTime = Time.realtimeSinceStartup;
        minimax.PlayMove();
    }

    public void MinimaxDoneCallback(MoveResult? moveResult)
    {
        testResults.Add(new MinimaxSpeedTestResult(minimax.movesEvaluated, minimax.alphaPrunes, minimax.betaPrunes,0));
        testCompleteCallback?.Invoke(testResults);
    }
}
