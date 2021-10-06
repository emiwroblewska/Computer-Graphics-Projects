using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Numerics;

namespace CG_Project_5
{
    public class Cylinder
    {
        public int N { get; set; }
        public int Height { get; set; }
        public int Radius { get; set; }
        public List<(Point4, Point4)> Vertices { get; }

        public Cylinder(int n, int h, int r)
        {
            N = n;
            Height = h;
            Radius = r;
            Vertices = new List<(Point4, Point4)>() { };
        }

        public void ClearVertices()
        {
            Vertices.Clear();
        }

        public void CreateCylinder()
        {
            //0...n       - top base
            //n+1...3n    - sides
            //3n+1...4n+1 - bottom base

            //Top base
            Vertices.Add((new Point4(0, Height, 0, 1), new Point4(0, 1, 0, 0)));
            for(int i = 0; i < N; i++)
            {
                double x = Radius * Math.Cos((2 * Math.PI * i) / N);
                double z = Radius * Math.Sin((2 * Math.PI * i) / N);
                Vertices.Add((new Point4(x, Height, z, 1), new Point4(0, 1, 0, 0)));
            }
            //Sides
            for (int i = 1; i <= N; i++)
            {
                double x = Radius * Math.Cos((2 * Math.PI * i) / N);
                double z = Radius * Math.Sin((2 * Math.PI * i) / N);
                Vertices.Add((new Point4(x, Height, z, 1), new Point4(x / Radius, 0, z / Radius, 0)));
            }
            for (int i = 1; i <= N; i++)
            {
                double x = Radius * Math.Cos((2 * Math.PI * i) / N);
                double z = Radius * Math.Sin((2 * Math.PI * i) / N);
                Vertices.Add((new Point4(x, 0, z, 1), new Point4(x / Radius, 0, z / Radius, 0)));
            }
            //Bottom base
            for (int i = 0; i < N; i++)
            {
                double x = Radius * Math.Cos((2 * Math.PI * i) / N);
                double z = Radius * Math.Sin((2 * Math.PI * i) / N);
                Vertices.Add((new Point4(x, 0, z, 1), new Point4(0, -1, 0, 0)));
            }
            Vertices.Add((new Point4(0, 0, 0, 1), new Point4(0, -1, 0, 0)));
        }


    }
}
