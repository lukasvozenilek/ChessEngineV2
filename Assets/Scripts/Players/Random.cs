using System.Collections.Generic;
namespace Players
{
    public class Random : Player
    {
        public Random(Board board) : base(board)
        {
            
        }

        public override MoveResult? PlayMove()
        {
            List<Move> moves = moveGenerator.GetAllLegalMoves(board);
            if (moves.Count == 0) return null;
            return board.MakeMove(moves[UnityEngine.Random.Range(0, moves.Count - 1)]);
        }
    }
}