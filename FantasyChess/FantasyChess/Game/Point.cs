
namespace FantasyChess.Game
{
    public class Point {
        public int x = 0;
        public int y = 0;

        public Point() { }

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Point(Point p) {
            x = p.x;
            y = p.y;
        }

        public override string ToString() {
            return "(" + x + "," + y + ")";
        }

        public int getX() {
            return x;
        }

        public int getY() { return y; }

        public Point getHorizontal() {
            return new Point(x, 0);
        }

        public Point getVertical() {
            return new Point(0, y);
        }

        public void Mult(int m) {
            x *= m;
            y *= m;
        }

        public void Add(Point p2) {
            x += p2.x;
            y += p2.y;
        }

        public static Point Mult(Point p1, int m) {
            return new Point(p1.x * m, p1.y * m);
        }

        public static Point Add(Point p1, Point p2) {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        public override bool Equals(object o) { 
            if (!(o is Point)) return false;
            Point p = (Point)o;
            return x == p.x && y == p.y;
        }
    }
}