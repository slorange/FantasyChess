
using System.Collections.Generic;

namespace FantasyChess.Game
{

    public class PlayerMove {
        List<PieceMove> moves = new List<PieceMove>();
        List<Piece> captures = new List<Piece>();
        int test = -1;
        Point ghostLocation = null;
        int score = 0;

        PlayerMove(Point from, Point to, Piece capture) {
            moves.add(new PieceMove(from, to));
            if (capture != null) {
                captures.add(capture);
            }
        }

        PlayerMove(Point from, Point to, Piece capture, int test) {
            this(from, to, capture);
            this.test = test;
        }

        PlayerMove(Point from, Point to, List<Piece> captures) {
            moves.add(new PieceMove(from, to));
            this.captures = captures;
        }

        PlayerMove(List<PieceMove> moves) {
            this.moves = moves;
        }

        PlayerMove(List<PieceMove> moves, List<Piece> captures) {
            this.moves = moves;
            this.captures = captures;
        }

        public boolean Captures() {
            return captures.size() > 0;
        }

        public static PlayerMove getFirstMove(List<PlayerMove> moves, Point p) {
            for (PlayerMove move : moves) {
                if (move.moves.getFirst().to.equals(p)) {
                    return move;
                }
            }
            return null;
        }

        @Override
    public boolean equals(Object o) {
            if (!(o instanceof PlayerMove)) return false;
            PlayerMove m = (PlayerMove)o;
            return moves.getFirst() == m.moves.getFirst();
        }

        public static PlayerMove Merge(PlayerMove m1, PlayerMove m2) {
            List<Piece> captures = new List<>();
            captures.addAll(m1.captures);
            captures.addAll(m2.captures);
            List<PieceMove> moves = new List<>();
            moves.addAll(m1.moves);
            moves.addAll(m2.moves);
            moves.remove(m1.moves.getFirst());
            moves.remove(m2.moves.getFirst());
            moves.add(PieceMove.Merge(m1.moves.getFirst(), m2.moves.getFirst()));
            PlayerMove newMove = new PlayerMove(moves, captures);
            return newMove;
        }

    }

    class PieceMove {
        Point from;
        Point to;

        PieceMove(Point from, Point to) {
            this.from = from;
            this.to = to;
        }

        public static PieceMove Merge(PieceMove m1, PieceMove m2) {
            return new PieceMove(m1.from, m2.to);
        }

        @Override
        public boolean equals(Object o) {
            if (!(o instanceof PieceMove)) return false;
            PieceMove m = (PieceMove)o;
            return from == m.from && to == m.to;
        }
    } 
}