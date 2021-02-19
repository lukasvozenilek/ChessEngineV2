using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class Minimax : Player
{
    private int movesPruned;
    private int movesEvaluated;
    
    private const float worstEval = -10000;
    public const float bestEval = 10000;

    private int depth;

    public Minimax(Board board, int depth) : base(board)
    {
        this.depth = depth;
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


    private void RunMinimaxSearch()
    {
        Debug.Log("Starting Threaded Minimax Search!");
        moveEvaluation.Clear();
        movesPruned = 0;
        movesEvaluated = 0;
        List<Move> legalMoves = moveGenerator.GetAllLegalMoves(board);
        if (legalMoves.Count == 0)
        {
            Debug.Log("No legal moves found");
            InvokeMoveComplete(null);
            return;
        }

        OrderMoves(legalMoves);
        foreach (Move move in legalMoves.ToList())
        {
            board.MakeMove(move, false);
            float eval = -Search(depth-1, depth-1, worstEval, bestEval );
            moveEvaluation.Add(move, eval);
            board.UnmakeMove();
        }

        Debug.Log("Total moves evaluated: " + movesEvaluated);
        Debug.Log("Total moves pruned: " + movesPruned);
        
        Move bestMove = moveEvaluation.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        
        InvokeMoveComplete(board.MakeMove(bestMove));
    }

    private float Search(int depthlept, int startdepth, float alpha, float beta)
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
        OrderMoves(legalMoves);
        foreach (Move move in legalMoves.ToList())
        {
            i++;
            board.MakeMove(move, false);
            float result = -Search(depthlept - 1, startdepth, -beta, -alpha);
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
