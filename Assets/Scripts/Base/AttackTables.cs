namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class AttackTables {
        public bool[] whiteAttackTable;
        public bool[] blackAttackTable;
        public Board board;
        public AttackTables(Board board){
            this.whiteAttackTable = new bool[64];
            this.blackAttackTable = new bool[64];
            this.board = board;
        }

        public void UpdateAttackTable(PieceColour colour){
            // to update we want to loop over all possible moves by every piece of
            // the desired colour, then set their target square to "true" on the 
            // attack table to show that square is attacked
            switch (colour)
            {
                case PieceColour.White:
                    List<int> wTargs = new List<int>();
                    for (int i = 0; i < 64; i++)
                    {
                        if(this.board.whitePieceTable.table[i] != 0){
                            // then we have a piece and should generate all moves for it
                            Move[] moves = PieceMoveData.GeneratePseudoLegalMoves(Positioning.GetIndexFromIndexAs64(i), this.board);
                            for (int m = 0; m < moves.Length; m++)
                            {
                                wTargs.Add(Positioning.GetIndexAs64FromIndex(moves[m].targetIndex));
                            }
                        }
                    }
                    // clear last attack table 
                    // !probably relatively slow to other approaches
                    for (int i = 0; i < this.whiteAttackTable.Length; i++) { this.whiteAttackTable[i] = false; }
                    // now reassign all values
                    foreach (int targ in wTargs) { 
                        if(targ >= 0 && targ <= 63){
                            this.whiteAttackTable[targ] = true; 
                        }
                    }
                    break;
                case PieceColour.Black:
                    List<int> bTargs = new List<int>();
                    for (int i = 0; i < 64; i++)
                    {
                        if(this.board.blackPieceTable.table[i] != 0){
                            // then we have a piece and should generate all moves for it
                            Move[] moves = PieceMoveData.GeneratePseudoLegalMoves(Positioning.GetIndexFromIndexAs64(i), this.board);
                            for (int m = 0; m < moves.Length; m++)
                            {
                                bTargs.Add(Positioning.GetIndexAs64FromIndex(moves[m].targetIndex));
                            }
                        }
                    }
                    // clear last attack table 
                    // !probably relatively slow to other approaches
                    for (int i = 0; i < this.blackAttackTable.Length; i++) { this.blackAttackTable[i] = false; }
                    // now reassign all values
                    foreach (int targ in bTargs) { 
                        if(targ >= 0 && targ <= 63){
                            this.blackAttackTable[targ] = true; 
                        }
                    }
                    break;
                
                default: DebugTools.PrintError($"Invalid PieceColour used to update attack tables: {colour}"); break; 
            }
        }
    }
}