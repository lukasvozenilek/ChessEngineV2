using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Minimax : Player
{
    private int movesPruned;
    private int movesEvaluated;
    
    private const int worstEval = -10000;
    public const int bestEval = 10000;

    private int depth;

    public Minimax(Board board, int depth) : base(board)
    {
        this.depth = depth;
    }


    public override MoveResult? GetMove()
    {
        movesPruned = 0;
        movesEvaluated = 0;
        List<Move> legalMoves = moveGenerator.GetAllLegalMoves(board);
        if (legalMoves.Count == 0)
        {
            return null;
        }

        Dictionary<Move, int> moveEvals = new Dictionary<Move, int>();
        foreach (Move move in legalMoves.ToList())
        {
            board.MakeMove(move, false);
            int eval = -Search(depth-1, depth-1, worstEval, bestEval );
            moveEvals.Add(move, eval);
            board.UnmakeMove();
        }

        foreach (KeyValuePair<Move, int> moveResult in moveEvals)
        {
            Debug.Log("Move " + Constants.MoveToString(moveResult.Key) + " had eval of " + moveResult.Value);
        }
        
        Debug.Log("Total moves evaluated: " + movesEvaluated);
        Debug.Log("Total moves pruned: " + movesPruned);
        
        Move bestMove = moveEvals.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        return board.MakeMove(bestMove);
    }

    private int Search(int depthlept, int startdepth, int alpha, int beta)
    {
        if (depthlept == 0)
        {
            int perspective = board.turn ? -1 : 1;
            movesEvaluated += 1;
            return perspective * Evaluation.EvaluateBoard(board);
        }
        List<Move> legalMoves = moveGenerator.GetAllLegalMoves(board);
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
        foreach (Move move in legalMoves.ToList())
        {
            i++;
            board.MakeMove(move, false);
            int result = -Search(depthlept - 1, startdepth, -beta, -alpha);
            board.UnmakeMove();
            if (startdepth - depthlept % 2 == 0)
            {
                if (result <= alpha)
                {
                    movesPruned += totalMoves - i;
                    return alpha;
                }
                beta = Math.Min(beta, result);
            }
            else
            {
                if (result >= beta)
                {
                    movesPruned += totalMoves - i;
                    return beta;
                }
                alpha = Math.Max(alpha, result);
            }
        }
        return alpha;
    }
}


/*
 //ABMax algorithm from From ChessEngine V1
 def LukasEngine(board, depth):
    global moves_checked
    starttime = time.time()
    # If white, pick a random first move from the list
    with console.status("[bold green]Analyzing moves...") as status:
        if board.fullmove_number == 1 and board.turn:
            move = board.parse_san(openingmoves[random.randrange(0, len(openingmoves), 1)])
        else:
            move = abmax(board, None, -10000, 10000, True, int(depth), None)[0]
    # log()(f'{moves_checked:,}' + " possible moves analysed!")
    # log()(f'{moves_checked / (time.time() - starttime):,}' " moves/second")
    moves_checked = 0
    return move
 
 def abmax(board, move, alpha, beta, maximize, depthleft, material):
    if material is None:
        material = evaluatematerial(board)

    # If on last depth return just the evaluation of the given move
    if depthleft == 0:
        score = evaluatemove(board, move, material)
        if maximize:
            score = -score
        return move, score

    pushed = False
    if move is not None:
        thismovescore = evaluatemove(board, move, material)
        board.push(move)
        pushed = True
    else:
        thismovescore = None

    if board.legal_moves.count() == 0:
        board.pop()
        if move is not None:
            if maximize:
                thismovescore = -thismovescore
            return move, thismovescore
        else:
            return ("", 0)

    if maximize:
        maxScore = -10000
        for move in presort(board, board.legal_moves):
            #print("\nNow analyzing next upper move ^" + str(move))
            #print("Starting AB: " + str(alpha) + " " + str(beta))
            score = depth_penalty * abmax(board, move, alpha, beta, False, depthleft - 1, material)[1]
            if (thismovescore is not None):
                score -= thismovescore
            if not pushed:
                #print("Results of upper move ^" + str(move) + " are " + str(score))
                console.log("Results of upper move ^" + str(move) + " are " + str(score))
            if score > maxScore:
                bestmove = move
            maxScore = max(score, maxScore)
            alpha = max(alpha, score)
            if alpha >= beta:
                break
        if pushed:
            board.pop()
        return (bestmove, maxScore)
    else:
        minScore = 10000
        for move in presort(board, board.legal_moves):
            score = depth_penalty * abmax(board, move, alpha, beta, True, depthleft - 1, material)[1]
            if (thismovescore is not None):
                score += thismovescore
            if score < minScore:
                worstMove = move
            minScore = min(score, minScore)
            beta = min(beta, score)
            if beta <= alpha:
                break
        if pushed:
            board.pop()
        return worstMove, minScore

 
 */
