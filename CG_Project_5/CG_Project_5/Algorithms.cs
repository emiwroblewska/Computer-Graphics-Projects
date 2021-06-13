using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CG_Project_5
{
    public static class Algorithms
    {
        public static double Distance(Point4 p1, Point4 p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static void lineDDA(List<Pixel> result, int x1, int y1, int x2, int y2, Color color)
        {
            //List<Pixel> result = new List<Pixel>();
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
                //DrawPixel(bitmap, _x, _y, color);
                result.Add(new Pixel(_x, _y, color));
            }
            //return result;
        }

        

        public static Point4 Low(Point4 first, Point4 second)
        {
            if (first.Y < second.Y)
                return first;
            else
                return second;
        }

        public static unsafe void FillTriangle(List<Point4> vert, Color c, List<Pixel> drawPoints)
        {
            int[] indices = new int[vert.Count];
            List<Point4> tmp = new List<Point4>();
            foreach (var v in vert)
            {
                tmp.Add(v);
            }
            tmp.Sort((a, b) => a.Y.CompareTo(b.Y));
            for (int j = 0; j < tmp.Count; j++)
                indices[j] = vert.IndexOf(vert.Find(x => x == tmp[j]));
            List<Tuple<int, double, double>> AET = new List<Tuple<int, double, double>>();
            int k = 0;
            int i = indices[0];
            int y, ymin, ymax;
            y = ymin = (int)vert[indices[0]].Y;
            ymax = (int)vert[indices[vert.Count - 1]].Y;

            while (y < ymax)
            {
                while ((int)vert[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (vert[i - 1].Y > vert[i].Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[i - 1].Y, vert[i].Y), Low(vert[i - 1], vert[i]).X,
                                                                (double)(vert[i - 1].X - vert[i].X) / (vert[i - 1].Y - vert[i].Y)));
                    }
                    else
                    {
                        if (vert[vert.Count - 1].Y > vert[i].Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[vert.Count - 1].Y, vert[i].Y), Low(vert[vert.Count - 1], vert[i]).X,
                                                                (double)(vert[vert.Count - 1].X - vert[i].X) / (vert[vert.Count - 1].Y - vert[i].Y)));
                    }
                    if (i < vert.Count - 1)
                    {
                        if (vert[i + 1].Y > vert[i].Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[i + 1].Y, vert[i].Y), Low(vert[i + 1], vert[i]).X,
                                                                (double)(vert[i + 1].X - vert[i].X) / (vert[i + 1].Y - vert[i].Y)));
                    }
                    else
                    {
                        if (vert[0].Y > vert[i].Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[0].Y, vert[i].Y), Low(vert[0], vert[i]).X,
                                                                (double)(vert[0].X - vert[i].X) / (vert[0].Y - vert[i].Y)));
                    }
                    ++k;
                    i = indices[k];
                }
                AET.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                for (int j = 0; j < AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        for (int x = (int)AET[j].Item2; x <= (int)AET[j + 1].Item2; x++)
                        {
                            drawPoints.Add(new Pixel(x, y, c));
                        }
                    }
                }
                y++;
                AET.RemoveAll(x => x.Item1 <= y);
                for (int j = 0; j < AET.Count; j++)
                    AET[j] = new Tuple<int, double, double>(AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
        }


        public static void DrawPixels(WriteableBitmap bitmap, List<Pixel> drawPoints)
        {
            try
            {
                bitmap.Lock();
                foreach (Pixel p in drawPoints)
                {
                    int column = p.X;
                    int row = p.Y;
                    if (row >= 0 && column >= 0 && row < (int)bitmap.Height - 1 && column < (int)bitmap.Width - 1)
                    {
                        unsafe
                        {
                            IntPtr pBackBuffer = bitmap.BackBuffer;
                            pBackBuffer += row * bitmap.BackBufferStride;
                            pBackBuffer += column * 4;
                            int color_data = 0;
                            color_data |= p.color.A << 24;    // A
                            color_data |= p.color.R << 16;    // R
                            color_data |= p.color.G << 8;     // G
                            color_data |= p.color.B << 0;     // B
                            *((int*)pBackBuffer) = color_data;
                        }
                        bitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    }
                }
            }
            finally
            {
                bitmap.Unlock();
            }
        }


        public static Tuple<Point4, Point4> InterpolatePoint(Tuple<Point4, Point4, Point4> v1, Tuple<Point4, Point4, Point4> v2)
        {
            //Item1 - Global coordinates, Item2 - normal vector, Item3 - projected 2D coordinates
            double u;
            double t = 0; //????
            Point4 pt = new Point4((v2.Item3.X - v1.Item3.X) * t + v1.Item3.X, (v2.Item3.Y - v1.Item3.Y) * t + v1.Item3.Y, (v2.Item3.Z - v1.Item3.Z) * t + v1.Item3.Z, 1);

            if (v1.Item3.Z == v2.Item3.Z || v1.Item3.Z == pt.Z)
                u = t;
            else
                u = ((1d / pt.Z) - (1d / v1.Item3.Z)) / ((1d / v2.Item3.Z) - (1d / v1.Item3.Z));

            Point4 pG = new Point4(u * (v2.Item1.X - v1.Item1.X) + v1.Item1.X, u * (v2.Item1.Y - v1.Item1.Y) + v1.Item1.Y,
                                   u * (v2.Item1.Z - v1.Item1.Z) + v1.Item1.Z, u * (v2.Item1.W - v1.Item1.W) + v1.Item1.W);

            Point4 nG = new Point4(u * (v2.Item2.X - v1.Item2.X) + v1.Item2.X, u * (v2.Item2.Y - v1.Item2.Y) + v1.Item2.Y,
                                   u * (v2.Item2.Z - v1.Item2.Z) + v1.Item2.Z, u * (v2.Item2.W - v1.Item2.W) + v1.Item2.W);

            //Item1 - Global coordinates, Item2 - normal vector
            Tuple<Point4, Point4> result = new Tuple<Point4, Point4>(pG, nG);
            return result;
        }
    }
}
