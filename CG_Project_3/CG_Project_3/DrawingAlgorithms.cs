using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_Project_3
{
    public static class DrawingAlgorithms
    {
        public static List<PixelPoint> lineDDA(int x1, int y1, int x2, int y2, Colour color, int brushSize)
        {
            List<PixelPoint> result = new List<PixelPoint>();
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

            if (brushSize > 1)
            {
                for (k = 0; k < steps; k++)
                {
                    x += mx;
                    _x = (int)x;
                    y += my;
                    _y = (int)y;

                    var line = CopyPixels(_x, _y, color, brushSize, dx, dy);
                    foreach (var p in line)
                        result.Add(p);
                }
            }
            else
            {
                for (k = 0; k < steps; k++)
                {
                    x += mx;
                    _x = (int)x;
                    y += my;
                    _y = (int)y;
                    result.Add(new PixelPoint(_x, _y, color));
                }
            }
            return result;
        }

        public static List<PixelPoint> CopyPixels(int x, int y, Colour color, int brushSize, int dx, int dy)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int size = (brushSize - 1) / 2;
            for (int i = -size; i <= size; i++)
            {
                if(Math.Abs(dx) >= Math.Abs(dy))
                    result.Add(new PixelPoint(x, y + i, color));
                else if(Math.Abs(dx) < Math.Abs(dy))
                    result.Add(new PixelPoint(x + i, y, color));
            }
            return result;
        }

        public static List<PixelPoint> MidpointCircle(int origin_x, int origin_y, int R, Colour color)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int d = 1 - R;
            int x = 0;
            int y = R;
            result.Add(new PixelPoint(origin_x, origin_y + y, color));
            result.Add(new PixelPoint(origin_x, origin_y - y, color));
            result.Add(new PixelPoint(origin_x - R, origin_y, color));
            result.Add(new PixelPoint(origin_x + R, origin_y, color));

            while (y > x)
            {
                if (d < 0)
                    d += (2 * x) + 3;
                else 
                {
                    d += (2 * x) - (2 * y) + 5;
                    --y;
                }
                ++x;
                result.Add(new PixelPoint(origin_x + x, origin_y + y, color));
                result.Add(new PixelPoint(origin_x + x, origin_y - y, color));
                result.Add(new PixelPoint(origin_x - x, origin_y + y, color));
                result.Add(new PixelPoint(origin_x - x, origin_y - y, color));

                result.Add(new PixelPoint(origin_x + y, origin_y + x, color));
                result.Add(new PixelPoint(origin_x + y, origin_y - x, color));
                result.Add(new PixelPoint(origin_x - y, origin_y + x, color));
                result.Add(new PixelPoint(origin_x - y, origin_y - x, color));
            }
            return result;
        }

        // ---------------- Anti-Aliasing ----------------
        public static double Lerp(int a, int b, double t)
        {
            return (1 - t) * a + t * b;
        }

        public static double cov(double d, double r)
        {
            if (d <= r)
                return ((Math.Acos(d / r) - d * Math.Sqrt(r * r - d * d)) / Math.PI);
            else
                return 0;
        }

        public static double Coverage(double w, double D, double r)
        {
            if (w >= r)
            {
                if (w <= D) return cov(D - w, r);
                else if (0 <= D && D <= w) return 1 - cov(w - D, r);
            }
            else
            {
                if (0 <= D && D <= w) return 1 - cov(w - D, r) - cov(w + D, r);
                else if (w <= D && D <= r - w) return cov(D - w, r) - cov(D + w, r);
                else if (r - w <= D && D <= r + w) return cov(D - w, r);
            }
            return 0;
        }

        public static double IntensifyPixel(List<PixelPoint> pixels, int x, int y, double thickness, double distance, Colour lineColor)
        {
            double r = 0.5;
            double cov = Coverage(thickness, distance, r);

            if (cov > 0)
            {
                byte R = (byte)Lerp(255, lineColor.R, cov);
                byte G = (byte)Lerp(255, lineColor.G, cov);
                byte B = (byte)Lerp(255, lineColor.B, cov);
                pixels.Add(new PixelPoint(x, y, new Colour(R, G, B)));
            }
            return cov;
        }

        private static void CopyPixelsAA(List<PixelPoint> pixels, int x, int y, double thickness, Colour c, double d_invDenom, double v_d, int dx, int dy)
        {
            pixels.Add(new PixelPoint(x, y, c));
            for (int i = 1; IntensifyPixel(pixels, x + i * dx, y + i * dy, thickness, i * d_invDenom - v_d, c) > 0; ++i) ;
            for (int i = 1; IntensifyPixel(pixels, x - i * dx, y - i * dy, thickness, i * d_invDenom + v_d, c) > 0; ++i) ;
        }

        public static List<PixelPoint> GuptaSproull(int x1, int y1, int x2, int y2, Colour color, double thickness)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int X1 = x1, X2 = x2, Y1 = y1, Y2 = y2;
            if (x2 < x1)
            {
                X1 = x2; Y1 = y2;
                X2 = x1; Y2 = y1;
            }
            int dy = Y2 - Y1;
            int dx = X2 - X1;
            if (Math.Abs(dx) > Math.Abs(dy)) //horizontal
                result = HorizontalLine(X1, Y1, X2, Y2, color, thickness);
            else
                result = VerticalLine(X1, Y1, X2, Y2, color, thickness);
            return result;
        }

        public static List<PixelPoint> HorizontalLine(int x1, int y1, int x2, int y2, Colour color, double thickness)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int d = 2 * (dy - dx);
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);
            int xStart = x1, yStart = y1;
            int xEnd = x2, yEnd = y2;

            int two_v_dx = 0; //numerator, v=0 for the first pixel
            double invDenom = 1 / (2 * Math.Sqrt(dx * dx + dy * dy)); //inverted denominator
            double two_dy_invDenom = 2 * dx * invDenom; //precomputed constant

            int delta = 1;
            if (y2 - y1 < 0) delta = -1;
            CopyPixelsAA(result, xStart, yStart, thickness / 2, color, two_dy_invDenom, two_v_dx * invDenom, 0, delta);
            CopyPixelsAA(result, xEnd, yEnd, thickness / 2, color, two_dy_invDenom, two_v_dx * invDenom, 0, -delta);
            while (xStart < xEnd)
            {
                xStart += 1;
                xEnd -= 1;
                if (d < 0)
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    yStart += delta;
                    yEnd -= delta;
                }
                CopyPixelsAA(result, xStart, yStart, thickness / 2, color, two_dy_invDenom, two_v_dx * invDenom, 0, delta);
                CopyPixelsAA(result, xEnd, yEnd, thickness / 2, color, two_dy_invDenom, two_v_dx * invDenom, 0, -delta);
            }
            return result;
        }

        public static List<PixelPoint> VerticalLine(int x1, int y1, int x2, int y2, Colour color, double thickness)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int d = 2 * (dx - dy);
            int dN = 2 * dx;
            int dNE = 2 * (dx - dy);
            int xStart = x1, yStart = y1;
            int xEnd = x2, yEnd = y2;

            int two_v_dy = 0; //numerator, v=0 for the first pixel
            double invDenom = 1 / (2 * Math.Sqrt(dx * dx + dy * dy)); //inverted denominator
            double two_dy_invDenom = 2 * dy * invDenom; //precomputed constant

            int delta = 1;
            if (y2 - y1 < 0) delta = -1;
            CopyPixelsAA(result, xStart, yStart, thickness / 2, color, two_dy_invDenom, two_v_dy * invDenom, 1, 0);
            CopyPixelsAA(result, xEnd, yEnd, thickness / 2, color, two_dy_invDenom, two_v_dy * invDenom, -1, 0);
            while (delta * (yStart - yEnd) < 0)
            {
                yStart += delta;
                yEnd -= delta;
                if (d < 0)
                {
                    two_v_dy = d + dy;
                    d += dN;
                }
                else
                {
                    two_v_dy = d - dy;
                    d += dNE;
                    xStart += 1;
                    xEnd -= 1;
                }
                CopyPixelsAA(result, xStart, yStart, thickness / 2, color, two_dy_invDenom, two_v_dy * invDenom, 1, 0); ;
                CopyPixelsAA(result, xEnd, yEnd, thickness / 2, color, two_dy_invDenom, two_v_dy * invDenom, -1, 0);
            }
            return result;
        }


        // ------------------- Capsule -------------------
        private static int Sign(int Dx, int Dy, int Ex, int Ey, int Fx, int Fy)
        {
            return Math.Sign((Ex - Dx) * (Fy - Dy) - (Ey - Dy) * (Fx - Dx));
        }

        public static List<PixelPoint> Capsule(int x1, int y1, int x2, int y2, int radius, Colour color)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int length = (int)Math.Round(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
            int Wx = (int)Math.Round(((double)(y2 - y1) / length) * radius);
            int Wy = (int)Math.Round(((double)(x2 - x1) / length) * radius) * -1;

            result = result.Union(lineDDA(x1 + Wx, y1 + Wy, x2 + Wx, y2 + Wy, color, 1)).ToList();
            result = result.Union(lineDDA(x1 - Wx, y1 - Wy, x2 - Wx, y2 - Wy, color, 1)).ToList();
            result = result.Union(CapsuleCircle(x1, y1, x1 + Wx, y1 + Wy, radius, color, false)).ToList();
            result = result.Union(CapsuleCircle(x2, y2, x2 + Wx, y2 + Wy, radius, color, true)).ToList();
            return result;
        }

        public static List<PixelPoint> CapsuleCircle(int Cx, int Cy, int Ex, int Ey, int R, Colour color, bool side)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int d = 1 - R;
            int x = 0;
            int y = R;

            while (y > x)
            {
                if (d < 0)
                    d += (2 * x) + 3;
                else
                {
                    d += (2 * x) - (2 * y) + 5;
                    --y;
                }
                ++x;
                if (side)
                {
                    if (Sign(Cx, Cy, Ex, Ey, Cx + x, Cy + y) >= 0) result.Add(new PixelPoint(Cx + x, Cy + y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx + x, Cy - y) >= 0) result.Add(new PixelPoint(Cx + x, Cy - y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - x, Cy + y) >= 0) result.Add(new PixelPoint(Cx - x, Cy + y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - x, Cy - y) >= 0) result.Add(new PixelPoint(Cx - x, Cy - y, color));

                    if (Sign(Cx, Cy, Ex, Ey, Cx + y, Cy + x) >= 0) result.Add(new PixelPoint(Cx + y, Cy + x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx + y, Cy - x) >= 0) result.Add(new PixelPoint(Cx + y, Cy - x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - y, Cy + x) >= 0) result.Add(new PixelPoint(Cx - y, Cy + x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - y, Cy - x) >= 0) result.Add(new PixelPoint(Cx - y, Cy - x, color));
                }
                else
                {
                    if (Sign(Cx, Cy, Ex, Ey, Cx + x, Cy + y) <= 0) result.Add(new PixelPoint(Cx + x, Cy + y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx + x, Cy - y) <= 0) result.Add(new PixelPoint(Cx + x, Cy - y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - x, Cy + y) <= 0) result.Add(new PixelPoint(Cx - x, Cy + y, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - x, Cy - y) <= 0) result.Add(new PixelPoint(Cx - x, Cy - y, color));

                    if (Sign(Cx, Cy, Ex, Ey, Cx + y, Cy + x) <= 0) result.Add(new PixelPoint(Cx + y, Cy + x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx + y, Cy - x) <= 0) result.Add(new PixelPoint(Cx + y, Cy - x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - y, Cy + x) <= 0) result.Add(new PixelPoint(Cx - y, Cy + x, color));
                    if (Sign(Cx, Cy, Ex, Ey, Cx - y, Cy - x) <= 0) result.Add(new PixelPoint(Cx - y, Cy - x, color));
                }
            }
            return result;
        }

    }
}
