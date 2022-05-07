using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyChess.Game
{
    public class Board {

        public int SIZE = 8;
        public bool AIOn = true;
        public string mode = "Fantasy Chess";

        private static ChessView ui;
        private static MainActivity activity;

        //test board is used by AI to try moves
        //some code isn't executed by test board to speed up AI
        public Piece[][] board;
        public List<Piece> p1captured = new List<Piece>();
        public List<Piece> p2captured = new List<Piece>();
        public List<PlayerMove> valid = new List<PlayerMove>();
        public Piece selectedPiece;
        public Piece lastMoved;
        public Piece lastCopyableMove;
        public Point ghostLocation = null;
        bool test = true;
        char turn;
        int AILEVEL = 2;

        List<Dependent>[][] dependents;
        public class Dependent {
            public Piece dep;
            public int c;
            public Dependent(Piece d, int c) {
                dep = d;
                this.c = c;
            }
        }

        public void setSize(int s) {
            SIZE = s;
            if (ui != null) ui.changeBoardSize(s);
            board = new Piece[s][];
            dependents = new List<Dependent>[s][];
            for (int x = 0; x < s; x++)
            {
                board[x] = new Piece[s];
                dependents[x] = new List<Dependent>[s];
                for (int y = 0; y < s; y++) {
                    dependents[x][y] = new List<Dependent>();
                }
            }
        }

        public static Board StartNewGame(bool AI) {
            mainInstance = new Board(AI, mainInstance.mode);
            return mainInstance;
        }

        public static Board StartNewGame(string mode) {
            mainInstance = new Board(mainInstance.AIOn, mode);
            return mainInstance;
        }

        public static Board StartNewGame(bool AI, string mode) {
            mainInstance = new Board(AI, mode);
            return mainInstance;
        }

        public static Board SetUI(ChessView ui, MainActivity activity) {
            Board.ui = ui;
            Board.activity = activity;
            return mainInstance;
        }

        private static Board mainInstance = new Board(true, "Fantasy Chess");

        private Board() { }

        private Board(bool AI, string mode) : this() { 
            this.mode = mode;
            AIOn = AI;
            test = false;
            StartGameMode();
            turn = 'W';
        }

        private void StartGameMode() {
            if (mode.Equals("Chess")) {
                newChessGame();
            }
            else if (mode.Equals("Checkers")) {
                newCheckersGame();
            }
            else if (mode.Equals("Test")) {
                testGame();
            }
            else {
                newGame();
            }
        }

        public void Touch(int x, int y) {
            if (y < 0) {
                int n = capturedPositionToInt('B', x, y);
                if (n < p2captured.Count()) {
                    selectedPiece = p2captured[n];
                    selectedPiece.x = x;
                    selectedPiece.y = y;
                    drawBoard();
                }
                return;
            }
            if (y >= SIZE) {
                int n = capturedPositionToInt('W', x, y);
                if (n < p1captured.Count) {
                    selectedPiece = p1captured[n];
                    selectedPiece.x = x;
                    selectedPiece.y = y;
                    drawBoard();
                }
                return;
            }

            selectedPiece = null;
            if (AIOn && turn == 'B') return;
            Point p = new Point(x, y);
            //Console.Writeln(p + " " + valid);
            if (onBoard(x, y) != 0) return;
            Piece piece = board[x][y];
            if (piece != null && piece.color == turn) { //first click
                selectedPiece = piece;
                valid = piece.getValidMoves(false); //TODO for checkmate this was set to true when I came back
                drawBoard();
            }
            else { //second click
                PlayerMove m = PlayerMove.FirstMove(valid, p);
                if (m == null) return;
                bool b = Move(m);
                valid = new List<PlayerMove>();
                nextTurn();
                drawBoard();
                //if(b)
                //else {
                // content = Button(text='OK')
                // popup = Popup(title='Cannot put king into check', content=content, auto_dismiss=False)
                // content.bind(on_press=popup.dismiss)
                // popup.open()
                //}
            }

        }

        public int capturedPositionToInt(char color, int x, int y) {
            if (color == 'W') {
                return (y - SIZE) * SIZE + x;
            }
            return (-1 - y) * SIZE + x;
        }

        public Point capturedIntToPosition(char color, int n) {
            if (color == 'W') {
                return new Point(n % SIZE, SIZE + n / SIZE);
            }
            return new Point(n % SIZE, -1 - n / SIZE);
        }

        //public List<PlayerMove> valid() {
        //    return valid;
        //}

        public Piece getPiece(Point p) {
            return getPiece(p.x, p.y);
        }

        public Piece getPiece(int x, int y) {
            return board[x][y];
        }

        public bool Move(PlayerMove move) {
            List<PieceMove> moves = move.moves;
            List<Piece> captures = move.captures;
            ghostLocation = move.ghostLocation;
            foreach (Piece capture in captures) {
                SetBoardPiece(capture.x, capture.y, null);
                if (capture.color == 'W') {
                    p2captured.Add(capture);
                }
                if (capture.color == 'B') {
                    p1captured.Add(capture);
                }
                //board[capture.x][capture.y] = null;
            }
            foreach (PieceMove m in moves) {
                Piece p = board[m.from.x][m.from.y];
                SetBoardPiece(m.from.x, m.from.y, null);
                //board[m.from.x][m.from.y] = null;
                SetBoardPiece(m.to.x, m.to.y, p);
                //board[m.to.x][m.to.y] = p;
                p.move(m.to);
            }
            Point firstMove = moves.First().to;
            lastMoved = board[firstMove.x][firstMove.y];
            if (!lastMoved.name.Equals("J")) {
                lastCopyableMove = lastMoved;
            }
            return true;
        }

        public void SetBoardPiece(int x, int y, Piece p) {
            board[x][y] = p;
            foreach (Dependent d in dependents[x][y]) {
                d.dep.invalidate(d.c);
            }
        }

        public void AddDep(Piece piece, int c, List<Point> l) {
            foreach (Point p in l) {
                dependents[p.x][p.y].Add(new Dependent(piece, c));
            }
        }

        public void getDependencies() {
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (board[x][y] != null)
                        board[x][y].getValidMoves(false);
        }

        /*public bool Move(Piece piece, Point loc){
            Board backup = null;
            if (!test) //implementing undo will get rid of this hack
                backup = [[copy.copy(self.board[i][j]) for j in range(8)] for i in range(8)]

            board[piece.x][piece.y] = null;
            board[loc.x][loc.y] = piece;
            piece.move(loc.x, loc.y);

            if (!test) {
                //if king is in check, undo
                if self.is_in_check(piece.color) {
                    self.board = backup_board;
                    return False;
                }
            }
            return true;
        }*/

        public bool inCheck(char color) {
            foreach (Piece p in getPieces(color)) {
                if (p.royalty && p.inCheck()) {
                    return true;
                }
            }
            return false;
        }

        public bool canMove(char color) {
            foreach (Piece p in getPieces(color)) {
                if (p.getValidMoves(true).Count() > 0) {
                    return true;
                }
            }
            return false;
        }

        public bool checkMate(char color) {
            return !canMove(color) && inCheck(color);
        }

        public bool staleMate(char color) {
            return !canMove(color) && !inCheck(color);
        }

        public void testGame() {
            setSize(8);
            //pieces =['Z', 'J', 'C', 'G', 'K', 'C', 'J', 'Z']
            board[0][7] = new Piece('W', "Z", R.drawable.wz, 500, this, 0, 7, Piece.getWizardMoves());
            board[7][7] = new Piece('W', "Z", R.drawable.wz, 500, this, 7, 7, Piece.getWizardMoves());
            board[1][7] = new Piece('W', "J", R.drawable.wj, 350, this, 1, 7, Piece.getJokerMoves());
            board[6][7] = new Piece('W', "J", R.drawable.wj, 350, this, 6, 7, Piece.getJokerMoves());
            board[2][7] = new Piece('W', "C", R.drawable.wc, 350, this, 2, 7, Piece.getChampionMoves());
            board[5][7] = new Piece('W', "C", R.drawable.wc, 350, this, 5, 7, Piece.getChampionMoves());
            board[3][7] = new Piece('W', "G", R.drawable.wg, 900, this, 3, 7, Piece.getGryphonMoves(this));
            board[4][7] = new Piece('W', "K", R.drawable.wk, 350, this, 4, 7, Piece.getKingMoves());
            board[0][0] = new Piece('B', "Z", R.drawable.bz, 500, this, 0, 0, Piece.getWizardMoves());
            board[7][0] = new Piece('B', "Z", R.drawable.bz, 500, this, 7, 0, Piece.getWizardMoves());
            board[1][0] = new Piece('B', "J", R.drawable.bj, 350, this, 1, 0, Piece.getJokerMoves());
            board[6][0] = new Piece('B', "J", R.drawable.bj, 350, this, 6, 0, Piece.getJokerMoves());
            board[2][0] = new Piece('B', "C", R.drawable.bc, 350, this, 2, 0, Piece.getChampionMoves());
            board[5][0] = new Piece('B', "C", R.drawable.bc, 350, this, 5, 0, Piece.getChampionMoves());
            board[3][0] = new Piece('B', "G", R.drawable.bg, 900, this, 3, 0, Piece.getGryphonMoves(this));
            board[4][0] = new Piece('B', "K", R.drawable.bk, 350, this, 4, 0, Piece.getKingMoves());

            for (int x = 0; x < 8; x++) {
                board[x][6] = new Piece('W', "P", R.drawable.wp, 100, this, x, 6, Piece.getPawnMoves('W'));
                board[x][6].promotion = new Promotion(new Piece("G", R.drawable.wg, 900, Piece.getGryphonMoves(this)));
                board[x][1] = new Piece('B', "P", R.drawable.bp, 100, this, x, 1, Piece.getPawnMoves('B'));
                board[x][1].promotion = new Promotion(new Piece("G", R.drawable.bg, 900, Piece.getGryphonMoves(this)));
            }
            turn = 'W';
            //print_board();
            drawBoard();
        }

        public void newGame() {
            setSize(10);
            //pieces =['R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R']
            board[0][8] = new Piece('W', "R", R.drawable.wr, 500, this, 0, 8, Piece.getRookMoves(this));
            board[9][8] = new Piece('W', "R", R.drawable.wr, 500, this, 9, 8, Piece.getRookMoves(this));
            board[1][8] = new Piece('W', "C", R.drawable.wc, 350, this, 1, 8, Piece.getChampionMoves());
            board[8][8] = new Piece('W', "C", R.drawable.wc, 350, this, 8, 8, Piece.getChampionMoves());
            board[2][8] = new Piece('W', "N", R.drawable.wn, 350, this, 2, 8, Piece.getKnightMoves());
            board[7][8] = new Piece('W', "N", R.drawable.wn, 350, this, 7, 8, Piece.getKnightMoves());
            board[3][8] = new Piece('W', "B", R.drawable.wb, 350, this, 3, 8, Piece.getBishopMoves(this));
            board[6][8] = new Piece('W', "B", R.drawable.wb, 350, this, 6, 8, Piece.getBishopMoves(this));
            board[4][8] = new Piece('W', "Q", R.drawable.wq, 900, this, 4, 8, Piece.getQueenMoves(this));
            board[5][8] = new Piece('W', "K", R.drawable.wk, 250, this, 5, 8, Piece.getKingMoves(), true);
            board[0][1] = new Piece('B', "R", R.drawable.br, 500, this, 0, 1, Piece.getRookMoves(this));
            board[9][1] = new Piece('B', "R", R.drawable.br, 500, this, 9, 1, Piece.getRookMoves(this));
            board[1][1] = new Piece('B', "C", R.drawable.bc, 350, this, 1, 1, Piece.getChampionMoves());
            board[8][1] = new Piece('B', "C", R.drawable.bc, 350, this, 8, 1, Piece.getChampionMoves());
            board[2][1] = new Piece('B', "N", R.drawable.bn, 350, this, 2, 1, Piece.getKnightMoves());
            board[7][1] = new Piece('B', "N", R.drawable.bn, 350, this, 7, 1, Piece.getKnightMoves());
            board[3][1] = new Piece('B', "B", R.drawable.bb, 350, this, 3, 1, Piece.getBishopMoves(this));
            board[6][1] = new Piece('B', "B", R.drawable.bb, 350, this, 6, 1, Piece.getBishopMoves(this));
            board[4][1] = new Piece('B', "Q", R.drawable.bq, 900, this, 4, 1, Piece.getQueenMoves(this));
            board[5][1] = new Piece('B', "K", R.drawable.bk, 250, this, 5, 1, Piece.getKingMoves(), true);
            board[1][9] = new Piece('W', "Z", R.drawable.wz, 500, this, 1, 9, Piece.getWizardMoves());
            board[8][9] = new Piece('W', "Z", R.drawable.wz, 500, this, 8, 9, Piece.getWizardMoves());
            board[2][9] = new Piece('W', "O", R.drawable.wo, 1350, this, 2, 9, Piece.getNightRiderMoves(this));
            board[7][9] = new Piece('W', "O", R.drawable.wo, 1350, this, 7, 9, Piece.getNightRiderMoves(this));
            board[3][9] = new Piece('W', "L", R.drawable.wl, 1100, this, 3, 9, Piece.getPaladinMoves(this));
            board[6][9] = new Piece('W', "G", R.drawable.wg, 750, this, 6, 9, Piece.getGryphonMoves(this));
            board[5][9] = new Piece('W', "E", R.drawable.we, 1900, this, 5, 9, Piece.getBeastMoves());
            board[4][9] = new Piece('W', "A", R.drawable.wa, 1100, this, 4, 9, Piece.getAmazonMoves(this));
            board[0][9] = new Piece('W', "J", R.drawable.wj, 900, this, 0, 9, Piece.getJokerMoves());
            board[9][9] = new Piece('W', "J", R.drawable.wj, 900, this, 9, 9, Piece.getJokerMoves());
            board[1][0] = new Piece('B', "Z", R.drawable.bz, 500, this, 1, 0, Piece.getWizardMoves());
            board[8][0] = new Piece('B', "Z", R.drawable.bz, 500, this, 8, 0, Piece.getWizardMoves());
            board[2][0] = new Piece('B', "O", R.drawable.bo, 1350, this, 2, 0, Piece.getNightRiderMoves(this));
            board[7][0] = new Piece('B', "O", R.drawable.bo, 1350, this, 7, 0, Piece.getNightRiderMoves(this));
            board[3][0] = new Piece('B', "L", R.drawable.bl, 1100, this, 3, 0, Piece.getPaladinMoves(this));
            board[6][0] = new Piece('B', "G", R.drawable.bg, 750, this, 6, 0, Piece.getGryphonMoves(this));
            board[5][0] = new Piece('B', "E", R.drawable.be, 1900, this, 5, 0, Piece.getBeastMoves());
            board[4][0] = new Piece('B', "A", R.drawable.ba, 1100, this, 4, 0, Piece.getAmazonMoves(this));
            board[0][0] = new Piece('B', "J", R.drawable.bj, 900, this, 0, 0, Piece.getJokerMoves());
            board[9][0] = new Piece('B', "J", R.drawable.bj, 900, this, 9, 0, Piece.getJokerMoves());
            for (int x = 0; x < 10; x++) {
                board[x][7] = new Piece('W', "P", R.drawable.wp, 100, this, x, 7, Piece.getPawnMoves('W'));
                board[x][7].promotion = new Promotion(new Piece("Q", R.drawable.wq, 900, Piece.getQueenMoves(this)));
                board[x][2] = new Piece('B', "P", R.drawable.bp, 100, this, x, 2, Piece.getPawnMoves('B'));
                board[x][2].promotion = new Promotion(new Piece("Q", R.drawable.bq, 900, Piece.getQueenMoves(this)));
            }
            turn = 'W';
            //print_board();
            drawBoard();
        }

        public void newChessGame() {
            setSize(8);
            //pieces =['R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R']
            board[0][7] = new Piece('W', "R", R.drawable.wr, 500, this, 0, 7, Piece.getRookMoves(this));
            board[7][7] = new Piece('W', "R", R.drawable.wr, 500, this, 7, 7, Piece.getRookMoves(this));
            board[1][7] = new Piece('W', "N", R.drawable.wn, 350, this, 1, 7, Piece.getKnightMoves());
            board[6][7] = new Piece('W', "N", R.drawable.wn, 350, this, 6, 7, Piece.getKnightMoves());
            board[2][7] = new Piece('W', "B", R.drawable.wb, 350, this, 2, 7, Piece.getBishopMoves(this));
            board[5][7] = new Piece('W', "B", R.drawable.wb, 350, this, 5, 7, Piece.getBishopMoves(this));
            board[3][7] = new Piece('W', "Q", R.drawable.wq, 900, this, 3, 7, Piece.getQueenMoves(this));
            board[4][7] = new Piece('W', "K", R.drawable.wk, 350, this, 4, 7, Piece.getKingMoves());
            board[0][0] = new Piece('B', "R", R.drawable.br, 500, this, 0, 0, Piece.getRookMoves(this));
            board[7][0] = new Piece('B', "R", R.drawable.br, 500, this, 7, 0, Piece.getRookMoves(this));
            board[1][0] = new Piece('B', "N", R.drawable.bn, 350, this, 1, 0, Piece.getKnightMoves());
            board[6][0] = new Piece('B', "N", R.drawable.bn, 350, this, 6, 0, Piece.getKnightMoves());
            board[2][0] = new Piece('B', "B", R.drawable.bb, 350, this, 2, 0, Piece.getBishopMoves(this));
            board[5][0] = new Piece('B', "B", R.drawable.bb, 350, this, 5, 0, Piece.getBishopMoves(this));
            board[3][0] = new Piece('B', "Q", R.drawable.bq, 900, this, 3, 0, Piece.getQueenMoves(this));
            board[4][0] = new Piece('B', "K", R.drawable.bk, 350, this, 4, 0, Piece.getKingMoves());
            for (int x = 0; x < 8; x++) {
                board[x][6] = new Piece('W', "P", R.drawable.wp, 100, this, x, 6, Piece.getPawnMoves('W'));
                board[x][6].promotion = new Promotion(new Piece("Q", R.drawable.wq, 900, Piece.getQueenMoves(this)));
                board[x][1] = new Piece('B', "P", R.drawable.bp, 100, this, x, 1, Piece.getPawnMoves('B'));
                board[x][1].promotion = new Promotion(new Piece("Q", R.drawable.bq, 900, Piece.getQueenMoves(this)));
            }
            turn = 'W';
            //print_board();
            drawBoard();
        }


        public void newCheckersGame() {
            setSize(8);
            for (int i = 0; i < 12; i++) {
                int y2 = i / 4;
                int y1 = SIZE - y2 - 1;
                int x1 = (i % 4) * 2 + (y1 % 2 == 0 ? 0 : 1);
                int x2 = SIZE - x1 - 1;

                board[x1][y1] = new Piece('W', "P", R.drawable.wp, 100, this, x1, y1, Piece.getCheckerMoves('W'));
                board[x1][y1].promotion = new Promotion(new Piece("P", R.drawable.wb, 250, Piece.getPromotedCheckerMoves()));
                board[x2][y2] = new Piece('B', "P", R.drawable.bp, 100, this, x2, y2, Piece.getCheckerMoves('B'));
                board[x2][y2].promotion = new Promotion(new Piece("P", R.drawable.bb, 250, Piece.getPromotedCheckerMoves()));
            }
            turn = 'W';
            //print_board();
            drawBoard();
        }

        public List<Piece> getPieces(char color) {
            //TODO make player class and keep list of pieces
            List<Piece> pieces = new List<Piece>();
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (board[x][y] != null)
                        if (board[x][y].color == color)
                            pieces.Add(board[x][y]);
            return pieces;
        }

        public void nextTurn() {
            if (turn == 'W') {
                turn = 'B';
                /*if(!AIOn) return;
                PlayerMove AIMove = AI.makeMove(this, 'B', 2);
                if(AIMove!= null)//shouldn't happen
                    Move(AIMove);
                drawBoard();
                turn = 'W';*/
            }
            else {
                turn = 'W';
            }
            if (getPieces(turn).Count() == 0) {
                nextTurn();
            }
        }

        public void ready() {
            if (!AIOn || turn != 'B') return;
            getDependencies();

            PlayerMove AIMove = AI.makeMove(this, 'B', AILEVEL);
            if (AIMove != null)//shouldnt happen
                Move(AIMove);
            drawBoard();
            turn = 'W';
        }

        //returns how far off the board the point is;
        public int onBoard(Point p) {
            return onBoard(p.x, p.y);
        }
        public int onBoard(int x, int y) {
            int off = onBoardX(x);
            if (off != 0) return off;
            return onBoardY(y);
        }
        public int onBoardX(int x) {
            if (x < 0) return -x;
            if (x >= SIZE) return x - SIZE + 1;
            return 0;
        }
        public int onBoardY(int y) {
            if (y < 0) return -y;
            if (y >= SIZE) return y - SIZE + 1;
            return 0;
        }

        //deep copy
        public Board copyBoard() {
            //TODO should lastMoved and ghostLocation be copied as well? Is this why AI sometimes is dumb?
            Board copy = new Board();
            copy.setSize(SIZE);
            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    if (board[i][j] != null)
                        copy.board[i][j] = new Piece(board[i][j], copy);
            return copy;
        }

        public Board testMove(PlayerMove m) {
            Board testBoard = copyBoard();
            testBoard.Move(m);
            return testBoard;
        }

        public void drawBoard() {
            if (activity == null) return;
            activity.refreshUI();
        }

        public void printBoard() {
            for (int i = 0; i < SIZE; i++) {
                for (int j = 0; j < SIZE; j++) {
                    if (board[j][i] == null)
                        Console.Write("-- ");
                else
                        Console.Write(board[j][i] + " ");
                }
                Console.WriteLine();
            }
        }

        public static char otherColor(char color) {
            return (color == 'B') ? 'W' : 'B';
        }

        public static Point getForwardDirection(char color) {
            return new Point(0, (color == 'B') ? 1 : -1);
        }
    }
}