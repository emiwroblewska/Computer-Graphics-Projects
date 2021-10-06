using System.Windows.Media;

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


}
