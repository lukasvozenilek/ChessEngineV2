using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public struct PERFTConfig
{
    public string FEN;
    public List<long> requirements;
}
public static class PERFT
{

    public static Dictionary<string, int> PERFTDivideResults = new Dictionary<string, int>();
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
            PERFTDivideResults.Clear();
            result = testdepth(i, i);
            totalevals += result;
            passed = result == requirement;
            Debug.Log("DEPTH " + i + ": " + result + ", REQUIREMENT: " + requirement + (passed?" PASSED" : " FAILED"));
            foreach (KeyValuePair<string, int> KVP in PERFTDivideResults)
            {
                Debug.Log(KVP.Key + ": " + KVP.Value);
            }
            i++;
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
        List<Move> moves = Board.GetAllLegalMoves();
        foreach (Move move in moves)
        {
            Board.MakeMove(move, false);
            int thisNode = testdepth(depth - 1, startdepth);
            nodes += thisNode;
            if (depth == startdepth)
            {
                string movename = MoveToString(move.StartSquare, move.DestinationSquare);
                PERFTDivideResults.Add(movename, thisNode);
            }

            Board.UnmakeMove(sendEvent:false);
        }
        return nodes;
    }

    private static string MoveToString(int start, int end)
    {
        return Board.ConvertToCoord(start) + Board.ConvertToCoord(end);
    }
}
