namespace Chess
{
    using UnityEngine;
    using GUI;
    using Audio;
    using System.Collections.Generic;
    using System.Linq;
    using Chess.Bot;
    using System;
    using UnityEngine.SceneManagement;

    enum MouseState { Dragging, Selecting }
    enum GameState { NotReady, Playing, WhiteCheckmate, BlackCheckmate, Draw }
    public enum Players { HumanVsHuman, HumanVsBot, BotVsHuman, BotVsBot, Perft }

    public class GameManager : MonoBehaviour {
        private Board board;
        public BoardGUI boardGUI;
        public string FEN = FENHandling.startingFEN;
        public AudioManager audioManager;
        public Players players;
        [HideInInspector] public BotController bot1, bot2;
        // game world stuff
        Camera cam;
        MouseState mouseState;
        GameState gameState;
        Vector2 mousePos;
        Move move;
        Stack<Move[]> validMoves = new Stack<Move[]>();


        // ? DUnno if i keep this, this is just for when bots play each other
        [Header("Bot controls")]
        public float timeToNextMove;
        public int searchDepth;
        float time = 0.0f;

        // Perft
        [Header("Perft")]
        public int perftDepth;
        Perft perft;

        private void Start() {
            Begin(FEN);
        }

        public void Begin(string FEN = FENHandling.startingFEN){
            gameState = GameState.NotReady;
            if(FEN == ""){ FEN = FENHandling.startingFEN; }
            board = new Board(FEN);
            mouseState = MouseState.Selecting;
            cam = Camera.main;
            audioManager = GetComponent<AudioManager>();
            boardGUI.CreateBoardGUI(board, audioManager);

            // if playing with bot, create a botController
            switch(players){
                case Players.HumanVsBot:
                    bot1 = new BotController(board, PieceColour.Black);
                    break;
                case Players.BotVsHuman:
                    bot1 = new BotController(board, PieceColour.White);
                    break;
                case Players.BotVsBot:
                    bot1 = new BotController(board, PieceColour.White);
                    bot2 = new BotController(board, PieceColour.Black);
                    break;
                case Players.Perft:
                    perft = new Perft(perftDepth, board, PieceColour.White);
                    break;
                default: break;
            }
            gameState = GameState.Playing;
        }

        public void NewGame(string FEN = FENHandling.startingFEN){
            boardGUI.checkmatePanel.SetActive(false);
            boardGUI.checkmateText.SetActive(false);
            boardGUI.EmptyGUI();
            Begin(FEN);
        }

        public void EmptyBoard(){
            boardGUI.EmptyGUI();
        }

        private void Update() {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            time += Time.deltaTime;

            boardGUI.ShowBoardRep();
            
            switch (gameState)
            {
                case GameState.Playing: 
                    // update gamestate if in checkmate
                    if((board.gameFlag & 0b01000000) != 0){
                        // white in checkmate
                        gameState = GameState.WhiteCheckmate;
                    } else if((board.gameFlag & 0b10000000) != 0){
                        gameState = GameState.BlackCheckmate;
                    } else if((board.gameFlag & 0b00100000) != 0){
                        gameState = GameState.Draw;
                    }
                    if(players != Players.BotVsBot){
                        switch (mouseState)
                        {
                            case MouseState.Selecting:
                                SelectingHandler(mousePos);
                                break;
                            case MouseState.Dragging:
                                // drag
                                DraggingHandler(mousePos);
                                break;
                            default:
                                break;
                        }
                    }
                    // bot moves
                    switch (players)
                    {
                        case Players.HumanVsBot:
                            if(board.nextToMove == PieceColour.Black){

                                //try { Move randMove = bot1.MakeRandomMove(validMoves.Peek()).Value; MakeMoveHandler(randMove); }
                                try { 
                                    Move move = bot1.MakeMinimaxMove(searchDepth).Value;
                                    validMoves.Push(PieceMoveData.GenerateAllValidMoves(PieceColour.Black, board));
                                    MakeMoveHandler(move); 
                                }
                                catch (InvalidOperationException){ DebugTools.PrintError("No valid bot moves"); }

                            }

                            break;
                        
                        case Players.BotVsHuman:
                            if(board.nextToMove == PieceColour.White){
                                validMoves.Push(PieceMoveData.GenerateAllValidMoves(board.nextToMove, board));

                                //try { Move randMove = bot1.MakeRandomMove(validMoves.Peek()).Value; MakeMoveHandler(randMove); }
                                try { 
                                    Move move = bot1.MakeMinimaxMove(searchDepth).Value;
                                    validMoves.Push(PieceMoveData.GenerateAllValidMoves(PieceColour.White, board));
                                    MakeMoveHandler(move); 
                                }
                                catch (InvalidOperationException){ DebugTools.PrintError("No valid bot moves"); }

                            }

                            break;

                        case Players.BotVsBot:
                            if(time >= timeToNextMove){
                                if(board.nextToMove == PieceColour.White){
                                    validMoves.Push(PieceMoveData.GenerateAllValidMoves(board.nextToMove, board));

                                    try { Move randMove = bot1.MakeRandomMove(validMoves.Peek()).Value; MakeMoveHandler(randMove); }
                                    catch (InvalidOperationException){ DebugTools.PrintError("No valid bot1 moves"); }

                                }else if(board.nextToMove == PieceColour.Black){
                                    validMoves.Push(PieceMoveData.GenerateAllValidMoves(board.nextToMove, board));

                                    try { Move randMove = bot2.MakeRandomMove(validMoves.Peek()).Value; MakeMoveHandler(randMove); }
                                    catch (InvalidOperationException){ DebugTools.PrintError("No valid bot2 moves"); }

                                }

                                time = time - timeToNextMove;
                            }

                            break;

                        

                        default: break;
                    }
                    
                    break;
                case GameState.WhiteCheckmate: boardGUI.ShowCheckmateCard(PieceColour.White); break;
                case GameState.BlackCheckmate: boardGUI.ShowCheckmateCard(PieceColour.Black); break;
                case GameState.Draw: boardGUI.ShowDrawCard(); break;
                default: break;
            }

        }

        void DraggingHandler(Vector2 _vec){
            int[] gp = Positioning.GetGamePositionFromIndex(board.selectedSquare);
            boardGUI.DragPiece(board, gp);
            if(Input.GetMouseButtonUp(0)){
                // let go, lets try to place the piece if different square, or 
                // just select the square if it's the same square
                
                int[] posUnderMouse = new int[] { Mathf.RoundToInt(_vec.x), Mathf.RoundToInt(_vec.y) };
                int index;
                if(HasClickedSquare(posUnderMouse[0], posUnderMouse[1])){
                    // we have square under mouse

                    // if square under is literally the starting one, we don't want to waste that move
                    if(posUnderMouse[0] == gp[0] && posUnderMouse[1] == gp[1]){
                        index = Positioning.GetIndexFromGamePosition(gp);
                        move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare]);
                        boardGUI.ResetPosition(move);
                    }else if(Input.GetMouseButtonUp(0)){
                        // * We want to move
                        index = Positioning.GetIndexFromGamePosition(posUnderMouse);


                        if(board.boardRep[board.selectedSquare] == Piece.wPawn && (index == board.selectedSquare + 9 || index == board.selectedSquare + 11) && board.boardRep[index] == Piece.empty){
                            // en passant
                            int target = index - 10;
                            move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare], true, target);
                        }
                        else if(board.boardRep[board.selectedSquare] == Piece.bPawn && (index == board.selectedSquare - 9 || index == board.selectedSquare - 11) && board.boardRep[index] == Piece.empty){
                            // en passant
                            int target = index + 10;
                            move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare], true, target);
                        }else{
                            move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare]);
                        }

                        MakeMoveHandler(move);

                        board.selectedSquare = -1;
                        // get outta this function
                    }
                    mouseState = MouseState.Selecting;
                }
            }            
        }

        void SelectingHandler(Vector2 _vec){
            // called when left mouse pressed down, we want to check whether a 
            // square has been clicked, and if so we want to select it.
            // TODO: Abstract this code into seperate function me thinks, and it is a mess especially with checking who's turn it is
            if(Input.GetMouseButtonDown(0)){
                int[] gamePosition = new int[] { Mathf.RoundToInt(_vec.x), Mathf.RoundToInt(_vec.y) };
                bool clickedSquare = HasClickedSquare(gamePosition[0], gamePosition[1]);
                int index = Positioning.GetIndexFromGamePosition(gamePosition);
                int prevSelectedIndex = board.selectedSquare;
                if(board.selectedSquare == -1 && clickedSquare && board.boardRep[index] != Piece.empty){  // means we can only select squares with pieces on
                    int[] prevSelected = Positioning.GetGamePositionFromIndex(board.selectedSquare);
                    board.selectedSquare = Positioning.GetIndexFromGamePosition(gamePosition);
                    move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare]);
                    if(board.IsItTurn(move, players)){
                        // populate any valid moves - do it here otherwise we get an issue where 
                        // the stack gets pushed to when clicking on the square we want to move to,
                        // which if empty means it sets the validMoves to being nothing.

                        validMoves.Push(PieceMoveData.GenerateValidMoves(index, board)); // !
                        validMoves.Push(PieceMoveData.GenerateAllValidMoves(move.pieceColour, board)); // !

                        // Update colours
                        boardGUI.SetSquareColour(index, boardGUI.SquareIsDark(gamePosition) == true ? boardGUI.colours.selectedDark : boardGUI.colours.selectedLight);
                        foreach (Move validMove in validMoves.Skip(1).First())
                        {
                            boardGUI.SetSquareColour(validMove.targetIndex, boardGUI.colours.validMoves);
                        }
                        // foreach (Move validMove in validMoves.Peek())
                        // {
                        //     boardGUI.SetSquareColour(validMove.targetIndex, boardGUI.colours.validMoves);
                        // }

                        mouseState = MouseState.Dragging;
                    } else{
                        board.selectedSquare = prevSelectedIndex;
                    }
                } 
                else if(board.selectedSquare != -1 && clickedSquare && board.selectedSquare != index){
                    // * We want to move
                    
                    if(board.boardRep[board.selectedSquare] == Piece.wPawn && (index == board.selectedSquare + 9 || index == board.selectedSquare + 11) && board.boardRep[index] == Piece.empty){
                        // en passant
                        int target = index - 10;
                        move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare], true, target);
                    }
                    else if(board.boardRep[board.selectedSquare] == Piece.bPawn && (index == board.selectedSquare - 9 || index == board.selectedSquare - 11) && board.boardRep[index] == Piece.empty){
                        // en passant
                        int target = index + 10;
                        move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare], true, target);
                    }else{
                        move = new Move(board.selectedSquare, index, board.boardRep[board.selectedSquare]);
                    }
                    if(board.IsItTurn(move, players)){
                        MakeMoveHandler(move);

                        board.selectedSquare = -1;
                    }
                }
            }
            if(Input.GetMouseButtonDown(1)){
                Deselect();
            }
        }

        void Deselect() {
            // want to deselect any square we have selected
            if(board.selectedSquare > -1){
                int[] prevSelected = Positioning.GetGamePositionFromIndex(board.selectedSquare);
                boardGUI.SetSquareColour(board.selectedSquare, boardGUI.SquareOriginalColour(prevSelected));
                board.selectedSquare = -1;
                foreach (Move validMove in validMoves.Peek())
                {
                    boardGUI.SetSquareColour(validMove.targetIndex, boardGUI.SquareOriginalColour(Positioning.GetGamePositionFromIndex(validMove.targetIndex)));
                }
                // ! DELETE AT SOME POINT
                boardGUI.ShowAttackedSquares();
            }
        }

        void MakeMoveHandler(Move move){
            // make the move
            board.MakeMove(move, validMoves.Peek());
            boardGUI.UpdateBoardGUI(board);
            board.UpdateNextToMove();

            // look for insufficient material
            board.InsufficientMaterialDraw();

            // look for stalemate
            board.Stalemate();

            // look if there's checkmate
            board.Checkmate();
        }

        bool HasClickedSquare(int x, int y){
            if((x >= 0 && x <= 7) && (y >= 0 && y <= 7)){
                return true;
            }
            return false;
        }


        public void ExitToStart(){
            SceneManager.LoadScene("StartScreen", LoadSceneMode.Single);
        }
        public void QuitGame(){
            Application.Quit();
        }
    }
}