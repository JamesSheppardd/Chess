namespace Chess.Bot
{
    using Chess;
    using System;
    using System.Collections;

    public class BotController
    {
        PieceColour colour;
        PieceColour oppColour;
        Board board;
        Random rand;
        Move bestMove;

        public BotController(Board board, PieceColour colour){
            this.board = board;
            this.colour = colour;
            this.oppColour = colour == PieceColour.White ? PieceColour.Black : PieceColour.White;
            this.rand = new Random();
        }

        public Move? MakeRandomMove(Move[] validMoves){
            // just a quick test script to make a random move
            if(validMoves.Length > 0){
                int chosenMoveIndex = this.rand.Next(validMoves.Length);
                return validMoves[chosenMoveIndex];
            }
            return null;
        }

        public Move? MakeMinimaxMove(int depth){
            Move[] moves = PieceMoveData.GenerateAllValidMoves(colour, board);
            int bestEval = int.MinValue;
            int eval = int.MinValue;
            foreach (Move m in moves)
            {
                board.MakeMove(m, moves);
                eval = MinimaxSearch(depth, oppColour, false, int.MinValue, int.MaxValue);
                board.UndoMove();
                if(eval > bestEval){
                    bestEval = eval;
                    bestMove = m;
                }
            }

            DebugTools.PrintAnnouncement(bestEval);
            
            
            return bestMove;
        }

        public int MinimaxSearch(int depth, PieceColour maxColour, bool maxPlayer, int alpha, int beta){
            if(depth == 0){
                return Evaluation.Evaluate(board);
            }
            Move[] moves = PieceMoveData.GenerateAllValidMoves(maxColour, board);
            
            if(moves.Length == 0){
                // no possible moves
                if((board.gameFlag & 0b00000010) != 0){
                    // white checkmate
                    return int.MaxValue * (int)colour * -1;
                }
                else if((board.gameFlag & 0b00000100) != 0){
                    // black checkmate
                    return int.MaxValue * (int)colour;
                }
                
                // draw
                return 0;
            }


            if(maxPlayer){
                int bestEval = int.MinValue;
                foreach (Move m in moves)
                {
                    board.MakeMove(m, moves);
                    int eval = MinimaxSearch(depth - 1, oppColour, false, alpha, beta);
                    bestEval = Math.Max(eval, bestEval);
                    board.UndoMove();

                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha){
                        break;
                    }
                }
                return bestEval;
            }else{
                int minEval = int.MaxValue;
                foreach (Move m in moves)
                {
                    board.MakeMove(m, moves);
                    int eval = MinimaxSearch(depth - 1, colour, true, alpha, beta);
                    minEval = Math.Min(eval, minEval);
                    board.UndoMove();

                    beta = Math.Min(beta, eval);
                    if(beta <= alpha){
                        break;
                    }
                }
                return minEval;
            }


        }
    }
}