using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;


public struct SearchResult
{
    public float Eval;
    public Move move;

    public SearchResult(float Eval)
    {
        this.Eval = Eval;
        this.move = new Move();
    }
}

public class LukasEngine : Player
{
    public int alphaPrunes;
    public int betaPrunes;
    public int movesEvaluated;
    
    private const float worstEval = -10000;
    public const float bestEval = 10000;
    
    public Evaluator evaluator;

    public int timeLimit;
    public bool TimeLimitReached = false;

    public int currentDepth;

    public Board searchBoard;
    public LukasEngine(Board board, int timeLimit) : base(board)
    {
        this.timeLimit = timeLimit;
    }

    public override void PlayMove()
    {
        Task.Factory.StartNew(() => StartDeepeningSearch(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(() => WaitForTimeUp(), TaskCreationOptions.LongRunning);
    }

    
    SearchResult upperResult;
    public void StartDeepeningSearch()
    {
        searchBoard = new Board(board);
        evaluator = new Evaluator(searchBoard);
        moveEvaluation.Clear();
        alphaPrunes = 0;
        betaPrunes = 0;
        movesEvaluated = 0;
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(searchBoard));
        if (legalMoves.Count == 0)
        {
            Debug.Log("No legal moves found");
            InvokeMoveComplete(null);
            return;
        }

        for (int i = 1; i < 20; i++)
        {
            currentDepth = i;
            upperResult = Search(i, i, worstEval, bestEval, true);
            //Debug.Log("Depth " + i + " complete. Best move: " + Constants.MoveToString(upperResult.move) + " with eval: " + upperResult.Eval);
        }
    }

    public async void WaitForTimeUp()
    {
        await Task.Delay(timeLimit);
        TimeLimitReached = true;
        InvokeMoveComplete(board.MakeMove(upperResult.move));
    }

    private SearchResult Search(int depthleft, int startdepth, float alpha, float beta, bool maximize)
    {
        if (TimeLimitReached)
        {
            TimeLimitReached = false;
            throw new NotSupportedException();
        }
        if (depthleft == 0)
        {
            movesEvaluated += 1;
            int perspective = board.turn ? -1 : 1;
            return new SearchResult(perspective * evaluator.EvaluateBoard());
        }
        
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(searchBoard));
        int totalMoves = legalMoves.Count;
        if (totalMoves == 0)
        {
            //If in check, this is checkmate
            if (moveGenerator.checkSquaresBB > 0)
            {
                int perspective = maximize? -1 : 1;
                //Penalize a checkmate by the depth it's reached to achieve it.
                return new SearchResult(perspective * ( 5000f - (startdepth - depthleft)));
            }
            //Stalemate
            return new SearchResult(0);
        }

        int i = 0;
        OrderMoves(legalMoves);
        if (maximize)
        {
            SearchResult bestResult = new SearchResult(worstEval);
            foreach (Move move in legalMoves)
            {
                i++;
                searchBoard.MakeMove(move);
                SearchResult result = Search(depthleft - 1, startdepth, alpha, beta, false);
                result.move = move;

                if (startdepth - depthleft == 0)
                {
                    if (!moveEvaluation.ContainsKey(result.move)) moveEvaluation.Add(result.move, result.Eval);
                    else moveEvaluation[result.move] = result.Eval;
                }

                if (result.Eval > bestResult.Eval)
                {
                    bestResult = result;
                }
                
                alpha = Math.Max(alpha, result.Eval);
                if (alpha >= beta)
                {
                    //Beta cutoff
                    betaPrunes += totalMoves - i;
                    searchBoard.UnmakeMove();
                    break;
                }
                searchBoard.UnmakeMove();
            }
            return bestResult;
        }
        else
        {
            SearchResult worstResult = new SearchResult(bestEval);
            foreach (Move move in legalMoves)
            {
                i++;
                searchBoard.MakeMove(move);
                SearchResult result = Search(depthleft - 1, startdepth, alpha, beta, true);
                result.move = move;
                
                if (startdepth - depthleft == 0)
                {
                    if (!moveEvaluation.ContainsKey(result.move)) moveEvaluation.Add(result.move, result.Eval);
                    else moveEvaluation[result.move] = result.Eval;
                }
                
                if (result.Eval < worstResult.Eval)
                {
                    worstResult = result;
                }
                
                beta = Math.Min(beta, result.Eval);
                if (beta <= alpha)
                {
                    //Alpha cutoff
                    alphaPrunes += totalMoves - i;
                    searchBoard.UnmakeMove();
                    break;
                }
                searchBoard.UnmakeMove();
            }
            return worstResult;
        }
    }

    //Right now just sorts based on captures
    private void OrderMoves (List<Move> moves)
    {
        int[] moveScores = new int[moves.Count];
        for (int i = 0; i < moves.Count; i++) {
            int score = 0;
            int movePieceType = Piece.GetType(board.Squares[moves[i].StartSquare]);
            int capturePieceType = Piece.GetType(board.Squares[moves[i].DestinationSquare]);
            if (capturePieceType != Piece.None)
            {
                score = 10;
            }
            else
            {
                score = 0;
            }
            moveScores[i] = score;
        }
        Sort (moves, moveScores);
    }
    
    
    private void Sort (List<Move> moves, int[] moveScores) {
        for (int i = 0; i < moves.Count - 1; i++) {
            for (int j = i + 1; j > 0; j--) {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j]) {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
    }
}

