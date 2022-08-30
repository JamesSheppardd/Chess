namespace Chess.GUI
{
    using UnityEngine;
    using System;
    using Audio; 
    using UnityEngine.UI;
    using System.Collections.Generic;
    public enum ColourUpdateType { Select, Deselect, ValidMoves }

    public class BoardGUI : MonoBehaviour
    {
        // UI stuff
        [Header("GUI")]
        public Text boardRepText;
        public Text boardCheckText;
        public GameObject checkmatePanel;
        public GameObject checkmateText;
        public GameObject whiteWinsText;
        public GameObject whiteWinsPieces;
        public GameObject blackWinsText;
        public GameObject blackWinsPieces;
        public GameObject drawText;
        int[] boardRep;
        public Colours colours;
        public Shader unlit;
        Board board;
        private MeshRenderer[,] squareRenderers = new MeshRenderer[8,8];
        private SpriteRenderer[,] spriteRenderers = new SpriteRenderer[8,8];
        public Sprite[] pieceSprites = new Sprite[12]; // first 6 are white, going p,kn,b,r,q,k, next 6 are black

        [Header("Debug Tools")]
        public bool showWhiteAttacks;
        public bool showBlackAttacks;
        public bool showBoardRep;

        [HideInInspector]public AudioManager audioManager;


        private float dragDepth = -0.2f;

        public void CreateBoardGUI(Board board, AudioManager am){
            this.board = board;
            boardRep = board.boardRep;
            audioManager = am;
            //Shader unlit = Shader.Find("Unlit/Color");
            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    GameObject square = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    square.name = $"{j}-{i+1}";   // replace j with proper file coordiante
                    square.transform.parent = transform;
                    square.transform.position = new Vector2(j, i);
                    // renderers - used for colouring
                    squareRenderers[i,j] = square.GetComponent<MeshRenderer>();
                    
                    // Materials - abstract colour setting later on when needing to add highlights
                    Material sMat = new Material(unlit);
                    sMat.color = colours.light;
                    int[] gp = new int[] { i,j };
                    sMat.color = SquareIsDark(gp) ? colours.dark : colours.light;
                    squareRenderers[i,j].material = sMat;
                }
            }

            // initialise board sprites
            for (int i = 0; i < board.boardRep.Length; i++)
            {
                SpriteFromIndex(board.boardRep[i], i);
            }
        }

        public void EmptyGUI(){
            int pieceCounter = GameObject.FindGameObjectWithTag("PieceParent").transform.childCount;
            if(transform.childCount > 0){
                for (int i = 0; i < 64; i++)
                {
                    GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
            for (int i = 0; i < pieceCounter; i++)
            {
                GameObject.DestroyImmediate(GameObject.FindGameObjectWithTag("PieceParent").transform.GetChild(0).gameObject);
            }
            
            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    // renderers - used for colouring
                    if(squareRenderers[i,j]){
                        squareRenderers[i,j] = new MeshRenderer();
                        spriteRenderers[i,j] = new SpriteRenderer();
                    }
                }
            }
        }

        public bool SquareIsDark(int[] gp){
            // want to check if the square is dark or light
            // TODO: probably just pass in file and rank Index instead of calculating that here - move into different file too (BoardRep.cs ?)
            return (gp[0] + gp[1]) % 2 == 0;
        }

        public void DragPiece(Board board, int[] gp){
            spriteRenderers[gp[0], gp[1]].transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, dragDepth);
        }

        public void UpdateBoardGUI(Board board) {
            // get rid of all changed colours to make board original colour again
            ClearColours();
            
            if(board.moveHistory.Count > 0){
                Move move = board.moveHistory.Peek();

                // Show attacked squares if debug is on for them
                ShowAttackedSquares();

                int[] gp = Positioning.GetGamePositionFromIndex(move.startIndex);
                int[] newPos = Positioning.GetGamePositionFromIndex(move.targetIndex);
                // update GUI representation of sprites and their position in spriteRenderer variable
                spriteRenderers[gp[0],gp[1]].transform.position = new Vector2(newPos[0], newPos[1]);
                RemovePieceSprite(spriteRenderers[newPos[0], newPos[1]]);   // hide the original sprite here
                spriteRenderers[newPos[0], newPos[1]] = spriteRenderers[gp[0],gp[1]];
                spriteRenderers[gp[0],gp[1]] = null;    // flush out old sprite renderer position

                // show last moved piece
                SetSquareColour(move.targetIndex, colours.newestMove);

                // Queen promotion
                if((board.promotionFlag & 0b0001) != 0){
                    // then promotion flag is set
                    PromoteToQueen(move);
                    board.QueenPromotionFlagFlip();
                }

                // castling
                if(move.isCastle){
                    switch (move.pieceColour)
                    {
                        case PieceColour.White:
                            if(move.targetIndex == 27){
                                // kingside
                                spriteRenderers[7,0].transform.position = new Vector2(5, 0);
                                spriteRenderers[5,0] = spriteRenderers[7, 0];
                                spriteRenderers[7,0] = null;
                            }
                            else if(move.targetIndex == 23){
                                spriteRenderers[0,0].transform.position = new Vector2(3, 0);
                                spriteRenderers[3,0] = spriteRenderers[0, 0];
                                spriteRenderers[0,0] = null;
                            }
                            break;

                        case PieceColour.Black:
                            if(move.targetIndex == 97){
                                // kingside
                                spriteRenderers[7,7].transform.position = new Vector2(5, 7);
                                spriteRenderers[5,7] = spriteRenderers[7, 7];
                                spriteRenderers[7,7] = null;
                            }
                            else if(move.targetIndex == 93){
                                spriteRenderers[0,7].transform.position = new Vector2(3, 7);
                                spriteRenderers[3,7] = spriteRenderers[0, 7];
                                spriteRenderers[0,7] = null;
                            }
                            break;
                    }
                }

                // en passant
                if(move.enPassant){
                    int[] epCapture = Positioning.GetGamePositionFromIndex(move.enPassantCapture);
                    RemovePieceSprite(spriteRenderers[epCapture[0], epCapture[1]]);
                    spriteRenderers[epCapture[0], epCapture[1]] = null;
                }
                
                // check
                if((board.gameFlag & 0b00000010) != 0){
                    // white check
                    SetSquareColour(Positioning.GetIndexFromIndexAs64(board.whiteKingPos), colours.checkingPiece);
                    SetCheckText(PieceColour.White);
                }
                if((board.gameFlag & 0b00000100) != 0){
                    // black check
                    SetSquareColour(Positioning.GetIndexFromIndexAs64(board.blackKingPos), colours.checkingPiece);
                    SetCheckText(PieceColour.Black);
                }
                if((board.gameFlag & 0b00000010) == 0 && (board.gameFlag & 0b00000100) == 0 && boardCheckText.text != ""){
                    // no check
                    SetCheckText(PieceColour.Null);
                }
                
                audioManager.PlayMoveSound();
                boardRep = board.boardRep;
            }
        }

        public void PromoteToQueen(Move move){
            int[] pos = Positioning.GetGamePositionFromIndex(move.targetIndex);
            if(move.pieceColour == PieceColour.White){
                // white
                spriteRenderers[pos[0], pos[1]].sprite = pieceSprites[4];
            } else{
                // black
                spriteRenderers[pos[0], pos[1]].sprite = pieceSprites[10];
            }
        }

        public void ResetPosition(Move move){
            int[] gp = Positioning.GetGamePositionFromIndex(move.startIndex);
            int[] newPos = Positioning.GetGamePositionFromIndex(move.targetIndex);
            spriteRenderers[gp[0],gp[1]].transform.position = new Vector2(newPos[0], newPos[1]);
        }
        public void DontMakeMove(Move move, Move[] validMoves){
            int[] gp = Positioning.GetGamePositionFromIndex(move.startIndex);
            int[] newPos = Positioning.GetGamePositionFromIndex(move.targetIndex);
            spriteRenderers[gp[0],gp[1]].transform.position = new Vector2(gp[0], gp[1]);
            SetSquareColour(move.startIndex, SquareOriginalColour(gp));
            foreach (Move validMove in validMoves)
            {
                SetSquareColour(validMove.targetIndex, SquareOriginalColour(Positioning.GetGamePositionFromIndex(validMove.targetIndex)));
            }
        }

        #region UI
        private void SetCheckText(PieceColour pieceColour){
            if(pieceColour == PieceColour.Black){
                boardCheckText.text = "Check: Black";
            }
            else if(pieceColour == PieceColour.White){
                boardCheckText.text = "Check: White";
            }
            else{
                // no check at all
                boardCheckText.text = "";
            }
        }    

        public void ShowCheckmateCard(PieceColour winner){
            checkmatePanel.SetActive(true);
            checkmateText.SetActive(true);
            switch (winner)
            {
                case PieceColour.White: whiteWinsText.SetActive(true); whiteWinsPieces.SetActive(true); break;
                case PieceColour.Black: blackWinsText.SetActive(true); blackWinsPieces.SetActive(true); break;
                default: break;
            }
        }

        public void ShowDrawCard(){
            checkmatePanel.SetActive(true);
            drawText.SetActive(true);
        }
        #endregion

        #region Colours

        public Color SquareOriginalColour(int[] gp){
            return SquareIsDark(gp) == true ? colours.dark : colours.light;
        }

        public void SetSquareColour(int index, Color colour){
            int[] gp = Positioning.GetGamePositionFromIndex(index);
            if(gp[1] > 7 || gp[0] > 7){
                return;
            }
            squareRenderers[gp[1],gp[0]].material.color = colour; 
        }

        public void ClearColours(){
            for (int i = 0; i < 64; i++)
            {
                int index = Positioning.GetIndexFromIndexAs64(i);
                SetSquareColour(index, SquareOriginalColour(Positioning.GetGamePositionFromIndex(index)));
            }
        }

        
        #endregion

        #region Debugging
        
        public void ShowAttackedSquares(){
            if(showWhiteAttacks){
                for (int i = 0; i < 64; i++)
                {
                    if(board.attackTables.whiteAttackTable[i]){
                        SetSquareColour(Positioning.GetIndexFromIndexAs64(i), colours.whiteAttackedSquare);
                    }
                }
            }
            if(showBlackAttacks){
                for (int i = 0; i < 64; i++)
                {
                    if(board.attackTables.blackAttackTable[i]){
                        SetSquareColour(Positioning.GetIndexFromIndexAs64(i), colours.blackAttackedSquare);
                    }
                }
            }
        } 

        #endregion

        #region Editor Functions
        public void ShowBoardRep(){
            if(showBoardRep){
                string[] temp = new string[12];
                string result = "";
                for(int i = 0; i <= 119; i++){
                    if(i % 10 == 0){
                        // new line
                        result += "\n";
                    }
                    if(i > 10 && i < 100 && i%10 == 0){
                        //left
                        result += $" {boardRep[i]} |";
                    }
                    else if(i > 19 && i < 109 && i%10==9){
                        //right
                        result += $"| {boardRep[i]} ";
                    }
                    else{
                        result += $" {boardRep[i]} ";
                    }
                }
                temp = result.Split('\n');
                Array.Reverse(temp);
                result = String.Join("\n", temp);
                boardRepText.text = result;
            }
        }
        public void HideBoardRep(){
            boardRepText.text = "";
        }

        #endregion

        #region Sprites
        void SpriteFromIndex(int pieceIndex, int position){
            switch (pieceIndex)
            {
                case 1:
                    // pawn
                    MakePieceSprite(pieceSprites[0], position, "wPawn");
                    break;
                case 2:
                    // knight
                    MakePieceSprite(pieceSprites[1], position, "wKnight");
                    break;
                case 3:
                    // bishop
                    MakePieceSprite(pieceSprites[2], position, "wBishop");
                    break;
                case 4:
                    // rook
                    MakePieceSprite(pieceSprites[3], position, "wRook");
                    break;
                case 5:
                    // queen
                    MakePieceSprite(pieceSprites[4],  position, "wQueen");
                    break;
                case 6:
                    // king
                    MakePieceSprite(pieceSprites[5], position, "wKing");
                    break;

                // black pieces
                case -1:
                    // pawn
                    MakePieceSprite(pieceSprites[6], position, "bPawn");
                    break;
                case -2:
                    // knight
                    MakePieceSprite(pieceSprites[7], position, "bKnight");
                    break;
                case -3:
                    // bishop
                    MakePieceSprite(pieceSprites[8], position, "bBishop");
                    break;
                case -4:
                    // rook
                    MakePieceSprite(pieceSprites[9], position, "bRook");
                    break;
                case -5:
                    // queen
                    MakePieceSprite(pieceSprites[10], position, "bQueen");
                    break;
                case -6:
                    // king
                    MakePieceSprite(pieceSprites[11], position, "bKing");
                    break;
                
                default:
                    // empty
                    break;
            }
        }

        void MakePieceSprite(Sprite _sprite, int _index, string _name = "Piece"){
            int file = Positioning.GetFileFromIndex(_index);
            int rank = Positioning.GetRankFromIndex(_index);
            GameObject _piece = new GameObject(_name, typeof(SpriteRenderer));
            spriteRenderers[file,rank] = _piece.GetComponent<SpriteRenderer>();
            _piece.transform.parent = GameObject.FindGameObjectWithTag("PieceParent").transform;
            _piece.transform.localScale = Vector3.one * 0.35f;

            // positioning
            int[] gamePositions = Positioning.GetGamePositionFromIndex(_index);
            _piece.transform.position = new Vector2(gamePositions[0], gamePositions[1]);
            
            // assign sprite last
            spriteRenderers[file,rank].sprite = _sprite;
        }

        void RemovePieceSprite(SpriteRenderer sr){
            if(sr){ 
                audioManager.PlayPieceTakeSound();
                sr.enabled = false;
            }
        }
    }   
    #endregion
}
