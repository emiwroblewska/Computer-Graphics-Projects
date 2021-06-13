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
        //public Canvas MyScene { get; set; }

        public Cylinder(int n, int h, int r)
        {
            N = n;
            Height = h;
            Radius = r;
            //MyScene = scene;
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


        public void DrawCylinder(List<(Point4, Point4)> vertices2D)
        {
            List<Pixel> pixels = new List<Pixel>();
            //Top Base
            for (int i = 0; i <= N - 2; i++)
            {
                if (BackFace(vertices2D[0].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1].Item1))
                {
                    Point4[] T1 = { vertices2D[0].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1].Item1 };
                    //Drawing.FillTriangle(MyScene, T1.ToList(), Colors.Gray, pixels);
                    DrawTriangle(vertices2D[0].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1].Item1);
                }
            }
            if (BackFace(vertices2D[0].Item1, vertices2D[1].Item1, vertices2D[N].Item1))
            {
                Point4[] T2 = { vertices2D[0].Item1, vertices2D[1].Item1, vertices2D[N].Item1 };
                //Drawing.FillTriangle(MyScene, T2.ToList(), Colors.Gray, pixels);
                DrawTriangle(vertices2D[0].Item1, vertices2D[1].Item1, vertices2D[N].Item1);
            }

            //Side triangles with base at the top
            for (int i = N; i <= 2 * N - 2; i++)
            {
                if (BackFace(vertices2D[i + 1].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1 + N].Item1))
                {
                    Point4[] S11 = { vertices2D[i + 1].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1 + N].Item1 };
                    //Drawing.FillTriangle(MyScene, S11.ToList(), Colors.Gray, pixels);
                    DrawTriangle(vertices2D[i + 1].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1 + N].Item1);
                }
            }
            if (BackFace(vertices2D[2 * N].Item1, vertices2D[N + 1].Item1, vertices2D[3 * N].Item1))
            {
                Point4[] S12 = { vertices2D[2 * N].Item1, vertices2D[N + 1].Item1, vertices2D[3 * N].Item1 };
                //Drawing.FillTriangle(MyScene, S12.ToList(), Colors.Gray, pixels);
                DrawTriangle(vertices2D[2 * N].Item1, vertices2D[N + 1].Item1, vertices2D[3 * N].Item1);
            }

            //Side triangles with base at the bottom
            for (int i = 2 * N; i <= 3 * N - 2; i++)
            {
                if (BackFace(vertices2D[i + 1].Item1, vertices2D[i + 2 - N].Item1, vertices2D[i + 2].Item1))
                {
                    Point4[] S21 = { vertices2D[i + 1].Item1, vertices2D[i + 2 - N].Item1, vertices2D[i + 2].Item1 };
                    //Drawing.FillTriangle(MyScene, S21.ToList(), Colors.Gray, pixels);
                    DrawTriangle(vertices2D[i + 1].Item1, vertices2D[i + 2 - N].Item1, vertices2D[i + 2].Item1);
                }
            }
            if (BackFace(vertices2D[2 * N + 1].Item1, vertices2D[3 * N].Item1, vertices2D[N + 1].Item1))
            {
                Point4[] S22 = { vertices2D[2 * N + 1].Item1, vertices2D[3 * N].Item1, vertices2D[N + 1].Item1 };
                //Drawing.FillTriangle(MyScene, S22.ToList(), Colors.Gray, pixels);
                DrawTriangle(vertices2D[2 * N + 1].Item1, vertices2D[3 * N].Item1, vertices2D[N + 1].Item1);
            }

            //Bottom base
            for (int i = 3 * N; i <= 4 * N - 2; i++)
            {
                if (BackFace(vertices2D[4 * N + 1].Item1, vertices2D[i + 1].Item1, vertices2D[i + 2].Item1))
                {
                    Point4[] B1 = { vertices2D[4 * N + 1].Item1, vertices2D[i + 1].Item1, vertices2D[i + 2].Item1 };
                    //Drawing.FillTriangle(MyScene, B1.ToList(), Colors.Gray, pixels);
                    DrawTriangle(vertices2D[4 * N + 1].Item1, vertices2D[i + 1].Item1, vertices2D[i + 2].Item1);
                }
            }
            if (BackFace(vertices2D[4 * N + 1].Item1, vertices2D[4 * N].Item1, vertices2D[3 * N + 1].Item1))
            {
                Point4[] B2 = { vertices2D[4 * N + 1].Item1, vertices2D[4 * N].Item1, vertices2D[3 * N + 1].Item1 };
                //Drawing.FillTriangle(MyScene, B2.ToList(), Colors.Gray, pixels);
                DrawTriangle(vertices2D[4 * N + 1].Item1, vertices2D[4 * N].Item1, vertices2D[3 * N + 1].Item1);
            }

            //Drawing.DrawPixels(MyScene, pixels);
            //pixels.Clear();
        }

        public void DrawTriangle(Point4 v1, Point4 v2, Point4 v3)
        {
            Polygon poly = new Polygon();
            poly.Stroke = Brushes.Gray;
            poly.StrokeLineJoin = PenLineJoin.Bevel;
            poly.Fill = Brushes.Gray;
            //poly.StrokeThickness = 0.3;
            Point Point1 = new Point(v1.X, v1.Y);
            Point Point2 = new Point(v2.X, v2.Y);
            Point Point3 = new Point(v3.X, v3.Y);
            PointCollection points = new PointCollection();
            points.Add(Point1);
            points.Add(Point2);
            points.Add(Point3);
            poly.Points = points;
            //MyScene.Children.Add(poly);

            //Drawing.lineDDA(MyScene, (int)v1.X, (int)v1.Y, (int)v2.X, (int)v2.Y, Colors.Black);
            //Drawing.lineDDA(MyScene, (int)v2.X, (int)v2.Y, (int)v3.X, (int)v3.Y, Colors.Black);
            //Drawing.lineDDA(MyScene, (int)v1.X, (int)v1.Y, (int)v3.X, (int)v3.Y, Colors.Black);

            //Point4[] tmp = { v1, v2, v3 };
            //Drawing.FillTriangle(MyScene, tmp.ToList(), Colors.Gray);
        }

        public bool BackFace(Point4 p1, Point4 p2, Point4 p3)
        {
            Vector3 v1 = new Vector3((float)(p2.X - p1.X), (float)(p2.Y - p1.Y), 0);
            Vector3 v2 = new Vector3((float)(p3.X - p1.X), (float)(p3.Y - p1.Y), 0);
            Vector3 result = Vector3.Cross(v1, v2);
            if (result.Z > 0) 
                return true;
            else 
                return false;
        }

        

    }
}
