namespace Chess
{
    public static class Evaluation {
        
        const int pawnValue = 100;
        const int knightValue = 300;
        const int bishopValue = 300;
        const int rookValue = 400;
        const int queenValue = 900;

        const int checkValue = 800;
        const int mateValue = queenValue * 16;

        public static int Evaluate(Board board){
            int whiteEval = CountPieces(PieceColour.White, board);
            int blackEval = CountPieces(PieceColour.Black, board);

            whiteEval += PieceSquareListEvaluations(PieceColour.White, board);
            whiteEval += PieceSquareListEvaluations(PieceColour.Black, board);

            int eval = whiteEval - blackEval;
            
            int perspective = board.nextToMove == PieceColour.White ? 1 : -1;
            return eval * perspective;
        }

        static int CountPieces(PieceColour colour, Board board){
            int pieces = 0;
            switch (colour)
            {
                case PieceColour.White:
                    pieces += board.whitePieceTable.GetNumberOfPieces(1) * pawnValue;
                    pieces += board.whitePieceTable.GetNumberOfPieces(2) * knightValue;
                    pieces += board.whitePieceTable.GetNumberOfPieces(3) * bishopValue;
                    pieces += board.whitePieceTable.GetNumberOfPieces(4) * rookValue;
                    pieces += board.whitePieceTable.GetNumberOfPieces(5) * queenValue;
                    break;
                case PieceColour.Black:
                    pieces += board.blackPieceTable.GetNumberOfPieces(1) * pawnValue;
                    pieces += board.blackPieceTable.GetNumberOfPieces(2) * knightValue;
                    pieces += board.blackPieceTable.GetNumberOfPieces(3) * bishopValue;
                    pieces += board.blackPieceTable.GetNumberOfPieces(4) * rookValue;
                    pieces += board.blackPieceTable.GetNumberOfPieces(5) * queenValue;
                    break;
                
            }

            return pieces;
        }

        static int CheckmateEvaluation(PieceColour colour, Board board){
            int result = 0;
            switch (colour)
            {
                case PieceColour.White:
                    if((board.gameFlag & 0b00000100) != 0){
                        Move[] moves = PieceMoveData.GenerateAllValidMoves(PieceColour.Black, board);
                        result += board.InCheckmate(moves) ? mateValue : 0;
                    }
                    break;
                case PieceColour.Black: 
                    if((board.gameFlag & 0b00000010) != 0){
                        Move[] moves = PieceMoveData.GenerateAllValidMoves(PieceColour.White, board);
                        result += board.InCheckmate(moves) ? mateValue : 0;
                    }
                    break;
                
                
            }
            //DebugTools.Print(result);
            return result;
        }

        static int PieceSquareListEvaluations(PieceColour colour, Board board){
            int value = 0;

            switch (colour)
            {
                case PieceColour.White:
                    value += PieceSquareListEvaluation( PawnPST,   board.whitePieceTable.GetPiecePositions(1), true) * (int)colour;
                    value += PieceSquareListEvaluation( KnightPST, board.whitePieceTable.GetPiecePositions(2), true) * (int)colour;
                    value += PieceSquareListEvaluation( BishopPST, board.whitePieceTable.GetPiecePositions(3), true) * (int)colour;
                    value += PieceSquareListEvaluation( RookPST,   board.whitePieceTable.GetPiecePositions(4), true) * (int)colour;
                    value += PieceSquareListEvaluation( QueenPST,  board.whitePieceTable.GetPiecePositions(5), true) * (int)colour;
                    value += PieceSquareListEvaluation( KingPST,   board.whitePieceTable.GetPiecePositions(6), true) * (int)colour; // for king can change in future as he only has 1 pos
                    break;
                case PieceColour.Black:
                    value += PieceSquareListEvaluation( PawnPST,   board.blackPieceTable.GetPiecePositions(1), false) * (int)colour;
                    value += PieceSquareListEvaluation( KnightPST, board.blackPieceTable.GetPiecePositions(2), false) * (int)colour;
                    value += PieceSquareListEvaluation( BishopPST, board.blackPieceTable.GetPiecePositions(3), false) * (int)colour;
                    value += PieceSquareListEvaluation( RookPST,   board.blackPieceTable.GetPiecePositions(4), false) * (int)colour;
                    value += PieceSquareListEvaluation( QueenPST,  board.blackPieceTable.GetPiecePositions(5), false) * (int)colour;
                    value += PieceSquareListEvaluation( KingPST,   board.blackPieceTable.GetPiecePositions(6), false) * (int)colour;
                    break;
                
            }

            return value;
        }

        static int PieceSquareListEvaluation(int[] pst, int[] piecePositions, bool white){
            int value = 0;

            for (int i = 0; i < piecePositions.Length; i++)
            {
                //DebugTools.PrintAnnouncement(piecePositions[i]);
                // piecePositions is as 64
                value += ReadPST(pst, piecePositions[i], white);
            }
            
            return value;
        }
        static int ReadPST(int[] pst, int pos, bool white){
            int value = 0;
            // as PST are other way around, we want to reverse what we have, only if we are white though,
            // as black tables are reverse of the white ones, which I have manually done down below
            if(white){
                int index = Positioning.GetIndexFromIndexAs64(pos);
                int rank = Positioning.GetRankFromIndex(index);
                rank = 7 - rank;
                int file = Positioning.GetFileFromIndex(index);
                pos = Positioning.GetIndexFromGamePosition( new int[]{file, rank} );
                pos = Positioning.GetIndexAs64FromIndex(pos);
            }
            value += pst[pos];
            
            return value;
        }

        // Piece Square tables - obtained values from https://www.chessprogramming.org/Simplified_Evaluation_Function#Piece-Square_Tables

        // Pawns - want to encourage them to move up the board
        static int[] PawnPST = {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        // Knights - want to encourage them to go towards the centre of the board - "one piece stands badly, the whole game stands badly" (Tartakover, according to the wiki linked)
        static int[] KnightPST = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        // Bishops - avoid corners and borders, and prefer central squares
        static int[] BishopPST = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        // Rooks - get in centre, occupy 7th rank
        static int[] RookPST = {
             0,  0,  0,  0,  0,  0,  0,  0,
             5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
             0,  0,  0,  5,  5,  0,  0,  0
        };
        
        // Queen - get slightly in centre
        static int[] QueenPST = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        // King - hide behind pawns like a little coward in midgame
        static int[] KingPST = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        // King endgame - avoid corners to avoid checkmate, and get to centre
        static int[] KingEndgamePST = {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };

        

        
        
    }
}