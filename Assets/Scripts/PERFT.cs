using System.Collections;
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

public class PERFT
{

    public Dictionary<string, int> PERFTDivideResults = new Dictionary<string, int>();

    private Board board;
    private MoveGenerator moveGenerator;

    public PERFT()
    {
        board = new Board();
        moveGenerator = new MoveGenerator();
    }
    
    public void RUN_PERFT(PERFTConfig config)
    {

        Debug.Log("RUNNING PERFT TEST!");
        board = new Board(config.FEN);
        float startTime = Time.realtimeSinceStartup;
        int result;
        bool passed = true;
        long totalevals = 0;
        for (int i = 0; i < Mathf.Min(config.requirements.Count,config.depth+1); i++)
        {
            long requirement = config.requirements[i];
            captures = 0;
            checkmates = 0;
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
        Debug.Log("Captures: " + captures);
        Debug.Log("Checkmates: " + checkmates);
    }

    private static int captures;
    private static int checks;
    private static int checkmates;
    
    
    private int testdepth(int depth, int startdepth)
    {
        if (depth == 0)
        {
            return 1;
        }
        int nodes = 0;
        List<Move> moves = moveGenerator.GetAllLegalMoves(board).ToList();
        if (moves.Count == 0) checkmates++;
        foreach (Move move in moves)
        {
            MoveResult result = board.MakeMove(move, false);
            if (result.capture) captures++;
            if (result.check) checks++;
            
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
