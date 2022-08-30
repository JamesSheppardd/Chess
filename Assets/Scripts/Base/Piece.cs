namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    public enum PieceColour { White=1, Black=-1, Null=0 };
    // public enum PieceType {
    //     Pawn, 
    //     Knight,
    //     Bishop,
    //     Rook,
    //     Queen,
    //     King
    // }
    // public class Piece
    // {
    //     internal PieceColour pieceColour;
    //     internal PieceType pieceType;
    //     internal bool selected;
    //     internal Stack<byte> validMoves; // hold all the possible valid moves for the specific piece

    //     public Piece(PieceType _type, PieceColour _colour){
    //         pieceColour = _colour;
    //         pieceType = _type;
    //         validMoves = new Stack<byte>();
    //     }
    // }

    public static class Piece {
        // *This gotta get a big rework
        public static int empty = 0;

        // pieces in general
        public const int BORDER = 64;
        public const int PAWN = 1;
        public const int KNIGHT = 2;
        public const int BISHOP = 3;
        public const int ROOK = 4;
        public const int QUEEN = 5;
        public const int KING = 6;
        // white pieces
        public static int wPawn = 1;
        public static int wKnight = 2;
        public static int wBishop = 3;
        public static int wRook = 4;
        public static int wQueen = 5;
        public static int wKing = 6;
        
        // black pieces
        public static int bPawn = -1;
        public static int bKnight = -2;
        public static int bBishop = -3;
        public static int bRook = -4;
        public static int bQueen = -5;
        public static int bKing = -6;
        
        public static string GetPieceFromValue(int pieceVal){
            switch (pieceVal)
            {
                case 1: return "wPawn";
                case 2: return "wKnight";
                case 3: return "wBishop";
                case 4: return "wRook";
                case 5: return "wQueen";
                case 6: return "wKing";
                
                case -1: return "bPawn";
                case -2: return "bKnight";
                case -3: return "bBishop";
                case -4: return "bRook";
                case -5: return "bQueen";
                case -6: return "bKing";

                default: return "No piece";
            }
        }
    }
}