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
        private int lightX, lightY, lightZ;

        private List<System.Drawing.Point> allPoints = new List<System.Drawing.Point>();
        private List<Point4> vertices = new List<Point4>();

        //private Diamond diamond;
        private Cylinder cylinder;

        public MainWindow()
        {
            InitializeComponent();
            bitmap = new WriteableBitmap(
               (int)Scene.Width,
               (int)Scene.Height,
               96,
               96,
               PixelFormats.Bgra32,
               BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            angleX = angleY = angleZ = 0;
            cPosX = 0;
            cPosY = 0;
            cPosZ = 200;
            sx = 300; sy = 200;
            cylinder = new Cylinder(bitmap, 30, 50, 20);
            cylinder.CreateCylinder();
            cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
        }

        public List<(Point4, Point4)> translate3Dto2D(List<(Point4, Point4)> vertices)
        {
            List<(Point4, Point4)> result = new List<(Point4, Point4)>();
            double alfaX = Math.PI * angleX / 180.0;
            double alfaY = Math.PI * angleY / 180.0;
            double alfaZ = Math.PI * angleZ / 180.0;
            //float sx = (float)Scene.Width / 2;
            //float sy = (float)Scene.Height / 2;
            double theta = Math.PI / 8;

            Matrix4x4 P = new Matrix4x4(-sx / (float)Math.Tan(theta), 0, sx, 0,
                                        0, sx / (float)Math.Tan(theta), sy, 0,
                                        0, 0, 0, 1,
                                        0, 0, 1, 0);

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

            //Matrix4x4 T = new Matrix4x4(1, 0, 0, tx,
            //                            0, 1, 0, ty,
            //                            0, 0, 1, tz,
            //                            0, 0, 0, 1);

            Vector3 CameraPos = new Vector3(cPosX, cPosY, cPosZ);
            Vector3 CameraTarget = new Vector3(0, 0, 0);
            Vector3 CameraUp = new Vector3(0, 1, 0);
            Vector3 cZ = Vector3.Divide(Vector3.Subtract(CameraPos, CameraTarget), Vector3.Subtract(CameraPos, CameraTarget).Length());
            Vector3 cX = Vector3.Divide(Vector3.Cross(CameraUp, cZ), Vector3.Cross(CameraUp, cZ).Length());
            Vector3 cY = Vector3.Divide(Vector3.Cross(cZ, cX), Vector3.Cross(cZ, cX).Length());

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
               
                result.Add((new Point4(x, y, z, w), v.Item2));
            }
            return result;
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
                cylinder.MyScene = bitmap;
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }
        private void HeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                cylinder.Height = (int)HeightSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                cylinder.MyScene = bitmap;
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }
        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                cylinder.Radius = (int)RadiusSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                cylinder.MyScene = bitmap;
                cylinder.ClearVertices();
                cylinder.CreateCylinder();
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        //Rotation
        private void AngleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            angleX = AngleXSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }
        private void AngleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            angleY = AngleYSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }
        private void AngleZSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cylinder != null)
            {
                angleZ = AngleZSlider.Value;
                bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                Scene.Source = bitmap;
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        //Translation
        private void SxSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sx = (int)SxSlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        private void SySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sy = (int)SySlider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        // Camera
        private void CamXslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosX = (int)CamXslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        private void CamYslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosY = (int)CamYslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        private void CamZslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cPosZ = (int)CamZslider.Value;
            bitmap = new WriteableBitmap((int)Scene.Width, (int)Scene.Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            Scene.Source = bitmap;
            if (cylinder != null)
            {
                cylinder.MyScene = bitmap;
                cylinder.DrawCylinder(translate3Dto2D(cylinder.Vertices));
            }
        }

        // Light
        private void LightXslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void LightYslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void LightZslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

       
    }
}
