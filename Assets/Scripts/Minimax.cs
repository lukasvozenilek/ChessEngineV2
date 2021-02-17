using System;
using System.Collections.Generic;

public class Minimax
{
    private Board board;
    private MoveGenerator moveGenerator;
    public Minimax(Board board)
    {
        this.board = board;
    }

    public MoveResult? PlayNextMove()
    {
        List<Move> legalMoves = moveGenerator.GetAllLegalMoves(board);
        if (legalMoves.Count == 0)
        {
            return null;
        }
        return board.MakeMove(legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)]);
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
