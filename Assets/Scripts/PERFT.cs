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

    private static Board board;
    
    public static void RUN_PERFT(PERFTConfig config)
    {
        Debug.Log("RUNNING PERFT TEST!");
        board = new Board();
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
        List<Move> moves = board.GetAllLegalMoves();
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
