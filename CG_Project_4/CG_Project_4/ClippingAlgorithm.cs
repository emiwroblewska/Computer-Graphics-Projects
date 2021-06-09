using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;

namespace CG_Project_4
{
    public static class ClippingAlgorithm
    {
        public static void DrawClippedLine(PixelPoint p1, PixelPoint p2, Shape shape, bool AA)
        {
            if (shape is Circle) return;
            List<PixelPoint> tmp;
            if (AA)
                tmp = DrawingAlgorithms.GuptaSproull(p1.X, p1.Y, p2.X, p2.Y, new Colour(Colors.Red), shape.GetThickness() + 2);
            else
                tmp = DrawingAlgorithms.lineDDA(p1.X, p1.Y, p2.X, p2.Y, new Colour(Colors.Red), shape.GetThickness() + 2);
            shape.AllPixels = shape.AllPixels.Union(tmp).ToList();
        }

        private static bool Clip(float denominator, float number, ref float tE, ref float tL)
        {
            if (denominator == 0)  //Paralel line
            {
                if (number < 0)
                    return false; // outside - discard
                return true; //skip to next edge
            }
            float t = number / denominator;
            if (denominator < 0) //PE
            { 
                if (t > tL) //tE > tL - discard
                    return false;
                if (t > tE)
                    tE = t;
            }
            else //denom < 0 - PL
            { 
                if (t < tE) //tL < tE - discard
                    return false;
                if (t < tL)
                    tL = t;
            }
            return true;
        }

        public static void LiangBarsky(PixelPoint p1, PixelPoint p2, Rectangle clip, Shape toClip, bool AA)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float tE = 0;
            float tL = 1;
            if (Clip(-dx, p1.X - clip.Left, ref tE, ref tL))
            {
                if (Clip(dx, clip.Right - p1.X, ref tE, ref tL))
                {
                    if (Clip(-dy, p1.Y - clip.Bottom, ref tE, ref tL))
                    {
                        if (Clip(dy, clip.Top - p1.Y, ref tE, ref tL))
                        {
                            if (tL < 1) 
                            {
                                p2.X = (int)Math.Round(p1.X + dx * tL); 
                                p2.Y = (int)Math.Round(p1.Y + dy * tL); 
                            }
                            if (tE > 0) 
                            { 
                                p1.X += (int)Math.Round(dx * tE);
                                p1.Y += (int)Math.Round(dy * tE); 
                            }
                            DrawClippedLine(p1, p2, toClip, AA);
                        }
                    }
                }
            }
        }

    }
}
