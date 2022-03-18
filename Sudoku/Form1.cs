using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class F_Sudoku : Form
    {
        const int GRID_MAX = 9;
        private bool mouseDown;
        private Point lastLocation;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse
        );


        // Hard: [19,26]
        // Medium: [27,36]
        // Easy: [36,40]

        EDifficulty Difficulty = EDifficulty.MEDUIM;
        int[,] board = new int[,]
        {
            { 0, 0, 9, 7, 4, 8, 0, 0, 0 },
            { 7, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 2, 0, 1, 0, 9, 0, 0, 0 },
            { 0, 0, 7, 0, 0, 0, 2, 4, 0 },
            { 0, 6, 4, 0, 1, 0, 5, 9, 0 },
            { 0, 9, 8, 0, 0, 0, 3, 0, 0 },
            { 0, 0, 0, 8, 0, 3, 0, 2, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 6 },
            { 0, 0, 0, 2, 7, 5, 9, 0, 0 },
        };

        int[,] boardTemplate = new int[,]
        {
            { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 4, 5, 6, 7, 8, 9, 1, 2, 3 },
            { 7, 8, 9, 1, 2, 3, 4, 5, 6 },
            { 2, 1, 4, 3, 6, 5, 8, 9, 7 },
            { 3, 6, 5, 8, 9, 7, 2, 1, 4 },
            { 8, 9, 7, 2, 1, 4, 3, 6, 5 },
            { 5, 3, 1, 6, 4, 2, 9, 7, 8 },
            { 6, 4, 2, 9, 7, 8, 5, 3, 1 },
            { 9, 7, 8, 5, 3, 1, 6, 4, 2 },
        };


        Stopwatch stopwatch = new Stopwatch();
        Timer tmrElapsed = new Timer();



        public F_Sudoku()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            InitializeEvents();
            setBoard(board, true);

            tmrElapsed.Tick += TmrElapsed_Tick;

            tmrElapsed.Start();
            stopwatch.Start();
        }



        private void F_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void F_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                this.Location.X - lastLocation.X + e.X,
                this.Location.Y - lastLocation.Y + e.Y
                );
                this.Update();
            }
        }

        private void F_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void TmrElapsed_Tick(object sender, EventArgs e)
        {
            string hh = stopwatch.Elapsed.Hours.ToString().PadLeft(2, '0');
            string mm = stopwatch.Elapsed.Minutes.ToString().PadLeft(2, '0');
            string ss = stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0');

            lblRemaining.Text = $"{hh}:{mm}:{ss}";
        }

        private void InitializeEvents()
        {
            this.SuspendLayout();
            for (int r = 0; r < GRID_MAX; r++)
            {
                for (int c = 0; c < GRID_MAX; c++)
                {
                    NumericUpDown nud = createNtxt(r * GRID_MAX + c);
                    nud.ReadOnly = true;
                    nud.Increment = 1;
                    tableLayoutPanel1.Controls.Add(nud, c, r);
                }
            }
            this.ResumeLayout(false);
        }



        private void F_SudokuSolver_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                Solve();
            }
        }


        private void Solve()
        {
            if (SolveSudoku(board, 0, 0))
            {
                setBoard(board);
                MessageBox.Show("Solved.");
            }
            else
            {
                MessageBox.Show("No solution.");
            }
        }

        private bool SolveSudoku(int[,] board, int r, int c)
        {
            if (IsValid(board))
                return true;

            int cNew = c + 1;
            int rNew = r;

            if (cNew > 8)
            {
                cNew = 0;
                rNew++;
            }

            if (board[r, c] > 0)
                return SolveSudoku(board, rNew, cNew);

            for (int n = 1; n <= 9; n++)
            {
                if (IsSafe(board, r, c, n))
                {
                    board[r, c] = n;

                    if (SolveSudoku(board, rNew, cNew))
                        return true;
                    else
                        board[r, c] = 0;
                }
            }

            return false;
        }

        private bool IsValid(int[,] board)
        {

            List<int> listH = new List<int>();
            List<int> listV = new List<int>();
            bool blnNoZero = true;

            // Horizontal c heck
            for (int r = 0; r < 9; r++)
            {
                listH.Clear();
                for (int c = 0; c < 9; c++)
                {
                    listH.Add(board[r, c]);

                    if (board[r, c] == 0)
                        blnNoZero = false;
                }

                int uCnt = listH.Where(v => v > 0).Distinct().Count();
                int oCnt = listH.Count(v => v > 0);

                if (uCnt != oCnt)
                    return false;
            }

            // Vertical check
            for (int r = 0; r < 9; r++)
            {
                listV.Clear();
                for (int c = 0; c < 9; c++)
                {
                    listV.Add(board[c, r]);
                }

                int uCnt = listV.Where(v => v > 0).Distinct().Count();
                int oCnt = listV.Count(v => v > 0);

                if (uCnt != oCnt)
                    return false;
            }
            if (blnNoZero)
                return true;
            return false;
        }

        private bool IsSafe(int[,] board, int r, int c, int n)
        {
            if (board[r, c] > 0 || r > 8 || r < 0 || c > 8 || c < 0) return false;

            // Vertical check
            for (int v = 0; v < 9; v++)
            {
                if (board[v, c] == n)
                    return false;
            }

            // Horizontal check
            for (int h = 0; h < 9; h++)
            {
                if (board[r, h] == n)
                    return false;
            }

            // Box check
            int rr = (int)(r / 3.0) * 3;
            int cc = (int)(c / 3.0) * 3;
            for (int i = rr; i < rr + 3; i++)
            {
                for (int j = cc; j < cc + 3; j++)
                {
                    if (board[i, j] == n)
                        return false;
                }
            }

            return true;
        }


        private void setBoard(int[,] boardData, bool blnInitial = false)
        {
            for (int i = 0; i < GRID_MAX; i++)
            {
                for (int j = 0; j < GRID_MAX; j++)
                {
                    NumericUpDown tb = tableLayoutPanel1.GetControlFromPosition(j, i) as NumericUpDown;
                    tb.Value = boardData[i, j] > 0
                        ? boardData[i, j]
                        : 0;

                    


                    if (blnInitial)
                    {
                        if ((int)tb.Value == 0)
                            tb.ForeColor = tb.BackColor;
                        else
                            tb.ForeColor = Color.Black;
                        tb.Enabled = true;
                        if (tb.Value > 0)
                        {
                            tb.Enabled = false;
                        }
                    }
                    else
                    {
                        if (tb.Enabled)
                        {
                            tb.ForeColor = Color.OrangeRed;
                        }

                    }


                }
            }
        }

        private bool checkBoard(int[,] boardData)
        {
            bool blnRet = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    NumericUpDown tb = tableLayoutPanel1.GetControlFromPosition(j, i) as NumericUpDown;
                    if (tb.Value != boardData[i, j])
                    {
                        blnRet = false;
                    }
                }
            }

            return blnRet;
        }

        private void btnSolver_Click(object sender, EventArgs e)
        {
            //Solve();
            CalculateScores();
        }

        private void CalculateScores()
        {
            if (SolveSudoku(board, 0, 0))
            {
                if (checkBoard(board))
                {
                    stopwatch.Stop();
                    MessageBox.Show($"You succesfully completed the sudoku!\r\nYour Score: {lblRemaining.Text}", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    MessageBox.Show("Not completed yet!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {

            DialogResult dr = MessageBox.Show("New board will be generated and the score will be lost. Do you want to continue?", "Generate New?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                stopwatch.Restart();
                Generate();
            }

        }

        private void Generate()
        {


            // Generate random shifting
            int vr = 3 * new Random().Next(1, 3);

            // Add rows to Row Queue
            Queue<List<int>> RowList = new Queue<List<int>>();
            for (int r = 0; r < 9; r++)
            {
                List<int> row = new List<int>();
                for (int c = 0; c < 9; c++)
                {
                    row.Add(boardTemplate[r, c]);
                }
                RowList.Enqueue(row);
            }

            // Shift rows using vr (random value)
            for (int i = 0; i < vr; i++)
            {
                var row = RowList.Dequeue();
                RowList.Enqueue(row);
            }

            // update board template
            for (int r = 0; r < RowList.ToList().Count; r++)
            {
                for (int c = 0; c < RowList.ToList()[r].Count; c++)
                {
                    boardTemplate[r, c] = RowList.ToList()[r][c];
                }
            }

            // Do the same thing for the columns
            int hr = vr + 3;

            Queue<List<int>> ColList = new Queue<List<int>>();
            for (int r = 0; r < 9; r++)
            {
                List<int> col = new List<int>();
                for (int c = 0; c < 9; c++)
                {
                    col.Add(boardTemplate[c, r]);
                }
                ColList.Enqueue(col);
            }

            for (int i = 0; i < hr; i++)
            {
                var row = ColList.Dequeue();
                ColList.Enqueue(row);
            }

            for (int r = 0; r < ColList.ToList().Count; r++)
            {
                for (int c = 0; c < ColList.ToList()[r].Count; c++)
                {
                    boardTemplate[r, c] = ColList.ToList()[r][c];
                }
            }



            if (IsValid(boardTemplate))
            {
                // Hide values according to the difficulty
                int limit = 27;
                switch (Difficulty)
                {
                    case EDifficulty.EASY:
                        limit = 36;
                        break;
                    case EDifficulty.MEDUIM:
                        limit = 27;
                        break;
                    case EDifficulty.HARD:
                        limit = 19;
                        break;
                    default:
                        break;
                }


                HashSet<int> RandList = new HashSet<int>();
                while (RandList.Count < limit)
                {
                    int rVal = new Random().Next(0, 81);
                    if (!RandList.Contains(rVal))
                        RandList.Add(rVal);
                }

                Random rnd = new Random();
                int counter = 0;

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (RandList.Contains(r * 9 + c))
                        {
                            counter++;
                            board[r, c] = boardTemplate[r, c];

                        }
                        else
                        {
                            board[r, c] = 0;
                        }
                    }
                }

                //Console.WriteLine(counter.ToString());
                setBoard(board, true);
            }

        }

        private void rbtnHARD_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHARD.Checked)
                Difficulty = EDifficulty.HARD;
        }

        private void rbtnMEDUIM_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnEASY.Checked)
                Difficulty = EDifficulty.MEDUIM;
        }

        private void rbtnEASY_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnEASY.Checked)
                Difficulty = EDifficulty.EASY;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnHighScores_Click(object sender, EventArgs e)
        {

            Form frmHighScores = new Form();
            frmHighScores.Show();

        }
    



    private void ntxt_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown sendNud = sender as NumericUpDown;   
            if ((int)sendNud.Value == 0)
                sendNud.ForeColor = sendNud.BackColor;
            else
                sendNud.ForeColor = Color.Black;

        }

        private NumericUpDown createNtxt(int order)
        {
            NumericUpDown numericUpDown1 = new NumericUpDown();
            numericUpDown1.BackColor = getBGColorOfCell(order);
            numericUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
            numericUpDown1.Font = new System.Drawing.Font("Century Gothic", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            numericUpDown1.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(70, 66);
            numericUpDown1.TabIndex = order;
            numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            numericUpDown1.ValueChanged += new System.EventHandler(this.ntxt_ValueChanged);
            //numericUpDown1.GotFocus += ntxt_GotFocus;
            numericUpDown1.LostFocus += ntxt_LostFocus;
            return numericUpDown1;
        }

        private void ntxt_GotFocus(object sender, EventArgs e)
        {
            NumericUpDown nSender = sender as NumericUpDown;
            nSender.ForeColor = Color.Black;
        }
        private void ntxt_LostFocus(object sender, EventArgs e)
        {
            NumericUpDown nSender = sender as NumericUpDown;
            if (nSender.Value == 0)
                nSender.ForeColor = nSender.BackColor;
        }

        private Color getBGColorOfCell(int order)
        {
            int row = (int)(order / GRID_MAX);
            int col = order % GRID_MAX;

            int rr = 3 * (int)(row / 3.0);
            int cc = 3 * (int)(col / 3.0);

            if (((rr >= 0 && rr < 3) || (rr >= 6 && rr < GRID_MAX)) && ((cc >= 0 && cc < 3) || (cc >= 6 && cc < 9)))
                return Color.White;

            else if (rr >= 3 && rr < 6 && cc >= 3 && cc < 6)
                return Color.White;

            else
                return Color.LightPink;
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {

            Solve();
            stopwatch.Stop();
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public enum EDifficulty
    {
        EASY,
        MEDUIM,
        HARD
    }
    

}
