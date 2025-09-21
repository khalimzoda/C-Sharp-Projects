using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1: Form
    {
        private const int BOARD_SIZE = 15;
        private const int CELL_SIZE = 30;
        private Button[,] board;
        private int[,] state;
        private int player;
        int win1 = 0, win2 = 0, win3 = 0;
        private Label labelWins;
        private TextBox textBoxPlayer1, textBoxPlayer2, textBoxPlayer3;
        private Button button1, button2;
        private string player1Name = "Player X", player2Name = "Player O", player3Name = "Player #";

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
            InitializeUI();
            this.Load += new EventHandler(Form1_Load);
        }

        private void InitializeBoard()
        {
            board = new Button[BOARD_SIZE, BOARD_SIZE];
            state = new int[BOARD_SIZE, BOARD_SIZE];
            player = 1;

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    Button button = new Button();
                    button.Size = new Size(CELL_SIZE, CELL_SIZE);
                    button.Location = new Point(i * CELL_SIZE, j * CELL_SIZE);
                    button.Click += new EventHandler(Button_Click);
                    Controls.Add(button);
                    board[i, j] = button;
                }
            }
        }

        private void InitializeUI()
        {
            Label labelPlayer1 = new Label() { Text = "Player X: ", Location = new Point(500, 50), AutoSize = true };
            textBoxPlayer1 = new TextBox() { Text = player1Name, Location = new Point(600, 50) };
            textBoxPlayer1.TextChanged += (sender, e) => { player1Name = textBoxPlayer1.Text; UpdateWinLabel(); };

            Label labelPlayer2 = new Label() { Text = "Player O: ", Location = new Point(500, 80), AutoSize = true };
            textBoxPlayer2 = new TextBox() { Text = player2Name, Location = new Point(600, 80) };
            textBoxPlayer2.TextChanged += (sender, e) => { player2Name = textBoxPlayer2.Text; UpdateWinLabel(); };

            Label labelPlayer3 = new Label() { Text = "Player #: ", Location = new Point(500, 110), AutoSize = true };
            textBoxPlayer3 = new TextBox() { Text = player3Name, Location = new Point(600, 110) };
            textBoxPlayer3.TextChanged += (sender, e) => { player3Name = textBoxPlayer3.Text; UpdateWinLabel(); };

            labelWins = new Label() { Location = new Point(500, 170), AutoSize = true };
            UpdateWinLabel();

            button1 = new Button() { Text = "New Game", Location = new Point(500, 200) };
            button2 = new Button() { Text = "Restart", Location = new Point(600, 200) };

            button1.Click += new EventHandler(button1_Click);
            button2.Click += new EventHandler(button2_Click);

            Controls.Add(labelPlayer1);
            Controls.Add(textBoxPlayer1);
            Controls.Add(labelPlayer2);
            Controls.Add(textBoxPlayer2);
            Controls.Add(labelPlayer3);
            Controls.Add(textBoxPlayer3);
            Controls.Add(labelWins);
            Controls.Add(button1);
            Controls.Add(button2);
        }

        private void UpdateWinLabel()
        {
            labelWins.Text = $"Wins: {player1Name}={win1} {player2Name}={win2} {player3Name}={win3}";
        }

        private void ResetBoard(bool clearWins)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    state[i, j] = 0;
                    board[i, j].Text = "";
                }
            }
            if (clearWins)
            {
                win1 = 0;
                win2 = 0;
                win3 = 0;
            }
            player = 1;
            UpdateWinLabel();
        }

        private bool CheckWin(int x, int y)
        {
            int[][] directions = { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { 1, 1 }, new int[] { 1, -1 } };
            foreach (var dir in directions)
            {
                int count = 1;
                for (int i = 1; i < 5; i++)
                {
                    int nx = x + dir[0] * i, ny = y + dir[1] * i;
                    if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && state[nx, ny] == player)
                        count++;
                    else break;
                }
                for (int i = 1; i < 5; i++)
                {
                    int nx = x - dir[0] * i, ny = y - dir[1] * i;
                    if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && state[nx, ny] == player)
                        count++;
                    else break;
                }
                if (count >= 5) return true;
            }
            return false;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int x = button.Location.X / CELL_SIZE;
            int y = button.Location.Y / CELL_SIZE;

            if (state[x, y] != 0)
                return;

            state[x, y] = player;
            button.Text = player == 1 ? "X" : player == 2 ? "O" : "#";

            if (CheckWin(x, y))
            {
                if (player == 1) win1++;
                else if (player == 2) win2++;
                else win3++;
                MessageBox.Show($"{(player == 1 ? player1Name : player == 2 ? player2Name : player3Name)} wins!");
                ResetBoard(false);
            }
            else
            {
                player = player % 3 + 1;
            }
            UpdateWinLabel();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateWinLabel();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetBoard(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }
}
