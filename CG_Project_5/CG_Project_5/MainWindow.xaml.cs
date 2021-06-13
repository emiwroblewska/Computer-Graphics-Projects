using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;

namespace CG_Project_5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;
        private double angleX, angleY, angleZ;
        private int sx, sy;
        private int cPosX, cPosY, cPosZ;
        private double lightX = 0;
        private double lightY = 25;
        private double lightZ = 0;
        private Vector3 Light;
        private Vector3 Ia;
        private Vector3 ka;
        private Vector3 ks;
        private Vector3 kd;

        //private List<System.Drawing.Point> allPoints = new List<System.Drawing.Point>();
        private List<Pixel> drawPoints = new List<Pixel>();

        //private Diamond diamond;
        private Cylinder cylinder;
        private Color lineColor = Colors.Gray;
        private Color fillColor = Colors.Gray;

        public MainWindow()
        {
            InitializeComponent();
            bitmap = new WriteableBitmap(
               600,
               600,
               96,
               96,
               PixelFormats.Bgra32,
               BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            angleX = angleY = angleZ = 0;
            cPosX = 0;
            cPosY = 0;
            cPosZ = 200;
            sx = 300; sy = 300;
            Light = new Vector3((float)lightX, (float)lightY, (float)lightZ);
            Ia = new Vector3(1, 1, 1);
            ka = new Vector3(0.5f, 0.5f, 0.5f);
            ks = new Vector3(0.75f, 0.75f, 0.75f);
            kd = new Vector3(0.25f, 0.25f, 0.25f);

            cylinder = new Cylinder(10, 50, 20);
            cylinder.CreateCylinder();
            DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
        }

       

        public List<Tuple<Point4, Point4, Point4>> translate3Dto2D(List<(Point4, Point4)> vertices)
        {
            //Item1 - projection result, Item2 - normal vector, Item3 - global coordinates
            List<Tuple<Point4, Point4, Point4>> result = new List<Tuple<Point4, Point4, Point4>>();
            double alfaX = Math.PI * angleX / 180.0;
            double alfaY = Math.PI * angleY / 180.0;
            double alfaZ = Math.PI * angleZ / 180.0;
            //float sx = (float)Scene.Width / 2;
            //float sy = (float)Scene.Height / 2;
            double theta = Math.PI / 8;

            Matrix4x4 P = new Matrix4x4(-sx / (float)Math.Tan(-theta), 0, sx, 0,
                                        0, sx / (float)Math.Tan(-theta), sy, 0,
                                        0, 0, 0, 1,
                                        0, 0, 1, 0);
            //Matrix4x4 P = new Matrix4x4((float)(Scene.Height / Scene.Width), 0, 0, 0,
            //                           0, 1, 0, 0,
            //                           0, 0, 0, -1,
            //                           0, 0, 1, 0);

            Matrix4x4 Rx = new Matrix4x4(1, 0, 0, 0,
                                         0, (float)Math.Cos(alfaX), -(float)Math.Sin(alfaX), 0,
                                         0, (float)Math.Sin(alfaX), (float)Math.Cos(alfaX), 0,
                                         0, 0, 0, 1);

            Matrix4x4 Ry = new Matrix4x4((float)Math.Cos(alfaY), 0, (float)Math.Sin(alfaY), 0,
                                         0, 1, 0, 0,
                                         -(float)Math.Sin(alfaY), 0, (float)Math.Cos(alfaY), 0,
                                         0, 0, 0, 1);

            Matrix4x4 Rz = new Matrix4x4((float)Math.Cos(alfaZ), -(float)Math.Sin(alfaZ), 0, 0,
                                         (float)Math.Sin(alfaZ), (float)Math.Cos(alfaZ), 0, 0,
                                         0, 0, 1, 0,
                                         0, 0, 0, 1);

            Vector3 CameraPos = new Vector3(cPosX, cPosY, cPosZ);
            Vector3 CameraTarget = new Vector3(0, 0, 0);
            Vector3 CameraUp = new Vector3(0, 1, 0);
            Vector3 cZ = Vector3.Normalize(Vector3.Subtract(CameraPos, CameraTarget)); //, Vector3.Subtract(CameraPos, CameraTarget).Length());
            Vector3 cX = Vector3.Normalize(Vector3.Cross(CameraUp, cZ)); //, Vector3.Cross(CameraUp, cZ).Length());
            Vector3 cY = Vector3.Normalize(Vector3.Cross(cZ, cX)); //, Vector3.Cross(cZ, cX).Length());Vector3.

            Matrix4x4 C = new Matrix4x4(cX.X, cX.Y, cX.Z, Vector3.Dot(cX, CameraPos),
                                        cY.X, cY.Y, cY.Z, Vector3.Dot(cY, CameraPos),
                                        cZ.X, cZ.Y, cZ.Z, Vector3.Dot(cZ, CameraPos),
                                        0, 0, 0, 1);

            Matrix4x4 matrix = Matrix4x4.Multiply(P, Matrix4x4.Multiply(C, Matrix4x4.Multiply(Rx, Matrix4x4.Multiply(Ry, Rz))));

            foreach (var v in vertices)
            {

                double x = matrix.M11 * v.Item1.X + matrix.M12 * v.Item1.Y + matrix.M13 * v.Item1.Z + matrix.M14 * v.Item1.W;
                double y = matrix.M21 * v.Item1.X + matrix.M22 * v.Item1.Y + matrix.M23 * v.Item1.Z + matrix.M24 * v.Item1.W;
                double z = matrix.M31 * v.Item1.X + matrix.M32 * v.Item1.Y + matrix.M33 * v.Item1.Z + matrix.M34 * v.Item1.W;
                double w = matrix.M41 * v.Item1.X + matrix.M42 * v.Item1.Y + matrix.M43 * v.Item1.Z + matrix.M44 * v.Item1.W;

                x /= w;
                y /= w;
                z /= w;
                w = 1;
                //x = (int)Math.Floor((Scene.Width / 2) * (1 + x));
                //y = (int)Math.Floor((Scene.Height / 2) * (1 - y));
                //x = (int)Math.Floor(Scene.Width * (1 - (x / 600)));
                //y = (int)Math.Floor(Scene.Height * (1 - (y / 600)));
                result.Add(new Tuple<Point4, Point4, Point4>(new Point4(x, y, z, w), v.Item2, v.Item1));
            }
            return result;
        }


        // ------------- DRAWING -------------
        public void lineDDA_3D(List<Pixel> result, Tuple<Point4, Point4, Point4> v1, Tuple<Point4, Point4, Point4> v2, Color color)
        {
            //List<Pixel> result = new List<Pixel>();
            int x1 = (int)v1.Item1.X;
            int y1 = (int)v1.Item1.Y;
            int x2 = (int)v2.Item1.X;
            int y2 = (int)v2.Item1.Y;
            if (y1 > y2)
            {
                int tx1 = x1;
                x1 = x2;
                x2 = tx1;
                int ty1 = y1;
                y1 = y2;
                y2 = ty1;
            }

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
                var interpolated = InterpolatePoint(new Point(x, y), v1, v2);
                Color finalColor = Phong(interpolated, color);
                result.Add(new Pixel(_x, _y, finalColor));
            }
        }
        public unsafe void FillTriangle(List<Tuple<Point4, Point4, Point4>> vert, Color initColor)
        {
            //vert: Item1 - projected coordinates, Item2 - normal vector, Item3 - global coordinates
            int[] indices = new int[vert.Count];
            List<Point4> tmp = new List<Point4>();
            foreach (var v in vert)
            {
                tmp.Add(v.Item1);
            }
            tmp.Sort((a, b) => a.Y.CompareTo(b.Y)); //a.compare to b
            for (int j = 0; j < tmp.Count; j++)
                indices[j] = vert.IndexOf(vert.Find(x => x.Item1 == tmp[j]));
            List<Tuple<int, double, double>> AET = new List<Tuple<int, double, double>>();
            int k = 0;
            int i = indices[0];
            int y, ymin, ymax;
            y = ymin = (int)vert[indices[0]].Item1.Y; //y,ymin
            ymax = (int)vert[indices[vert.Count - 1]].Item1.Y; //ymax

            while (y < ymax)
            {
                while ((int)vert[i].Item1.Y == y)
                {
                    if (i > 0)
                    {
                        if (vert[i - 1].Item1.Y > vert[i].Item1.Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[i - 1].Item1.Y, vert[i].Item1.Y), 
                                Low(vert[i - 1].Item1, vert[i].Item1).X, 
                                (double)(vert[i - 1].Item1.X - vert[i].Item1.X) / (vert[i - 1].Item1.Y - vert[i].Item1.Y)));
                    }
                    else
                    {
                        if (vert[vert.Count - 1].Item1.Y > vert[i].Item1.Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[vert.Count - 1].Item1.Y, vert[i].Item1.Y), 
                                Low(vert[vert.Count - 1].Item1, vert[i].Item1).X,
                                (double)(vert[vert.Count - 1].Item1.X - vert[i].Item1.X) / (vert[vert.Count - 1].Item1.Y - vert[i].Item1.Y)));
                    }
                    if (i < vert.Count - 1)
                    {
                        if (vert[i + 1].Item1.Y > vert[i].Item1.Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[i + 1].Item1.Y, vert[i].Item1.Y), 
                                Low(vert[i + 1].Item1, vert[i].Item1).X,
                                (double)(vert[i + 1].Item1.X - vert[i].Item1.X) / (vert[i + 1].Item1.Y - vert[i].Item1.Y)));
                    }
                    else
                    {
                        if (vert[0].Item1.Y > vert[i].Item1.Y)
                            AET.Add(new Tuple<int, double, double>((int)Math.Max(vert[0].Item1.Y, vert[i].Item1.Y), 
                                Low(vert[0].Item1, vert[i].Item1).X,
                                (double)(vert[0].Item1.X - vert[i].Item1.X) / (vert[0].Item1.Y - vert[i].Item1.Y)));
                    }
                    ++k;
                    i = indices[k];
                }
                AET.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                for (int j = 0; j < AET.Count; j += 2)
                {
                    if (j + 1 < AET.Count)
                    {
                        Tuple<Point4, Point4, Point4> pLeft, pRight;
                        double xL = AET[j].Item2;
                        double xR = AET[j + 1].Item2;
                        if(y > vert[indices[1]].Item1.Y)
                        {
                            if (vert[indices[1]].Item1.X < vert[indices[2]].Item1.X) //1 < 2
                            {
                                pLeft = InterpolatePoint(new Point(xL, y), vert[indices[1]], vert[indices[2]]); //1,2
                                pRight = InterpolatePoint(new Point(xR, y), vert[indices[0]], vert[indices[2]]);
                            }
                            else
                            {
                                pLeft = InterpolatePoint(new Point(xL, y), vert[indices[0]], vert[indices[2]]); 
                                pRight = InterpolatePoint(new Point(xR, y), vert[indices[1]], vert[indices[2]]); //1,2
                            }
                        }
                        else
                        {
                            pLeft = InterpolatePoint(new Point(xL, y), vert[indices[0]], LowerX(vert[indices[1]], vert[indices[2]])); //0,1,2
                            pRight = InterpolatePoint(new Point(xR, y), vert[indices[0]], UpperX(vert[indices[1]], vert[indices[2]])); //0,1,2
                        }
                        for (int x = (int)AET[j].Item2; x <= (int)AET[j + 1].Item2; x++)
                        {
                            Tuple<Point4, Point4, Point4> interpolated = InterpolatePoint(new Point(x, y), pLeft, pRight);
                            Color finalColor = Phong(interpolated, initColor);
                            drawPoints.Add(new Pixel(x, y, finalColor));
                        }
                    }
                }
                y++;
                AET.RemoveAll(x => x.Item1 <= y);
                for (int j = 0; j < AET.Count; j++)
                    AET[j] = new Tuple<int, double, double>(AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
            }
        }

        public Point4 Low(Point4 first, Point4 second)
        {
            if (first.Y < second.Y)
                return first;
            else
                return second;
        }

        public Tuple<Point4, Point4, Point4> LowerX(Tuple<Point4, Point4, Point4> first, Tuple<Point4, Point4, Point4> second)
        {
            if (first.Item1.X < second.Item1.X)
                return first;
            else
                return second;
        }

        public Tuple<Point4, Point4, Point4> UpperX(Tuple<Point4, Point4, Point4> first, Tuple<Point4, Point4, Point4> second)
        {
            if (first.Item1.X > second.Item1.X)
                return first;
            else
                return second;
        }

        public Tuple<Point4, Point4, Point4> InterpolatePoint(Point middleP, Tuple<Point4, Point4, Point4> vertex1, Tuple<Point4, Point4, Point4> vertex2)
        {
            //Item1 - projected coordinates, Item2 - normal vector, Item3 - global coordinates
            var v1 = vertex1; 
            var v2 = vertex2;
            if(v1.Item1.X > v2.Item1.X || v1.Item1.Y > v2.Item1.Y)
            {
                var tmp = v1; v1 = v2; v2 = tmp;
            }
            double u;
            double t = Vector2.Distance(new Vector2((float)v1.Item1.X, (float)v1.Item1.Y), new Vector2((float)middleP.X, (float)middleP.Y)) /
                       Vector2.Distance(new Vector2((float)v1.Item1.X, (float)v1.Item1.Y), new Vector2((float)v2.Item1.X, (float)v2.Item1.Y)); 

            Point4 pt = new Point4((v2.Item1.X - v1.Item1.X) * t + v1.Item1.X, (v2.Item1.Y - v1.Item1.Y) * t + v1.Item1.Y, 
                                    (v2.Item1.Z - v1.Item1.Z) * t + v1.Item1.Z, 1);

            if (v1.Item1.Z == v2.Item1.Z) u = t;
            else u = ((1d / pt.Z) - (1d / v1.Item1.Z)) / ((1d / v2.Item1.Z) - (1d / v1.Item1.Z));

            Point4 pG = new Point4(u * (v2.Item3.X - v1.Item3.X) + v1.Item3.X, u * (v2.Item3.Y - v1.Item3.Y) + v1.Item3.Y,
                                   u * (v2.Item3.Z - v1.Item3.Z) + v1.Item3.Z, 1); // u * (v2.Item3.W - v1.Item3.W) + v1.Item3.W);

            Point4 nG = new Point4(u * (v2.Item2.X - v1.Item2.X) + v1.Item2.X, u * (v2.Item2.Y - v1.Item2.Y) + v1.Item2.Y,
                                   u * (v2.Item2.Z - v1.Item2.Z) + v1.Item2.Z, 0); // u * (v2.Item2.W - v1.Item2.W) + v1.Item2.W);

            double nGlen = Math.Sqrt(nG.X * nG.X + nG.Y * nG.Y + nG.Z * nG.Z); //+ nG.W * nG.W);
            Point4 normalG = new Point4(nG.X / nGlen, nG.Y / nGlen, nG.Z / nGlen, 0);
            //Item1 - projected coordinates, Item2 - normal vector, Item3 - global coordinates
            Tuple<Point4, Point4, Point4> result = new Tuple<Point4, Point4, Point4>(pt, normalG, pG);
            return result;
        }

        public Color Phong(Tuple<Point4, Point4, Point4> triPoint, Color startColor)
        {
            //Item1 - projected coordinates, Item2 - normal vector, Item3 - global coordinates
            Vector3 p = new Vector3((float)triPoint.Item3.X, (float)triPoint.Item3.Y, (float)triPoint.Item3.Z);
            Vector3 n = new Vector3((float)triPoint.Item2.X, (float)triPoint.Item2.Y, (float)triPoint.Item2.Z);
            Vector3 camera = new Vector3(cPosX, cPosY, cPosZ);

            Vector3 v = Vector3.Divide(Vector3.Subtract(camera, p), Vector3.Subtract(camera, p).Length());
            Vector3 li = Vector3.Divide(Vector3.Subtract(Light, p), Vector3.Subtract(Light, p).Length());
            Vector3 ri = Vector3.Subtract(Vector3.Multiply(2 * (Vector3.Dot(n, li)), n), li);

            Vector3 I = new Vector3(Ia.X * ka.X, Ia.Y * ka.Y, Ia.Z * ka.Z);
            I.X += (float)(kd.X * Ia.X * Math.Max(Vector3.Dot(n, li), 0) + ks.X * Ia.X * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 1));
            I.Y += (float)(kd.Y * Ia.Y * Math.Max(Vector3.Dot(n, li), 0) + ks.Y * Ia.Y * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 1));
            I.Z += (float)(kd.Z * Ia.Z * Math.Max(Vector3.Dot(n, li), 0) + ks.Z * Ia.Z * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 1));
            //double fatt = 1.0 / Math.Sqrt(Math.Pow(p.X + Light.X, 2) + Math.Pow(p.Y + Light.Y, 2) + Math.Pow(p.Z + Light.Z, 2));
            //I.X += (float)(kd.X * 600 * fatt * Math.Max(Vector3.Dot(n, li), 0) + fatt * ks.X * 600 * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 5));
            //I.Y += (float)(kd.Y * 600 * fatt * Math.Max(Vector3.Dot(n, li), 0) + fatt * ks.Y * 600 * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 5));
            //I.Z += (float)(kd.Z * 600 * fatt * Math.Max(Vector3.Dot(n, li), 0) + fatt * ks.Z * 600 * Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), 5));

            Color result = new Color();
            result.A = startColor.A;
            result.R = (byte)Clamp(startColor.R * I.X);
            result.G = (byte)Clamp(startColor.G * I.Y);
            result.B = (byte)Clamp(startColor.B * I.Z);

            return result;
        }
        public static int Clamp(double val)
        {
            if (val > 255) return 255;
            else if (val < 0) return 0;
            else return (int)val;
        }


        // ---------------  Cylinder --------------------
        public void DrawCylinder(List<Tuple<Point4, Point4, Point4>> vertices2D, int N)
        {
            //vertices2D: Item1 - projected coordinates, Item2 - normal vector, Item3 - global coordinates
            for (int i = 0; i <= N - 2; i++)
            {
                if (BackFace(vertices2D[0].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1].Item1))
                {
                    Tuple<Point4, Point4, Point4>[] F1 = { vertices2D[0], vertices2D[i + 2], vertices2D[i + 1] };
                    FillTriangle(F1.ToList(), fillColor);
                    lineDDA_3D(drawPoints, vertices2D[0], vertices2D[i + 2], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 1], vertices2D[i + 2], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[0], vertices2D[i + 1], lineColor);
                }
            }
            if (BackFace(vertices2D[0].Item1, vertices2D[1].Item1, vertices2D[N].Item1))
            {
                Tuple<Point4, Point4, Point4>[] F11 = { vertices2D[0], vertices2D[1], vertices2D[N] };
                FillTriangle(F11.ToList(), fillColor);
                lineDDA_3D(drawPoints, vertices2D[0], vertices2D[1], lineColor);
                lineDDA_3D(drawPoints, vertices2D[1], vertices2D[N], lineColor);
                lineDDA_3D(drawPoints, vertices2D[0], vertices2D[N], lineColor);
            }

            for (int i = N; i <= 2 * N - 2; i++)
            {
                if (BackFace(vertices2D[i + 1].Item1, vertices2D[i + 2].Item1, vertices2D[i + 1 + N].Item1))
                {
                    Tuple<Point4, Point4, Point4>[] F2 = { vertices2D[i + 1], vertices2D[i + 2], vertices2D[i + 1 + N] };
                    FillTriangle(F2.ToList(), fillColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 1], vertices2D[i + 2], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 1], vertices2D[i + 1 + N], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 2], vertices2D[i + 1 + N], lineColor);
                }
            }
            if (BackFace(vertices2D[2 * N].Item1, vertices2D[N + 1].Item1, vertices2D[3 * N].Item1))
            {
                Tuple<Point4, Point4, Point4>[] F22 = { vertices2D[2 * N], vertices2D[N + 1], vertices2D[3 * N] };
                FillTriangle(F22.ToList(), fillColor);
                lineDDA_3D(drawPoints, vertices2D[2 * N], vertices2D[N + 1], lineColor);
                lineDDA_3D(drawPoints, vertices2D[N + 1], vertices2D[3 * N], lineColor);
                lineDDA_3D(drawPoints, vertices2D[2 * N], vertices2D[3 * N], lineColor);
            }

            for (int i = 2 * N; i <= 3 * N - 2; i++)
            {
                if (BackFace(vertices2D[i + 1].Item1, vertices2D[i + 2 - N].Item1, vertices2D[i + 2].Item1))
                {
                    Tuple<Point4, Point4, Point4>[] F3 = { vertices2D[i + 1], vertices2D[i + 2 - N], vertices2D[i + 2] };
                    FillTriangle(F3.ToList(), fillColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 1], vertices2D[i + 2], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 1], vertices2D[i + 2 - N], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 2], vertices2D[i + 2 - N], lineColor);
                }
            }
            if (BackFace(vertices2D[2 * N + 1].Item1, vertices2D[3 * N].Item1, vertices2D[N + 1].Item1))
            {
                Tuple<Point4, Point4, Point4>[] F33 = { vertices2D[2 * N + 1], vertices2D[3 * N], vertices2D[N + 1] };
                FillTriangle(F33.ToList(), fillColor);
                lineDDA_3D(drawPoints, vertices2D[2 * N + 1], vertices2D[N + 1], lineColor);
                lineDDA_3D(drawPoints, vertices2D[N + 1], vertices2D[3 * N], lineColor);
                lineDDA_3D(drawPoints, vertices2D[2 * N + 1], vertices2D[3 * N], lineColor);
            }

            for (int i = 3 * N; i <= 4 * N - 2; i++)
            {
                if (BackFace(vertices2D[4 * N + 1].Item1, vertices2D[i + 1].Item1, vertices2D[i + 2].Item1))
                {
                    Tuple<Point4, Point4, Point4>[] F4 = { vertices2D[4 * N + 1], vertices2D[i + 1], vertices2D[i + 2] };
                    FillTriangle(F4.ToList(), fillColor);
                    lineDDA_3D(drawPoints, vertices2D[4 * N + 1], vertices2D[i + 1], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[4 * N + 1], vertices2D[i + 2], lineColor);
                    lineDDA_3D(drawPoints, vertices2D[i + 2], vertices2D[i + 1], lineColor);
                }
            }
            if (BackFace(vertices2D[4 * N + 1].Item1, vertices2D[4 * N].Item1, vertices2D[3 * N + 1].Item1))
            {
                Tuple<Point4, Point4, Point4>[] F44 = { vertices2D[4 * N + 1], vertices2D[4 * N], vertices2D[3 * N + 1] };
                FillTriangle(F44.ToList(), fillColor);
                lineDDA_3D(drawPoints, vertices2D[4 * N + 1], vertices2D[4 * N], lineColor);
                lineDDA_3D(drawPoints, vertices2D[4 * N + 1], vertices2D[3 * N + 1], lineColor);
                lineDDA_3D(drawPoints, vertices2D[4 * N], vertices2D[3 * N + 1], lineColor);
            }

            Algorithms.DrawPixels(bitmap, drawPoints);
            drawPoints.Clear();
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


        // ---------------- Sliders --------------------
        // Cylinder
        private void Nslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                cylinder.N = (int)Nslider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                //cylinder.MyScene = bitmap;
                //Scene.Children.Clear();
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }
        private void HeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                cylinder.Height = (int)HeightSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                //cylinder.MyScene = bitmap;
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }
        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                cylinder.Radius = (int)RadiusSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                //cylinder.MyScene = bitmap;

                //Scene.Children.Clear();
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        //Rotation
        private void AngleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            angleX = AngleXSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }
        private void AngleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            angleY = AngleYSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }
        private void AngleZSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                angleZ = AngleZSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                //cylinder.MyScene = bitmap;
                //Scene.Children.Clear();
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        //Translation
        private void SxSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sx = (int)SxSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        private void SySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sy = (int)SySlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        // Camera
        private void CamXslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosX = (int)CamXslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        private void CamYslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosY = (int)CamYslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        private void CamZslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosZ = (int)CamZslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        // Light
        private void LightXslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lightX = LightXslider.Value;
            Light = new Vector3((float)lightX, (float)lightY, (float)lightZ);
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        private void LightYslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lightY = LightYslider.Value;
            Light = new Vector3((float)lightX, (float)lightY, (float)lightZ);
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

        private void LightZslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lightZ = LightZslider.Value;
            Light = new Vector3((float)lightX, (float)lightY, (float)lightZ);
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            //Scene.Children.Clear();
            if (cylinder != null)
            {
                //cylinder.MyScene = bitmap;
                DrawCylinder(translate3Dto2D(cylinder.Vertices), cylinder.N);
            }
        }

       
    }
}
