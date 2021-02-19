using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class MonteCarlo : Player
{
    private Board board2;
    private int simnum=500;
    public MonteCarlo(Board board): base(board){}
    public override MoveResult? PlayMove()
    {
        int bestscore=-99999;
        float start = Time.realtimeSinceStartup;
        List<Move> legalMoves = moveGenerator.GetAllLegalMoves(board);
        if (legalMoves.Count==0){
            return null;
        }
        Move bestmove = new Move();

        foreach (Move move in legalMoves.ToList())
        {   
            board.MakeMove(move, false);
            int[] result = simresult();
            int score = result[0]-result[1];
            
            Debug.Log("Move " + Constants.MoveToString(move) + " had eval of " + score + " with " + result[0] + "-" + result[1]);
            
            if (score>bestscore){
                bestmove=move;
                bestscore=score;
            }
            
            board.UnmakeMove();
            
            // if (bestscore==simnum){
            //     break;
            // }
        }
        float finish = Time.realtimeSinceStartup;
        float totaltime=finish-start;
        Debug.Log("took "+ totaltime + " seconds." );
        return board.MakeMove(bestmove);
    }
    int[] simresult()
    {   
        int win=0;
        int loss=0;
        int draw = 0;
        int cores = SystemInfo.processorCount;
        List <int> outcomes= new List<int>();
        for (int i = 0; i < simnum; i++)
        {
            board2 = new Board(board);
            outcomes.Add(worker());
        }

        foreach (int outcome in outcomes)
        {
            if (outcome == Board.BOARD_WHITEWON){
                win++;
            }
            else if(outcome==Board.BOARD_BLACKWON){
                loss++;
            }
            else
            {
                draw++;
            }
        }
        int[] result = {win,loss,draw};
        return result;

    }
    int worker()
    {
        while (true)
        {
            List<Move> moves = moveGenerator.GetAllLegalMoves(board2);
            if (moves.Count == 0){
                break;
            }
            if (board2.moves.Count > 150)
            {
                board2.BoardResult = Board.BOARD_DRAW;
                break;
            }
            board2.MakeMove(moves[Random.Range(0, moves.Count-1)], false);                
        }
        return board2.BoardResult;



    }
}