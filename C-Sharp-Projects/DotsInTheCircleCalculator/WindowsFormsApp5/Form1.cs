using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Randomizeselector
{
    public partial class Form1 : Form
    {
        private void Form1_Load(object sender, EventArgs e)
        {
            // Optional: put any initialization code you want here
        }
        int maxPieces = 100;
        mini_circle[] miniCircles;
        Rectangle mainRect;
        int x0, y0, radius;
        int mainCircX0, mainCircY0;
        string selectorShape = "Circle";
        string pointShape = "Circle";
        System.Windows.Forms.Label labelResult;

        public Form1()
        {
            InitializeComponent();
            this.Resize += new EventHandler(Form1_Resize);
            InitializeUI();
            GenerateRandomItems();
        }

        private void InitializeUI()
        {
            Button button1 = new Button()
            {
                Text = "Randomize (Again)!",
                Location = new Point(10, 10)
            };
            button1.Click += button1_Click;
            this.Controls.Add(button1);

            System.Windows.Forms.Label labelPoints = new System.Windows.Forms.Label()
            {
                Text = "Number of Points:",
                TextAlign= ContentAlignment.MiddleCenter,
                Location = new Point(140, 10)
            };
            this.Controls.Add(labelPoints);

            NumericUpDown numPoints = new NumericUpDown()
            {
                Minimum = 10,
                Maximum = 1000,
                Value = maxPieces,
                Location = new Point(250, 10)
            };
            numPoints.ValueChanged += (s, e) => { maxPieces = (int)numPoints.Value; GenerateRandomItems(); Refresh(); };
            this.Controls.Add(numPoints);

            ComboBox shapeSelector = new ComboBox()
            {
                Location = new Point(400, 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            shapeSelector.Items.AddRange(new string[] { "Circle", "Square" });
            shapeSelector.SelectedIndex = 0;
            shapeSelector.SelectedIndexChanged += (s, e) => { selectorShape = shapeSelector.SelectedItem.ToString(); Refresh(); };
            this.Controls.Add(shapeSelector);

            ComboBox pointSelector = new ComboBox()
            {
                Location = new Point(550, 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pointSelector.Items.AddRange(new string[] { "Circle", "Rectangle", "Star" });
            pointSelector.SelectedIndex = 0;
            pointSelector.SelectedIndexChanged += (s, e) => { pointShape = pointSelector.SelectedItem.ToString(); Refresh(); };
            this.Controls.Add(pointSelector);

            labelResult = new System.Windows.Forms.Label()
            {
                Text = "0 points are inside.",
                Location = new Point(10, 40),
                AutoSize = true
            };
            this.Controls.Add(labelResult);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GenerateRandomItems();
            Refresh();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Brush basicBrush = new SolidBrush(Color.Blue);
            Brush insideBrush = new SolidBrush(Color.Red);
            Pen outlinePen = new Pen(Color.Green);
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            int piecesInside = 0;
            foreach (var miniCircle in miniCircles)
            {
                bool isInside = selectorShape == "Circle"
                    ? ((miniCircle.X - mainCircX0) * (miniCircle.X - mainCircX0) + (miniCircle.Y - mainCircY0) * (miniCircle.Y - mainCircY0)) <= radius * radius
                    : (miniCircle.X >= x0 && miniCircle.X <= x0 + 2 * radius && miniCircle.Y >= y0 && miniCircle.Y <= y0 + 2 * radius);

                if (isInside) piecesInside++;
                DrawShape(g, isInside ? insideBrush : basicBrush, miniCircle.X, miniCircle.Y);
            }

            if (selectorShape == "Circle")
                g.DrawEllipse(outlinePen, mainRect);
            else
                g.DrawRectangle(outlinePen, mainRect);

            labelResult.Text = $"{piecesInside} points are inside.";
        }

        private void DrawShape(Graphics g, Brush brush, int x, int y)
        {
            switch (pointShape)
            {
                case "Circle":
                    g.FillEllipse(brush, x, y, 5, 5);
                    break;
                case "Rectangle":
                    g.FillRectangle(brush, x, y, 5, 5);
                    break;
                case "Star":
                    DrawStar(g, brush, x, y);
                    break;
            }
        }

        private void DrawStar(Graphics g, Brush brush, int x, int y)
        {
            Point[] starPoints = new Point[]
            {
                new Point(x, y - 3),
                new Point(x + 2, y + 3),
                new Point(x - 3, y - 1),
                new Point(x + 3, y - 1),
                new Point(x - 2, y + 3)
            };
            g.FillPolygon(brush, starPoints);
        }

        private void GenerateRandomItems()
        {
            Random rnd = new Random();
            x0 = rnd.Next(10, this.ClientSize.Width / 2);
            y0 = rnd.Next(10, this.ClientSize.Height / 2);
            int diameter = rnd.Next(50, this.ClientSize.Width / 3);
            radius = diameter / 2;
            mainCircX0 = x0 + radius;
            mainCircY0 = y0 + radius;
            mainRect = new Rectangle(x0, y0, diameter, diameter);

            miniCircles = new mini_circle[maxPieces];
            for (int i = 0; i < maxPieces; i++)
                miniCircles[i] = new mini_circle(rnd.Next(this.ClientSize.Width), rnd.Next(this.ClientSize.Height));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            GenerateRandomItems();
            Refresh();
        }

        public class mini_circle
        {
            public int X { get; set; }
            public int Y { get; set; }
            public mini_circle(int xx, int yy) { X = xx; Y = yy; }
        }
    }
}
   