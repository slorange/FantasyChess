using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyChess.Game
{
    public class Piece {
        public int x, y;
        public char color;
        public string name;
        public int image;
        public Board board;
        public bool moved;
        public int value;
        public List<Move> moveList;
        public Promotion promotion;
        public bool royalty;
        public bool stunner;

        //dependency stuff
        List<Point> dependencies;
        List<PlayerMove> validMoves;
        bool valid = false;
        int c = 0;

        //TODO MoveRange
        //TODO Bodyguard

        public Piece(string name, int image, int value, List<Move> moveList) : this(name, image, value, moveList, false) { }

        public Piece(string name, int image, int value, List<Move> moveList, bool royalty) {
            this.image = image;
            this.name = name;
            this.value = value;
            this.moveList = moveList;
            this.royalty = royalty;
            if (name.Equals("Z")) stunner = true;
        }

        public Piece(char color, string name, int image, int value, Board board, int x, int y, List<Move> moveList) :
            this(color, name, image, value, board, x, y, moveList, false) { }
      

        public Piece(char color, string name, int image, int value, Board board, int x, int y, List<Move> moveList, bool royalty) 
                    : this(name, image, value, moveList) { 
            this.color = color;
            this.x = x;
            this.y = y;
            this.board = board;
            this.royalty = royalty;
            moved = false;
            dependencies = new List<Point>();
        }

        public Piece(Piece copy, Board b) :
                    this(copy.color, copy.name, copy.image, copy.value, b, copy.x, copy.y, copy.moveList, copy.royalty) { 
            this.board = b;
            moved = copy.moved;
            promotion = copy.promotion;

            valid = copy.valid;
            c = copy.c;
            if (valid) {
                dependencies = copy.dependencies;
                validMoves = copy.validMoves;
            }
            board.AddDep(this, c, dependencies);
        }

        public void move(Point p) {
            move(p.x, p.y);
        }

        public void move(int x, int y) {
            this.x = x;
            this.y = y;
            moved = true;
            invalidate(c);
            promoValue = -1;

            if (promotion != null && distanceToPromotion() == 0) {
                Promote();
            }
        }

        public int distanceToPromotion() {
            if (promotion == null) return -1;
            int dir = Board.getForwardDirection(color).y;
            if (dir == 1) {
                return Math.Max(board.SIZE - promotion.rows - y, 0);
            }
            return Math.Max(y - promotion.rows + 1, 0);
        }

        private double promoValue = -1;
        public double getPromoValue() {
            if (promoValue != -1) return promoValue;
            if (promotion != null) {//TODO do this in the piece move method
                int diff = promotion.pieces.First().value - value;
                promoValue = diff / Math.Pow(2, distanceToPromotion());
            }
            return promoValue;
        }

        public int getValue() {
            int v = value;
            v += (int)getPromoValue(); //TODO we're losing precision with this cast, is that ok??
            if (royalty) v += 1000000;
            //v += getValidMoves().size();//TODO remove duplicates
            return v;
        }

        public void Promote() {
            if (promotion == null) return;
            Piece p = promotion.pieces.First();
            name = p.name;
            image = p.image;
            value = p.value;
            moveList = p.moveList;
            promotion = p.promotion;
        }

        public bool inCheck() {
            foreach (Piece p in board.getPieces(Board.otherColor(color))) {
                foreach (PlayerMove m in p.getValidMoves(false)) {
                    if (m.captures.Contains(this)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public Point Location() {
            return new Point(x, y);
        }

        public List<PlayerMove> getValidMoves(bool check) {
            if (!valid) Validate();
            if (!check) return validMoves;
            List<PlayerMove> stillValid = new List<PlayerMove>();
            foreach (PlayerMove m in validMoves) {
                Board test = board.testMove(m);
                if (!test.inCheck(color))
                    stillValid.Add(m);
            }
            return stillValid;
        }

        void Validate() {
            dependencies.Clear();
            validMoves = getValidMoves(this, moveList, board, dependencies);
            board.AddDep(this, ++c, dependencies);
            valid = true;
        }

        private static List<PlayerMove> getValidMoves(Piece piece, List<Move> moveList, Board board, List<Point> dependencies) {
            HashSet<PlayerMove> moves = new HashSet<PlayerMove>();
            foreach (Move m in moveList) {
                moves.AddRange(m.getValidMoves(piece, board, dependencies));
            }
            return new List<PlayerMove>(moves);
        }

        public void invalidate(int i) {
            if (i == c) valid = false;
        }

        public string tostring() {
            return color + "" + name;
        }

        public static List<Move> getPawnMoves(char color) {
            List<Move> moveList = new List<Move>();

            Point forward = Board.getForwardDirection(color);

            List<Point> direction = new List<Point>();
            direction.Add(forward);
            Move m = new Move(Move.MoveType.SLIDE, direction, 1);
            m.captureType = Move.CaptureType.MOVEONLY;
            moveList.Add(m);

            direction = new List<Point>();
            direction.Add(forward);
            m = new Move(Move.MoveType.SLIDE, direction, 2);
            m.firstOnly = true;
            m.captureType = Move.CaptureType.MOVEONLY;
            m.relativeGhostLocation = forward;
            moveList.Add(m);

            direction = new List<Point>();
            direction.Add(Point.Add(forward, new Point(1, 0)));
            direction.Add(Point.Add(forward, new Point(-1, 0)));
            m = new Move(Move.MoveType.SLIDE, direction, 1);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            m.ghostEater = true;
            moveList.Add(m);

            return moveList;
        }

        public static List<Move> getKnightMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), 1);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getBishopMoves(Board b) {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), b.SIZE);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getRookMoves(Board b) {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getOrthogonal(), b.SIZE);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getQueenMoves(Board b) {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getAllEight(), b.SIZE);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getKingMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getAllEight(), 1);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getGryphonMoves(Board b) {
            List<Move> moveList = new List<Move>();

            Move m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            Move m2 = new Move(Move.MoveType.SLIDE, Move.getOrthogonal(), b.SIZE);
            TwoPartMove m3 = new TwoPartMove(m, m2, false);
            moveList.Add(m3);

            return moveList;
        }

        public static List<Move> getAmazonMoves(Board b) {
            List<Move> moveList = new List<Move>();
            moveList.AddRange(getQueenMoves(b));
            moveList.AddRange(getNightRiderMoves(b));
            return moveList;
        }

        public static List<Move> getPaladinMoves(Board b) {
            List<Move> moveList = new List<Move>();
            moveList.AddRange(getBishopMoves(b));
            moveList.Add(new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), b.SIZE));
            return moveList;
        }

        public static List<Move> getNightRiderMoves(Board b) {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), b.SIZE);
            //m.bounce = true;
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getWizardMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 3), 1);
            moveList.Add(m);
            m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getChampionMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.HOP, Move.Mult(Move.getDiagonal(), 2), 1);
            moveList.Add(m);
            m = new Move(Move.MoveType.HOP, Move.getOrthogonal(), 2);
            moveList.Add(m);
            return moveList;
        }

        public static List<Move> getBeastMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.HOP, Move.getKnightMoves(1, 2), 2);
            m.cleave = Move.getAllEight();
            moveList.Add(m);
            Move m2 = new Move(Move.MoveType.HOP, Move.Mult(Move.getOrthogonal(), 4), 1);
            m2.cleave = Move.getAllEight();
            moveList.Add(m2);
            return moveList;
        }

        public static List<Move> getCheckerMoves(char color) {
            List<Move> moveList = new List<Move>();
            List<Point> direction = new List<Point>();
            direction.Add(Point.Add(Board.getForwardDirection(color), new Point(-1, 0)));
            direction.Add(Point.Add(Board.getForwardDirection(color), new Point(1, 0)));
            Move m = new Move(Move.MoveType.LOCUST, direction, 2);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            moveList.Add(new InfiniteMove(m, true));

            Move m2 = new Move(Move.MoveType.SLIDE, direction, 1);
            m2.captureType = Move.CaptureType.MOVEONLY;
            moveList.Add(m2);
            return moveList;
        }

        public static List<Move> getPromotedCheckerMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new Move(Move.MoveType.LOCUST, Move.getDiagonal(), 2);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            moveList.Add(new InfiniteMove(m, true));

            Move m2 = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            m2.captureType = Move.CaptureType.MOVEONLY;
            moveList.Add(m2);
            return moveList;
        }

        public static List<Move> getJokerMoves() {
            List<Move> moveList = new List<Move>();
            Move m = new CopyLastMove();
            moveList.Add(m);
            return moveList;
        }
    }

    public class Promotion {
        public List<Piece> pieces;
        public int rows = 1;
        public Promotion(Piece p) {
            pieces = new List<Piece>();
            pieces.Add(p);
        }
        Promotion(List<Piece> pieces) {
            this.pieces = pieces;
        }
        Promotion(Piece p, int rows) : this(p) { 
            this.rows = rows;
        }
        Promotion(List<Piece> pieces, int rows) : this(pieces) { 
            this.rows = rows;
        }
    }
}