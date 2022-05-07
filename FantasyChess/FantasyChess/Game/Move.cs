
namespace FantasyChess.Game
{
    class Move {

        public enum MoveType {SLIDE, LEAP, HOP, LOCUST, OTHER}
        public enum CaptureType {ALL, CAPTUREONLY, MOVEONLY}
        LinkedList<Point> direction = new LinkedList<>();
        int distance = 1;
        MoveType moveType = MoveType.OTHER;
        CaptureType captureType = CaptureType.ALL;
        boolean bounce = false;
        boolean firstOnly = false;
        String captureTargetName = null;
        int captureMaxValue = -1;
        LinkedList<Point> captureLocations = null;
        Point relativeGhostLocation = null;
        Point absoluteGhostLocation = null;
        boolean ghostEater = false;
        LinkedList<Point> cleave = new LinkedList<>();

        //TODOs
        //Value based capture
        //capture piece not on square
        //twopiece (castling)
        //alternate

        public Move(){}

        public Move(MoveType moveType, LinkedList<Point> direction, int distance) {
            this.moveType = moveType;
            this.direction = direction;
            this.distance = distance;
        }

        public LinkedList<PlayerMove> getValidMoves(Piece piece, Board board, LinkedList<Point> dependencies){
            return getValidMoves(piece, board, dependencies, false);
        }

        public LinkedList<PlayerMove> getValidMoves(Piece piece, Board board, LinkedList<Point> dependencies, boolean reverseDirection) {
            LinkedList<PlayerMove> moves = new LinkedList<>();
            if(Stunned(piece, board, dependencies)) return moves;
            if(moveType == MoveType.OTHER) return moves;
            if(firstOnly && piece.moved) return moves;
            Point l = new Point(piece.x, piece.y);
            AbsoluteGhostPosition(l, reverseDirection);
            for(Point io : direction){
                Point i = new Point(io);
                if(reverseDirection) i.y = -i.y;
                Piece leaptOver = null;
                Point p = new Point(l);
                for(int j = 1; j <= distance; j++){
                    if(bounce){//needs debugging
                        int offBoard = board.onBoardX(p.x + i.x);
                        if(offBoard != 0){
                            int inBoard = Math.abs(i.x) - offBoard;
                            p.x += i.x / Math.abs(i.x) * inBoard*2;
                            i.x = -i.x;
                        }
                        offBoard = board.onBoardY(p.y + i.y);
                        if(offBoard != 0){
                            int inBoard = Math.abs(i.y) - offBoard;
                            p.y += i.y / Math.abs(i.y) * inBoard*2;
                            i.y = -i.y;
                        }
                    }
                    p = Point.Add(p, i);
                    if(board.onBoard(p) != 0) break;
                    dependencies.add(new Point(p));
                    Piece other = board.getPiece(p);
                    if(ghostEater && p.equals(board.ghostLocation)) other = board.lastMoved;
                    if(moveType == MoveType.SLIDE) {
                        if(other != null && other.color == piece.color) break;
                        //int tmp = (int)((Math.atan(((double)io.y)/io.x)/(Math.PI/2)+1)*2);
                        moves.add(newMove(piece, board, p, other, dependencies));
                        if(other != null) break;
                    }
                    else if(moveType == MoveType.HOP){
                        if(other != null && other.color == piece.color) continue;
                        moves.add(newMove(piece, board, p, other, dependencies));
                    }
                    else if(moveType == MoveType.LEAP){
                        if(leaptOver != null){
                            if(other != null && other.color == piece.color) break;
                            moves.add(newMove(piece, board, p, other, dependencies));
                            break;
                        }
                        if(other == null) continue;
                        leaptOver = other;
                    }
                    else if(moveType == MoveType.LOCUST){
                        if(leaptOver != null){
                            if(other != null) break;
                            moves.add(newMove(piece, board, p, leaptOver, dependencies));
                            break;
                        }
                        if(other == null) continue;
                        if(other.color == piece.color) break;
                        leaptOver = other;
                    }
                }
            }

            //TODO capture everything and sort out your pieces here (maybe)

            if(captureType == CaptureType.ALL) return moves;

            LinkedList<PlayerMove> moves2 = new LinkedList<>();
            for(PlayerMove move : moves){
                if((captureType == CaptureType.MOVEONLY ^ move.Captures())){
                    moves2.add(move);
                }
            }
            return moves2;
        }

        private boolean Stunned(Piece piece, Board board, LinkedList<Point> dependencies){
            int x = piece.x;
            int y = piece.y;
            List<Point> ps = new LinkedList<Point>();
            ps.add(new Point(x-1, y));
            ps.add(new Point(x+1, y));
            ps.add(new Point(x, y-1));
            ps.add(new Point(x, y+1));
            for(Point p : ps){
                if(board.onBoard(p) == 0) {
                    Piece p1 = board.getPiece(p);
                    if(p1 != null && p1.stunner && p1.color != piece.color){
                        dependencies.add(p);
                        return true;
                    }
                }
            }
            return false;
        }

        private void AbsoluteGhostPosition(Point l, boolean reverseDirection){
            if(relativeGhostLocation != null){
                Point tmp = new Point(relativeGhostLocation);
                if(reverseDirection) tmp.y = -tmp.y;
                absoluteGhostLocation = Point.Add(l, tmp);
            }
        }

        private PlayerMove newMove(Piece piece, Board board, Point to, Piece capture, LinkedList<Point> dependencies){
            LinkedList<Piece> captures = new LinkedList<>();
            if(capture != null) captures.add(capture);
            for(Point p : cleave){
                Point p2 = Point.Add(to,p);
                if(board.onBoard(p2) != 0) continue;
                dependencies.add(p2);
                Piece other = board.getPiece(p2);
                if(other != null && other.color != piece.color) captures.add(other);
            }
            PlayerMove move = new PlayerMove(piece.Location(), new Point(to), captures);
            move.ghostLocation = absoluteGhostLocation;
            return move;
        }

        public boolean CanCapture() { return captureType != CaptureType.MOVEONLY; }

        public boolean CanMove() { return captureType != CaptureType.CAPTUREONLY; }

        public boolean Slide() { return moveType == MoveType.SLIDE; }

        public boolean Hop() {
            return moveType == MoveType.HOP;
        }

        public boolean Leap() {
            return moveType == MoveType.LEAP;
        }

        public boolean Locust() {
            return moveType == MoveType.LOCUST;
        }

        public static LinkedList<Point> getOrthogonal() {
            return new LinkedList<>(Arrays.asList(new Point(1, 0), new Point(0, 1), new Point(0, -1), new Point(-1, 0)));
        }

        public static LinkedList<Point> getDiagonal() {
            return new LinkedList<>(Arrays.asList(new Point(1, 1), new Point(-1, -1), new Point(1, -1), new Point(-1, 1)));
        }

        public static LinkedList<Point> getAllEight() {
            return new LinkedList<>(Arrays.asList(new Point(1, 0), new Point(0, 1), new Point(0, -1), new Point(-1, 0), new Point(1, 1), new Point(-1, -1), new Point(1, -1), new Point(-1, 1)));
        }

        public static LinkedList<Point> getKnightMoves(int x, int y) {
            return new LinkedList<>(Arrays.asList(new Point(x, y), new Point(y, x), new Point(x, -y), new Point(y, -x), new Point(-x, -y), new Point(-y, -x), new Point(-x, y), new Point(-y, x)));
        }

        public static LinkedList<Point> Mult(LinkedList<Point> points, int m){
            LinkedList<Point> points2 = new LinkedList<>();
            for(Point p : points){
                points2.add(Point.Mult(p,m));
            }
            return points2;
        }
    }

    class TwoPartMove extends Move {

        Move m1, m2;
        boolean multikill;

        public TwoPartMove(Move m1, Move m2, boolean multikill){
            this.m1 = m1;
            this.m2 = m2;
            this.multikill = multikill;
        }

        @Override
        public LinkedList<PlayerMove> getValidMoves(Piece piece, Board board, LinkedList<Point> dependencies, boolean reverseDirection) {
            LinkedList<PlayerMove> moves1 = m1.getValidMoves(piece, board, dependencies, reverseDirection);
            HashSet<PlayerMove> movestotal = new HashSet<>();
            for(PlayerMove move : moves1){
                Board copy = board.copyBoard();
                copy.Move(move);
                Point loc = move.moves.getFirst().to;
                Piece p2 = copy.getPiece(loc.x, loc.y);
                LinkedList<PlayerMove> moves2 = m2.getValidMoves(p2, copy, dependencies, reverseDirection);
                movestotal.add(move);
                if(multikill || !move.Captures()) {
                    for (PlayerMove move2 : moves2) {
                        movestotal.add(PlayerMove.Merge(move, move2));
                    }
                }
            }
            return new LinkedList<>(movestotal);
        }
    }

    class InfiniteMove extends Move {
        Move m;
        boolean multikill;
        Move multiMove;

        public InfiniteMove(Move m, boolean multikill){
            this.m = m;
            this.multikill = multikill;
            multiMove = new TwoPartMove(m, this, multikill);
        }

        @Override
        public LinkedList<PlayerMove> getValidMoves(Piece piece, Board board, LinkedList<Point> dependencies, boolean reverseDirection) {
            if(m.getValidMoves(piece, board, dependencies, reverseDirection).size() == 0) return new LinkedList<>();
            return multiMove.getValidMoves(piece, board, dependencies, reverseDirection);
        }
    }

    class TwoPieceMove {

    }

    class CopyLastMove extends Move{
        @Override
        public LinkedList<PlayerMove> getValidMoves(Piece piece, Board board, LinkedList<Point> dependencies) {
            Piece last = board.lastCopyableMove;
            if(last == null) return new LinkedList<>();
            LinkedList<Move> moveList = last.moveList;

            /* TODO could make joker promote if it reaches the end as a pawn
            //it's close, the color is off which is hard to fix right now
            piece.promotion = last.promotion;
            if(piece.promotion != null){
                piece.promotion.pieces.getFirst().
            }*/
            LinkedList<PlayerMove> moves = new LinkedList<>();
            for(Move m : moveList){
                moves.addAll(m.getValidMoves(piece, board, dependencies, last.color != piece.color));
            }
            for(Piece p : board.getPieces(Board.otherColor(piece.color))){
                dependencies.add(p.Location());
            }
            return moves;
        }
    }

    class AlternateMove{

    }
}
