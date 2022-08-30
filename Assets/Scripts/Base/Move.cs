namespace Chess
{
    using System;
    public struct Move {
        public int startIndex;
        public int targetIndex;
        public int pieceIndicator; // value of 1 for pawn etc, and - if black
        public PieceColour pieceColour;    // Math.Sign pieceIndicator to get if - or +
        public int capturedPiece;
        public bool isCastle;
        public bool enPassant;
        public int enPassantCapture;
        public bool pawnMoveTwo;

        public Move (int startIndex, int targetIndex, int piece){
            this.startIndex = startIndex;
            this.targetIndex = targetIndex;
            this.pieceIndicator = piece;
            switch (Math.Sign(piece))
            {
                case 1: this.pieceColour = PieceColour.White; break;
                case -1: this.pieceColour = PieceColour.Black; break;
                default: this.pieceColour = PieceColour.White; break;
            }
            this.capturedPiece = 0;

            if((piece == Piece.wPawn && targetIndex == startIndex+20) || (piece == Piece.bPawn && targetIndex == startIndex-20)){
                this.pawnMoveTwo = true;
            }else{
                this.pawnMoveTwo = false;
            }

            // !This might be improved to find if king is trying to castle
            if(this.pieceIndicator == Piece.wKing && (startIndex == 25 && (targetIndex == 23 || targetIndex == 27)) || this.pieceIndicator == Piece.bKing && (startIndex == 95 && (targetIndex == 93 || targetIndex == 97) )){
                this.isCastle = true;
            }else{
                this.isCastle = false;
            }
            this.enPassant = false;
            this.enPassantCapture = 0;
        }public Move (int startIndex, int targetIndex, int piece, bool isEnPassant, int enPassantCapture){
            this.startIndex = startIndex;
            this.targetIndex = targetIndex;
            this.pieceIndicator = piece;
            switch (Math.Sign(piece))
            {
                case 1: this.pieceColour = PieceColour.White; break;
                case -1: this.pieceColour = PieceColour.Black; break;
                default: this.pieceColour = PieceColour.White; break;
            }

            if(isEnPassant){
                this.enPassant = true;
                this.enPassantCapture = enPassantCapture;
            }else{
                this.enPassant = false;
                this.enPassantCapture = 0;
            }
            
            this.capturedPiece = Math.Sign(piece) * -1 * Piece.PAWN;

            this.pawnMoveTwo = false;
            this.isCastle = false;
        }

        public bool isWhiteTurn {
            get {
                return pieceColour == PieceColour.White;
            }
        }
    }
}