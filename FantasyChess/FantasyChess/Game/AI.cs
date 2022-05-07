

namespace FantasyChess.Game {
    public class AI {

        //TODO: use alpha beta pruning
        //example depth 2, p1 does move A, p2 moves, even game,
        //                 p1 does move B, p2 wins a rook, quit early, because we know p1 has a better move

        //TODO: Change AI level based on time instead of a static int

        public static PlayerMove makeMove(Board board, final char color, final int skill) {
            List<Piece> mine = board.getPieces(color);
            List<PlayerMove> validMoves = new List<>();
            for (Piece piece : mine) {
                List<PlayerMove> moves = piece.getValidMoves(false);
                for (PlayerMove move : moves) {
                    final Board testBoard = board.copyBoard();//TODO use Board.testMove()
                    testBoard.Move(move);
                    if (testBoard.inCheck(color)) continue;//can't move into check
                    validMoves.add(move);
                    boolean b = false;
                    if (skill > 1) {
                        PlayerMove AIMove = AI.makeMove(testBoard, Board.otherColor(color), skill - 1);
                        if (AIMove != null) {//TODO stalemate??
                            move.score = -AIMove.score;
                            b = true;
                        }
                    }
                    if (!b) move.score = AI.eval(testBoard, color);
                }
            }

            PlayerMove bestMove = null;
            for (PlayerMove move : validMoves) {
                if (bestMove == null || move.score > bestMove.score) {
                    bestMove = move;
                }
            }
            if (bestMove == null) return null;
            return bestMove;
        }

        public static int eval(Board board, char color) {
            List<Piece> mine = board.getPieces(color);
            List<Piece> his = board.getPieces(Board.otherColor(color));
            int score = 0;
            for (Piece piece : mine) {
                score += piece.getValue();
            }
            for (Piece piece : his) {
                score -= piece.getValue();
            }
            return score;
        }
    }
}