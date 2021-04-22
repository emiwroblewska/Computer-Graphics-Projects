using System;
using System.Collections.Generic;
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

                    var line = DrawingAlgorithms.CopyPixels(_x, _y, color, brushSize, dx, dy);
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

        public static List<PixelPoint> MidPointCircle(int origin_x, int origin_y, int R, Colour color)
        {
            List<PixelPoint> result = new List<PixelPoint>();
            int d = 1 - R;
            int x = 0;
            int y = R;

            while (y > x)
            {
                if (d < 0) //move to E
                    d += (2 * x) + 3;
                else //move to SE
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



    }
}
