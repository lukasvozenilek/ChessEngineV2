
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeedTest
{
    private Board board;
    private Minimax minimax;
    private const int numberOfTests = 3;
    private int currentIteration = 0;
    private float startTime;

    public bool TestRunning = false;

    public void RunSpeedtest()
    {
        board = new Board();
        minimax = new Minimax(board, 6);
        minimax.MoveCompleteEvent += MinimaxDoneCallback;
        Debug.Log("Running speed test");
        currentIteration = 0;
        startTime = Time.realtimeSinceStartup;
        minimax.PlayMove();
        TestRunning = true;
        while (TestRunning)
        {
        }
        Debug.Log("Speed test complete. Took: " + (Time.realtimeSinceStartup - startTime));
    }

    public void MinimaxDoneCallback(MoveResult? moveResult)
    {
        TestRunning = false;
    }
}
