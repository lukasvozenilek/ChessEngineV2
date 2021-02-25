
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
    private float startTime;

    public bool TestRunning = false;

    public SpeedTest()
    {
        board = new Board();
        m_LukasEngine = new LukasEngine(board, 1000);
        m_LukasEngine.MoveCompleteEvent += LukasEngineDoneCallback;
    }

    public void RunSpeedtest()
    {
        Debug.Log("Running speed test");
        startTime = Time.realtimeSinceStartup;
        m_LukasEngine.RunSingleDepthSearch(5);
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
