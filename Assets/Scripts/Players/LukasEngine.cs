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
    public int transpositionsFound;
    public int checkMatesFound;

    private const float worstEval = -10000;
    public const float bestEval = 10000;

    public Evaluator evaluator;

    public int timeLimit;
    public bool TimeLimitReached = false;
    public bool PlayedMove = false;

    public int currentDepth;

    public int maxDepth = 30;

    public int maxDepthReached;

    public Board searchBoard;

    public TranspositionTable transTable;

    public LukasEngine(Board board, int timeLimit) : base(board)
    {
        this.timeLimit = timeLimit;
        transTable = new TranspositionTable(board, 256000);
    }

    public override void PlayMove()
    {
        Task.Factory.StartNew(() => StartDeepeningSearch(), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(() => WaitForTimeUp(), TaskCreationOptions.LongRunning);
    }

    public void RunSingleDepthSearch(int depth)
    {
        Task.Factory.StartNew(() =>
        {
            ResetVars();
            searchBoard = new Board(board);
            evaluator = new Evaluator(searchBoard);
            InvokeMoveComplete(board.MakeMove(Search(depth, depth, alpha, beta, true).move));
            DebugResults();
        }, TaskCreationOptions.LongRunning);
    }

    private float alpha;
    private float beta;
    SearchResult upperResult;

    private void StartDeepeningSearch()
    {
        ResetVars();
        moveEvaluation.Clear();
        alpha = worstEval;
        beta = bestEval;
        searchBoard = new Board(board);
        evaluator = new Evaluator(searchBoard);

        PlayedMove = false;

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
            ResetVars();
            upperResult = Search(i, i, alpha, beta, true);
        }

        if (!PlayedMove)
        {
            PlayedMove = true;
            InvokeMoveComplete(board.MakeMove(upperResult.move));
            DebugResults();
        }
    }

    private void ResetVars()
    {
        alphaPrunes = 0;
        betaPrunes = 0;
        movesEvaluated = 0;
        alpha = worstEval;
        beta = bestEval;
        transpositionsFound = 0;
        maxDepthReached = 0;
        checkMatesFound = 0;
        transTable.Clear();
    }

    private async void WaitForTimeUp()
    {
        await Task.Delay(timeLimit);
        TimeLimitReached = true;
        if (!PlayedMove)
        {
            PlayedMove = true;
            InvokeMoveComplete(board.MakeMove(upperResult.move));
            DebugResults();
        }
    }

    private void DebugResults()
    {
        Debug.Log("Moves evaluated: " + movesEvaluated);
        Debug.Log("Alpha prunes: " + alphaPrunes);
        Debug.Log("Beta prunes: " + betaPrunes);
        Debug.Log("Transpositions: " + transpositionsFound);
        Debug.Log("Max depth reached: " + maxDepthReached);
        Debug.Log("Checkmates found: " + checkMatesFound);
        Debug.Log("Final move evaluations:");
        foreach (var kvp in moveEvaluation)
        {
            Debug.Log(Constants.MoveToString(kvp.Key) + ": " + kvp.Value);
        }

        Debug.Log("\n");
    }

    private SearchResult Search(int depthleft, int startdepth, float alpha, float beta, bool maximize)
    {
        maxDepthReached = Math.Max(maxDepthReached, startdepth - depthleft);

        //Interrupts search if time is over
        if (TimeLimitReached)
        {
            TimeLimitReached = false;
            throw new NotSupportedException();
        }

        float value = transTable.GetEvaluation(searchBoard.currentHash, startdepth - depthleft, alpha, beta);
        if ((int) value != TranspositionTable.NORESULT)
        {
            transpositionsFound += 1;
            int perspective = (searchBoard.turn ? -1 : 1) * (maximize ? 1 : -1);
            return new SearchResult(perspective * value);
        }

        //If bottom of depth, return evaluation of position
        if (depthleft == 0)
        {
            movesEvaluated += 1;
            int perspective = (searchBoard.turn ? -1 : 1) * (maximize ? 1 : -1);
            float evaluation = evaluator.EvaluateBoard();
            transTable.StorePosition(searchBoard.currentHash, evaluation, TranspositionTable.FLAG_EXACT, startdepth);
            return new SearchResult(perspective * evaluation);
        }

        //Next, ensure position has legal moves
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(searchBoard));
        int totalMoves = legalMoves.Count;
        if (totalMoves == 0)
        {
            //If in check, this is checkmate
            if (moveGenerator.checkSquaresBB > 0)
            {
                checkMatesFound += 1;
                int perspective = maximize ? -1 : 1;
                //Penalize a checkmate by the depth it's reached to achieve it.
                return new SearchResult(perspective * (5000f - (10 * (startdepth - depthleft))));
            }

            //Otherwise, draw due to stalemate
            return new SearchResult(0);
        }

        int i = 0;
        OrderMoves(legalMoves);
        if (depthleft == startdepth)
        {
            //legalMoves = new List<Move>{new Move(0, 1)}; 
            //legalMoves = new List<Move>{new Move(0, 9)}; 
            //legalMoves = new List<Move>{new Move(0, 8)}; 
        }

        if (maximize)
        {
            SearchResult bestResult = new SearchResult(worstEval);
            foreach (Move move in legalMoves)
            {
                if (depthleft == startdepth) transTable.Clear();

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

                    transTable.StorePosition(searchBoard.currentHash, beta, TranspositionTable.FLAG_BETA,
                        startdepth - depthleft);
                    searchBoard.UnmakeMove();
                    return new SearchResult(beta);
                }

                searchBoard.UnmakeMove();
            }

            transTable.StorePosition(searchBoard.currentHash, bestResult.Eval, TranspositionTable.FLAG_EXACT,
                startdepth - depthleft);
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
                    transTable.StorePosition(searchBoard.currentHash, alpha, TranspositionTable.FLAG_ALPHA,
                        startdepth - depthleft);
                    searchBoard.UnmakeMove();
                    return new SearchResult(alpha);
                }

                searchBoard.UnmakeMove();
            }

            transTable.StorePosition(searchBoard.currentHash, worstResult.Eval, TranspositionTable.FLAG_EXACT,
                startdepth - depthleft);
            return worstResult;
        }
    }

    //Right now just sorts based on captures
    private void OrderMoves(List<Move> moves)
    {
        int[] moveScores = new int[moves.Count];
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];

            int score = 0;
            int movePieceType = Piece.GetType(searchBoard.Squares[move.StartSquare]);
            int capturePieceType = Piece.GetType(searchBoard.Squares[move.DestinationSquare]);

            searchBoard.MakeMove(move);

            //Exists in transposition table, top priority
            if (transTable.ContainsPosition(searchBoard.currentHash))
            {
                score += 1000;
            }

            //Capture
            if (capturePieceType != Piece.None)
            {
                score += 10;
            }

            //Previous depth's best move
            if (move.StartSquare == upperResult.move.StartSquare &&
                move.DestinationSquare == upperResult.move.DestinationSquare)
            {
                score += 100;
            }

            moveScores[i] = score;
            searchBoard.UnmakeMove();
        }

        Sort(moves, moveScores);
    }

    private void Sort(List<Move> moves, int[] moveScores)
    {
        for (int i = 0; i < moves.Count - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j])
                {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
    }
}