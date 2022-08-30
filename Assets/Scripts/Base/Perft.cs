namespace Chess
{
    using System.Collections.Generic;
    public class Perft {
        public int[] results;
        public int depth;
        public Board board;
        public PieceColour startColour;
        public Perft(int depth, Board board, PieceColour startColour){
            this.results = new int[depth];
            this.depth = depth;
            this.board = board;
            this.startColour = startColour;

            for (int i = 1; i < depth+1; i++)
            {
            }
                DebugTools.PrintAnnouncement($"Depth {depth}: {PerftTest(depth, this.board, this.startColour)}");
        }

        public int PerftTest(int depth, Board board, PieceColour startColour){
            if (depth == 0){
                return 1;
            }

            Move[] moves = PieceMoveData.GenerateAllValidMoves(startColour, board);
            int numPositions = 0;

            foreach (Move move in moves)
            {
                board.MakeMove(move, moves);
                numPositions += PerftTest(depth - 1, board, startColour == PieceColour.White ? PieceColour.Black : PieceColour.White);
                board.UndoMove();
            }


            return numPositions;
        }
    }
}