using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Numerics;

namespace CG_Project_5
{
    public class Point4
    {
        public double X;
        public double Y;
        public double Z;
        public double W;

        public Point4(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    public class Pixel
    {
        public int X;
        public int Y;
        public Color color;

        public Pixel(int x, int y, Color c)
        {
            X = x;
            Y = y;
            color = c;
        }
    }


    public static class Drawing
    {
        private static Point4 LowwerY(Point4 p1, Point4 p2)
        {
            if (p1.Y <= p2.Y) return p1;
            return p2;
        }
        private static Point4 UpperY(Point4 p1, Point4 p2)
        {
            if (p1.Y >= p2.Y) return p1;
            return p2;
        }

       

        

        public static void FillTriangle(WriteableBitmap bmp, List<Point4> vertices, Color color, List<Pixel> result)
        {
            int N = vertices.Count();
            //List<Pixel> result = new List<Pixel>();
            List<(int, double, double)> AET = new List<(int, double, double)>();
            var P = vertices;
            var P1 = P.OrderBy(p => p.Y).ToList();
            int[] indices = new int[N];
            for (int j = 0; j < N; j++)
                indices[j] = P.IndexOf(P.Find(x => x == P1[j]));
            int k = 0;
            int i = indices[k];
            int y, ymin, ymax;
            y = ymin = (int)Math.Round(P[indices[0]].Y);
            ymax = (int)Math.Round(P[indices[N - 1]].Y);
            //bmp.Lock();
            while (y < ymax)
            {
                while ((int)P[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (P[i - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i - 1], P[i]);
                            var u = UpperY(P[i - 1], P[i]);
                            AET.Add(((int)u.Y, l.X, (double)(P[i - 1].X - P[i].X) / (P[i - 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[N - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[N - 1], P[i]);
                            var u = UpperY(P[N - 1], P[i]);
                            AET.Add(((int)u.Y, l.X, (double)(P[N - 1].X - P[i].X) / (P[N - 1].Y - P[i].Y)));
                        }
                    }
                    if (i < N - 1)
                    {
                        if (P[i + 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i + 1], P[i]);
                            var u = UpperY(P[i + 1], P[i]);
                            AET.Add(((int)u.Y, l.X, (double)(P[i + 1].X - P[i].X) / (P[i + 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[0].Y > P[i].Y)
                        {
                            var l = LowwerY(P[0], P[i]);
                            var u = UpperY(P[0], P[i]);
                            AET.Add(((int)u.Y, l.X, (double)(P[0].X - P[i].X) / (P[0].Y - P[i].Y)));
                        }
                    }
                    ++k;
                    i = indices[k];
                }
                //sort AET by x value
                AET = AET.OrderBy(item => item.Item2).ToList();
                //fill pixels between pairs of intersections
                for (int j = 0; j < AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        for (int x = (int)AET[j].Item2; x <= (int)AET[j + 1].Item2; x++)
                        {
                            if (x > 0 && y > 0 && x < bmp.PixelWidth && y < bmp.PixelHeight)
                            {
                                result.Add(new Pixel(x, y, color));
                            }
                            
                        }
                    }
                }
                ++y;
                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => x.Item1 == y);

                for (int j = 0; j < AET.Count; j++)
                    AET[j] = (AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
            //bmp.Unlock();
            //return result;
        }

        public static void DrawPixel(WriteableBitmap bmp, int x, int y, System.Windows.Media.Color color)
        {
            if (x < 0 || y < 0 || x >= bmp.PixelWidth || y >= bmp.PixelHeight)
                return;
            unsafe
            {
                IntPtr pBackBuffer = bmp.BackBuffer + y * bmp.BackBufferStride + x * 4;

                int color_data = 0;
                color_data |= color.A << 24;    // A
                color_data |= color.R << 16;    // R
                color_data |= color.G << 8;     // G
                color_data |= color.B << 0;     // B

                *((int*)pBackBuffer) = color_data;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }

        public static void DrawPixels(WriteableBitmap bmp, List<Pixel> pixels)
        {
            try
            {
                bmp.Lock();
                foreach (Pixel pp in pixels)
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
                            color_data |= pp.color.A << 24;           // A
                            color_data |= pp.color.R << 16;  // R
                            color_data |= pp.color.G << 8;   // G
                            color_data |= pp.color.B << 0;   // B

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
}
