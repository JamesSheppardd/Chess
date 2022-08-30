namespace Chess
{
    using System;
    using System.Collections.Generic;
    public static class PieceMoveData {
        public static int[,] pieceOffsets = {
            {10, 9, 11, 20, 0, 0, 0, 0},          // Pawn 
            {-21, -19, -12, -8, 8, 12, 19, 21}, // Knight
            {-11, -9, 9, 11, 0, 0, 0, 0},       // Bishop
            {-10, -1, 1, 10, 0, 0, 0, 0},       // Rook
            {-11, -10, -9, -1, 1, 9, 10, 11},   // Queen
            {9, 10, 11, 1, -9, -10, -11, -1}    // King
        };

        public static Move[] GenerateAllValidMoves(PieceColour pieceColour, Board board){
            Move[] vm;
            List<Move> validMoves = new List<Move>();
            if(pieceColour == PieceColour.White){
                for (int i = 0; i < 64; i++)
                {
                    if(board.whitePieceTable.table[i] > 0){
                        vm = GenerateValidMoves(Positioning.GetIndexFromIndexAs64(i), board);
                        foreach (Move valid in vm)
                        {
                            validMoves.Add(valid);
                        }
                    }
                }
            }
            if(pieceColour == PieceColour.Black){
                for (int i = 0; i < 64; i++)
                {
                    if(board.blackPieceTable.table[i] < 0){
                        vm = GenerateValidMoves(Positioning.GetIndexFromIndexAs64(i), board);
                        foreach (Move valid in vm)
                        {
                            validMoves.Add(valid);
                        }
                    }
                }
            }

            return validMoves.ToArray();
        }

        public static Move[] GenerateValidMoves(int boardPosition, Board board){
            int piece = board.boardRep[boardPosition];
            int pieceAbs = Math.Abs(piece);
            int colour = Math.Sign(piece);
            int enemyColour = colour * -1;
            List<Move> results = new List<Move>();
            Move[] pseudoLegalMoves;
            // Generate the pseudo legal moves for this piece
            pseudoLegalMoves = GeneratePseudoLegalMoves(boardPosition, board);

            foreach (Move pseudoLegal in pseudoLegalMoves)
            {
                // if the move lands on a square that is of your colour
                if(Math.Sign(board.boardRep[pseudoLegal.targetIndex]) == (int)pseudoLegal.pieceColour){
                    continue;
                }

                // if the kings move goes into check, can't do it
                // want to make the move to see if the resulting move ends in check?
                board.MakeMove(pseudoLegal, pseudoLegalMoves);

                if(colour == 1){
                    // see if WHITE in check
                    if((board.gameFlag & 0b00000010) != 0){
                        // then white is in check
                        board.UndoMove();
                        continue;
                    }
                }
                else if(colour == -1){
                    // see if BLACK in check
                    if((board.gameFlag & 0b00000100) != 0){
                        // then black is in check
                        board.UndoMove();
                        continue;
                    }
                }
                board.UndoMove();

                // we want to look at the attack tables of the opponent
                // and see if we are in check. If so, king has to move.
                // Then, we want to generate pinned pieces and the lines
                // the opponents attack through them, so that we don't move them
                // to reveal a new check
                


                results.Add(pseudoLegal);
            }

            return results.ToArray();
        }

        public static Move[] GeneratePseudoLegalMoves(int boardPosition, Board board){
            int piece = board.boardRep[boardPosition];
            int pieceAbs = Math.Abs(piece);
            int colour = Math.Sign(piece);
            int enemyColour = colour * -1;
            switch (pieceAbs)
            {
                case Piece.PAWN:
                    return GeneratePawnMoves(boardPosition, colour, enemyColour, board);
                case Piece.KNIGHT:
                    return GenerateKnightMoves(boardPosition, colour, enemyColour, board);
                    
                case Piece.BISHOP:
                    return GenerateBishopMoves(boardPosition, colour, enemyColour, board);
                    
                case Piece.ROOK:
                    return GenerateRookMoves(boardPosition, colour, enemyColour, board);
                    
                case Piece.QUEEN:
                    return GenerateQueenMoves(boardPosition, colour, enemyColour, board);
                    
                case Piece.KING:
                    return GenerateKingMoves(boardPosition, colour, enemyColour, board);
                    

                default:
                    return new Move[]{};
            }
        }

        static Move[] GeneratePawnMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<int> moveableSquares = new List<int>{};
            List<Move> result = new List<Move>();
            if(IsPawnFirstMove(boardPosition, colour) && Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,3] * colour]) != enemyColour && Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,0] * colour]) != enemyColour && Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,0] * colour]) != colour ){
                // move 2 spaces on first turn
                moveableSquares.Add(boardPosition + pieceOffsets[0,3] * colour);
            }
            if(Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,0] * colour]) != enemyColour/*  && Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,0] * colour]) != colour */){
                // can only move forward when no enemy here
                moveableSquares.Add(boardPosition + pieceOffsets[0,0] * colour);
            }
            if(Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,1] * colour]) == enemyColour && !IsBorderSquareFromInt(boardPosition + pieceOffsets[0,1] * colour, board)){
                // add this to attackable squares (up and left)
                moveableSquares.Add(boardPosition + pieceOffsets[0,1] * colour);
            }
            if(Math.Sign(board.boardRep[boardPosition + pieceOffsets[0,2] * colour]) == enemyColour && !IsBorderSquareFromInt(boardPosition + pieceOffsets[0,2] * colour, board)){
                // add this to attackable squares (up and right)
                moveableSquares.Add(boardPosition + pieceOffsets[0,2] * colour);
            }

            // en passant
            if(board.moveHistory.Count > 0){
                Move lastMove = board.moveHistory.Peek();
                if(lastMove.pawnMoveTwo && (boardPosition - 1 == lastMove.targetIndex || boardPosition + 1 == lastMove.targetIndex)){
                    int target = lastMove.pieceColour == PieceColour.White ? lastMove.targetIndex - 10 : lastMove.targetIndex + 10;
                    result.Add(new Move(boardPosition, target, Piece.PAWN * colour, true, lastMove.targetIndex));
                }
            }

            foreach (int square in moveableSquares)
            {
                result.Add(new Move(boardPosition, square, Piece.PAWN * colour));

            }
            return result.ToArray();
            
        }

        static Move[] GenerateKnightMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<int> moveableSquares = new List<int>{};
            List<Move> result = new List<Move>();
            for(int i = 0; i <= 7; i++){
                int potential = boardPosition + pieceOffsets[1, i];
                if(/* Math.Sign(board.boardRep[potential]) != colour &&  */(!IsBorderSquareFromInt(potential, board))){    // if is a border square we ignore
                    moveableSquares.Add(potential);
                }
            }

            foreach (int square in moveableSquares)
            {
                result.Add(new Move(boardPosition, square, Piece.KNIGHT * colour));
            }
            return result.ToArray();
            
        }

        static Move[] GenerateBishopMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<Move> result = new List<Move>();
            for(int i = 0; i <= 3; i++){
                int potential = boardPosition + pieceOffsets[2, i];
                if(/* Math.Sign(board.boardRep[potential]) != colour && */ !IsBorderSquareFromInt(potential, board)){
                    // what we are doing is recursively going through until getting to a move we cannot make,
                    // passing in the next position instead of current one.
                    
                    // ! if it == pieceColour, then we stop there
                    if(Math.Sign(board.boardRep[potential]) == colour){
                        result.Add(new Move(boardPosition, potential, Piece.QUEEN * colour));
                        continue;
                    }
                    
                    if(Math.Sign(board.boardRep[potential]) != enemyColour){
                        // only do this until getting to other colour
                        Move[] recursiveResult = RecursiveSlidingMoves(boardPosition, potential, pieceOffsets[2,i], board, colour, enemyColour, Piece.BISHOP);
                        foreach (Move move in recursiveResult)
                        {
                            result.Add(new Move(boardPosition, move.targetIndex, Piece.BISHOP * colour));
                        }
                    }
                    result.Add(new Move(boardPosition, potential, Piece.BISHOP * colour));
                }
            }
            return result.ToArray();
        }
        static Move[] GenerateRookMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<Move> result = new List<Move>();
            for(int i = 0; i <= 3; i++){
                int potential = boardPosition + pieceOffsets[3, i];
                if(/* Math.Sign(board.boardRep[potential]) != colour &&  */!IsBorderSquareFromInt(potential, board)){
                    // what we are doing is recursively going through until getting to a move we cannot make,
                    // passing in the next position instead of current one.
                    
                    // ! if it == pieceColour, then we stop there
                    if(Math.Sign(board.boardRep[potential]) == colour){
                        result.Add(new Move(boardPosition, potential, Piece.QUEEN * colour));
                        continue;
                    }

                    if(Math.Sign(board.boardRep[potential]) != enemyColour){
                        // only do this until getting to other colour
                        Move[] recursiveResult = RecursiveSlidingMoves(boardPosition, potential, pieceOffsets[3,i], board, colour, enemyColour, Piece.ROOK);
                        foreach (Move move in recursiveResult)
                        {
                            result.Add(new Move(boardPosition, move.targetIndex, Piece.ROOK * colour));
                        }
                    }
                    result.Add(new Move(boardPosition, potential, Piece.ROOK * colour));
                }
            }
            return result.ToArray();
        }
        static Move[] GenerateQueenMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<Move> result = new List<Move>();
            for(int i = 0; i <= 7; i++){
                int potential = boardPosition + pieceOffsets[4, i];
                if(/* Math.Sign(board.boardRep[potential]) != colour &&  */!IsBorderSquareFromInt(potential, board)){
                    // what we are doing is recursively going through until getting to a move we cannot make,
                    // passing in the next position instead of current one.

                    // ! if it == pieceColour, then we stop there
                    if(Math.Sign(board.boardRep[potential]) == colour){
                        result.Add(new Move(boardPosition, potential, Piece.QUEEN * colour));
                        continue;
                    }

                    if(Math.Sign(board.boardRep[potential]) != enemyColour){
                        // only do this until getting to other colour
                        Move[] recursiveResult = RecursiveSlidingMoves(boardPosition, potential, pieceOffsets[4,i], board, colour, enemyColour, Piece.QUEEN);
                        foreach (Move move in recursiveResult)
                        {
                            result.Add(new Move(boardPosition, move.targetIndex, Piece.QUEEN * colour));
                        }
                    }
                    result.Add(new Move(boardPosition, potential, Piece.QUEEN * colour));
                }
            }
            return result.ToArray();
        }

        static Move[] GenerateKingMoves(int boardPosition, int colour, int enemyColour, Board board){
            List<int> moveableSquares = new List<int>{};
            List<Move> result = new List<Move>();

            for(int i = 0; i <= 7; i++){
                int potential = boardPosition + pieceOffsets[5, i];
                if(!IsBorderSquareFromInt(potential, board)){    // if is a border square we ignore
                    moveableSquares.Add(potential);
                }
            }
            // for castling
            switch (colour)
            {
                case 1:
                    if(boardPosition == 25 && Math.Sign(board.boardRep[26]) != 1 && /*((board.stateFlag & board.whiteKingsideCastleMask) != 0)*/ board.wKingside){
                        result.Add(new Move(boardPosition, 27, Piece.wKing));
                    }
                    if(boardPosition == 25 && Math.Sign(board.boardRep[24]) != 1 && Math.Sign(board.boardRep[22]) != 1 && /*((board.stateFlag & board.whiteQueensideCastleMask) != 0)*/ board.wQueenside){
                        result.Add(new Move(boardPosition, 23, Piece.wKing));
                    }
                    
                    break;
                
                case -1:
                    // for(int i = 0; i <= 7; i++){
                    //     int potential = boardPosition + pieceOffsets[5, i];
                    //     bool isAttacked = board.attackTables.whiteAttackTable[Positioning.GetIndexAs64FromIndex(potential)];
                    //     if(!isAttacked && !IsBorderSquareFromInt(potential, board)){    // if is a border square we ignore
                    //         moveableSquares.Add(potential);
                    //     }
                    // }
                    if(boardPosition == 95 && Math.Sign(board.boardRep[96]) != -1 && /*((board.stateFlag & board.blackKingsideCastleMask) != 0)*/ board.bKingside){
                        result.Add(new Move(boardPosition, 97, Piece.bKing));
                    }
                    if(boardPosition == 95 && Math.Sign(board.boardRep[94]) != -1 && Math.Sign(board.boardRep[92]) != -1 && /*((board.stateFlag & board.blackQueensideCastleMask) != 0)*/ board.bQueenside){
                        result.Add(new Move(boardPosition, 93, Piece.bKing));
                    }
                    
                    break;
                
            }

            foreach (int square in moveableSquares)
            {
                result.Add(new Move(boardPosition, square, Piece.KING * colour));
            }
            return result.ToArray();
            
        }

        static Move[] RecursiveSlidingMoves(int originalPosition, int lastPosition, int direction, Board board, int colour, int enemyColour, int piece){
            List<Move> result = new List<Move>();
            int potential = lastPosition + direction;
            if(/* Math.Sign(board.boardRep[potential]) != colour &&  */!IsBorderSquareFromInt(potential, board)){
                    // what we are doing is recursively going through until getting to a move we cannot make,
                    // passing in the next position instead of current one.

                    // ! if it == pieceColour, then we stop there
                    if(Math.Sign(board.boardRep[potential]) == colour){
                        result.Add(new Move(originalPosition, potential, Piece.QUEEN * colour));
                        return result.ToArray();
                    }

                    if(Math.Sign(board.boardRep[potential]) != enemyColour){
                        // only do this until getting to other colour
                        Move[] recursiveResult = RecursiveSlidingMoves(originalPosition, potential, direction, board, colour, enemyColour, piece);
                        foreach (Move move in recursiveResult)
                        {
                            result.Add(new Move(originalPosition, move.targetIndex, piece * colour));
                        }
                    }
                    result.Add(new Move(originalPosition, potential, piece * colour));
                }
            return result.ToArray();
        }

        
        static bool IsBorderSquareFromInt(int square, Board board){
            if(board.boardRep[square] == Piece.BORDER) { return true; }
            else{ return false; }
            
        }

        static int WhatPawnIsIt(int boardPosition){
            switch (boardPosition)
            {
                case 31: return 1;
                case 32: return 2;
                case 33: return 3;
                case 34: return 4;
                case 35: return 5;
                case 36: return 6;
                case 37: return 7;
                case 38: return 8;
                case 81: return -1;
                case 82: return -2;
                case 83: return -3;
                case 84: return -4;
                case 85: return -5;
                case 86: return -6;
                case 87: return -7;
                case 88: return -8;

                default: return 0;
            }
        }

        static bool IsPawnFirstMove(int boardPosition, int colour){
            int pawn = WhatPawnIsIt(boardPosition);
            if(pawn != 0 && Math.Sign(pawn) == colour){
                return true;
            }
            return false;
        }
    }
}
