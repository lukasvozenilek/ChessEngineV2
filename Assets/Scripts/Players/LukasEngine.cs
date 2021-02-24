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

    public int maxDepth = 20;

    public Board searchBoard;

    public TranspositionTable transTable;
    
    public LukasEngine(Board board, int timeLimit) : base(board)
    {
        this.timeLimit = timeLimit;
        transTable = new TranspositionTable(board, 10000);
    }

    public override void PlayMove()
    {
        Task.Factory.StartNew(() => StartDeepeningSearch(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(() => WaitForTimeUp(), TaskCreationOptions.LongRunning);
    }

    public void RunSingleDepthSearch(int depth)
    {
        Task.Factory.StartNew(()=>
        {
            ResetVars();
            searchBoard = new Board(board);
            evaluator = new Evaluator(searchBoard);
            InvokeMoveComplete(board.MakeMove(Search(depth, depth, alpha, beta, true).move));
            Debug.Log(movesEvaluated);
        }, TaskCreationOptions.LongRunning);
    }

    private float alpha;
    private float beta;
    SearchResult upperResult;
    public void StartDeepeningSearch()
    {
        ResetVars();
        searchBoard = new Board(board);
        evaluator = new Evaluator(searchBoard);

        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(searchBoard));
        if (legalMoves.Count == 0)
        {
            Debug.Log("No legal moves found");
            InvokeMoveComplete(null);
            return;
        }
        
        for (int i = 1; i < maxDepth; i++)
        {
            currentDepth = i;
            upperResult = Search(i, i, alpha, beta, true);
        }
    }

    private void ResetVars()
    {
        moveEvaluation.Clear();
        alphaPrunes = 0;
        betaPrunes = 0;
        movesEvaluated = 0;
        alpha = worstEval;
        beta = bestEval;
    }

    public async void WaitForTimeUp()
    {
        await Task.Delay(timeLimit);
        TimeLimitReached = true;
        InvokeMoveComplete(board.MakeMove(upperResult.move));
    }

    private SearchResult Search(int depthleft, int startdepth, float alpha, float beta, bool maximize)
    {
        //Interrupts search if time is over
        if (TimeLimitReached)
        {
            TimeLimitReached = false;
            throw new NotSupportedException();
        }

        float value = transTable.GetEvaluation(board.currentHash);
        if ((int)value != TranspositionTable.NORESULT)
        {
            //return new SearchResult(value);
        }
        
        //If bottom of depth, return evaluation of position
        if (depthleft == 0)
        {
            movesEvaluated += 1;
            int perspective = board.turn ? -1 : 1;
            float evaluation = perspective * evaluator.EvaluateBoard();
            transTable.StorePosition(board.currentHash, evaluation, TranspositionTable.FLAG_EXACT, startdepth);
            return new SearchResult(evaluation);
        }

        
        //Next ensure position has legal moves
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(searchBoard));
        int totalMoves = legalMoves.Count;
        if (totalMoves == 0)
        {
            //If in check, this is checkmate
            if (moveGenerator.checkSquaresBB > 0)
            {
                int perspective = maximize? -1 : 1;
                //Penalize a checkmate by the depth it's reached to achieve it.
                return new SearchResult(perspective * ( 5000f - (10 * (startdepth - depthleft))));
            }
            //Otherwise, draw due to stalemate
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
                SearchResult result;
                if (searchBoard.BoardResult == Board.BOARD_DRAW)
                {
                    result = new SearchResult(0);
                }
                else
                {
                    result = Search(depthleft - 1, startdepth, alpha, beta, false);
                    
                }
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
                SearchResult result;
                if (searchBoard.BoardResult == Board.BOARD_DRAW)
                {
                    result = new SearchResult(0);
                }
                else
                {
                    result = Search(depthleft - 1, startdepth, alpha, beta, true);
                    
                }
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

