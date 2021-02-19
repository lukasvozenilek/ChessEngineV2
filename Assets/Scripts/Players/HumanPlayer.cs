namespace Players
{
    public class HumanPlayer : Player
    {
        private ChessBoard chessBoardRef;
        public HumanPlayer(Board board, ChessBoard chessBoardRef) : base(board)
        {
            this.chessBoardRef = chessBoardRef;
        }

        public override void PlayMove()
        {
            if (moveGenerator.GetAllLegalMoves(board).Count == 0)
            {
                InvokeMoveComplete(null);
            }
            
            if (board.turn)
            {
                chessBoardRef.canMoveBlackPieces = true;
                chessBoardRef.canMoveWhitePieces = false;
            }
            else
            {
                chessBoardRef.canMoveWhitePieces = true;
                chessBoardRef.canMoveBlackPieces = false;
            }
        }
    }
}

