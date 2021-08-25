using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenSaver
{
    public partial class ScreenSaverForm : Form
    {

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);


        private Point mouseLocation;
        private Random rand = new Random();
        private bool previewMode = false;

        private int[,] board;
        private int numCols;
        private int numRows;

        private int liveCount;
        private int last;
        private int skipLast;
        private int repeatCount;


        public const int CELL_SIZE = 5;
        public const int CELL_ALIVE = 1;
        public const int CELL_DEAD = 0;

        public const int STABLE_THRESHOLD = 50;


        public ScreenSaverForm()
        {
            InitializeComponent();

            int left = SystemInformation.WorkingArea.Left;
            int top = SystemInformation.WorkingArea.Top;
            int width = SystemInformation.WorkingArea.Width;
            int height = SystemInformation.WorkingArea.Height;
            displayImage.Location = new Point(left, top);
            displayImage.Size = new Size(width, height);

            numCols = width / CELL_SIZE;
            numRows = height / CELL_SIZE;

            board = generateBoard();
        }

        public ScreenSaverForm(Rectangle Bounds)
        {
            InitializeComponent();
            this.Bounds = Bounds;

            int left = SystemInformation.WorkingArea.Left;
            int top = SystemInformation.WorkingArea.Top;
            int width = SystemInformation.WorkingArea.Width;
            int height = SystemInformation.WorkingArea.Height;
            displayImage.Location = new Point(left, top);
            displayImage.Size = new Size(width, height);

            numCols = width / CELL_SIZE;
            numRows = height / CELL_SIZE;

            last = skipLast = 0;
            repeatCount = 0;
            liveCount = 0;

            board = generateBoard();
        }

        public ScreenSaverForm(IntPtr PreviewWndHandle)
        {
            InitializeComponent();

            // set preview window as parent of this window
            SetParent(this.Handle, PreviewWndHandle);

            // make this window a child of the parent (so it closes when parent does)
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            Size = ParentRect.Size;
            Location = new Point(0, 0);

            // make text smaller
            // textLabel.Font = new Font("Arial", 6);

            previewMode = true;
        }

        private int[,] generateBoard()
        {
            int[,] board = new int[numCols, numRows];

            for (int i = 0; i < numCols; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    board[i, j] = rand.Next(0, 2);
                }
            }

            return board;
        }

        private int[,] nextGeneration(int[,] board)
        {
            int liveNeighbors;
            bool live;

            int[,] nextGen = new int[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    liveNeighbors = checkNeighbors(board, i, j);
                    live = board[i, j] == 1;

                    if (liveNeighbors < 2 || liveNeighbors > 3)
                    {
                        nextGen[i, j] = CELL_DEAD;
                    }
                    else
                    {
                        if (live || liveNeighbors == 3)
                        {
                            nextGen[i, j] = CELL_ALIVE;
                        }
                    }
                }
            }

            return nextGen;
        }

        private int checkNeighbors(int[,] currentGen, int l1, int l2)
        {
            int count = 0;

            // please ignore this ugly code
            for (int i = l1 - 1; i < l1 + 2; i++)
            {
                for (int j = l2 - 1; j < l2 + 2; j++)
                {
                    if (i == l1 && j == l2)
                    {
                        continue;
                    }
                    else
                    {
                        if (currentGen[colCheck(i), rowCheck(j)] == 1)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private int colCheck(int index)
        {
            if (index < 0)
            {
                return numCols - 1;
            }
            else if (index > numCols - 1)
            {
                return 0;
            }
            return index;
        }

        private int rowCheck(int index)
        {
            if (index < 0)
            {
                return numRows - 1;
            }
            else if (index > numRows - 1)
            {
                return 0;
            }
            return index;
        }

        private void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            Cursor.Hide();
            TopMost = true;

            moveTimer.Interval = 75;
            moveTimer.Tick += new EventHandler(moveTimer_Tick);
            moveTimer.Start();
        }

        private void moveTimer_Tick(object sender, System.EventArgs e)
        {
            // textLabel.Left = rand.Next(Math.Max(1, Bounds.Width - textLabel.Width));
            // textLabel.Top = rand.Next(Math.Max(1, Bounds.Height - textLabel.Height));


            // move to new location. (this is the action handler)
            // make label show current location
            
            /*
            int newLeft = rand.Next(Math.Max(1, Bounds.Width - textLabel.Width));
            int newTop = rand.Next(Math.Max(1, Bounds.Height - textLabel.Height));

            string coordString = "(" + newLeft + ", " + newTop + ")";
            textLabel.Left = newLeft;
            textLabel.Top = newTop;
            textLabel.Text = coordString;
            */

            // this is our main loop
            // calculate next generation, create bitmap
            // reference: https://swharden.com/CsharpDataVis/life/game-of-life-using-csharp.md.html

            using var bmp = new Bitmap(displayImage.Width, displayImage.Height);
            using var gfx = Graphics.FromImage(bmp);
            using var cellBrush = new SolidBrush(Color.White);

            gfx.Clear(Color.Black);

            // if we are stable for at least THRESHOLD num times, reset
            if (repeatCount >= STABLE_THRESHOLD)
            {
                board = generateBoard();
                repeatCount = 0;
                last = skipLast = 0;
            }
            else
            {
                board = nextGeneration(board);
            }

            liveCount = 0;

            // assume we have a board at this point
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == CELL_ALIVE)
                    {
                        liveCount++;
                        Point cellLocation = new Point(i * CELL_SIZE, j * CELL_SIZE);
                        Rectangle cellRect = new Rectangle(cellLocation, new Size(CELL_SIZE, CELL_SIZE));
                        gfx.FillRectangle(cellBrush, cellRect);
                    }
                }
            }

            // logic to check whether we are in a stable state
            if (skipLast == liveCount)
            {
                repeatCount++;
            }
            skipLast = last;
            last = liveCount;


            displayImage.Image = (Bitmap)bmp.Clone();
        }

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!previewMode)
            {
                if (!mouseLocation.IsEmpty)
                {
                    if (Math.Abs(mouseLocation.X - e.X) > 3 || Math.Abs(mouseLocation.Y - e.Y) > 3)
                    {
                        Application.Exit();
                    }
                }

                mouseLocation = e.Location;
            }
        }

        private void ScreenSaverForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (!previewMode)
            {
                Application.Exit();
            }
        }

        private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!previewMode)
            {
                Application.Exit();
            }
        }

        private void displayImage_MouseMove(object sender, MouseEventArgs e)
        {
            ScreenSaverForm_MouseMove(sender, e);
        }

        private void displayImage_MouseClick(object sender, MouseEventArgs e)
        {
            ScreenSaverForm_MouseClick(sender, e);
        }
    }
}
