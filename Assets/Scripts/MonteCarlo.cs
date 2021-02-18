using UnityEngine;
using Random;
public class MonteCarlo
{
    private Board board;
    private Board board2;
    private MoveGenerator moveGenerator;
    public Move moveselect(Board board)
    {
        int bestscore=-99999;
        int start = Time.realTimesincestart;
        
        foreach (Move move in legalMoves.ToList())
        {   
            board2=new Board();
            board2.Squares = board.Squares;
            board2.MakeMove(move, false);
            simresult = simresult(board);
            int score = simresult[0]-simresult[1];
            
            print(simresult,move,score);
            
            if (score>bestscore){
                Move bestmove=move;
                bestscore=score;
            }
            
            board.UnmakeMove();
            
            if (bestscore==100){
                break;
            }
        }
        int finish = Time.realTimesincestart;
        totaltime=finish-start;
        Debug.Log("took "+ totaltime + " seconds." );
        return bestmove;
    }
    void simresult()
    {   
        fen = board.fen();
        int win=0;
        int loss=0;
        int draw = 0;
        int cores = SystemInfo.processorCount;
        int simnum= 100;
        string[] values;
        string[] results;
        /* 
        find way to make array of fens
        */
        foreach (string value in values)
            results.append(worker());

        foreach (string result in results)
        {
            if (result == "1-0"){
                win++;
            }
            else if(result=="0-1"){
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
    void worker()
    {
        
        board2.setfen(fen);
        while (true)
        {
            moves=board2.legalMoves;
            if (moves.Length==0){
                break;
            }
            Random rand = new Random();
            int index = rand.Next(moves.Length);
            board2.MakeMove(moves[index]);            
        }
        string result = board2.result();
        return(result);



    }
}