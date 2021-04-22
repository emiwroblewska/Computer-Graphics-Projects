using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CG_Project_2
{
    class ConvolutionFilter
    {
        //public ConvolutionFilter() { }

        public byte Clamp(int value)
        {
            int result = value;
            if (value.CompareTo(255) > 0)
                result = 255;
            if (value.CompareTo(0) < 0)
                result = 0;
            return (byte)result;
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
                    pixel[0] = (byte)(Clamp(blue / sumWeight + offset));
                    pixel[1] = (byte)(Clamp(green / sumWeight + offset));
                    pixel[2] = (byte)(Clamp(red / sumWeight + offset));
                }
            }
            image.UnlockBits(bmpData);
        }

        public WriteableBitmap Apply_Convolution_OnGrayscale(BitmapSource grayImage, int[,] kernel)
        {
            int stride = ((grayImage.PixelWidth * grayImage.Format.BitsPerPixel + 31) / 32) * 4;
            int length = grayImage.PixelHeight * stride;
            byte[] imageCopy = new byte[length];
            grayImage.CopyPixels(imageCopy, stride, 0);

            WriteableBitmap wb = new WriteableBitmap(grayImage.PixelWidth, grayImage.PixelHeight, grayImage.DpiX, grayImage.DpiY,
                grayImage.Format, null);
            Int32Rect rect = new Int32Rect(0, 0, grayImage.PixelWidth, grayImage.PixelHeight);
            byte[] pixels = new byte[length];
            
            int bitsPerPixel = wb.Format.BitsPerPixel;
            int kernelSize = kernel.GetLength(0);
            int kernelStep = (kernelSize - 1) / 2;
            int intensity, sumWeight;
            int offset = 0;

            for (int i = 0; i < wb.PixelHeight; i++)
            {
                for (int j = 0; j < wb.PixelWidth; j++)
                {
                    intensity = 0;
                    sumWeight = 0;
                    int position = ((j + (wb.PixelWidth * i)) * (wb.Format.BitsPerPixel / 8));

                    for (int x = 0; x < kernelSize; x++)
                    {
                        for (int y = 0; y < kernelSize; y++)
                        {
                            int xDiff = i - kernelStep + x;
                            int yDiff = j - kernelStep + y;
                            // Kernel coefficients that "fit" inside image
                            if (xDiff >= 0 && xDiff < wb.PixelHeight && yDiff >= 0 && yDiff < wb.PixelWidth)
                            {
                                int val = ((yDiff + (wb.PixelWidth * xDiff)) * (wb.Format.BitsPerPixel / 8));
                                intensity += imageCopy[val] * kernel[x, y]; 
                                sumWeight += kernel[x, y];
                            }
                            else { continue; }
                        }
                    }
                    if (sumWeight == 0) { sumWeight = 1; offset = 128; }
                    pixels[position] = (byte)(Clamp(intensity / sumWeight + offset));
                }
            }
            wb.WritePixels(rect, pixels, stride, 0);
            return wb;
        }
    }
}
