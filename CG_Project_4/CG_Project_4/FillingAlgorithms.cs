using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CG_Project_4
{
    public static class FillingAlgorithms
    {
        public static byte Clamp(int value)
        {
            int result = value;
            if (value.CompareTo(255) > 0)
                result = 255;
            if (value.CompareTo(0) < 0)
                result = 0;
            return (byte)result;
        }
        private static PixelPoint LowwerY(PixelPoint p1, PixelPoint p2)
        {
            if (p1.Y <= p2.Y) return p1;
            return p2;
        }
        private static PixelPoint UpperY(PixelPoint p1, PixelPoint p2)
        {
            if (p1.Y >= p2.Y) return p1;
            return p2;
        }

        public static void FillPolygon(List<PixelPoint> vertices, Polygon poly, Colour color)
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
            y = ymin = P[indices[0]].Y;
            ymax = P[indices[N - 1]].Y;
            while (y < ymax)
            {
                while (P[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (P[i - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i - 1], P[i]);
                            var u = UpperY(P[i - 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[i - 1].X - P[i].X) / (P[i - 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[N - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[N - 1], P[i]);
                            var u = UpperY(P[N - 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[N - 1].X - P[i].X) / (P[N - 1].Y - P[i].Y)));
                        }
                    }
                    if (i < N - 1)
                    {
                        if (P[i + 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i + 1], P[i]);
                            var u = UpperY(P[i + 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[i + 1].X - P[i].X) / (P[i + 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[0].Y > P[i].Y)
                        {
                            var l = LowwerY(P[0], P[i]);
                            var u = UpperY(P[0], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[0].X - P[i].X) / (P[0].Y - P[i].Y)));
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
                            poly.AllPixels.Add(new PixelPoint(x, y, color));
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

        public static unsafe void FillPolygon(List<PixelPoint> vertices, Polygon poly, Bitmap pattern)
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
            int y, ymin, ymax, xmin;
            y = ymin = P[indices[0]].Y;
            ymax = P[indices[N - 1]].Y;
            xmin = P.OrderBy(p => p.X).First().X;

            BitmapData bData = pattern.LockBits(new System.Drawing.Rectangle(0, 0, (int)pattern.Width, (int)pattern.Height), ImageLockMode.ReadOnly, pattern.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            while (y < ymax)
            {
                while (P[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (P[i - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i - 1], P[i]);
                            var u = UpperY(P[i - 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[i - 1].X - P[i].X) / (P[i - 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[N - 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[N - 1], P[i]);
                            var u = UpperY(P[N - 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[N - 1].X - P[i].X) / (P[N - 1].Y - P[i].Y)));
                        }
                    }
                    if (i < N - 1)
                    {
                        if (P[i + 1].Y > P[i].Y)
                        {
                            var l = LowwerY(P[i + 1], P[i]);
                            var u = UpperY(P[i + 1], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[i + 1].X - P[i].X) / (P[i + 1].Y - P[i].Y)));
                        }
                    }
                    else
                    {
                        if (P[0].Y > P[i].Y)
                        {
                            var l = LowwerY(P[0], P[i]);
                            var u = UpperY(P[0], P[i]);
                            AET.Add((u.Y, l.X, (double)(P[0].X - P[i].X) / (P[0].Y - P[i].Y)));
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
                            Colour color = new Colour();
                            unsafe
                            {
                                byte* tmp = scan0 + ((y - ymin) % bData.Height) * bData.Stride + ((x - xmin) % bData.Width) * bitsPerPixel / 8;
                                color.B = tmp[0];
                                color.G = tmp[1];
                                color.R = tmp[2];
                            }
                            poly.AllPixels.Add(new PixelPoint(x, y, color));
                        }
                    }
                }
                ++y;
                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => x.Item1 == y);
                
                for (int j = 0; j < AET.Count; j++)
                    AET[j] = (AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
            pattern.UnlockBits(bData);
        }

        // -------------------- Flod Fill ---------------------
        public static unsafe void FloodFill(WriteableBitmap bitmap, int x, int y, Bitmap pattern)
        {
            Stack<(int, int)> myStack = new Stack<(int, int)>();
            myStack.Push((x, y));
            System.Windows.Media.Color oldColor = GetColor(bitmap, x, y); 

            BitmapData bData = pattern.LockBits(new System.Drawing.Rectangle(0, 0, (int)pattern.Width, (int)pattern.Height), ImageLockMode.ReadOnly, pattern.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            bitmap.Lock();
            while (myStack.Count != 0)
            {
                var item = myStack.Pop();
                if (item.Item1 < 0 || item.Item1 >= bitmap.PixelWidth || item.Item2 < 0 || item.Item2 >= bitmap.PixelHeight)
                    continue;

                var col = GetColor(bitmap, item.Item1, item.Item2);
                if (col.Equals(oldColor))
                {
                    System.Windows.Media.Color newColor = new System.Windows.Media.Color();
                    byte* tmp = scan0 + (item.Item2 % bData.Height) * bData.Stride + (item.Item1 % bData.Width) * bitsPerPixel / 8;
                    newColor.B = tmp[0];
                    newColor.G = tmp[1];
                    newColor.R = tmp[2];
                    newColor.A = 255;

                    DrawPixel(bitmap, item.Item1, item.Item2, newColor);
                    myStack.Push((item.Item1 + 1, item.Item2));
                    myStack.Push((item.Item1 - 1, item.Item2));
                    myStack.Push((item.Item1, item.Item2 + 1));
                    myStack.Push((item.Item1, item.Item2 - 1));
                }
            }
            pattern.UnlockBits(bData);
            bitmap.Unlock();
        }

        public static System.Windows.Media.Color GetColor(WriteableBitmap bitmap, int x, int y)
        {
            var color = new System.Windows.Media.Color();
            if (x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
                return color;
            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer + y * bitmap.BackBufferStride + x * 4;

                int color_data = *((int*)pBackBuffer);
                color.B = (byte)((color_data & 0x000000FF) >> 0);
                color.G = (byte)((color_data & 0x0000FF00) >> 8);
                color.R = (byte)((color_data & 0x00FF0000) >> 16);
                color.A = (byte)((color_data & 0xFF000000) >> 24);
            }
            return color;
        }

        public static void DrawPixel(WriteableBitmap bitmap, int x, int y, System.Windows.Media.Color color)
        {
            if (x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
                return;
            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer + y * bitmap.BackBufferStride + x * 4;

                int color_data = 0;
                color_data |= color.A << 24;    // A
                color_data |= color.R << 16;    // R
                color_data |= color.G << 8;     // G
                color_data |= color.B << 0;     // B

                *((int*)pBackBuffer) = color_data;
            }
            bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }


    }
}
