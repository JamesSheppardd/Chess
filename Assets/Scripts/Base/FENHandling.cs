namespace Chess{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FEN{
        public int[] board;
        public int ply;
        public PieceColour nextToMove;
        public bool[] castlingAbility;

        public FEN(int[] board, int ply, PieceColour nextToMove, bool[] castlingAbility){
            this.board = board;
            this.ply = ply;
            this.nextToMove = nextToMove;
            this.castlingAbility = castlingAbility;
        }
    }

    public static class FENHandling
    {
        public const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static FEN SetupWithFEN(string FEN){
            // example is "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 
            // first block is each rank, 
            // second - ie "w" - means white to move
            // third - castling ability
            // fourth - en passant target square
            // fifth - Halfmove Clock - specifies decimal number of half moves with respect to 50 move draw rule. Reset to 0 after capture or pawn move
            // sixth - Fullmove counter - ply / 2

            int[] board = new int[120];
            // fill with empties
            for(int i = 0; i < 120; i++){
                board[i] = Piece.BORDER;
            }
            string[] FENArray = FEN.Split();
            string[] ranks = FENArray[0].Split('/');
            string concat = string.Join("", ranks);
            char[] positions = concat.ToCharArray();
            int ind = 91;
            foreach (char c in positions)
            {
                if(char.IsDigit(c)){
                    // want to fill with 0
                    int temp = ind;
                    if(char.GetNumericValue(c) == 1){
                        board[ind] = 0;
                        if(ind % 10 == 8){
                            ind -= 17;
                        }else{
                            ind++;
                        }
                    }
                    else{
                        for (int i = temp; i < temp+char.GetNumericValue(c); i++)
                        {
                            board[i] = 0;
                            if(i % 10 == 8){
                                ind -= 17;
                            }else{
                                ind++;
                            }
                        }
                    }
                }
                else{
                    // get actual piece
                    board[ind] = PieceTranslations(c);
                    if(ind % 10 == 8){
                        ind -= 17;
                    }else{
                        ind++;
                    }
                }
            }
            int ply = GetPly(FENArray[1], int.Parse(FENArray[5]));
            PieceColour nextToMove = GetNextToMove(FENArray[1]);
            bool[] castlingAbility = GetCastle(FENArray[2]);
            return new FEN(board, ply, nextToMove, castlingAbility);
        }

        private static int GetPly(string SideToMove, int FullmoveCounter){
            // ply = even if white, odd if black to move
            // FMC starts at 1, when ply = 0
            if(SideToMove == "w"){
                return (FullmoveCounter-1) * 2;
            } else{
                return (FullmoveCounter-1) * 2 + 1;
            }
        }
        private static PieceColour GetNextToMove(string sideToMove){
            if(sideToMove != "w" && sideToMove != "b"){
                return PieceColour.Null;
            }
            return sideToMove == "w" ? PieceColour.White : PieceColour.Black;
        }

        private static bool[] GetCastle(string castling){
            bool[] result = new bool[4];
            foreach (char c in castling)
            {
                if(c == 'K'){
                    result[0] = true;
                }
                else if(c == 'Q'){
                    result[1] = true;
                }else if(c == 'k'){
                    result[2] = true;
                }else if(c == 'q'){
                    result[3] = true;
                }
            }
            return result;
        }

        private static int PieceTranslations(char FENChar){
            switch (FENChar){
                case 'P': return Piece.wPawn;
                case 'N': return Piece.wKnight;
                case 'B': return Piece.wBishop;
                case 'R': return Piece.wRook;
                case 'Q': return Piece.wQueen;
                case 'K': return Piece.wKing;

                case 'p': return Piece.bPawn;
                case 'n': return Piece.bKnight;
                case 'b': return Piece.bBishop;
                case 'r': return Piece.bRook;
                case 'q': return Piece.bQueen;
                case 'k': return Piece.bKing;

                default: return 0;
            }
        }
    }
}