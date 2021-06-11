using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
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

        public static List<System.Drawing.Point> lineDDA(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, System.Windows.Media.Color color)
        {
            List<System.Drawing.Point> result = new List<System.Drawing.Point>();
            int steps, k, _x, _y;
            float mx, my, x, y;
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (Math.Abs(dx) > Math.Abs(dy)) steps = Math.Abs(dx);
            else steps = Math.Abs(dy);

            mx = dx / (float)steps;
            my = dy / (float)steps;
            x = x1;
            y = y1;
            
            for (k = 0; k < steps; k++)
            {
                x += mx;
                _x = (int)x;
                y += my;
                _y = (int)y;
                DrawPixel(bitmap, _x, _y, color);
                result.Add(new System.Drawing.Point(_x, _y));
            }
            return result;
        }

        

        public static void FillTriangle(WriteableBitmap bmp, List<Point4> vertices, System.Windows.Media.Color color)
        {
            int N = vertices.Count();
            List<(int, double, double)> AET = new List<(int, double, double)>();
            var P = vertices;
            var P1 = P.OrderBy(p => p.Y).ToList();
            int[] indices = new int[N];
            for (int j = 0; j < N; j++)
                indices[j] = P.IndexOf(P.Find(x => x == P1[j]));
            int k = 0;
            int i = indices[k];
            int y, ymin, ymax;
            y = ymin = (int)(P[indices[0]].Y);
            ymax = (int)(P[indices[N - 1]].Y);
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
                            DrawPixel(bmp, x, y, color);
                        }
                    }
                }
                ++y;
                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => x.Item1 == y);

                for (int j = 0; j < AET.Count; j++)
                    AET[j] = (AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
        }

        internal static void DrawPixel(WriteableBitmap bmp, int x, int y, System.Windows.Media.Color color)
        {
            if (x < 0 || y < 0 || x >= bmp.PixelWidth || y >= bmp.PixelHeight)
                return;
            try
            {
                bmp.Lock();
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
            finally
            {
                bmp.Unlock();
            }
        }


    }
}
