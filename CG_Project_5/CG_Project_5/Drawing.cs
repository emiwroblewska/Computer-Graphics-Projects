using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;


namespace CG_Project_5
{
    public static class Drawing
    {
        public static double Distance(Point4 p1, Point4 p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static Tuple<Point4, Point4, Point4> LowerX(Tuple<Point4, Point4, Point4> first, Tuple<Point4, Point4, Point4> second)
        {
            if (first.Item1.X < second.Item1.X)
                return first;
            else
                return second;
        }
        public static Tuple<Point4, Point4, Point4> UpperX(Tuple<Point4, Point4, Point4> first, Tuple<Point4, Point4, Point4> second)
        {
            if (first.Item1.X > second.Item1.X)
                return first;
            else
                return second;
        }
        public static Point4 Low(Point4 first, Point4 second)
        {
            if (first.Y < second.Y)
                return first;
            else
                return second;
        }

        public static bool BackFace(Point4 p1, Point4 p2, Point4 p3)
        {
            Vector3 v1 = new Vector3((float)(p2.X - p1.X), (float)(p2.Y - p1.Y), 0);
            Vector3 v2 = new Vector3((float)(p3.X - p1.X), (float)(p3.Y - p1.Y), 0);
            Vector3 result = Vector3.Cross(v1, v2);
            if (result.Z > 0)
                return true;
            else
                return false;
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

    }
}
