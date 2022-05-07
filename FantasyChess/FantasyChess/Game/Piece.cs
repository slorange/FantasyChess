
namespace FantasyChess.Game
{
    public class Piece {
        int x, y;
        char color;
        String name;
        int image;
        Board board;
        boolean moved;
        int value;
        LinkedList<Move> moveList;
        Promotion promotion;
        boolean royalty;
        boolean stunner;

        //dependency stuff
        LinkedList<Point> dependencies;
        LinkedList<PlayerMove> validMoves;
        boolean valid = false;
        int c = 0;

        //TODO MoveRange
        //TODO Bodyguard

        public Piece(String name, int image, int value, LinkedList<Move> moveList) {
            this(name, image, value, moveList, false);
        }

        public Piece(String name, int image, int value, LinkedList<Move> moveList, boolean royalty) {
            this.image = image;
            this.name = name;
            this.value = value;
            this.moveList = moveList;
            this.royalty = royalty;
            if (name.equals("Z")) stunner = true;
        }

        public Piece(char color, String name, int image, int value, Board board, int x, int y, LinkedList<Move> moveList) {
            this(color, name, image, value, board, x, y, moveList, false);
        }

        public Piece(char color, String name, int image, int value, Board board, int x, int y, LinkedList<Move> moveList, boolean royalty) {
            this(name, image, value, moveList);
            this.color = color;
            this.x = x;
            this.y = y;
            this.board = board;
            this.royalty = royalty;
            moved = false;
            dependencies = new LinkedList<>();
        }

        public Piece(Piece copy, Board b) {
            this(copy.color, copy.name, copy.image, copy.value, b, copy.x, copy.y, copy.moveList, copy.royalty);
            this.board = b;
            moved = copy.moved;
            promotion = copy.promotion;

            valid = copy.valid;
            c = copy.c;
            if (valid) {
                dependencies = copy.dependencies;
                validMoves = copy.validMoves;
            }
            board.addDep(this, c, dependencies);
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
                return Math.max(board.SIZE - promotion.rows - y, 0);
            }
            return Math.max(y - promotion.rows + 1, 0);
        }

        private double promoValue = -1;
        public double getPromoValue() {
            if (promoValue != -1) return promoValue;
            if (promotion != null) {//TODO do this in the piece move method
                int diff = promotion.pieces.getFirst().value - value;
                promoValue = diff / Math.pow(2, distanceToPromotion());
            }
            return promoValue;
        }

        public int getValue() {
            int v = value;
            v += getPromoValue();
            if (royalty) v += 1000000;
            //v += getValidMoves().size();//TODO remove duplicates
            return v;
        }

        public void Promote() {
            if (promotion == null) return;
            Piece p = promotion.pieces.getFirst();
            name = p.name;
            image = p.image;
            value = p.value;
            moveList = p.moveList;
            promotion = p.promotion;
        }

        public boolean inCheck() {
            for (Piece p : board.getPieces(Board.otherColor(color))) {
                for (PlayerMove m : p.getValidMoves(false)) {
                    if (m.captures.contains(this)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public Point Location() {
            return new Point(x, y);
        }

        public LinkedList<PlayerMove> getValidMoves(boolean check) {
            if (!valid) Validate();
            if (!check) return validMoves;
            LinkedList<PlayerMove> stillValid = new LinkedList<>();
            for (PlayerMove m : validMoves) {
                Board test = board.testMove(m);
                if (!test.inCheck(color))
                    stillValid.add(m);
            }
            return stillValid;
        }

        void Validate() {
            dependencies.clear();
            validMoves = getValidMoves(this, moveList, board, dependencies);
            board.addDep(this, ++c, dependencies);
            valid = true;
        }

        private static LinkedList<PlayerMove> getValidMoves(Piece piece, LinkedList<Move> moveList, Board board, LinkedList<Point> dependencies) {
            HashSet<PlayerMove> moves = new HashSet<>();
            for (Move m : moveList) {
                moves.addAll(m.getValidMoves(piece, board, dependencies));
            }
            return new LinkedList<PlayerMove>(moves);
        }

        public void invalidate(int i) {
            if (i == c) valid = false;
        }

        public String toString() {
            return color + "" + name;
        }

        public static LinkedList<Move> getPawnMoves(char color) {
            LinkedList<Move> moveList = new LinkedList<Move>();

            Point forward = Board.getForwardDirection(color);

            LinkedList<Point> direction = new LinkedList<>();
            direction.add(forward);
            Move m = new Move(Move.MoveType.SLIDE, direction, 1);
            m.captureType = Move.CaptureType.MOVEONLY;
            moveList.add(m);

            direction = new LinkedList<>();
            direction.add(forward);
            m = new Move(Move.MoveType.SLIDE, direction, 2);
            m.firstOnly = true;
            m.captureType = Move.CaptureType.MOVEONLY;
            m.relativeGhostLocation = forward;
            moveList.add(m);

            direction = new LinkedList<>();
            direction.add(Point.Add(forward, new Point(1, 0)));
            direction.add(Point.Add(forward, new Point(-1, 0)));
            m = new Move(Move.MoveType.SLIDE, direction, 1);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            m.ghostEater = true;
            moveList.add(m);

            return moveList;
        }

        public static LinkedList<Move> getKnightMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), 1);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getBishopMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), b.SIZE);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getRookMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getOrthogonal(), b.SIZE);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getQueenMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getAllEight(), b.SIZE);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getKingMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getAllEight(), 1);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getGryphonMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();

            Move m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            Move m2 = new Move(Move.MoveType.SLIDE, Move.getOrthogonal(), b.SIZE);
            TwoPartMove m3 = new TwoPartMove(m, m2, false);
            moveList.add(m3);

            return moveList;
        }

        public static LinkedList<Move> getAmazonMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            moveList.addAll(getQueenMoves(b));
            moveList.addAll(getNightRiderMoves(b));
            return moveList;
        }

        public static LinkedList<Move> getPaladinMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            moveList.addAll(getBishopMoves(b));
            moveList.add(new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), b.SIZE));
            return moveList;
        }

        public static LinkedList<Move> getNightRiderMoves(Board b) {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 2), b.SIZE);
            //m.bounce = true;
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getWizardMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.SLIDE, Move.getKnightMoves(1, 3), 1);
            moveList.add(m);
            m = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getChampionMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.HOP, Move.Mult(Move.getDiagonal(), 2), 1);
            moveList.add(m);
            m = new Move(Move.MoveType.HOP, Move.getOrthogonal(), 2);
            moveList.add(m);
            return moveList;
        }

        public static LinkedList<Move> getBeastMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.HOP, Move.getKnightMoves(1, 2), 2);
            m.cleave = Move.getAllEight();
            moveList.add(m);
            Move m2 = new Move(Move.MoveType.HOP, Move.Mult(Move.getOrthogonal(), 4), 1);
            m2.cleave = Move.getAllEight();
            moveList.add(m2);
            return moveList;
        }

        public static LinkedList<Move> getCheckerMoves(char color) {
            LinkedList<Move> moveList = new LinkedList<>();
            LinkedList<Point> direction = new LinkedList<Point>();
            direction.add(Point.Add(Board.getForwardDirection(color), new Point(-1, 0)));
            direction.add(Point.Add(Board.getForwardDirection(color), new Point(1, 0)));
            Move m = new Move(Move.MoveType.LOCUST, direction, 2);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            moveList.add(new InfiniteMove(m, true));

            Move m2 = new Move(Move.MoveType.SLIDE, direction, 1);
            m2.captureType = Move.CaptureType.MOVEONLY;
            moveList.add(m2);
            return moveList;
        }

        public static LinkedList<Move> getPromotedCheckerMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new Move(Move.MoveType.LOCUST, Move.getDiagonal(), 2);
            m.captureType = Move.CaptureType.CAPTUREONLY;
            moveList.add(new InfiniteMove(m, true));

            Move m2 = new Move(Move.MoveType.SLIDE, Move.getDiagonal(), 1);
            m2.captureType = Move.CaptureType.MOVEONLY;
            moveList.add(m2);
            return moveList;
        }

        public static LinkedList<Move> getJokerMoves() {
            LinkedList<Move> moveList = new LinkedList<>();
            Move m = new CopyLastMove();
            moveList.add(m);
            return moveList;
        }
    }

    class Promotion {
        LinkedList<Piece> pieces;
        int rows = 1;
        Promotion(Piece p) {
            pieces = new LinkedList<>();
            pieces.add(p);
        }
        Promotion(LinkedList<Piece> pieces) {
            this.pieces = pieces;
        }
        Promotion(Piece p, int rows) {
            this(p);
            this.rows = rows;
        }
        Promotion(LinkedList<Piece> pieces, int rows) {
            this(pieces);
            this.rows = rows;
        }
    }
}