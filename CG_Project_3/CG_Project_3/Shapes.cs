using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;


namespace CG_Project_3
{
    [Serializable]
    public class Colour
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public Colour(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
    }


    [Serializable]
    public class PixelPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Colour MyColor { get; set; }

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

                foreach(PixelPoint pp in AllPixels)
                {
                    int column = pp.X;
                    int row = pp.Y;
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
        public Polygon()
        {
            this.Vertices = new List<PixelPoint>() { };
            this.thickness = 1;
            this.shapeColor = new Colour(0, 0, 0);
            this.AllPixels = null;
        }
        public Polygon(Colour colour, int brushSize)
        {
            this.Vertices = new List<PixelPoint>() { };
            this.thickness = brushSize;
            this.shapeColor = colour;
            this.AllPixels = null;
        }
        
        public void AddVertex(PixelPoint point)
        {
            Vertices.Add(point);
        }

        public (int, int) Center()
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


}
