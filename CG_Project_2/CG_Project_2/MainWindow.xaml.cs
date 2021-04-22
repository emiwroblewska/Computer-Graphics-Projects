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

namespace CG_Project_2
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
        private static bool isGray = false;
        private ConvolutionFilter convolutionFilter = new ConvolutionFilter();
        private OrderedDithering orderedDithering = new OrderedDithering();
        private UniformQuantization uniformQuantization = new UniformQuantization();


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
            for (int i = 0; i < 256; i++)
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

        public static BitmapSource Apply_Functional_Filter(BitmapSource image, Func<int, int> filter)
        {
            int stride = ((image.PixelWidth * image.Format.BitsPerPixel + 31) / 32) * 4;
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


        //------------------------- Convertions & Helpers -------------------------
        public static int Clamp_Value(int val)
        {
            if (val > 255) return 255;
            else if (val < 0) return 0;
            else return val;
        }

        public static int Clamp_Level(int val)
        {
            if (val > 255) return 255;
            else if (val < 2) return 2;
            else return val;
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

            var format = PixelFormats.Pbgra32;
            if (isGray == true) format = PixelFormats.Gray8;
            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution, format, null,
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
                isGray = false;
                ClearGrayButton.IsEnabled = false;
                grayButton.IsEnabled = true;
                YCrCbBtn.IsEnabled = true;
                rgbPanel.Visibility = Visibility.Visible;
                grayPanel.Visibility = Visibility.Collapsed;
                rgbPanelQuant.Visibility = Visibility.Visible;
                grayPanelQuant.Visibility = Visibility.Collapsed;
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
                isGray = false;
                grayButton.IsEnabled = true;
                ClearGrayButton.IsEnabled = false;
                YCrCbBtn.IsEnabled = true;
                rgbPanel.Visibility = Visibility.Visible;
                grayPanel.Visibility = Visibility.Collapsed;
                rgbPanelQuant.Visibility = Visibility.Visible;
                grayPanelQuant.Visibility = Visibility.Collapsed;
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
                if (isGray)
                {
                    filterImage.Source = this.convolutionFilter.Apply_Convolution_OnGrayscale(filterImage.Source as BitmapSource, blurKernel);
                }
                else
                {
                    Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                    this.convolutionFilter.Apply_Convolution_Filter(bmp, blurKernel);
                    filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
                }
            }
        }

        private void Gaussian_Blur_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                if (isGray)
                {
                    filterImage.Source = this.convolutionFilter.Apply_Convolution_OnGrayscale(filterImage.Source as BitmapSource, GaussianBlurKernel);
                }
                else
                {
                    Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                    this.convolutionFilter.Apply_Convolution_Filter(bmp, GaussianBlurKernel);
                    filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
                }
            }
        }

        private void Sharpen_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                if (isGray)
                {
                    filterImage.Source = this.convolutionFilter.Apply_Convolution_OnGrayscale(filterImage.Source as BitmapSource, sharpenKernel);
                }
                else
                {
                    Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                    this.convolutionFilter.Apply_Convolution_Filter(bmp, sharpenKernel);
                    filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
                }
            }
        }

        private void Edge_Detection_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                if (isGray)
                {
                    filterImage.Source = this.convolutionFilter.Apply_Convolution_OnGrayscale(filterImage.Source as BitmapSource, edgeDetectionKernel);
                }
                else
                {
                    Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                    this.convolutionFilter.Apply_Convolution_Filter(bmp, edgeDetectionKernel);
                    filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
                }
            }
        }

        private void Emboss_Button_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                if (isGray)
                {
                    filterImage.Source = this.convolutionFilter.Apply_Convolution_OnGrayscale(filterImage.Source as BitmapSource, embossKernel);
                }
                else
                {
                    Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                    this.convolutionFilter.Apply_Convolution_Filter(bmp, embossKernel);
                    filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
                }
            }
        }

        private void Inversion_Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 256; i++)
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
        //private myPoint dragPoint = new myPoint();

        private void Draw_Lines()
        {
            myCanvas.Children.Clear();
            double startX = polylinePoints[0].Point.X;
            double startY = polylinePoints[0].Point.Y;
            double endX, endY;

            for (int i = 1; i < 255; i++)
            {
                if (isInLine(polylinePoints[i - 1].Point, polylinePoints[i + 1].Point, polylinePoints[i].Point))
                    polylinePoints[i].isIncluded = false;
            }
            for (int i = 1; i < 256; i++)
            {
                if (polylinePoints[i].isIncluded == true)
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

        private bool isInLine(System.Drawing.Point p1, System.Drawing.Point p2, System.Drawing.Point p3)
        {
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

        //----------------------- Dithering & Quantization ------------------------
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[a-z]");
            double result;
            if (!double.TryParse(sender.ToString(), out result))
            {
                e.Handled = regex.IsMatch(e.Text);
            }
        }
        private void OnlyNumbersValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private unsafe void Convert_To_Grayscale_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source != null)
            {
                if (isGray) return;
                FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
                grayBitmap.BeginInit();
                grayBitmap.Source = filterImage.Source as BitmapSource;
                grayBitmap.DestinationFormat = PixelFormats.Gray8;
                grayBitmap.EndInit();
                
                filterImage.Source = grayBitmap;
                isGray = true;
                grayButton.IsEnabled = false;
                ClearGrayButton.IsEnabled = true;
                YCrCbBtn.IsEnabled = false;
                rgbPanel.Visibility = Visibility.Collapsed;
                grayPanel.Visibility = Visibility.Visible;
                rgbPanelQuant.Visibility = Visibility.Collapsed;
                grayPanelQuant.Visibility = Visibility.Visible;
            }
        }

        private void Clear_Grayscale_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source == null) return;
            if (isGray)
            {
                FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
                grayBitmap.BeginInit();
                grayBitmap.Source = originalImage.Source as BitmapSource;
                grayBitmap.DestinationFormat = PixelFormats.Gray8;
                grayBitmap.EndInit();
                filterImage.Source = grayBitmap;
            }
        }

        private void Apply_Ordered_Dithering_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source == null) return;
            if (isGray)
            {
                int grayValue = 2;
                int mapSize = 2;
                if (ValuePerChannel.Text.ToString() != "")
                {
                    if (!Int32.TryParse(ValuePerChannel.Text.ToString(), out grayValue)) { grayValue = 2; }
                    else grayValue = Clamp_Level(grayValue);
                }
                if (!Int32.TryParse(mapsComboBox.Text, out mapSize)) { mapSize = 2; }
                filterImage.Source = (BitmapSource)this.orderedDithering.Ordered_Dithering_Gray(filterImage.Source as BitmapSource, mapSize, grayValue);
            }
            else
            {
                int redValue = 2;
                int greenValue = 2;
                int blueValue = 2;
                int mapSize = 2;
                if (ValuePerRed.Text.ToString() != "")
                {
                    if (!Int32.TryParse(ValuePerRed.Text.ToString(), out redValue)) { redValue = 2; }
                    else redValue = Clamp_Level(redValue);
                }
                if (ValuePerGreen.Text.ToString() != "")
                {
                    if (!Int32.TryParse(ValuePerGreen.Text.ToString(), out greenValue)) { greenValue = 2; }
                    else greenValue = Clamp_Level(greenValue);
                }
                if (ValuePerBlue.Text.ToString() != "")
                {
                    if (!Int32.TryParse(ValuePerBlue.Text.ToString(), out blueValue)) { blueValue = 2; }
                    else blueValue = Clamp_Level(blueValue);
                }
                if (!Int32.TryParse(mapsComboBox.Text, out mapSize)) { mapSize = 2; }

                System.Drawing.Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                this.orderedDithering.Ordered_Dithering_Color(bmp, mapSize, redValue, greenValue, blueValue);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Apply_Uniform_Quantization_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source == null) return;
            if(isGray)
            {
                int grayLevel = 2;
                if (ValuePerChannel.Text.ToString() != "")
                {
                    if (!Int32.TryParse(grayLevels.Text.ToString(), out grayLevel)) { grayLevel = 2; }
                    else grayLevel = Clamp_Level(grayLevel);
                }
                filterImage.Source = (BitmapSource)this.uniformQuantization.Uniform_Quantization_Gray(filterImage.Source as BitmapSource, grayLevel);
            }
            else
            {
                int redValue = 4;
                int greenValue = 4;
                int blueValue = 4;
                if (ValuePerRed.Text.ToString() != "")
                {
                    if (!Int32.TryParse(redLevels.Text.ToString(), out redValue)) { redValue = 4; }
                    else redValue = Clamp_Level(redValue);
                }
                if (ValuePerGreen.Text.ToString() != "")
                {
                    if (!Int32.TryParse(greenLevels.Text.ToString(), out greenValue)) { greenValue = 4; }
                    else greenValue = Clamp_Level(greenValue);
                }
                if (ValuePerBlue.Text.ToString() != "")
                {
                    if (!Int32.TryParse(blueLevels.Text.ToString(), out blueValue)) { blueValue = 4; }
                    else blueValue = Clamp_Level(blueValue);
                }

                System.Drawing.Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
                this.uniformQuantization.Uniform_Quantization_Color(bmp, redValue, greenValue, blueValue);
                filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
            }
        }

        private void Apply_YCrCb_Dithering_Click(object sender, RoutedEventArgs e)
        {
            if (filterImage.Source == null || isGray) return;
            int YValue = 2;
            int CrValue = 2;
            int CbValue = 2;
            int mapSize = 2;
            if (ValuePerRed.Text.ToString() != "")
            {
                if (!Int32.TryParse(ValueY.Text.ToString(), out YValue)) { YValue = 2; }
                else YValue = Clamp_Level(YValue);
            }
            if (ValuePerGreen.Text.ToString() != "")
            {
                if (!Int32.TryParse(ValueCr.Text.ToString(), out CrValue)) { CrValue = 2; }
                else CrValue = Clamp_Level(CrValue);
            }
            if (ValuePerBlue.Text.ToString() != "")
            {
                if (!Int32.TryParse(ValueCb.Text.ToString(), out CbValue)) { CbValue = 2; }
                else CbValue = Clamp_Level(CbValue);
            }
            if (!Int32.TryParse(mapYCrCbComboBox.Text, out mapSize)) { mapSize = 2; }

            System.Drawing.Bitmap bmp = this.convert_To_Bitmap(filterImage.Source);
            this.orderedDithering.Ordered_Dithering_YCrCb(bmp, mapSize, YValue, CrValue, CbValue);
            filterImage.Source = (BitmapSource)Convert_To_Bitmap_Source(bmp);
        }
    }
}
