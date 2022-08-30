namespace Chess
{
    using System;
    using System.Collections.Generic;
    public class PieceTable {
        
        public int[] table;
        public Board board;
        private PieceColour pieceColour;
        private int count = 0;
        public PieceTable(PieceColour pieceColour, Board board){
            this.table = new int[64];
            this.board = board;
            this.pieceColour = pieceColour;
            try{
                UpdateTable();
            }
            catch{
                DebugTools.PrintError($"Passed in invalid value for pieceColour: {pieceColour}");
            }
            
        }

        public void UpdateTable(){
            for (int i = 0; i < 64; i++)
            {
                int currPiece = board.boardRep[Positioning.GetIndexFromIndexAs64(i)];
                if(Math.Sign(currPiece) == (int)this.pieceColour){
                    // this means the found piece is of the colour we want
                    this.table[i] = currPiece;
                }else{
                    this.table[i] = 0;
                }
            }
        }

        public int[] GetPiecePositions(int piece){
            // find speccific positions of certain pieces. Note, piece passed in
            // is always going to be positive, so we need to remove sign from our 
            // search.
            List<int> positions = new List<int>();
            for (int i = 0; i < 64; i++)
            {
                if(Math.Abs(this.table[i]) == piece){
                    positions.Add(i);
                }
            }
            return positions.ToArray();
        }

        public int[] GetPieces(){
            List<int> result = new List<int>();
            for (int i = 0; i < 64; i++)
            {
                if(Math.Abs(this.table[i]) != Piece.empty){
                    result.Add(this.table[i]);
                }
            }
            return result.ToArray();
        }

        public int GetNumberOfPieces(int piece){
            int result = 0;
            for (int i = 0; i < 64; i++)
            {
                if(Math.Abs(this.table[i]) == piece){
                    result += 1;
                }
            }
            return result;
        }
    }
}