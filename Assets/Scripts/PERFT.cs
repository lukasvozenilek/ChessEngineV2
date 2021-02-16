﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public struct PERFTConfig
{
    public string FEN;
    public List<long> requirements;
    public int depth;
}

public static class PERFT
{

    public static Dictionary<string, int> PERFTDivideResults = new Dictionary<string, int>();

    private static Board board;
    
    public static void RUN_PERFT(PERFTConfig config)
    {
        Debug.Log("RUNNING PERFT TEST!");
        board = new Board();
        float startTime = Time.realtimeSinceStartup;
        int result;
        bool passed = true;
        long totalevals = 0;
        for (int i = 0; i < Mathf.Min(config.requirements.Count,config.depth+1); i++)
        {
            long requirement = config.requirements[i];
            PERFTDivideResults.Clear();
            result = testdepth(i, i);
            totalevals += result;
            passed = result == requirement;
            foreach (KeyValuePair<string, int> KVP in PERFTDivideResults)
            {
                Debug.Log(KVP.Key + ": " + KVP.Value);
            }
            Debug.Log("DEPTH " + i + ": " + result + ", REQUIREMENT: " + requirement + (passed?" PASSED" : " FAILED"));
        }
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Duration: " +  elapsedTime);
        Debug.Log("Result: " + (passed?" PASSED" : " FAILED"));
        Debug.Log("Evaluation Speed: " + ( totalevals / elapsedTime) + "(" + (10 * Mathf.Log10(totalevals / elapsedTime)) + ") moves per second (dB)");
    }

    private static int testdepth(int depth, int startdepth)
    {
        if (depth == 0)
        {
            return 1;
        }
        int nodes = 0;
        List<Move> moves = MoveGenerator.GetAllLegalMoves(board).ToList();
        foreach (Move move in moves)
        {
            board.MakeMove(move, false);
            int thisNode = testdepth(depth - 1, startdepth);
            nodes += thisNode;
            if (depth == startdepth)
            {
                string movename = Constants.MoveToString(move);
                PERFTDivideResults.Add(movename, thisNode);
            }

            board.UnmakeMove(sendEvent:false);
        }
        return nodes;
    }

    
}
