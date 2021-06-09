using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;


namespace CG_Project_4
{
    [Serializable]
    public class Colour
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public Colour()
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
        }
        public Colour(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
        public Colour(System.Windows.Media.Color color)
        {
            this.R = color.R;
            this.G = color.G;
            this.B = color.B;
        }
    }


    [Serializable]
    public class PixelPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Colour MyColor { get; set; }

        public PixelPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
            MyColor = new Colour(Colors.Black);
        }
        public PixelPoint(int x, int y, Colour c)
        {
            this.X = x;
            this.Y = y;
            this.MyColor = c;
        }
    }

    [Serializable]
    public abstract class Shape
    {
        public List<PixelPoint> AllPixels { get; set; }

        public Colour shapeColor { get; set; }

        protected int thickness;
        public void SetThickness(int newThickness)
        {
            if (thickness != -1)
            {
                thickness = newThickness;
                if (thickness < 1) thickness = 1;
                else if (thickness > 30) thickness = 29;
            }
        }
        public int GetThickness() { return thickness; }

        public void DrawPixels(WriteableBitmap bmp)
        {
            try
            {
                bmp.Lock();

                foreach (PixelPoint pp in AllPixels)
                {
                    int column = pp.X;
                    int row = pp.Y;
                    Colour p = pp.MyColor;
                    if (row >= 0 && column >= 0 && row < ((int)bmp.PixelHeight - 1) && column < (int)bmp.PixelWidth - 1)
                    {
                        unsafe
                        {
                            IntPtr pBackBuffer = bmp.BackBuffer;
                            pBackBuffer += row * bmp.BackBufferStride;
                            pBackBuffer += column * 4;

                            int color_data = 0;
                            color_data |= 255 << 24;           // A
                            color_data |= pp.MyColor.R << 16;  // R
                            color_data |= pp.MyColor.G << 8;   // G
                            color_data |= pp.MyColor.B << 0;   // B

                            *((int*)pBackBuffer) = color_data;
                        }
                        bmp.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    }
                }
            }
            finally
            {
                bmp.Unlock();
            }
        }
    }

    [Serializable]
    public class Line : Shape
    {
        public PixelPoint Start { get; set; }
        public PixelPoint End { get; set; }

        public Line(PixelPoint start, PixelPoint end, Colour colour)
        {
            this.Start = start;
            this.End = end;
            this.shapeColor = colour;
            this.thickness = 1;
            this.AllPixels = null;
        }
        public Line(PixelPoint start, PixelPoint end, Colour colour, int brushSize)
        {
            this.Start = start;
            this.End = end;
            this.shapeColor = colour;
            this.thickness = brushSize;
            this.AllPixels = null;
        }

    }


    [Serializable]
    public class Circle : Shape
    {
        public PixelPoint Origin { get; set; }
        public int Radius { get; set; }

        public Circle(PixelPoint origin, int r, Colour colour)
        {
            this.Origin = origin;
            this.Radius = r;
            this.shapeColor = colour;
            this.thickness = -1;
            this.AllPixels = null;
        }
    }

    [Serializable]
    public class Polygon : Shape
    {
        public List<PixelPoint> Vertices { get; set; }
        public Colour FillColor { get; set; }
        public Bitmap FillPattern { get; set; }
        public Polygon()
        {
            this.Vertices = new List<PixelPoint>() { };
            this.thickness = 1;
            this.shapeColor = new Colour(0, 0, 0);
            this.AllPixels = null;
            this.FillColor = null;
            this.FillPattern = null;
        }
        public Polygon(Colour colour, int brushSize)
        {
            this.Vertices = new List<PixelPoint>() { };
            this.thickness = brushSize;
            this.shapeColor = colour;
            this.AllPixels = null;
            this.FillColor = null;
            this.FillPattern = null;
        }

        public virtual void AddVertex(PixelPoint point)
        {
            Vertices.Add(point);
        }
        public virtual (int, int) Center()
        {
            int sum_x = 0, sum_y = 0;
            foreach (var vertex in Vertices)
            {
                sum_x += vertex.X;
                sum_y += vertex.Y;
            }
            sum_x /= Vertices.Count();
            sum_y /= Vertices.Count();
            return (sum_x, sum_y);
        }
    }

    [Serializable]
    public class Rectangle : Polygon
    {
        public Rectangle(PixelPoint p1, PixelPoint p2, Colour colour)
        {
            this.Vertices = new List<PixelPoint>() { };
            this.Vertices.Add(p1);
            this.Vertices.Add(new PixelPoint(p2.X, p1.Y, colour));
            this.Vertices.Add(p2);
            this.Vertices.Add(new PixelPoint(p1.X, p2.Y, colour));
            this.shapeColor = colour;
            this.thickness = 1;
            this.AllPixels = null;
            this.FillColor = null;
            this.FillPattern = null;
        }

        public Rectangle(PixelPoint p1, PixelPoint p2, int brushSize, Colour colour)
        {
            this.Vertices = new List<PixelPoint>() { };
            this.Vertices.Add(p1);
            this.Vertices.Add(new PixelPoint(p2.X, p1.Y, colour));
            this.Vertices.Add(p2);
            this.Vertices.Add(new PixelPoint(p1.X, p2.Y, colour));
            this.shapeColor = colour;
            this.thickness = brushSize;
            this.AllPixels = null;
            this.FillColor = null;
            this.FillPattern = null;
        }

        public override void AddVertex(PixelPoint point) { return; }

        public override (int, int) Center()
        {
            int centerX = (Vertices[0].X + Vertices[2].X) / 2;
            int centerY = (Vertices[0].Y + Vertices[2].Y) / 2;
            return (centerX, centerY);
        }

        public int Left
        {
            get { return Vertices.Min(v => v.X); }
        }
        public int Right
        {
            get { return Vertices.Max(v => v.X); }
        }
        public int Bottom
        {
            get { return Vertices.Min(v => v.Y); }
        }
        public int Top
        {
            get { return Vertices.Max(v => v.Y); }
        }
    }
}

