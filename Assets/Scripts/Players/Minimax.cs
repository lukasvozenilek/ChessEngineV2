﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

public class Minimax : Player
{
    public int alphaPrunes;
    public int betaPrunes;
    public int movesEvaluated;
    
    private const float worstEval = -10000;
    public const float bestEval = 10000;

    private int depth;

    public Evaluator evaluator;

    public Minimax(Board board, int depth) : base(board)
    {
        this.depth = depth;
        evaluator = new Evaluator(board);
    }


    public override void PlayMove()
    {
        Task.Factory.StartNew(() => RunMinimaxSearch(), TaskCreationOptions.LongRunning);
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


    public void RunMinimaxSearch()
    {
        Debug.Log("Starting Threaded Minimax Search!");
        moveEvaluation.Clear();
        alphaPrunes = 0;
        betaPrunes = 0;
        movesEvaluated = 0;
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(board));
        if (legalMoves.Count == 0)
        {
            Debug.Log("No legal moves found");
            InvokeMoveComplete(null);
            return;
        }
        
        OrderMoves(legalMoves);
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move, false);
            //float eval = -Search(depth-1, depth-1, worstEval, bestEval );
            float eval = -Search(depth-1, depth-1, worstEval, bestEval , true);
            moveEvaluation.Add(move, eval);
            board.UnmakeMove();
        }

        Debug.Log("Total moves evaluated: " + movesEvaluated);
        Debug.Log("Alpha prunes: " + alphaPrunes);
        Debug.Log("Beta prunes: " + betaPrunes);
        Move bestMove = moveEvaluation.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        
        InvokeMoveComplete(board.MakeMove(bestMove));
    }
    
    
    private float Search(int depthlept, int startdepth, float alpha, float beta, bool maximize)
    {
        if (depthlept == 0)
        {
            movesEvaluated += 1;
            return evaluator.EvaluateBoard();
        }
        
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(board));
        int totalMoves = legalMoves.Count;
        if (totalMoves == 0)
        {
            //If in check, this is checkmate
            if (moveGenerator.checkSquaresBB > 0)
            {
                //Value sooner checkmates as much worse for the attacked player
                return (1 + startdepth-depthlept) * (2 - Constants.depthLoss) * worstEval;
            }
            //Stalemate
            return 0;
        }

        int i = 0;
        OrderMoves(legalMoves);
        if (maximize)
        {
            float maxScore = worstEval;
            foreach (Move move in legalMoves)
            {
                i++;
                board.MakeMove(move);
                float score = Search(depthlept - 1, startdepth, alpha, beta, false);
                maxScore = Math.Max(score, maxScore);
                alpha = Math.Max(alpha, score);
                if (alpha >= beta)
                {
                    alphaPrunes += totalMoves - i;
                    board.UnmakeMove();
                    return beta;
                }
                board.UnmakeMove();
            }
            return (maxScore);
        }
        else
        {
            float minScore = bestEval;
            foreach (Move move in legalMoves)
            {
                i++;
                board.MakeMove(move);
                float score = Search(depthlept - 1, startdepth, alpha, beta, true);
                minScore = Math.Min(score, minScore);
                beta = Math.Min(beta, score);
                if (beta <= alpha)
                {
                    betaPrunes += totalMoves - i;
                    board.UnmakeMove();
                    return alpha;
                }
                board.UnmakeMove();
            }
            return (minScore);
        }
    }

    /*
    private float Search(int depthlept, int startdepth, float alpha, float beta)
    {
        if (depthlept == 0)
        {
            int perspective = board.turn ? -1 : 1;
            movesEvaluated += 1;
            return perspective * evaluator.EvaluateBoard();
        }
        List<Move> legalMoves = new List<Move>(moveGenerator.GetAllLegalMoves(board));
        int totalMoves = legalMoves.Count;
        if (totalMoves == 0)
        {
            //If in check, this is checkmate
            if (moveGenerator.checkSquaresBB > 0)
            {
                return worstEval;
            }
            //Stalemate
            return 0;
        }

        int i = 0;
        OrderMoves(legalMoves);
        
        
        foreach (Move move in legalMoves)
        {
            i++;
            board.MakeMove(move, false);
            float result = -Search(depthlept - 1, startdepth, -beta, -alpha);
            board.UnmakeMove();
            if (startdepth - depthlept % 2 == 0)
            {
                beta = Math.Min(beta, result);
                if (beta <= alpha)
                {
                    alphaPrunes += totalMoves - i;
                    return alpha;
                }
            }
            else
            {
                alpha = Math.Max(alpha, result);
                if (alpha >= beta)
                {
                    betaPrunes += totalMoves - i;
                    return beta;
                }
            }
        }
        return alpha;
    }
    */
}

