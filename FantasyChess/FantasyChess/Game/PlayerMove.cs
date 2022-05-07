using System.Collections.Generic;
using System.Linq;

namespace FantasyChess.Game
{

    public class PlayerMove {
        public List<PieceMove> moves = new List<PieceMove>();
        public List<Piece> captures = new List<Piece>();
        public int test = -1;
        public Point ghostLocation = null;
        public int score = 0;

        public PlayerMove(Point from, Point to, Piece capture) {
            moves.Add(new PieceMove(from, to));
            if (capture != null) {
                captures.Add(capture);
            }
        }

        publicPlayerMove(Point from, Point to, Piece capture, int test) : this(from, to, capture) { 
            this.test = test;
        }

        public PlayerMove(Point from, Point to, List<Piece> captures) {
            moves.Add(new PieceMove(from, to));
            this.captures = captures;
        }

        public PlayerMove(List<PieceMove> moves) {
            this.moves = moves;
        }

        public PlayerMove(List<PieceMove> moves, List<Piece> captures) {
            this.moves = moves;
            this.captures = captures;
        }

        public bool Captures() {
            return captures.Count() > 0;
        }

        public static PlayerMove FirstMove(List<PlayerMove> moves, Point p) {
            foreach (PlayerMove move in moves) {
                if (move.moves.First().to.Equals(p)) {
                    return move;
                }
            }
            return null;
        }

        public override bool Equals(object o) {
            if (!(o is PlayerMove)) return false;
            PlayerMove m = (PlayerMove)o;
            return moves.First() == m.moves.First();
        }

        public static PlayerMove Merge(PlayerMove m1, PlayerMove m2) {
            var captures = new List<Piece>();
            captures.AddRange(m1.captures);
            captures.AddRange(m2.captures);
            List<PieceMove> moves = new List<PieceMove>();
            moves.AddRange(m1.moves);
            moves.AddRange(m2.moves);
            moves.Remove(m1.moves.First());
            moves.Remove(m2.moves.First());
            moves.Add(PieceMove.Merge(m1.moves.First(), m2.moves.First()));
            PlayerMove newMove = new PlayerMove(moves, captures);
            return newMove;
        }
    }

    public class PieceMove {
        public Point from;
        public Point to;

        public PieceMove(Point from, Point to) {
            this.from = from;
            this.to = to;
        }

        public static PieceMove Merge(PieceMove m1, PieceMove m2) {
            return new PieceMove(m1.from, m2.to);
        }

        public override bool Equals(object o) {
            if (!(o is PieceMove)) return false;
            PieceMove m = (PieceMove)o;
            return from == m.from && to == m.to;
        }
    } 
}