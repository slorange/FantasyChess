
public class MainActivity : AppCompatActivity, View.OnClickListener {

    ChessView chessView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

//        this.requestWindowFeature(Window.FEATURE_NO_TITLE);
        //this.getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, WindowManager.LayoutParams.FLAG_FULLSCREEN);

        chessView = new ChessView(this);
        chessView.setBackgroundColor(Color.WHITE);
        setContentView(chessView);
    }

    @Override
    public bool onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this Adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public bool onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        //if (id == R.id.action_settings) {
        //    return true;
        //}

        return super.onOptionsItemSelected(item);
    }

    public void refreshUI(){
        if(chessView == null) return;
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                chessView.invalidate();
            }
        });
    }

    @Override
    public void onClick(View v) {

    }
}
