using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class MonteCarlo : Player
{
    private Board board2;
    private int simnum=100;
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
            
            Debug.Log(result[0]+result[1]+result[2]+Constants.MoveToString(move)+score);
            
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
        List <string> outcomes= new List<string>();
        for(int i=0; i< simnum; i++)
            board2=new Board(board);
            outcomes.Add(worker());

        foreach (string outcome in outcomes)
        {
            if (outcome == "1-0"){
                win++;
            }
            else if(outcome=="0-1"){
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
    string worker()
    {
        while (true)
        {
            List<Move> moves = moveGenerator.GetAllLegalMoves(board2);
            if (moves.Count==0 || board2.moves.Count>170){
                break;
            }
            board2.MakeMove(moves[Random.Range(0, moves.Count-1)]);                
        }
        string ramification = "1-0";
        return ramification;



    }
}