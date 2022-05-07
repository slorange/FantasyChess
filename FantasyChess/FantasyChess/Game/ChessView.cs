using System.Collections.Generic;
using System.Linq;

namespace FantasyChess.Game
{

    public class ChessView extends View implements View.OnTouchListener {

    MainActivity activity;
    Board board;
    Paint paint = new Paint();
    static int GAP = 10;
    static double PIECE_GAP = 0.1;
    static int STROKE_WIDTH = 8;
    static int DARK_COLOR = Color.rgb(150,100,50);
    static int LIGHT_COLOR = Color.rgb(250,200,100);
    static int VALID_COLOR = Color.rgb(50,200,200);
    static int LAST_COLOR = Color.rgb(200,50,50);
    static int[] TEST_COLOR = {Color.rgb(200,50,50), Color.rgb(50,50,200), Color.rgb(50,200,50), Color.rgb(200,50,200),
                                Color.rgb(225,225,75), Color.rgb(50,200,200), Color.rgb(200,50,125), Color.rgb(50,200,125)};
    static double VALID_GAP = 0.15;//0.15 default, 0.5 hidden

    int squares;
    int width, height;
    int minx, miny, size, squareSize; //board left,top,size
    bool settings = false;
    List<Button> buttons = new List<Button>();

    public class Button{
        int x, y, w, h;
        string event;
        public Button(int x, int y, int w, int h, string event){
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.event = event;
        }
    }

    public ChessView(MainActivity context) {
        super(context);
        activity = context;
        board = Board.SetUI(this, activity);
        changeBoardSize(board.SIZE);
        setOnTouchListener(this);
    }

    public void changeBoardSize(int s){
        squares = s;
        squareSize = size/squares;
    }

    @Override
    protected void onSizeChanged(int w, int h, int oldw, int oldh) {
        width = w - 2*GAP;
        height = h - 2*GAP;
        size = Math.min(width,height);
        minx = width/2-size/2 + GAP;
        miny = height/2-size/2 + GAP;
        squareSize = size/squares;
        super.onSizeChanged(w, h, oldw, oldh);
    }

    @Override
    public void onDraw(Canvas canvas) {
        drawBoard(canvas);
        drawCaptured(canvas);
        drawSettings(canvas);

        //drawButton(canvas, width - GAP, GAP, "Menu", 100);

        Timer timer = new Timer();
        RemindTask task = new RemindTask();
        timer.schedule(task, 1);
    }

    private void drawSettings(Canvas canvas){
        buttons.clear();

        Drawable d = getResources().getDrawable(R.drawable.gear);
        if(d == null){
            Console.WriteLine("Resource not found: gear");
        }
        else {
            int x = 10;
            int y = 10;
            d.setBounds(x, y, x + 150, y + 150);
            buttons.Add(new Button(x, y, 150, 150, "gear"));
            d.draw(canvas);
        }

        if(settings) {
            int x = (int) (width * 0.1);
            int y = (int) (height * 0.1);
            int w = (int) (width * 0.8);
            int h = (int) (height * 0.8);

            paint.setColor(Color.BLACK);
            paint.setStrokeWidth(0);
            paint.setStyle(Paint.Style.FILL);
            canvas.drawRect(x, y, x + w, y + h, paint);

            drawButton2(canvas, x + (int) (w * 0.05), y + (int) (h * 0.1), (int) (w * 0.4), (int) (h * 0.1), "Singleplayer");
            drawButton2(canvas, x + (int) (w * 0.55), y + (int) (h * 0.1), (int) (w * 0.4), (int) (h * 0.1), "Multiplayer");

            drawButton2(canvas, x + (int) (w * 0.05), y + (int) (h * 0.35), (int) (w * 0.4), (int) (h * 0.1), "Checkers");
            drawButton2(canvas, x + (int) (w * 0.55), y + (int) (h * 0.35), (int) (w * 0.4), (int) (h * 0.1), "Chess");
            drawButton2(canvas, x + (int) (w * 0.05), y + (int) (h * 0.50), (int) (w * 0.4), (int) (h * 0.1), "Fantasy Chess");
            drawButton2(canvas, x + (int) (w * 0.55), y + (int) (h * 0.50), (int) (w * 0.4), (int) (h * 0.1), "Test");
        }
    }

    private void drawButton(Canvas canvas, int x, int y, string text, int s){
        paint.setColor(Color.WHITE);
        paint.setStrokeWidth(STROKE_WIDTH);
        paint.setStyle(Paint.Style.STROKE);
        paint.setTextSize(s);
        int b = s/5;
        Rect textBounds = new Rect();
        paint.getTextBounds(text, 0, text.length(), textBounds);
        canvas.drawRect(x - b*2 - textBounds.width(), y, x, y + textBounds.height() + 2*b, paint);
        paint.setStyle(Paint.Style.FILL);
        canvas.drawText(text, x - textBounds.width() - b, y + textBounds.height() + b, paint);
    }

    private void drawButton2(Canvas canvas, int x, int y, int w, int h, string text) {
        drawButton2(canvas, x, y, w, h, text, Color.WHITE);
    }

    private void drawButton2(Canvas canvas, int x, int y, int w, int h, string text, int c){
        buttons.Add(new Button(x, y, w, h, text));

        paint.setColor(c);
        paint.setStrokeWidth(STROKE_WIDTH);
        paint.setStyle(Paint.Style.STROKE);
        canvas.drawRect(x, y, x + w, y + h, paint);

        int b = h/10;
        Rect bounds = findRightTextSize(new Rect(0, 0, w - 2 * b, h - 2 * b), 0, 1000, text);

        paint.setStyle(Paint.Style.FILL);
        canvas.drawText(text, x + w / 2 - bounds.width() / 2, y + h / 2 + bounds.height() / 2, paint);
    }

    //binary search for the right size of text to fit in bounds
    private Rect findRightTextSize(Rect bounds, int l, int u, string text){
        int n = (l + u)/2;
        paint.setTextSize(n);
        Rect textBounds = new Rect();
        paint.getTextBounds(text, 0, text.length(), textBounds);
        if(l == n || u == n) return textBounds;
        if(textBounds.width() > bounds.width() || textBounds.height() > bounds.height()){
            return findRightTextSize(bounds, l, n, text);
        } else {
            return findRightTextSize(bounds, n, u, text);
        }
    }

    private void drawBoard(Canvas canvas){
        for(int i = 0; i < squares; i++){
            for(int j = 0; j < squares; j++){
                drawSquare(canvas, BackgroundColor(i,j), translate(i,j));
            }
        }

        if(board.lastMoved != null) {
            Point lastP = board.lastMoved.Location();
            drawHighlightedSquare(canvas, LAST_COLOR, lastP);
        }

        if(board.selectedPiece != null){
            Point selectedP = board.selectedPiece.Location();
            drawHighlightedSquare(canvas, TEST_COLOR[2], selectedP);
        }

        for (PlayerMove m : board.valid) {
            Point p = m.moves.First().to;
            int color = VALID_COLOR;
            if(m.test != -1) color = TEST_COLOR[m.test];
            drawHighlightedSquare(canvas, color, p);
        }

        for(int i = 0; i < squares; i++) {
            for (int j = 0; j < squares; j++) {
                Piece p = board.getPiece(i,j);
                if (p != null) {
                    //highlight pieces in check
                    //if(p.inCheck()) drawHighlightedSquare(canvas, TEST_COLOR[2], new Point(i,j));
                    drawPiece(canvas, p.image, minx + size * p.x / squares + squareSize * PIECE_GAP, miny + size * p.y / squares + squareSize * PIECE_GAP, squareSize * (1 - PIECE_GAP * 2));
                }
            }
        }
    }

    private void drawCaptured(Canvas canvas){
        int i = 0;
        double spacing =  + squareSize * PIECE_GAP;
        for(Piece p : board.p2captured){
            Point po = board.capturedIntToPosition('B', i);
            drawPiece(canvas, p.image, minx + po.x*squareSize + spacing, miny + po.y*squareSize + spacing, squareSize * (1 - PIECE_GAP * 2));
            i++;
        }
        i = 0;
        for(Piece p : board.p1captured){
            Point po = board.capturedIntToPosition('W', i);
            drawPiece(canvas, p.image, minx + po.x*squareSize + spacing, miny + po.y*squareSize + spacing, squareSize * (1 - PIECE_GAP * 2));
            i++;
        }
    }

    class RemindTask extends TimerTask {
        public void run() {
            board.ready();
        }
    }

    public void drawHighlightedSquare(Canvas canvas, int color, Point p){
        drawSquare(canvas, color, translate(p));
        Point gap = new Point((int)(squareSize * VALID_GAP), (int)(squareSize * VALID_GAP));
        drawSquare(canvas, BackgroundColor(p), Point.Add(translate(p), gap), false, (int)(squareSize * (1 - VALID_GAP * 2)));
    }

    public int BackgroundColor(int x, int y){
        return ((x+y)%2==0) ? LIGHT_COLOR : DARK_COLOR;
    }
    public int BackgroundColor(Point p){
        return BackgroundColor(p.x,p.y);
    }

    public void drawPiece(Canvas canvas, int piece, double x, double y, double size){
        drawPiece(canvas, piece, (int) x, (int) y, (int) size);
    }
    public void drawPiece(Canvas canvas, int piece, int x, int y, int size){
        try {
            Drawable d = getResources().getDrawable(piece);
            d.setBounds(x, y, x+size, y+size);
            d.draw(canvas);
        }
        catch(Resources.NotFoundException e) {
            Console.WriteLine("Resource not found: " + piece);
        }
    }

    public void drawSquare(Canvas canvas, int color, Point p) {
        drawSquare(canvas, color, p, true, squareSize);
    }

    public void drawSquare(Canvas canvas, int color, Point p, bool border, int squareSize){
        int x1 = p.x;
        int y1 = p.y;

        paint.setColor(color);
        paint.setStrokeWidth(0);
        paint.setStyle(Paint.Style.FILL);
        canvas.drawRect(x1, y1, x1+squareSize, y1+squareSize, paint);

        if(border) {
            paint.setColor(Color.BLACK);
            paint.setStrokeWidth(STROKE_WIDTH);
            paint.setStyle(Paint.Style.STROKE);
            canvas.drawRect(x1, y1, x1 + squareSize, y1 + squareSize, paint);
        }
    }

    public Point translate(int x, int y){
        return new Point((int)(minx+size*x/squares), (int)(miny+size*y/squares));
    }
    public Point translate(Point p){
        return translate(p.x, p.y);
    }

    long lastClickTime = System.currentTimeMillis();
    @Override
    public bool onTouch(View v, MotionEvent event) {
        long time = System.currentTimeMillis();
        if(System.currentTimeMillis() - lastClickTime < 150) return true;
        lastClickTime = time;

        float x = event.getX();
        float y = event.getY();

        for(Button b : buttons){
            if(x >= b.x && x <= b.x+b.w && y >= b.y && y <= b.y+b.h){
                HandleClick(b.event);
                return true;
            }
        }

        int a = (int)((event.getX() - minx) / squareSize);
        int b = (int)((event.getY() - miny) / squareSize);
        if(a < 0 || a >= squares) return true;
        board.Touch(a,b);

        return true;
    }

    private void HandleClick(string event){
        if(event.Equals("gear")){
            Settings();
        }
        else if(event.Equals("Singleplayer")){
            SetMode(true);
            HideSettings();
        }
        else if(event.Equals("Multiplayer")){
            SetMode(false);
            HideSettings();
        }
        else if(event.Equals("Checkers") || event.Equals("Chess") || event.Equals("Fantasy Chess") || event.Equals("Test")){
            SetMode(event);
            HideSettings();
        }
    }

    private void Settings(){
        settings = !settings;
        activity.refreshUI();
    }

    private void SetMode(bool AI){
        if(board.AIOn == AI) return;
        board = Board.StartNewGame(AI);
    }

    private void SetMode(string Mode){
        if(board.mode.Equals(Mode)) return;
        board = Board.StartNewGame(Mode);
    }

    private void HideSettings(){
        settings = false;
        activity.refreshUI();
    }
}
}
