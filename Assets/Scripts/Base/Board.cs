namespace Chess
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    public class Board {
        // board is stored as a list of 120 indexs, where playable 
        // area is 21, 22, 23, 24, 25, 26, 27, 28, 31, 32, 33, 34, 35, 26, 37, 38, 41, ... up to 98
        public int[] boardRep;
        public PieceColour nextToMove;
        public int selectedSquare; // setting to -1 if nothing, then using board[] system to determine selected square 
        public Stack<Move> moveHistory;
        public bool whiteCheck = false;
        public bool blackCheck = false;
        public int[] movedPawns = new int[16]; // pawn on a2 is value 1, pawn on a7 is value -1
        public int lastMoveEndIndex;    // index of where the last move ended - //? might get deprecated at some point so watch out
        public int ply; // even value it is white turn, odd it is black turn
        public int whiteKingPos, blackKingPos;
        public AttackTables attackTables;

        // Piece Tables to store each all pieces of a certain colour
        public PieceTable whitePieceTable;
        public PieceTable blackPieceTable;

        // Flag
        public uint promotionFlag = 0b0000; // from left to write: promote to queen
        public uint gameFlag = 0b00000000; // from left to write: checkmate - white wins, checkmate - black wins, draw, blank, blank, black in check, white in check, blacnk
        public uint stateFlag = 0b00000000000000000000000000001111; // bit 0 to 3: castle ability (0 is white king side, 1 is white queenside etc), bit 4 to 7: file of en passant square startiung at 1, so if 0 then no en passant, bit 14 to end: halfmove counter for 50 move rule

        public Stack<uint> stateHistory;
        public int fiftyMoveRule = 0;

        public bool wKingside, wQueenside, bKingside, bQueenside;
        // ! THis is a really crude way of doing this
        public Stack<bool> wKingsideHistory, wQueensideHistory, bKingsideHistory, bQueensideHistory;

        public FEN fen;

        public Board(string FEN = FENHandling.startingFEN){
            moveHistory = new Stack<Move>();
            stateHistory = new Stack<uint>();
            boardRep = GenerateStartingBoard(FEN);
            stateHistory.Push(stateFlag);

            wKingsideHistory = new Stack<bool>();
            wQueensideHistory = new Stack<bool>();
            bKingsideHistory = new Stack<bool>();
            bQueensideHistory = new Stack<bool>();

            wKingsideHistory.Push(wKingside);
            wQueensideHistory.Push(wQueenside);
            bKingsideHistory.Push(bKingside);
            bQueensideHistory.Push(bQueenside);

        }

        public int[] GenerateStartingBoard(string FEN){ // NB in future take in FEN string to streamline this process
            boardRep = new int[120];
            fen = FENHandling.SetupWithFEN(FEN);
            boardRep = fen.board;
            selectedSquare = -1;
            lastMoveEndIndex = -1;
            ply = fen.ply;
            nextToMove = fen.nextToMove;

            wKingside = fen.castlingAbility[0];
            wQueenside = fen.castlingAbility[1];
            bKingside = fen.castlingAbility[2];
            bQueenside = fen.castlingAbility[3];

            // setup piece tables
            GeneratePieceTables();
            DebugTools.PrintIntArray(whitePieceTable.table);

            attackTables = new AttackTables(this);
            attackTables.UpdateAttackTable(PieceColour.White);
            attackTables.UpdateAttackTable(PieceColour.Black);
            
            return boardRep;
        }

        private void GeneratePieceTables(){
            DebugTools.PrintAnnouncement("Generating Piece Tables");
            whitePieceTable = new PieceTable(PieceColour.White, this);
            blackPieceTable = new PieceTable(PieceColour.Black, this);
            // find kings 
            FindKings();
        }

        private void FindKings(){
            
            whiteKingPos = whitePieceTable.GetPiecePositions(6)[0];
            blackKingPos = blackPieceTable.GetPiecePositions(6)[0];
        }

        // TODO: Split this into another class somewhere me thinks
        public bool IsItTurn(Move move, Players players){
            if((move.isWhiteTurn && ply % 2 == 0 && (players == Players.HumanVsHuman || players == Players.HumanVsBot)) || (!move.isWhiteTurn && ply % 2 == 1 && (players == Players.HumanVsHuman || players == Players.BotVsHuman))){
                // then it is the current person's turn
                return true;
            } 
            return false;
        }

        public void InCheck(){
            if(attackTables.blackAttackTable[whiteKingPos]){ 
                // update flag
                gameFlag |= 1 << 1;
                //Checkmate(PieceColour.Black);
            }else{
                // remove check
                if((gameFlag & 0b00000010) != 0){
                    // if flag set, reset it
                    gameFlag = gameFlag ^ 0b00000010;
                }
            }
            if(attackTables.whiteAttackTable[blackKingPos]){ 
                // black in check
                gameFlag |= 1 << 2;
                //Checkmate(PieceColour.White);
            }
            else{
                // remove check
                if((gameFlag & 0b00000100) != 0){
                    // if flag set, reset it
                    gameFlag = gameFlag ^ 0b00000100;
                }
            }
        }

        public void UpdateNextToMove(){
            nextToMove = nextToMove == PieceColour.White ? PieceColour.Black : PieceColour.White;
        }

        public bool InCheckmate(Move[] validMoves){
            if(validMoves.Length <= 0 && ((gameFlag & 0b00000010) != 0 || (gameFlag & 0b00000100) != 0)){
                return true;
            }
            return false;
        }
        public bool ColourInCheckmate(Move[] validMoves, PieceColour colour){
            switch (colour)
            {
                case PieceColour.White:
                    if(validMoves.Length <= 0 && (gameFlag & 0b00000010) != 0){
                        return true;
                    }
                    break;
                case PieceColour.Black:
                    if(validMoves.Length <= 0 && (gameFlag & 0b00000100) != 0){
                        return true;
                    }
                    break;
                    
            }
            if(validMoves.Length <= 0 && ((gameFlag & 0b00000010) != 0 || (gameFlag & 0b00000100) != 0)){
                return true;
            }
            return false;
        }
        
        public void Checkmate(){
            // Check for checkmate
            Move move = moveHistory.Peek();
            
            // don't use next to move as we don't want to change that variable
            PieceColour nextColour = move.pieceColour == PieceColour.White ? PieceColour.Black : PieceColour.White;
            Move[] nextValidMoves = PieceMoveData.GenerateAllValidMoves(nextColour, this);
            if(InCheckmate(nextValidMoves)){
                DebugTools.PrintAnnouncement("Checkmate");
                if(move.pieceColour == PieceColour.White){
                    gameFlag |= 1 << 6;
                } else if(move.pieceColour == PieceColour.Black){
                    gameFlag |= 1 << 7;
                }
            }
        }

        public bool InStalemate(Move[] validMoves){
            if(validMoves.Length <= 0 && ((gameFlag & 0b00000010) == 0 && (gameFlag & 00000100) == 0)){
                return true;
            }
            return false;
        }

        public void Stalemate(){
            Move move = moveHistory.Peek();
            
            // don't use next to move as we don't want to change that variable
            PieceColour nextColour = move.pieceColour == PieceColour.White ? PieceColour.Black : PieceColour.White;
            if(InStalemate(PieceMoveData.GenerateAllValidMoves(nextColour, this))){
                DebugTools.PrintAnnouncement("Stalemate");
                gameFlag |= 1 << 5;
            }
        }

        public void InsufficientMaterialDraw(){
            int[] whitePieces = whitePieceTable.GetPieces();
            int[] blackPieces = blackPieceTable.GetPieces();
            // * 2 Kings
            if(whitePieces.Length == 1 && blackPieces.Length == 1){
                DebugTools.PrintAnnouncement("Insufficient material - 2 Kings");
                gameFlag |= 1 << 5;
            }
            // * King vs king & minor (Knight or Bishop)
            else if(whitePieces.Length == 1 && blackPieces.Length == 2 && (blackPieces.Contains(-2) || blackPieces.Contains(-3))){
                DebugTools.PrintAnnouncement("Insufficient material - King vs King and minor piece");
                gameFlag |= 1 << 5;
            }
            else if(blackPieces.Length == 1 && whitePieces.Length == 2 && (whitePieces.Contains(2) || whitePieces.Contains(3))){
                DebugTools.PrintAnnouncement("Insufficient material - King vs King and minor piece");
                gameFlag |= 1 << 5;
            }
            // ? Lone king vs all pieces - only works if player with all pieces runs out of time - aka timeout vs insufficient material
            // else if(whitePieces.Length == 1 && blackPieces.Length == 16){
            //     DebugTools.PrintAnnouncement("Insufficient material - Lone king vs all pieces");
            //     gameFlag |= 1 << 5;
            // }else if(blackPieces.Length == 1 && whitePieces.Length == 16){
            //     DebugTools.PrintAnnouncement("Insufficient material - Lone king vs all pieces");
            //     gameFlag |= 1 << 5;
            // }
            // * King vs 2 knights & King
            else if(whitePieces.Length == 1 && blackPieces.Length == 3 && blackPieces.Contains(-2)){
                int knightCount = 0;
                foreach (int p in blackPieces)
                {
                    if(p == -2){
                        knightCount += 1;
                    }
                }
                if(knightCount == 2){
                    DebugTools.PrintAnnouncement("Insufficient material - King vs King and 2 knights");
                    gameFlag |= 1 << 5;
                }
            }else if(blackPieces.Length == 1 && whitePieces.Length == 3 && whitePieces.Contains(2)){
                int knightCount = 0;
                foreach (int p in whitePieces)
                {
                    if(p == 2){
                        knightCount += 1;
                    }
                }
                if(knightCount == 2){
                    DebugTools.PrintAnnouncement("Insufficient material - King vs King and 2 knights");
                    gameFlag |= 1 << 5;
                }
            }
            // * king & minor vs king & minor
            else if(blackPieces.Length == 2 && (blackPieces.Contains(-2) || blackPieces.Contains(-3)) && whitePieces.Length == 2  && (whitePieces.Contains(2) || whitePieces.Contains(3))){ 
                DebugTools.PrintAnnouncement("Insufficient material - King and minor vs King and minor");
                gameFlag |= 1 << 5;
            }

            
        }

        public void FiftyMoveDraw(Move move, bool isUndo=false){
            // increment counter if no pawn moved or no piece taken
            // 50 whole moves, not 50 ply
            if(Math.Abs(move.pieceIndicator) != Piece.PAWN && boardRep[move.targetIndex] == Piece.empty && !isUndo){
                fiftyMoveRule += 1;
                //DebugTools.Print($"50 move-rule: {fiftyMoveRule}.");
            }
            if(isUndo && Math.Abs(move.pieceIndicator) != Piece.PAWN && boardRep[move.targetIndex] == Piece.empty){
                fiftyMoveRule -= 1;
            }

            if(Math.Abs(move.pieceIndicator) == Piece.PAWN || boardRep[move.targetIndex] != Piece.empty){
                fiftyMoveRule = 0;
            }
            

            if(fiftyMoveRule >= 100){
                DebugTools.PrintAnnouncement("Draw - 50 move-rule fufilled");
                gameFlag |= 1 << 5;
            }
        }


        public int MakeMove(Move move, Move[] validMoves){
            if(Array.IndexOf(validMoves, move) > -1){  // means that the move is in validMoves
                if(Math.Sign(boardRep[move.targetIndex]) != (int)move.pieceColour){
                    
                    // set captured piece in move
                    if(!move.enPassant){
                        move.capturedPiece = boardRep[move.targetIndex];
                    }

                    // 50 move rule
                    FiftyMoveDraw(move);

                    // if at any point i move a rook or king, then castling legality changes
                    if(move.pieceIndicator == Piece.wKing && ( wKingside || wQueenside ) ){
                        // remove both castling
                        wKingside = false;
                        wQueenside = false;
                    }else if(move.pieceIndicator == Piece.bKing && ( bKingside || bQueenside ) ){
                        // remove both castling
                        bKingside = false;
                        bQueenside = false;
                    }
                    else if(move.pieceIndicator == Piece.wRook){
                        if(move.startIndex == 28){
                            wKingside = false;
                        }
                        else if(move.startIndex == 21){
                            wQueenside = false;
                        }
                    }
                    else if(move.pieceIndicator == Piece.bRook){
                        if(move.startIndex == 98){
                            bKingside = false;
                        }
                        else if(move.startIndex == 91){
                            bQueenside = false;
                        }
                    }

                    // See if castling
                    if(move.isCastle){
                        switch (move.pieceColour)
                        {
                            case PieceColour.White:
                                if(move.targetIndex == 27){
                                    // kingside
                                    wKingside = false;
                                    boardRep[26] = Piece.wRook;
                                    boardRep[28] = Piece.empty;
                                }
                                else if(move.targetIndex == 23){
                                    wQueenside = false;
                                    boardRep[24] = Piece.wRook;
                                    boardRep[21] = Piece.empty;
                                }
                                break;
                            
                            case PieceColour.Black:
                                if(move.targetIndex == 97){
                                    // kingside
                                    bKingside = false;
                                    boardRep[96] = Piece.bRook;
                                    boardRep[98] = Piece.empty;
                                }
                                else if(move.targetIndex == 93){
                                    bQueenside = false;
                                    boardRep[94] = Piece.bRook;
                                    boardRep[91] = Piece.empty;
                                }
                                break;
                            
                        }
                    }
                    // ?Possibly cringe vvvvvvvvvvvvv
                    wKingsideHistory.Push(wKingside);
                    wQueensideHistory.Push(wQueenside);
                    bKingsideHistory.Push(bKingside);
                    bQueensideHistory.Push(bQueenside);
                    //?^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                    // en passant
                    if(move.enPassant){
                        boardRep[move.enPassantCapture] = Piece.empty;
                    }

                    boardRep[move.targetIndex] = boardRep[move.startIndex]; // fill the new position with the piece
                    boardRep[move.startIndex] = Piece.empty;   // empty start position 

                    
                    // Update the moveHistory by pushing to stack
                    lastMoveEndIndex = moveHistory.Count > 0 ? moveHistory.Peek().targetIndex : 0;
                    moveHistory.Push(move);
                    ply++;
                    
                    // handle promotion
                    if(Math.Abs(move.pieceIndicator) == 1 && (move.targetIndex / 10 == 9 || move.targetIndex / 10 == 2)){
                        //DebugTools.PrintAnnouncement("Promotion");
                        PromoteToQueen(move);
                    }
                    // Update piece tables to show new representation after move
                    whitePieceTable.UpdateTable();
                    blackPieceTable.UpdateTable();

                    // handle moving Kings
                    FindKings();
                    
                    // Update both attack tables, as after one move both can be influenced,
                    // ie a white move being taken after a black piece moved
                    attackTables.UpdateAttackTable(PieceColour.White);
                    attackTables.UpdateAttackTable(PieceColour.Black);

                    // Are we in check after attack and piece tables updated??
                    InCheck();

                    stateHistory.Push(stateFlag);

                    return 1;
                }
            }
            for (int i = 0; i < validMoves.Length; i++)
            {
                if(validMoves[i].enPassant){
                    DebugTools.Print($"Valid En Passant: {validMoves[i].targetIndex}");
                    DebugTools.Print($"{validMoves[i].capturedPiece}, {validMoves[i].enPassant}, {validMoves[i].enPassantCapture}, {validMoves[i].isCastle}, {validMoves[i].pawnMoveTwo}, {validMoves[i].pieceColour}, {validMoves[i].pieceIndicator}, {validMoves[i].startIndex}, {validMoves[i].targetIndex}");
                    DebugTools.Print($"{move.capturedPiece}, {move.enPassant}, {move.enPassantCapture}, {move.isCastle}, {move.pawnMoveTwo}, {move.pieceColour}, {move.pieceIndicator}, {move.startIndex}, {move.targetIndex}");
                    DebugTools.Print(validMoves.Contains(move));
                }
            }
            DebugTools.PrintError($"No move: {move.pieceIndicator} from {move.startIndex} to {move.targetIndex}");
            DebugTools.Print($"{validMoves[0].capturedPiece}, {validMoves[0].enPassant}, {validMoves[0].enPassantCapture}, {validMoves[0].isCastle}, {validMoves[0].pawnMoveTwo}, {validMoves[0].pieceColour}, {validMoves[0].pieceIndicator}, {validMoves[0].startIndex}, {validMoves[0].targetIndex}");
            //DebugTools.Print($"{move.capturedPiece}, {move.enPassant}, {move.enPassantCapture}, {move.isCastle}, {move.pawnMoveTwo}, {move.pieceColour}, {move.pieceIndicator}, {move.startIndex}, {move.targetIndex}");
            // ? if set selctedSquare to -1, when dragging piece over own piece and letting go it freezes
            //selectedSquare = -1;
            return -1;

        }

        public void UndoMove(bool changedNextToMove=false){
            Move move = moveHistory.Peek();
            if(!move.enPassant){
                boardRep[move.targetIndex] = move.capturedPiece; // fill the new position with the piece
            }
            boardRep[move.startIndex] = move.pieceIndicator;   // empty start position 
            moveHistory.Pop();

            FiftyMoveDraw(move, true);
            // see if promotion flag set, if so undo it
            if((promotionFlag & 0b0001) == 1){
                QueenPromotionFlagFlip();
            }

            if(move.pieceIndicator == Piece.wKing){
                if( wKingsideHistory.Skip(1).First() && !wKingsideHistory.Peek() ){
                    // last move was first king move
                    wKingside = true;
                }
                if( wQueensideHistory.Skip(1).First() && !wQueensideHistory.Peek() ){
                    // last move was first king move
                    wQueenside = true;
                }
                
            }
            else if(move.pieceIndicator == Piece.bKing){
                if( bKingsideHistory.Skip(1).First() && !bKingsideHistory.Peek() ){
                    // last move was first king move
                    bKingside = true;
                }
                if( bQueensideHistory.Skip(1).First() && !bQueensideHistory.Peek() ){
                    // last move was first king move
                    bQueenside = true;
                }
            }
            else if(move.pieceIndicator == Piece.wRook){
                if( wKingsideHistory.Skip(1).First() && !wKingsideHistory.Peek() ){
                    // last move was first king move
                    wKingside = true;
                }
                if( wQueensideHistory.Skip(1).First() && !wQueensideHistory.Peek() ){
                    // last move was first king move
                    wQueenside = true;
                }
            }
            else if(move.pieceIndicator == Piece.bRook){
                if( bKingsideHistory.Skip(1).First() && !bKingsideHistory.Peek() ){
                    // last move was first king move
                    bKingside = true;
                }
                if( bQueensideHistory.Skip(1).First() && !bQueensideHistory.Peek() ){
                    // last move was first king move
                    bQueenside = true;
                }
            }
            wKingsideHistory.Pop();
            wQueensideHistory.Pop();
            bKingsideHistory.Pop();
            bQueensideHistory.Pop();

            // see if castle
            if(move.isCastle){
                switch (move.pieceColour)
                {
                    case PieceColour.White:
                        if(move.targetIndex == 27){
                            // kingside
                            //stateFlag ^= 1;
                            wKingside = true;
                            boardRep[26] = Piece.empty;
                            boardRep[28] = Piece.wRook;
                        }
                        else if(move.targetIndex == 23){
                            //stateFlag ^= (1 << 1);
                            wQueenside = true; 
                            boardRep[24] = Piece.empty;
                            boardRep[21] = Piece.wRook;
                        }
                        break;
                    
                    case PieceColour.Black:
                        if(move.targetIndex == 97){
                            // kingside
                            //stateFlag ^= (1 << 2);
                            bKingside = true; 
                            boardRep[96] = Piece.empty;
                            boardRep[98] = Piece.bRook;
                        }
                        else if(move.targetIndex == 93){
                            //stateFlag ^= (1 << 3);
                            bQueenside = true; 
                            boardRep[94] = Piece.empty;
                            boardRep[91] = Piece.bRook;
                        }
                        break;
                }
            }

            // enPassant
            if(move.enPassant){
                boardRep[move.enPassantCapture] = move.capturedPiece;
                boardRep[move.targetIndex] = Piece.empty;
            }

            // Update piece tables to show new representation after move
            whitePieceTable.UpdateTable();
            blackPieceTable.UpdateTable();

            // handle moving Kings
            FindKings();
            
            // Update both attack tables, as after one move both can be influenced,
            // ie a white move being taken after a black piece moved
            attackTables.UpdateAttackTable(PieceColour.White);
            attackTables.UpdateAttackTable(PieceColour.Black);

            if(changedNextToMove){
                // Undo next to move
                UpdateNextToMove();
            }

            InCheck();


            ply--;
        }

        public void PromoteToQueen(Move move){
            // promote to queen
            boardRep[move.targetIndex] = (int)move.pieceColour * Piece.QUEEN;
            // turn on bit flag for promotion
            QueenPromotionFlagFlip();
        }

        #region Flags

        public void QueenPromotionFlagFlip(){
            // bitwise to flip queen promotion flag bit
            promotionFlag = promotionFlag ^ 0b0001;
        }
        public void WhiteCheckFlagFlip(){
            // bitwise to flip queen promotion flag bit
            gameFlag = gameFlag ^ 0b00000010;
        }
        public void BlackCheckFlagFlip(){
            // bitwise to flip queen promotion flag bit
            gameFlag = gameFlag ^ 0b00000100;
        }
        

        #endregion
    }

}