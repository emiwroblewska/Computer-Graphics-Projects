using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Globalization;

namespace CG_Project_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //----------------- Variables ------------------
        private static int brightnessConst = 20;
        private static int contrastConst = 2;
        private static double gammaConst = 1.2;
        private double[] gridFilter;

        private int[,] blurKernel = {{ 1, 1, 1 },
                                     { 1, 1, 1 },
                                     { 1, 1, 1 }};

        private int[,] blur2Kernel = {{ 1, 1, 1, 1, 1, 1, 1 },
                                     { 1, 1, 1, 1, 1, 1, 1 },
                                     { 1, 1, 1, 1, 1, 1, 1 },
                                     { 1, 1, 1, 1, 1, 1, 1 },
                                     { 1, 1, 1, 1, 1, 1, 1 }, 
                                     { 1, 1, 1, 1, 1, 1, 1 },
                                     { 1, 1, 1, 1, 1, 1, 1 }};

        private int[,] GaussianBlurKernel = {{ 0, 1, 0 },
                                             { 1, 4, 1 },
                                             { 0, 1, 0 }};

        private int[,] sharpenKernel = {{  0, -1,  0 },
                                        { -1,  5, -1 },
                                        {  0, -1,  0 }};

        private int[,] sharpen2Kernel = {{ -1, -1, -1 },
                                         { -1,  9, -1 },
                                         { -1, -1, -1 }};

        private int[,] edgeDetectionKernel = {{-1, 0, 0 },
                                              { 0, 1, 0 },
                                              { 0, 0, 0 }};

        private int[,] LaplacianDetectionKernel = {{ 0, -1,  0 },
                                                   {-1,  4, -1 },
                                                   { 0, -1,  0 }};

        private int[,] embossKernel = {{ -1, -1, 0 },
                                       { -1,  1, 1 },
                                       {  0,  1, 1 }};

        //----------------- Drawing ------------------
        public struct myPoint
        {
            public System.Drawing.Point Point;
            public bool isIncluded;
        }
        public myPoint[] polylinePoints;

        public MainWindow()
        {
            InitializeComponent();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            polylinePoints = new myPoint[256];
            gridFilter = new double[9];
            for(int i= 0; i < 9; i++) { gridFilter[i] = 0d; }
            for(int i = 0; i < 256; i++)
            {
                polylinePoints[i].Point.X = i;
                polylinePoints[i].Point.Y = 255 - i;
                polylinePoints[i].isIncluded = true;
            }
            Draw_Lines();
        }

        //-------------------- Functional Filters ------------------------
        public static int Inversion(int x)
        {
            return 255 - x;
        }

        public static int Brightness(int x)
        {
            return brightnessConst + x;
        }

        public static int Contrast(int x)
        {
            return (contrastConst * (x - 128)) + 128;
        }

        public static int Gamma_Correction(int x)
        {
            return (int)(Math.Pow((double)(x / 255), gammaConst) * 255);
        }

        public static int Clamp_Value(int val)
        {
            if (val > 255) return 255;
            else if (val < 0) return 0;
            else return val;
        }

        public static BitmapSource Apply_Functional_Filter(BitmapSource image, Func<int, int> filter)
        {
            int stride = ((image.PixelWidth * image.Format.BitsPerPixel + 31) / 32) * 4;
            //int stride = ((image.PixelWidth * image.Format.BitsPerPixel + 7) / 8);
            int length = image.PixelHeight * stride;
            byte[] imageCopy = new byte[length];
            image.CopyPixels(imageCopy, stride, 0);

            int p1, p2, p3, p4;
            for (int i = 0; i < length; i += 4)
            {
                p1 = (int)filter(imageCopy[i]);
                p2 = (int)filter(imageCopy[i + 1]);
                p3 = (int)filter(imageCopy[i + 2]);
                p4 = (int)filter(imageCopy[i + 3]);

                //Clamp the values
                imageCopy[i] = (byte)Clamp_Value(p1);
                imageCopy[i + 1] = (byte)Clamp_Value(p2);
                imageCopy[i + 2] = (byte)Clamp_Value(p3);
                imageCopy[i + 3] = (byte)Clamp_Value(p4);
            }
            BitmapSource bitmap = BitmapSource.Create(image.PixelWidth, image.PixelHeight,
                image.DpiX, image.DpiY, image.Format, image.Palette, imageCopy, stride);
            return bitmap;
        }

        public BitmapSource Apply_Custom_Filter(BitmapSource image)
        {
            //int stride = ((image.PixelWidth * image.Format.BitsPerPixel + 31) / 32) * 4;
            int stride = ((image.PixelWidth * image.Format.BitsPerPixel + 7) / 8);
            int length = image.PixelHeight * stride;
            byte[] imageCopy = new byte[length];
            image.CopyPixels(imageCopy, stride, 0);

            int p1, p2, p3, p4;
            for (int i = 0; i < length; i += 4)
            {
                p1 = (int)(255 - polylinePoints[imageCopy[i]].Point.Y);
                p2 = (int)(255 - polylinePoints[imageCopy[i + 1]].Point.Y);
                p3 = (int)(255 - polylinePoints[imageCopy[i + 2]].Point.Y);
                p4 = (int)(255 - polylinePoints[imageCopy[i + 3]].Point.Y);

                //Clamp the values
                imageCopy[i] = (byte)Clamp_Value(p1);
                imageCopy[i + 1] = (byte)Clamp_Value(p2);
                imageCopy[i + 2] = (byte)Clamp_Value(p3);
                imageCopy[i + 3] = (byte)Clamp_Value(p4);
            }
            BitmapSource bitmap = BitmapSource.Create(image.PixelWidth, image.PixelHeight,
                image.DpiX, image.DpiY, image.Format, image.Palette, imageCopy, stride);
            return bitmap;
        }

        //-------------------- Convolution Filters --------------------------
        public unsafe void Apply_Convolution_Filter(Bitmap image, int[,] kernel)
        {
            // Copy original bitmap to store pixel values
            Bitmap bmpCopy = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmpCopy))
            {
                graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, bmpCopy.Width, bmpCopy.Height), 
                    new System.Drawing.Rectangle(0, 0, bmpCopy.Width, bmpCopy.Height), GraphicsUnit.Pixel);
                graphics.Flush();
            }
            BitmapData copyData = bmpCopy.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), 
                ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte* copy0 = (byte*)copyData.Scan0.ToPointer();

            // Prepare original bitmap to write to it
            BitmapData bmpData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            byte* scan0 = (byte*)bmpData.Scan0.ToPointer();

            byte bitsPerPixel = (byte)(System.Drawing.Image.GetPixelFormatSize(bmpData.PixelFormat));
            int kernelSize = kernel.GetLength(0);
            int kernelStep = (kernelSize - 1) / 2;
            int red, green, blue, sumWeight;
            int offset = 0;

            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j++)
                {
                    byte* pixel = scan0 + i * bmpData.Stride + j * bitsPerPixel / 8;
                    red = 0;
                    green = 0;
                    blue = 0;
                    sumWeight = 0;
                    for (int x = 0; x < kernelSize; x++)
                    {
                        for (int y = 0; y < kernelSize; y++)
                        {
                            int xDiff = i - kernelStep + x;
                            int yDiff = j - kernelStep + y;
                            byte* rgb;
                            // Kernel coefficients that "fit" inside image
                            if (xDiff >= 0 && xDiff < bmpData.Height && yDiff >= 0 && yDiff < bmpData.Width)
                            {
                                rgb = copy0 + (xDiff) * copyData.Stride + (yDiff) * bitsPerPixel / 8;
                                blue += rgb[0] * kernel[x, y];
                                green += rgb[1] * kernel[x, y];
                                red += rgb[2] * kernel[x, y];
                                sumWeight += kernel[x, y];
                            }
                            else { continue; }
                        }
                    }
                    if (sumWeight == 0) { sumWeight = 1; offset = 128; }
                    pixel[0] = (byte)(Clamp_Value(blue / sumWeight + offset));
                    pixel[1] = (byte)(Clamp_Value(green / sumWeight + offset));
                    pixel[2] = (byte)(Clamp_Value(red / sumWeight + offset));
                }
            }
            image.UnlockBits(bmpData);
        }

        public Bitmap convert_To_Bitmap(ImageSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapSource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapSource Convert_To_Bitmap_Source(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Pbgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        //------------------------ Button Clicks -----------------------------
        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select image";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*bmp|" +
             "JPEG|*.jpg;*.jpeg|" +
             "PNG|*.png|" +
             "Bitmap|*.bmp";
            if (op.ShowDialog() == true)
            {
                originalImage.Source = new BitmapImage(new Uri(op.FileName));
                filterImage.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void Save_Image_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source == null)
            {
                MessageBox.Show("No image to save!");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Select image";
            dialog.Filter = "JPEG Image|*.jpg|Portable Network Graphic|*.png|Bitmap Image|*.bmp";

            if (dialog.ShowDialog() == true && dialog.FileName != "")
            {
                FileStream fileStream = (FileStream)dialog.OpenFile();
                switch (dialog.FilterIndex)
                {
                    case 1:
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(filterImage.Source as BitmapSource));
                        encoder.Save(fileStream);
                        break;
                    case 2:
                        BitmapEncoder pencoder = new PngBitmapEncoder();
                        pencoder.Frames.Add(BitmapFrame.Create(filterImage.Source as BitmapSource));
                        pencoder.Save(fileStream);
                        break;
                    case 3:
                        BitmapEncoder bencoder = new BmpBitmapEncoder();
                        bencoder.Frames.Add(BitmapFrame.Create(filterImage.Source as BitmapSource));
                        bencoder.Save(fileStream);
                        break;
                }
            }
        }

        private void Clear_Image_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                filterImage.Source = originalImage.Source;
            }
            else MessageBox.Show("No image loaded!");
        }

        private void Inversion_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                filterImage.Source = Apply_Functional_Filter(filterImage.Source as BitmapSource, Inversion);
            }
        }

        private void Brightness_Button_Click(object sender, RoutedEventArgs e)
         {
            if (filterImage.Source != null)
            {
                filterImage.Source = Apply_Functional_Filter(filterImage.Source as BitmapSource, Brightness);
            }
        }

        private void Contrast_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                filterImage.Source = Apply_Functional_Filter(filterImage.Source as BitmapSource, Contrast);
            }
        }

        private void Gamma_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                filterImage.Source = Apply_Functional_Filter(filterImage.Source as BitmapSource, Gamma_Correction);
            }
        }

        private void Custom_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                filterImage.Source = Apply_Custom_Filter(filterImage.Source as BitmapSource);
            }
        }

        private void Blur_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                Apply_Convolution_Filter(bmp, blurKernel);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Gaussian_Blur_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                Apply_Convolution_Filter(bmp, GaussianBlurKernel);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Sharpen_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                Apply_Convolution_Filter(bmp, sharpenKernel);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Edge_Detection_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                Apply_Convolution_Filter(bmp, edgeDetectionKernel);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Emboss_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                Apply_Convolution_Filter(bmp, embossKernel);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Inversion_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < 256; i++)
            {
                polylinePoints[i].Point.Y = 255 - polylinePoints[i].Point.Y;
            }
            Draw_Lines();
        }

        private void Brightness_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 256; i++)
            {
                polylinePoints[i].isIncluded = true;
                polylinePoints[i].Point.Y = Clamp_Value((int)(polylinePoints[i].Point.Y - brightnessConst));
            }
            Draw_Lines();
        }

        private void Contrast_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 256; i++)
            {
                polylinePoints[i].isIncluded = true;
                polylinePoints[i].Point.Y = Clamp_Value((int)((contrastConst * (polylinePoints[i].Point.Y - 128)) + 128));
            }
            Draw_Lines();
        }

        private void Custom_Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 256; i++)
            {
                polylinePoints[i].Point.Y = 255 - i;
            }
            Draw_Lines();
        }

        //--------------------- Filter Editing -------------------------

        private System.Windows.Point mousePos = new System.Windows.Point();
        private myPoint dragPoint = new myPoint();

        private void Draw_Lines()
        {
            myCanvas.Children.Clear();
            double startX = polylinePoints[0].Point.X;
            double startY = polylinePoints[0].Point.Y;
            double endX, endY;

            for(int i = 1; i < 255; i++)
            {
                if (isInLine(polylinePoints[i - 1].Point, polylinePoints[i + 1].Point, polylinePoints[i].Point))
                    polylinePoints[i].isIncluded = false;
            }
            for(int i = 1; i < 256; i++)
            {
                if(polylinePoints[i].isIncluded == true)
                {
                    endX = polylinePoints[i].Point.X;
                    endY = polylinePoints[i].Point.Y;
                    Line line = new Line();
                    line.Stroke = new SolidColorBrush(Colors.Blue);
                    line.StrokeThickness = 1.5;
                    line.X1 = startX;
                    line.Y1 = startY;
                    line.X2 = endX;
                    line.Y2 = endY;
                    myCanvas.Children.Add(line);
                    startX = endX;
                    startY = endY;
                    if (i < 255)
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        rect.Height = rect.Width = 4;
                        rect.Stroke = new SolidColorBrush(Colors.Blue);
                        rect.Fill = new SolidColorBrush(Colors.Blue);
                        myCanvas.Children.Add(rect);
                        Canvas.SetLeft(rect, polylinePoints[i].Point.X - 2);
                        Canvas.SetTop(rect, polylinePoints[i].Point.Y - 2);
                    }
                }
            }
        }

        private void Canvas_Add_Point(object sender, MouseButtonEventArgs e)
        {
            mousePos = e.GetPosition(myCanvas);
            int index = (int)mousePos.X;
            if (index < 0 || index >= 256 || mousePos.Y < 0 || mousePos.Y >= 256) return;
            if (polylinePoints[index].Point.Y == mousePos.Y && polylinePoints[index].isIncluded == true)
            {
                if (index != 0 && index != 255) polylinePoints[index].isIncluded = false;
            }
            else
            {
                polylinePoints[index].isIncluded = true;
                polylinePoints[index].Point.Y = (int)mousePos.Y;
                for (int i = index - 1; i >= 0; i--)
                {
                    if (polylinePoints[i].isIncluded == true)
                    {
                        for (int k = i + 1; k < (int)polylinePoints[index].Point.X; k++)
                            polylinePoints[k].Point.Y = Get_Y_Coordinate(polylinePoints[i].Point, polylinePoints[index].Point, k);
                        break;
                    }
                }
                for (int j = index + 1; j < 256; j++)
                {
                    if (polylinePoints[j].isIncluded == true)
                    {
                        for (int k = j - 1; k < (int)polylinePoints[index].Point.X; k--)
                            polylinePoints[k].Point.Y = Get_Y_Coordinate(polylinePoints[j].Point, polylinePoints[index].Point, k);
                        break;
                    }
                }
                Draw_Lines();
            }
        }

        private void Canvas_Delete_Point(object sender, MouseButtonEventArgs e)
        {
            mousePos = e.GetPosition(myCanvas);
            int index = (int)mousePos.X;
            if (index < 0 || index >= 256 || mousePos.Y < 0 || mousePos.Y >= 256) return;
            if (index != 0 && index != 255 && polylinePoints[index].isIncluded == true)
            {
                polylinePoints[index].isIncluded = false;
            }
            Draw_Lines();
        }

        private int Find_Closest_Point(System.Windows.Point mouse)
        {
            int index = (int)mouse.X;
            double distX = polylinePoints[index].Point.X - mouse.X;
            double distY = polylinePoints[index].Point.Y - mouse.Y;
            double distance = Math.Sqrt(distX * distX + distY * distY);
            if (distance <= 100)
            {
                dragPoint.Point.X = polylinePoints[index].Point.X;
                dragPoint.Point.Y = polylinePoints[index].Point.Y;
                dragPoint.isIncluded = polylinePoints[index].isIncluded;
                return index;
            }
            return -1;
        }

        private bool isInLine(System.Drawing.Point p1, System.Drawing.Point p2, System.Drawing.Point p3)
        {
            //double crossproduct = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
            double distanceMax = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
            double distance1 = Math.Sqrt((p3.X - p1.X) * (p3.X - p1.X) + (p3.Y - p1.Y) * (p3.Y - p1.Y)); ;
            double distance2 = Math.Sqrt((p2.X - p3.X) * (p2.X - p3.X) + (p2.Y - p3.Y) * (p2.Y - p3.Y)); ;

            if (distance1 + distance2 == distanceMax) return true;
            return false;
        }

        private int Get_Y_Coordinate(System.Drawing.Point p1, System.Drawing.Point p2, int x)
        {
            double a = (double)(p2.Y - p1.Y) / (double)(p2.X - p1.X);
            double b = p1.Y - (a * p1.X);
            int result = (int)(a * x + b);
            if (result < 255 - x) return result - 1;
            return result;
        }

        private bool is_Valid_Move(System.Windows.Point newMouse)
        {
            bool result = false;
            if ((dragPoint.Point.X == 0 && newMouse.X != 0) || (dragPoint.Point.X == 255 && newMouse.X != 255)) return false; 
            for(int i = (int)dragPoint.Point.X - 1; i >= 0; i--)
            {
                if(polylinePoints[i].isIncluded == true)
                {
                    if (polylinePoints[i].Point.X >= (int)newMouse.X) return false;
                    result = true;
                    break;
                }
            }
            for (int i = (int)dragPoint.Point.X + 1; i < 256; i++)
            {
                if (polylinePoints[i].isIncluded == true)
                {
                    if (polylinePoints[i].Point.X <= (int)newMouse.X) return false;
                    result = true;
                    break;
                }
            }
            return result;
        }

        //------------------- Grid Filter ---------------------
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[a-z]");
            double result;
            if (!double.TryParse(sender.ToString(), out result))
            {
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        public static Bitmap Apply_Grid_Filter(Bitmap input, double[] gridFilter)
        {
            Bitmap result = new Bitmap(input.Width, input.Height);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    System.Drawing.Color c = input.GetPixel(x, y);
                    int red = Clamp_Value((int)(gridFilter[0] * c.R + gridFilter[1] * c.G + gridFilter[2] * c.B));
                    int green = Clamp_Value((int)(gridFilter[3] * c.R + gridFilter[4] * c.G + gridFilter[5] * c.B));
                    int blue = Clamp_Value((int)(gridFilter[6] * c.R + gridFilter[7] * c.G + gridFilter[8] * c.B));
                    //int grey = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    result.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, red, green, blue));
                }
            }
            return result;
        }

        private void Grid_Filter_Button_Click(object sender, RoutedEventArgs e)
        {
            if(filterImage.Source != null)
            {
                double res;
                if (a1.Text.ToString() != "" && Double.TryParse(a1.Text.ToString(), out res)) gridFilter[0] = Convert.ToDouble(a1.Text.ToString());
                if (a2.Text.ToString() != "" && Double.TryParse(a2.Text.ToString(), out res)) gridFilter[1] = Double.Parse(a2.Text.ToString());
                if (a3.Text.ToString() != "" && Double.TryParse(a3.Text.ToString(), out res)) gridFilter[2] = Double.Parse(a3.Text.ToString());
                if (a4.Text.ToString() != "" && Double.TryParse(a4.Text.ToString(), out res)) gridFilter[3] = Double.Parse(a4.Text.ToString());
                if (a5.Text.ToString() != "" && Double.TryParse(a5.Text.ToString(), out res)) gridFilter[4] = Double.Parse(a5.Text.ToString());
                if (a6.Text.ToString() != "" && Double.TryParse(a6.Text.ToString(), out res)) gridFilter[5] = Double.Parse(a6.Text.ToString());
                if (a7.Text.ToString() != "" && Double.TryParse(a7.Text.ToString(), out res)) gridFilter[6] = Double.Parse(a7.Text.ToString());
                if (a8.Text.ToString() != "" && Double.TryParse(a8.Text.ToString(), out res)) gridFilter[7] = Double.Parse(a8.Text.ToString());
                if (a9.Text.ToString() != "" && Double.TryParse(a9.Text.ToString(), out res)) gridFilter[8] = Double.Parse(a9.Text.ToString());

                Bitmap tmp = convert_To_Bitmap(filterImage.Source);
                Bitmap next = Apply_Grid_Filter(tmp, gridFilter);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(next);
            }
        }

        private void Clear_Grid_Button_Click(object sender, RoutedEventArgs e)
        {
            a1.Text = "";
            a2.Text = "";
            a3.Text = "";
            a4.Text = "";
            a5.Text = "";
            a6.Text = "";
            a7.Text = "";
            a8.Text = "";
            a9.Text = "";
            for (int i = 0; i < 9; i++)
                gridFilter[i] = 0d;
        }
        
    }
}
