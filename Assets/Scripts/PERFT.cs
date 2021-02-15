using System.Collections.Generic;
using UnityEngine;

public struct PERFTConfig
{
    public string FEN;
    public List<long> requirements;
}
public static class PERFT
{
    public static void RUN_PERFT(PERFTConfig config)
    {
        Debug.Log("RUNNING PERFT TEST!");
        Board.LoadPositionFromFEN(config.FEN);
        float startTime = Time.realtimeSinceStartup;
        int result;
        int i = 0;
        bool passed = true;
        long totalevals = 0;
        foreach (int requirement in config.requirements)
        {
            result = testdepth(i);
            totalevals += result;
            passed = result == requirement;
            Debug.Log("DEPTH " + i + ": " + result + ", REQUIREMENT: " + requirement + (passed?" PASSED" : " FAILED"));
            i++;
        }
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Duration: " +  elapsedTime);
        Debug.Log("Result: " + (passed?" PASSED" : " FAILED"));
        Debug.Log("Evaluation Speed: " + ( totalevals / elapsedTime) + "(" + (10 * Mathf.Log10(totalevals / elapsedTime)) + ") moves per second (dB)");
    }

    private static int testdepth(int depth)
    {
        if (depth == 0)
        {
            return 1;
        }
        int nodes = 0;
        List<Move> moves = Board.GetAllLegalMoves();
        foreach (Move move in moves)
        {
            Board.MakeMove(move, sendEvent:false);
            nodes += testdepth(depth - 1);
            Board.UnmakeMove(sendEvent:false);
        }
        return nodes;
    }
}
