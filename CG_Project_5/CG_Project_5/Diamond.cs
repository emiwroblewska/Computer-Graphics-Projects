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

namespace CG_Project_5
{
    //Only for Canvas!
    public class Diamond
    {
        public List<Point4> vertices = new List<Point4>();
        public Canvas Canvas { get; set; }
        public Diamond(Canvas canvas)
        {
            Canvas = canvas;
            vertices = new List<Point4>();
        }

        public void CreateDiamond(double angle, double tx, double ty, double tz)
        {
            int l = 50;
            double a = Math.PI * angle / 180.0;
            double d = 300;

            vertices.Add(new Point4(-l, -l, -l, 1));
            vertices.Add(new Point4(+l, -l, -l, 1));
            vertices.Add(new Point4(+l, -l, +l, 1));
            vertices.Add(new Point4(-l, -l, +l, 1));
            vertices.Add(new Point4(-l, +l, -l, 1));
            vertices.Add(new Point4(+l, +l, -l, 1));
            vertices.Add(new Point4(+l, +l, +l, 1));
            vertices.Add(new Point4(-l, +l, +l, 1));

            //Diamond Part
            vertices.Add(new Point4(0, 2 * l, 0, 1));
            vertices.Add(new Point4(0, -2 * l, 0, 1));

            for (int i = 0; i < vertices.Count; i++)
            {
                double x = vertices[i].X;
                double y = vertices[i].Y;
                double z = vertices[i].Z;
                double w = vertices[i].W;

                //Rotation around Y
                vertices[i].X = Math.Cos(a) * x + Math.Sin(a) * z;
                vertices[i].Z = -Math.Sin(a) * z + Math.Cos(a) * z;

                //Translation by d
                vertices[i].X += tx;
                vertices[i].Y += ty;
                vertices[i].Z += tz;

                x = vertices[i].X;
                z = vertices[i].Z;
                w = vertices[i].W;

                //Projection
                vertices[i].X = (Canvas.Height / Canvas.Width) * x;
                vertices[i].Z = w;
                vertices[i].W = z;

                //Projecting 4d vector back to the 3d space
                vertices[i].X /= vertices[i].W;
                vertices[i].Y /= vertices[i].W;
                vertices[i].Z /= vertices[i].W;

                //Transform to screen coordinates
                x = vertices[i].X;
                y = vertices[i].Y;
                vertices[i].X = (Canvas.Width * (1 + x)) / (double)2;
                vertices[i].Y = (Canvas.Height * (1 - y)) / (double)2;
            }
        }

        public void DrawDiamond()
        {
            //Cube Part
            for (int i = 0; i < 3; i++)
            {
                DrawLine((int)vertices[i].X, (int)vertices[i].Y, (int)vertices[i + 1].X, (int)vertices[i + 1].Y);
                DrawLine((int)vertices[i + 4].X, (int)vertices[i + 4].Y, (int)vertices[i + 5].X, (int)vertices[i + 5].Y);
                DrawLine((int)vertices[i].X, (int)vertices[i].Y, (int)vertices[i + 4].X, (int)vertices[i + 4].Y);
            }
            DrawLine((int)vertices[0].X, (int)vertices[0].Y, (int)vertices[3].X, (int)vertices[3].Y);
            DrawLine((int)vertices[4].X, (int)vertices[4].Y, (int)vertices[7].X, (int)vertices[7].Y);
            DrawLine((int)vertices[3].X, (int)vertices[3].Y, (int)vertices[7].X, (int)vertices[7].Y);

            //Diamond Part
            DrawLine((int)vertices[0].X, (int)vertices[0].Y, (int)vertices[9].X, (int)vertices[9].Y);
            DrawLine((int)vertices[1].X, (int)vertices[1].Y, (int)vertices[9].X, (int)vertices[9].Y);
            DrawLine((int)vertices[2].X, (int)vertices[2].Y, (int)vertices[9].X, (int)vertices[9].Y);
            DrawLine((int)vertices[3].X, (int)vertices[3].Y, (int)vertices[9].X, (int)vertices[9].Y);

            DrawLine((int)vertices[4].X, (int)vertices[4].Y, (int)vertices[8].X, (int)vertices[8].Y);
            DrawLine((int)vertices[5].X, (int)vertices[5].Y, (int)vertices[8].X, (int)vertices[8].Y);
            DrawLine((int)vertices[6].X, (int)vertices[6].Y, (int)vertices[8].X, (int)vertices[8].Y);
            DrawLine((int)vertices[7].X, (int)vertices[7].Y, (int)vertices[8].X, (int)vertices[8].Y);
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.X1 = x1;
            myLine.X2 = x2;
            myLine.Y1 = y1;
            myLine.Y2 = y2;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Top;
            myLine.StrokeThickness = 1;
            Canvas.Children.Add(myLine);
        }

        public void ClearVertices()
        {
            vertices.Clear();
        }
    }
}
