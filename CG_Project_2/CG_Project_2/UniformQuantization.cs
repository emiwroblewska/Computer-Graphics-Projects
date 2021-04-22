using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace CG_Project_2
{
    class UniformQuantization
    {
        public unsafe void Uniform_Quantization_Color(Bitmap bmp, int redValue, int greenValue, int blueValue)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            double qR = 256d / redValue;
            double qRhalf = (double)qR / 2;
            double qG = 256d / greenValue;
            double qGhalf = (double)qG / 2;
            double qB = 256d / blueValue;
            double qBhalf = (double)qB / 2;

            for (int i = 0; i < bData.Height; i++)
            {
                for (int j = 0; j < bData.Width; j++)
                {
                    byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    double Ired = ((double)data[0]) / qR;
                    double Igreen = ((double)data[1]) / qG;
                    double Iblue = ((double)data[2]) / qB;

                    data[0] = (byte)((int)Math.Floor(Ired) * qR + qRhalf);
                    data[1] = (byte)((int)Math.Floor(Igreen) * qG + qGhalf);
                    data[2] = (byte)((int)Math.Floor(Iblue) * qB + qBhalf);
                }
            }
            bmp.UnlockBits(bData);
        }

        public WriteableBitmap Uniform_Quantization_Gray(BitmapSource grayImage, int divisionNum)
        {
            int stride = ((grayImage.PixelWidth * grayImage.Format.BitsPerPixel + 31) / 32) * 4;
            int length = grayImage.PixelHeight * stride;
            byte[] imageCopy = new byte[length];
            grayImage.CopyPixels(imageCopy, stride, 0);

            WriteableBitmap wb = new WriteableBitmap(grayImage.PixelWidth, grayImage.PixelHeight, grayImage.DpiX, grayImage.DpiY,
                grayImage.Format, null);
            Int32Rect rect = new Int32Rect(0, 0, grayImage.PixelWidth, grayImage.PixelHeight);
            byte[] pixels = new byte[length];

            int position;
            double q = 256d / divisionNum;
            double qHalf = (double)q / 2;

            for (int i = 0; i < wb.PixelHeight; i++)
            {
                for (int j = 0; j < wb.PixelWidth; j++)
                {
                    position = ((j + (wb.PixelWidth * i)) * (wb.Format.BitsPerPixel / 8));

                    double I = ((double)imageCopy[position]) / q;
                    //int value = (int)Math.Floor(I) * q + qHalf;
                    pixels[position] = (byte)((int)Math.Floor(I) * q + qHalf);
                }
            }
            wb.WritePixels(rect, pixels, stride, 0);
            return wb;
        }
    }
}
