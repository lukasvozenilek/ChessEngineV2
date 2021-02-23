﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeedTest
{
    private Board board;
    private LukasEngine m_LukasEngine;
    private const int numberOfTests = 3;
    private int currentIteration = 0;
    private float startTime;

    public bool TestRunning = false;

    public void RunSpeedtest()
    {
        board = new Board();
        m_LukasEngine = new LukasEngine(board, 6);
        m_LukasEngine.MoveCompleteEvent += LukasEngineDoneCallback;
        Debug.Log("Running speed test");
        currentIteration = 0;
        startTime = Time.realtimeSinceStartup;
        m_LukasEngine.PlayMove();
        TestRunning = true;
        while (TestRunning)
        {
        }
        Debug.Log("Speed test complete. Took: " + (Time.realtimeSinceStartup - startTime));
    }

    public void LukasEngineDoneCallback(MoveResult? moveResult)
    {
        TestRunning = false;
    }
}
