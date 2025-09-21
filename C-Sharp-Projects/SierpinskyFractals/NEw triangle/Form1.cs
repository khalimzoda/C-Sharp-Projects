using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SierpienskyTriangle
{
    public partial class Form1 : Form
    {
        private PointF[] triangleVertices;
        private PointF[] pentagonVertices;
        private PointF[] hexagonVertices;
        private PointF currentTrianglePoint;
        private PointF currentCarpetPoint;
        private PointF currentPentagonPoint;
        private PointF currentHexagonPoint;
        private Random rand = new Random();
        private int numberOfPoints = 0;
        private int totalPointsDrawnTriangle = 0;
        private int totalPointsDrawnCarpet = 0;
        private int totalPointsDrawnPentagon = 0;
        private int totalPointsDrawnHexagon = 0;

        private Button btnStart;
        private Button btnRestart;
        private Button btnStep;
        private Label lblInfo;
        private PictureBox canvas;
        private ComboBox fractalSelector;
        private CheckBox chkShowGrid;

        private enum FractalType { Triangle, Carpet, Pentagon, Hexagon }
        private FractalType currentFractal = FractalType.Triangle;
        // Colors for triangle vertices
        private Color[] triangleColors = new Color[3]
        {
    Color.Red,      // Top vertex
    Color.Green,    // Bottom left
    Color.Blue      // Bottom right
        };

        // Colors for different attractor regions
        private Color[] carpetColors = new Color[8]
        {
            Color.Red, Color.Blue, Color.Green, Color.Orange,
            Color.Purple, Color.Teal, Color.Magenta, Color.DarkGreen
        };

        // Colors for pentagon vertices
        private Color[] pentagonColors = new Color[5]
        {
            Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple
        };

        // Colors for hexagon vertices
        private Color[] hexagonColors = new Color[6]
        {
            Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Teal
        };

        // Maps for 3x3 grid positions (skip center)
        private static readonly int[] CarpetRowMap = { 0, 0, 0, 1, 1, 2, 2, 2 };
        private static readonly int[] CarpetColMap = { 0, 1, 2, 0, 2, 0, 1, 2 };

        // Cached brushes for rendering (created in InitializeFractalBitmap, disposed in CleanupResources)
        private Brush[] carpetBrushes;
        private Brush[] pentagonBrushes;
        private Brush[] hexagonBrushes;
        private Brush[] triangleBrushes;
        private Bitmap fractalBitmap;
        private Graphics fractalGraphics;

        // For recursive carpet drawing
        private int carpetIteration = 0;
        private const int MAX_CARPET_ITERATIONS = 5;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            InitializeTriangle();
            InitializePentagon();
            InitializeHexagon();
            InitializeFractalBitmap();
            this.Shown += (s, e) => AskNumberOfPoints();
        }


        private void InitializeUI()
        {
            this.Text = "Fractal Generator";
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;

            btnStart = new Button() { Text = "Start", Location = new Point(10, 10), Size = new Size(80, 30) };
            btnRestart = new Button() { Text = "Restart", Location = new Point(100, 10), Size = new Size(80, 30) };
            btnStep = new Button() { Text = "Step", Location = new Point(190, 10), Size = new Size(80, 30) };
            lblInfo = new Label() { Text = $"Points drawn: 0", Location = new Point(280, 15), AutoSize = true };

            btnStart.Click += (s, e) => StartFractal();
            btnRestart.Click += (s, e) => RestartCanvas();
            btnStep.Click += (s, e) => StepFractal();

            fractalSelector = new ComboBox()
            {
                Location = new Point(400, 10),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            fractalSelector.Items.AddRange(new string[] { "Sierpinski Triangle", "Sierpinski Carpet", "Pentagon Fractal", "Hexagon Fractal" });
            fractalSelector.SelectedIndex = 0;
            fractalSelector.SelectedIndexChanged += (s, e) =>
            {
                currentFractal = (FractalType)fractalSelector.SelectedIndex;
                RestartCanvas();
            };

            chkShowGrid = new CheckBox()
            {
                Text = "Show Grid",
                Location = new Point(560, 15),
                Size = new Size(100, 30),
                Checked = false
            };

            this.Controls.Add(btnStart);
            this.Controls.Add(btnRestart);
            this.Controls.Add(btnStep);
            this.Controls.Add(lblInfo);
            this.Controls.Add(fractalSelector);
            this.Controls.Add(chkShowGrid);

            canvas = new PictureBox()
            {
                Location = new Point(0, 50),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 50),
                BackColor = Color.White
            };
            canvas.Paint += Canvas_Paint;
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(canvas);
        }

        private void InitializeFractalBitmap()
        {
            CleanupResources();

            // Create bitmap with 32bpp format for safe LockBits operation
            fractalBitmap = new Bitmap(Math.Max(1, canvas.Width), Math.Max(1, canvas.Height), PixelFormat.Format32bppArgb);
            fractalGraphics = Graphics.FromImage(fractalBitmap);
            fractalGraphics.Clear(Color.White);

            // Initialize cached brushes (disposed in CleanupResources)
            if (carpetBrushes == null)
            {
                carpetBrushes = carpetColors.Select(c => (Brush)new SolidBrush(c)).ToArray();
            }

            if (pentagonBrushes == null)
            {
                pentagonBrushes = pentagonColors.Select(c => (Brush)new SolidBrush(c)).ToArray();
            }
            if (triangleBrushes == null)
            {
                triangleBrushes = triangleColors.Select(c => (Brush)new SolidBrush(c)).ToArray();
            }


            if (hexagonBrushes == null)
            {
                hexagonBrushes = hexagonColors.Select(c => (Brush)new SolidBrush(c)).ToArray();
            }
        }

        private void CleanupResources()
        {
            if (fractalGraphics != null)
            {
                fractalGraphics.Dispose();
                fractalGraphics = null;
            }
            if (fractalBitmap != null)
            {
                fractalBitmap.Dispose();
                fractalBitmap = null;
            }
            if (carpetBrushes != null)
            {
                foreach (var b in carpetBrushes)
                {
                    if (b is IDisposable d) d.Dispose();
                }
                carpetBrushes = null;
            }
            if (pentagonBrushes != null)
            {
                foreach (var b in pentagonBrushes)
                {
                    if (b is IDisposable d) d.Dispose();
                }
                pentagonBrushes = null;
            }
            if (hexagonBrushes != null)
            {
                foreach (var b in hexagonBrushes)
                {
                    if (b is IDisposable d) d.Dispose();
                }
                hexagonBrushes = null;
            }
        }

        private void InitializeTriangle()
        {
            int margin = 50;
            triangleVertices = new PointF[3];
            triangleVertices[0] = new PointF(this.ClientSize.Width / 2, margin); // top
            triangleVertices[1] = new PointF(margin, this.ClientSize.Height - margin); // bottom-left
            triangleVertices[2] = new PointF(this.ClientSize.Width - margin, this.ClientSize.Height - margin); // bottom-right
        }

        private void InitializePentagon()
        {
            int margin = 50;
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            int radius = Math.Min(centerX, centerY) - margin;

            pentagonVertices = new PointF[5];

            // Calculate pentagon vertices (regular pentagon)
            for (int i = 0; i < 5; i++)
            {
                double angle = 2 * Math.PI * i / 5 - Math.PI / 2; // Start from top
                pentagonVertices[i] = new PointF(
                    centerX + (float)(radius * Math.Cos(angle)),
                    centerY + (float)(radius * Math.Sin(angle))
                );
            }
        }

        private void InitializeHexagon()
        {
            int margin = 50;
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            int radius = Math.Min(centerX, centerY) - margin;

            hexagonVertices = new PointF[6];

            // Calculate hexagon vertices (regular hexagon)
            for (int i = 0; i < 6; i++)
            {
                double angle = 2 * Math.PI * i / 6 - Math.PI / 6; // Rotate 30 degrees to have flat top
                hexagonVertices[i] = new PointF(
                    centerX + (float)(radius * Math.Cos(angle)),
                    centerY + (float)(radius * Math.Sin(angle))
                );
            }
        }

        private void AskNumberOfPoints()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 350;
                prompt.Height = 180;
                prompt.Text = "Number of Points";
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.BackColor = Color.White;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter number of points per iteration:", Width = 300, ForeColor = Color.Black };
                TextBox inputBox = new TextBox() { Left = 20, Top = 50, Width = 300, Text = "10000" };
                Button confirmation = new Button() { Text = "OK", Left = 230, Width = 90, Top = 80, DialogResult = DialogResult.OK, BackColor = SystemColors.ButtonFace };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog(this) == DialogResult.OK && int.TryParse(inputBox.Text, out int n) && n > 0)
                {
                    numberOfPoints = n;
                }
                else
                {
                    MessageBox.Show("Invalid number! Using default 10000.");
                    numberOfPoints = 10000;
                }
            }
        }

        private PointF RandomPointInTriangle(PointF v0, PointF v1, PointF v2)
        {
            double r1 = rand.NextDouble();
            double r2 = rand.NextDouble();
            if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
            float x = (float)(v0.X + r1 * (v1.X - v0.X) + r2 * (v2.X - v0.X));
            float y = (float)(v0.Y + r1 * (v1.Y - v0.Y) + r2 * (v2.Y - v0.Y));
            return new PointF(x, y);
        }

        private PointF RandomPointInPolygon(PointF[] polygon)
        {
            // Simple approach: generate random point in bounding box and check if it's inside polygon
            float minX = polygon.Min(p => p.X);
            float maxX = polygon.Max(p => p.X);
            float minY = polygon.Min(p => p.Y);
            float maxY = polygon.Max(p => p.Y);

            while (true)
            {
                float x = (float)(minX + rand.NextDouble() * (maxX - minX));
                float y = (float)(minY + rand.NextDouble() * (maxY - minY));

                if (IsPointInPolygon(new PointF(x, y), polygon))
                {
                    return new PointF(x, y);
                }
            }
        }

        private bool IsPointInPolygon(PointF point, PointF[] polygon)
        {
            // Ray casting algorithm to determine if point is inside polygon
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private void StartFractal()
        {
            if (fractalBitmap == null)
                InitializeFractalBitmap();

            if (currentFractal == FractalType.Triangle)
            {
                DrawTriangleFractal(); // This will now use colors
            }
            else if (currentFractal == FractalType.Carpet)
            {
                DrawCarpetFractalChaosGame();
            }
            else if (currentFractal == FractalType.Pentagon)
            {
                DrawPentagonFractal();
            }
            else if (currentFractal == FractalType.Hexagon)
            {
                DrawHexagonFractal();
            }

            canvas.Invalidate();
        }

        private void StepFractal()
        {
            if (currentFractal == FractalType.Carpet)
            {
                if (carpetIteration < MAX_CARPET_ITERATIONS)
                {
                    carpetIteration++;
                    DrawCarpetRecursive();
                    lblInfo.Text = $"Iteration: {carpetIteration}";
                    canvas.Invalidate();
                }
            }
        }

        // Optimized version of DrawTriangleFractal using LockBits
        private void DrawTriangleFractal()
        {
            if (fractalBitmap == null)
                InitializeFractalBitmap();

            if (currentTrianglePoint == PointF.Empty)
                currentTrianglePoint = RandomPointInTriangle(triangleVertices[0], triangleVertices[1], triangleVertices[2]);

            // Use LockBits for performance
            BitmapData bmpData = fractalBitmap.LockBits(new Rectangle(0, 0, fractalBitmap.Width, fractalBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(bmpData.Stride);
                int height = fractalBitmap.Height;
                int bytes = stride * height;

                byte[] pixels = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, pixels, 0, bytes);

                for (int i = 0; i < numberOfPoints; i++)
                {
                    int vertexIndex = rand.Next(3);
                    PointF vertex = triangleVertices[vertexIndex];

                    currentTrianglePoint = new PointF(
                        (currentTrianglePoint.X + vertex.X) / 2f,
                        (currentTrianglePoint.Y + vertex.Y) / 2f
                    );

                    int x = (int)currentTrianglePoint.X;
                    int y = (int)currentTrianglePoint.Y;

                    if (x >= 0 && x < fractalBitmap.Width && y >= 0 && y < fractalBitmap.Height)
                    {
                        int idx = y * stride + x * 4; // 4 bytes per pixel (BGRA)

                        // Use different colors for different vertices
                        Color color = triangleColors[vertexIndex];
                        pixels[idx + 0] = color.B;     // B
                        pixels[idx + 1] = color.G;     // G
                        pixels[idx + 2] = color.R;     // R
                        pixels[idx + 3] = 255;         // A
                    }
                }

                Marshal.Copy(pixels, 0, bmpData.Scan0, bytes);
            }
            finally
            {
                fractalBitmap.UnlockBits(bmpData);
            }

            totalPointsDrawnTriangle += numberOfPoints;
            lblInfo.Text = $"Points drawn: {totalPointsDrawnTriangle}";
        }
        // Alternative method for gradient coloring
        private void DrawTriangleFractalGradient()
        {
            if (fractalBitmap == null)
                InitializeFractalBitmap();

            if (currentTrianglePoint == PointF.Empty)
                currentTrianglePoint = RandomPointInTriangle(triangleVertices[0], triangleVertices[1], triangleVertices[2]);

            BitmapData bmpData = fractalBitmap.LockBits(new Rectangle(0, 0, fractalBitmap.Width, fractalBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(bmpData.Stride);
                int height = fractalBitmap.Height;
                int bytes = stride * height;

                byte[] pixels = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, pixels, 0, bytes);

                for (int i = 0; i < numberOfPoints; i++)
                {
                    int vertexIndex = rand.Next(3);
                    PointF vertex = triangleVertices[vertexIndex];

                    currentTrianglePoint = new PointF(
                        (currentTrianglePoint.X + vertex.X) / 2f,
                        (currentTrianglePoint.Y + vertex.Y) / 2f
                    );

                    int x = (int)currentTrianglePoint.X;
                    int y = (int)currentTrianglePoint.Y;

                    if (x >= 0 && x < fractalBitmap.Width && y >= 0 && y < fractalBitmap.Height)
                    {
                        int idx = y * stride + x * 4;

                        // Calculate color based on barycentric coordinates (gradient effect)
                        Color color = CalculateTriangleColor(currentTrianglePoint, triangleVertices);

                        pixels[idx + 0] = color.B;
                        pixels[idx + 1] = color.G;
                        pixels[idx + 2] = color.R;
                        pixels[idx + 3] = 255;
                    }
                }

                Marshal.Copy(pixels, 0, bmpData.Scan0, bytes);
            }
            finally
            {
                fractalBitmap.UnlockBits(bmpData);
            }

            totalPointsDrawnTriangle += numberOfPoints;
            lblInfo.Text = $"Points drawn: {totalPointsDrawnTriangle}";
        }

        private Color CalculateTriangleColor(PointF point, PointF[] vertices)
        {
            // Calculate barycentric coordinates
            float totalArea = TriangleArea(vertices[0], vertices[1], vertices[2]);
            float area1 = TriangleArea(point, vertices[1], vertices[2]) / totalArea;
            float area2 = TriangleArea(vertices[0], point, vertices[2]) / totalArea;
            float area3 = TriangleArea(vertices[0], vertices[1], point) / totalArea;

            // Blend colors based on barycentric coordinates
            Color color1 = triangleColors[0];
            Color color2 = triangleColors[1];
            Color color3 = triangleColors[2];

            int r = (int)(color1.R * area1 + color2.R * area2 + color3.R * area3);
            int g = (int)(color1.G * area1 + color2.G * area2 + color3.G * area3);
            int b = (int)(color1.B * area1 + color2.B * area2 + color3.B * area3);

            return Color.FromArgb(255,
                Math.Min(255, Math.Max(0, r)),
                Math.Min(255, Math.Max(0, g)),
                Math.Min(255, Math.Max(0, b)));
        }

        private float TriangleArea(PointF a, PointF b, PointF c)
        {
            return Math.Abs((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2f);
        }

        private void DrawCarpetFractalChaosGame()
        {
            float margin = 50;
            float squareSize = Math.Min(canvas.Width, canvas.Height) - 2 * margin;
            float startX = (canvas.Width - squareSize) / 2f;
            float startY = (canvas.Height - squareSize) / 2f;

            if (currentCarpetPoint == PointF.Empty)
            {
                // Start with a point inside the square
                currentCarpetPoint = new PointF(startX + squareSize / 2f, startY + squareSize / 2f);
            }

            if (carpetBrushes == null)
            {
                carpetBrushes = carpetColors.Select(c => (Brush)new SolidBrush(c)).ToArray();
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                // Choose one of 8 valid transformations (3x3 without center)
                int attractorIndex = rand.Next(8);

                // Cell coordinates (0..2)
                int row = CarpetRowMap[attractorIndex];
                int col = CarpetColMap[attractorIndex];

                // Each cell is reduced by 3 times
                float cellSize = squareSize / 3f;

                // IFS: new position = (old / 3) + offset
                currentCarpetPoint = new PointF(
                    (currentCarpetPoint.X - startX) / 3f + startX + col * cellSize,
                    (currentCarpetPoint.Y - startY) / 3f + startY + row * cellSize
                );

                // Check boundaries
                if (currentCarpetPoint.X >= 0 && currentCarpetPoint.X < fractalBitmap.Width &&
                    currentCarpetPoint.Y >= 0 && currentCarpetPoint.Y < fractalBitmap.Height)
                {
                    fractalGraphics.FillRectangle(carpetBrushes[attractorIndex], currentCarpetPoint.X, currentCarpetPoint.Y, 1, 1);
                }
            }

            totalPointsDrawnCarpet += numberOfPoints;
            lblInfo.Text = $"Points drawn: {totalPointsDrawnCarpet}";
        }

        private void DrawPentagonFractal()
        {
            if (fractalBitmap == null)
                InitializeFractalBitmap();

            if (currentPentagonPoint == PointF.Empty)
                currentPentagonPoint = RandomPointInPolygon(pentagonVertices);

            // Use LockBits for performance
            BitmapData bmpData = fractalBitmap.LockBits(new Rectangle(0, 0, fractalBitmap.Width, fractalBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(bmpData.Stride);
                int height = fractalBitmap.Height;
                int bytes = stride * height;

                byte[] pixels = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, pixels, 0, bytes);

                for (int i = 0; i < numberOfPoints; i++)
                {
                    // For pentaflake, we need to choose one of 6 transformations:
                    // 5 outer pentagons + 1 central pentagon
                    int transformationIndex = rand.Next(6);

                    PointF vertex;
                    float ratio;

                    if (transformationIndex < 5)
                    {
                        // Outer pentagons - use vertices of the main pentagon
                        vertex = pentagonVertices[transformationIndex];
                        // Scale factor for outer pentagons (typically 1/3 or golden ratio related)
                        ratio = 0.618f; // This creates the classic pentaflake pattern
                    }
                    else
                    {
                        // Central pentagon - use the center point
                        PointF center = CalculatePentagonCenter(pentagonVertices);
                        vertex = center;
                        // Scale factor for central pentagon (same as outer ones)
                        ratio = 0.619f;
                    }

                    currentPentagonPoint = new PointF(
                        currentPentagonPoint.X + ratio * (vertex.X - currentPentagonPoint.X),
                        currentPentagonPoint.Y + ratio * (vertex.Y - currentPentagonPoint.Y)
                    );

                    int x = (int)currentPentagonPoint.X;
                    int y = (int)currentPentagonPoint.Y;

                    if (x >= 0 && x < fractalBitmap.Width && y >= 0 && y < fractalBitmap.Height)
                    {
                        int idx = y * stride + x * 4; // 4 bytes per pixel (BGRA)

                        // Use different colors for different transformations
                        Color color = transformationIndex < 5 ?
                            pentagonColors[transformationIndex] :
                            Color.White; // Central pentagon in white

                        pixels[idx + 0] = color.B;     // B
                        pixels[idx + 1] = color.G;     // G
                        pixels[idx + 2] = color.R;     // R
                        pixels[idx + 3] = 255;         // A
                    }
                }

                Marshal.Copy(pixels, 0, bmpData.Scan0, bytes);
            }
            finally
            {
                fractalBitmap.UnlockBits(bmpData);
            }

            totalPointsDrawnPentagon += numberOfPoints;
            lblInfo.Text = $"Points drawn: {totalPointsDrawnPentagon}";
        }

        // Helper method to calculate the center of the pentagon
        private PointF CalculatePentagonCenter(PointF[] vertices)
        {
            float centerX = 0;
            float centerY = 0;

            foreach (PointF vertex in vertices)
            {
                centerX += vertex.X;
                centerY += vertex.Y;
            }

            return new PointF(centerX / vertices.Length, centerY / vertices.Length);
        }
       /*   private void DrawPentagonFractal()
  {
      if (fractalBitmap == null)
          InitializeFractalBitmap();

      if (currentPentagonPoint == PointF.Empty)
          currentPentagonPoint = RandomPointInPolygon(pentagonVertices);

      // Use LockBits for performance
      BitmapData bmpData = fractalBitmap.LockBits(new Rectangle(0, 0, fractalBitmap.Width, fractalBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
      try
      {
          int stride = Math.Abs(bmpData.Stride);
          int height = fractalBitmap.Height;
          int bytes = stride * height;

          byte[] pixels = new byte[bytes];
          Marshal.Copy(bmpData.Scan0, pixels, 0, bytes);

          for (int i = 0; i < numberOfPoints; i++)
          {
              int vertexIndex = rand.Next(5);
              PointF vertex = pentagonVertices[vertexIndex];

              // For pentagon fractal, we use a ratio of 1/φ (golden ratio conjugate) ≈ 0.618
              float ratio = 0.618f;

              currentPentagonPoint = new PointF(
                  currentPentagonPoint.X + ratio * (vertex.X - currentPentagonPoint.X),
                  currentPentagonPoint.Y + ratio * (vertex.Y - currentPentagonPoint.Y)
              );

              int x = (int)currentPentagonPoint.X;
              int y = (int)currentPentagonPoint.Y;

              if (x >= 0 && x < fractalBitmap.Width && y >= 0 && y < fractalBitmap.Height)
              {
                  int idx = y * stride + x * 4; // 4 bytes per pixel (BGRA)

                  // Use different colors for different vertices
                  Color color = pentagonColors[vertexIndex];
                  pixels[idx + 0] = color.B;     // B
                  pixels[idx + 1] = color.G;     // G
                  pixels[idx + 2] = color.R;     // R
                  pixels[idx + 3] = 255;         // A
              }
          }

          Marshal.Copy(pixels, 0, bmpData.Scan0, bytes);
      }
      finally
      {
          fractalBitmap.UnlockBits(bmpData);
      }

      totalPointsDrawnPentagon += numberOfPoints;
      lblInfo.Text = $"Points drawn: {totalPointsDrawnPentagon}";
  }*/
        private void DrawHexagonFractal()
        {
            if (fractalBitmap == null)
                InitializeFractalBitmap();

            if (currentHexagonPoint == PointF.Empty)
                currentHexagonPoint = RandomPointInPolygon(hexagonVertices);

            // Use LockBits for performance
            BitmapData bmpData = fractalBitmap.LockBits(new Rectangle(0, 0, fractalBitmap.Width, fractalBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(bmpData.Stride);
                int height = fractalBitmap.Height;
                int bytes = stride * height;

                byte[] pixels = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, pixels, 0, bytes);

                // More precise ratio for hexagonal fractal
                // This value ensures the points don't converge to vertices but create fractal patterns
                float ratio = 0.6631f; // More precise value

                for (int i = 0; i < numberOfPoints; i++)
                {
                    int vertexIndex = rand.Next(6);
                    PointF vertex = hexagonVertices[vertexIndex];

                    currentHexagonPoint = new PointF(
                        currentHexagonPoint.X + ratio * (vertex.X - currentHexagonPoint.X),
                        currentHexagonPoint.Y + ratio * (vertex.Y - currentHexagonPoint.Y)
                    );

                    int x = (int)currentHexagonPoint.X;
                    int y = (int)currentHexagonPoint.Y;

                    if (x >= 0 && x < fractalBitmap.Width && y >= 0 && y < fractalBitmap.Height)
                    {
                        int idx = y * stride + x * 4;

                        Color color = hexagonColors[vertexIndex];
                        pixels[idx + 0] = color.B;
                        pixels[idx + 1] = color.G;
                        pixels[idx + 2] = color.R;
                        pixels[idx + 3] = 255;
                    }
                }

                Marshal.Copy(pixels, 0, bmpData.Scan0, bytes);
            }
            finally
            {
                fractalBitmap.UnlockBits(bmpData);
            }

            totalPointsDrawnHexagon += numberOfPoints;
            lblInfo.Text = $"Points drawn: {totalPointsDrawnHexagon}";
        }
        private void DrawCarpetRecursive()
        {
            float margin = 50;
            float squareSize = Math.Min(canvas.Height, canvas.Width) - 2 * margin;
            float startX = (canvas.Width - squareSize) / 2;
            float startY = (canvas.Height - squareSize) / 2;

            fractalGraphics.Clear(Color.White);
            DrawCarpetRecursiveHelper(fractalGraphics, startX, startY, squareSize, 0, Brushes.Black);

            canvas.Invalidate();
        }

        private void DrawCarpetRecursiveHelper(Graphics g, float x, float y, float size, int depth, Brush brush)
        {
            if (depth >= carpetIteration)
            {
                g.FillRectangle(brush, x, y, size, size);
                return;
            }

            float segmentSize = size / 3f;

            // Draw the 8 sub-squares around the center void
            for (int rowIdx = 0; rowIdx < 3; rowIdx++)
            {
                for (int colIdx = 0; colIdx < 3; colIdx++)
                {
                    if (rowIdx == 1 && colIdx == 1) continue; // Skip center

                    float newX = x + colIdx * segmentSize;
                    float newY = y + rowIdx * segmentSize;
                    DrawCarpetRecursiveHelper(g, newX, newY, segmentSize, depth + 1, brush);
                }
            }
        }

        private void RestartCanvas()
        {
            CleanupResources();
            InitializeFractalBitmap();
            totalPointsDrawnTriangle = 0;
            totalPointsDrawnCarpet = 0;
            totalPointsDrawnPentagon = 0;
            totalPointsDrawnHexagon = 0;
            currentTrianglePoint = PointF.Empty;
            currentCarpetPoint = PointF.Empty;
            currentPentagonPoint = PointF.Empty;
            currentHexagonPoint = PointF.Empty;
            carpetIteration = 0;
            lblInfo.Text = "Points drawn: 0";

            if (currentFractal == FractalType.Carpet || currentFractal == FractalType.Pentagon || currentFractal == FractalType.Hexagon)
            {
                AskNumberOfPoints();
            }

            canvas.Invalidate();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (fractalBitmap != null)
            {
                e.Graphics.DrawImage(fractalBitmap, 0, 0);
            }
            // Draw triangle outline for reference
            if (currentFractal == FractalType.Triangle && chkShowGrid.Checked)
            {
                using (Pen trianglePen = new Pen(Color.DarkGray, 2))
                {
                    for (int i = 0; i < triangleVertices.Length; i++)
                    {
                        int next = (i + 1) % triangleVertices.Length;
                        e.Graphics.DrawLine(trianglePen, triangleVertices[i], triangleVertices[next]);
                    }

                    // Also draw colored dots at vertices
                    for (int i = 0; i < triangleVertices.Length; i++)
                    {
                        using (Brush vertexBrush = new SolidBrush(triangleColors[i]))
                        {
                            e.Graphics.FillEllipse(vertexBrush, triangleVertices[i].X - 3, triangleVertices[i].Y - 3, 6, 6);
                        }
                    }
                }
            }
            // Draw grid for Sierpinski Carpet
            if (currentFractal == FractalType.Carpet && chkShowGrid.Checked)
            {
                float margin = 50;
                float squareSize = Math.Min(canvas.Width, canvas.Height) - 2 * margin;
                float startX = (canvas.Width - squareSize) / 2f;
                float startY = (canvas.Height - squareSize) / 2f;

                using (Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                using (Pen outlinePen = new Pen(Color.DarkGray, 2))
                using (Brush voidBrush = new SolidBrush(Color.FromArgb(40, Color.Gray)))
                {
                    // Outer square
                    e.Graphics.DrawRectangle(outlinePen, startX, startY, squareSize, squareSize);

                    // Grid lines
                    for (int i = 1; i <= 2; i++)
                    {
                        float lineX = startX + i * squareSize / 3f;
                        float lineY = startY + i * squareSize / 3f;

                        e.Graphics.DrawLine(gridPen, lineX, startY, lineX, startY + squareSize);
                        e.Graphics.DrawLine(gridPen, startX, lineY, startX + squareSize, lineY);
                    }

                    // Central "void" area of the carpet
                    float voidSize = squareSize / 3f;
                    float voidX = startX + voidSize;
                    float voidY = startY + voidSize;
                    e.Graphics.FillRectangle(voidBrush, voidX, voidY, voidSize, voidSize);
                    e.Graphics.DrawRectangle(outlinePen, voidX, voidY, voidSize, voidSize);
                }
            }

            // Draw pentagon outline for reference
            if (currentFractal == FractalType.Pentagon && chkShowGrid.Checked)
            {
                using (Pen pentagonPen = new Pen(Color.DarkGray, 2))
                {
                    for (int i = 0; i < pentagonVertices.Length; i++)
                    {
                        int next = (i + 1) % pentagonVertices.Length;
                        e.Graphics.DrawLine(pentagonPen, pentagonVertices[i], pentagonVertices[next]);
                    }
                }
            }

            // Draw hexagon outline for reference
            if (currentFractal == FractalType.Hexagon && chkShowGrid.Checked)
            {
                using (Pen hexagonPen = new Pen(Color.DarkGray, 2))
                {
                    for (int i = 0; i < hexagonVertices.Length; i++)
                    {
                        int next = (i + 1) % hexagonVertices.Length;
                        e.Graphics.DrawLine(hexagonPen, hexagonVertices[i], hexagonVertices[next]);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Optional initialization code here
        }
    }
}