using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyChess.Game
{
    public class Move {

        public enum MoveType {SLIDE, LEAP, HOP, LOCUST, OTHER}
        public enum CaptureType {ALL, CAPTUREONLY, MOVEONLY}
        public List<Point> direction = new List<Point>();
        public int distance = 1;
        public MoveType moveType = MoveType.OTHER;
        public CaptureType captureType = CaptureType.ALL;
        public bool bounce = false;
        public bool firstOnly = false;
        public string captureTargetName = null;
        public int captureMaxValue = -1;
        public List<Point> captureLocations = null;
        public Point relativeGhostLocation = null;
        public Point absoluteGhostLocation = null;
        public bool ghostEater = false;
        public List<Point> cleave = new List<Point>();

        //TODOs
        //Value based capture
        //capture piece not on square
        //twopiece (castling)
        //alternate

        public Move(){}

        public Move(MoveType moveType, List<Point> direction, int distance) {
            this.moveType = moveType;
            this.direction = direction;
            this.distance = distance;
        }

        public virtual List<PlayerMove> getValidMoves(Piece piece, Board board, List<Point> dependencies){
            return getValidMoves(piece, board, dependencies, false);
        }

        public virtual List<PlayerMove> getValidMoves(Piece piece, Board board, List<Point> dependencies, bool reverseDirection) {
            List<PlayerMove> moves = new List<PlayerMove>();
            if(Stunned(piece, board, dependencies)) return moves;
            if(moveType == MoveType.OTHER) return moves;
            if(firstOnly && piece.moved) return moves;
            Point l = new Point(piece.x, piece.y);
            AbsoluteGhostPosition(l, reverseDirection);
            foreach (Point io in direction){
                Point i = new Point(io);
                if(reverseDirection) i.y = -i.y;
                Piece leaptOver = null;
                Point p = new Point(l);
                for(int j = 1; j <= distance; j++){
                    if(bounce){//needs debugging
                        int offBoard = board.onBoardX(p.x + i.x);
                        if(offBoard != 0){
                            int inBoard = Math.Abs(i.x) - offBoard;
                            p.x += i.x / Math.Abs(i.x) * inBoard*2;
                            i.x = -i.x;
                        }
                        offBoard = board.onBoardY(p.y + i.y);
                        if(offBoard != 0){
                            int inBoard = Math.Abs(i.y) - offBoard;
                            p.y += i.y / Math.Abs(i.y) * inBoard*2;
                            i.y = -i.y;
                        }
                    }
                    p = Point.Add(p, i);
                    if(board.onBoard(p) != 0) break;
                    dependencies.Add(new Point(p));
                    Piece other = board.getPiece(p);
                    if(ghostEater && p.Equals(board.ghostLocation)) other = board.lastMoved;
                    if(moveType == MoveType.SLIDE) {
                        if(other != null && other.color == piece.color) break;
                        //int tmp = (int)((Math.atan(((double)io.y)/io.x)/(Math.PI/2)+1)*2);
                        moves.Add(newMove(piece, board, p, other, dependencies));
                        if(other != null) break;
                    }
                    else if(moveType == MoveType.HOP){
                        if(other != null && other.color == piece.color) continue;
                        moves.Add(newMove(piece, board, p, other, dependencies));
                    }
                    else if(moveType == MoveType.LEAP){
                        if(leaptOver != null){
                            if(other != null && other.color == piece.color) break;
                            moves.Add(newMove(piece, board, p, other, dependencies));
                            break;
                        }
                        if(other == null) continue;
                        leaptOver = other;
                    }
                    else if(moveType == MoveType.LOCUST){
                        if(leaptOver != null){
                            if(other != null) break;
                            moves.Add(newMove(piece, board, p, leaptOver, dependencies));
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

            List<PlayerMove> moves2 = new List<PlayerMove>();
            foreach (PlayerMove move in moves){
                if((captureType == CaptureType.MOVEONLY ^ move.Captures())){
                    moves2.Add(move);
                }
            }
            return moves2;
        }

        private bool Stunned(Piece piece, Board board, List<Point> dependencies){
            int x = piece.x;
            int y = piece.y;
            List<Point> ps = new List<Point>();
            ps.Add(new Point(x-1, y));
            ps.Add(new Point(x+1, y));
            ps.Add(new Point(x, y-1));
            ps.Add(new Point(x, y+1));
            foreach (Point p in ps){
                if(board.onBoard(p) == 0) {
                    Piece p1 = board.getPiece(p);
                    if(p1 != null && p1.stunner && p1.color != piece.color){
                        dependencies.Add(p);
                        return true;
                    }
                }
            }
            return false;
        }

        private void AbsoluteGhostPosition(Point l, bool reverseDirection){
            if(relativeGhostLocation != null){
                Point tmp = new Point(relativeGhostLocation);
                if(reverseDirection) tmp.y = -tmp.y;
                absoluteGhostLocation = Point.Add(l, tmp);
            }
        }

        private PlayerMove newMove(Piece piece, Board board, Point to, Piece capture, List<Point> dependencies){
            List<Piece> captures = new List<Piece>();
            if(capture != null) captures.Add(capture);
            foreach (Point p in cleave){
                Point p2 = Point.Add(to,p);
                if(board.onBoard(p2) != 0) continue;
                dependencies.Add(p2);
                Piece other = board.getPiece(p2);
                if(other != null && other.color != piece.color) captures.Add(other);
            }
            PlayerMove move = new PlayerMove(piece.Location(), new Point(to), captures);
            move.ghostLocation = absoluteGhostLocation;
            return move;
        }

        public bool CanCapture() { return captureType != CaptureType.MOVEONLY; }

        public bool CanMove() { return captureType != CaptureType.CAPTUREONLY; }

        public bool Slide() { return moveType == MoveType.SLIDE; }

        public bool Hop() {
            return moveType == MoveType.HOP;
        }

        public bool Leap() {
            return moveType == MoveType.LEAP;
        }

        public bool Locust() {
            return moveType == MoveType.LOCUST;
        }

        public static List<Point> getOrthogonal() {
            return new List<Point>(){ new Point(1, 0), new Point(0, 1), new Point(0, -1), new Point(-1, 0) };
        }

        public static List<Point> getDiagonal() {
            return new List<Point>() { new Point(1, 1), new Point(-1, -1), new Point(1, -1), new Point(-1, 1)};
        }

        public static List<Point> getAllEight() {
            return new List<Point>() { new Point(1, 0), new Point(0, 1), new Point(0, -1), new Point(-1, 0), new Point(1, 1), new Point(-1, -1), new Point(1, -1), new Point(-1, 1)};
        }

        public static List<Point> getKnightMoves(int x, int y) {
            return new List<Point>() { new Point(x, y), new Point(y, x), new Point(x, -y), new Point(y, -x), new Point(-x, -y), new Point(-y, -x), new Point(-x, y), new Point(-y, x)};
        }

        public static List<Point> Mult(List<Point> points, int m){
            List<Point> points2 = new List<Point>();
            foreach (Point p in points){
                points2.Add(Point.Mult(p,m));
            }
            return points2;
        }
    }

    class TwoPartMove : Move {

        Move m1, m2;
        bool multikill;

        public TwoPartMove(Move m1, Move m2, bool multikill){
            this.m1 = m1;
            this.m2 = m2;
            this.multikill = multikill;
        }

        public override List<PlayerMove> getValidMoves(Piece piece, Board board, List<Point> dependencies, bool reverseDirection) {
            List<PlayerMove> moves1 = m1.getValidMoves(piece, board, dependencies, reverseDirection);
            HashSet<PlayerMove> movestotal = new HashSet<PlayerMove>();
            foreach (PlayerMove move in moves1){
                Board copy = board.copyBoard();
                copy.Move(move);
                Point loc = move.moves.First().to;
                Piece p2 = copy.getPiece(loc.x, loc.y);
                List<PlayerMove> moves2 = m2.getValidMoves(p2, copy, dependencies, reverseDirection);
                movestotal.Add(move);
                if(multikill || !move.Captures()) {
                    foreach (PlayerMove move2 in moves2) {
                        movestotal.Add(PlayerMove.Merge(move, move2));
                    }
                }
            }
            return new List<PlayerMove>(movestotal);
        }
    }

    class InfiniteMove : Move {
        Move m;
        bool multikill;
        Move multiMove;

        public InfiniteMove(Move m, bool multikill){
            this.m = m;
            this.multikill = multikill;
            multiMove = new TwoPartMove(m, this, multikill);
        }

        
        public override List<PlayerMove> getValidMoves(Piece piece, Board board, List<Point> dependencies, bool reverseDirection) {
            if(m.getValidMoves(piece, board, dependencies, reverseDirection).Count == 0) return new List<PlayerMove>();
            return multiMove.getValidMoves(piece, board, dependencies, reverseDirection);
        }
    }

    class TwoPieceMove {

    }

    class CopyLastMove : Move {
        
        public override List<PlayerMove> getValidMoves(Piece piece, Board board, List<Point> dependencies) {
            Piece last = board.lastCopyableMove;
            if(last == null) return new List<PlayerMove>();
            List<Move> moveList = last.moveList;

            /* TODO could make joker promote if it reaches the end as a pawn
            //it's close, the color is off which is hard to fix right now
            piece.promotion = last.promotion;
            if(piece.promotion != null){
                piece.promotion.pieces.First().
            }*/
            List<PlayerMove> moves = new List<PlayerMove>();
            foreach (Move m in moveList){
                moves.AddRange(m.getValidMoves(piece, board, dependencies, last.color != piece.color));
            }
            foreach (Piece p in board.getPieces(Board.otherColor(piece.color))){
                dependencies.Add(p.Location());
            }
            return moves;
        }
    }

    class AlternateMove{

    }
}
