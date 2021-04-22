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
    class OrderedDithering
    {
        private int[][,] BayerMatrices = new int[][,]
        {
            new int[2,2]  {{ 1, 3 },
                           { 4, 2 }},

            new int[3,3] {{ 3, 7, 4 },
                          { 6, 1, 9 },
                          { 2, 8, 5 }},

            new int[4,4] {{  1,  9,  3, 11 },
                          { 13,  5, 15,  7 },
                          {  4, 12,  2, 10 },
                          { 16,  8, 14,  6 }},

            new int[6,6] {{  9, 25, 13, 11, 27, 15 },
                          { 21,  1, 33, 23,  3, 35 },
                          {  5, 29, 17,  7, 31, 19 },
                          { 12, 28, 16, 10, 26, 14 },
                          { 24,  4, 36, 22,  2, 34 },
                          {  8, 32, 20,  6, 30, 18 }}
        };

        private int Get_Map_Position(int mapSize)
        {
            if (mapSize == 6) return 3;
            else return (mapSize - 2);
        }

        public byte Clamp(int value)
        {
            int result = value;
            if (value.CompareTo(255) > 0)
                result = 255;
            if (value.CompareTo(0) < 0)
                result = 0;
            return (byte)result;
        }

        public int Clamp_YCrCB(int value, int limit)
        {
            if (value > limit)
                return limit;
            if (value < 16)
                return 16;
            return value;
        }

        public unsafe void Ordered_Dithering_Color(Bitmap bmp, int mapSize, int valuePerRed, int valuePerGreen, int valuePerBlue)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            int i, redCol, greenCol, blueCol;
            int mapPos = Get_Map_Position(mapSize);
            double divisor = mapSize * mapSize + 1;
            double[] redLevels = new double[valuePerRed];
            double[] greenLevels = new double[valuePerGreen];
            double[] blueLevels = new double[valuePerBlue];

            for (i = 0; i < valuePerRed; i++)
            {
                double delta = 1 / ((double)(valuePerRed - 1));
                redLevels[i] = i * delta;
            }
            for (i = 0; i < valuePerGreen; i++)
            {
                double delta = 1 / ((double)(valuePerGreen - 1));
                greenLevels[i] = i * delta;
            }
            for (i = 0; i < valuePerBlue; i++)
            {
                double delta = 1 / ((double)(valuePerBlue - 1));
                blueLevels[i] = i * delta;
            }
            
            for (i = 0; i < bData.Height; i++)
            {
                for (int j = 0; j < bData.Width; j++)
                {
                    byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    double Ired = ((double)data[0]) / 255;
                    double Igreen = ((double)data[1]) / 255;
                    double Iblue = ((double)data[2]) / 255;

                    redCol = (int)Math.Floor((double)(valuePerRed - 1) * Ired);
                    greenCol = (int)Math.Floor((double)(valuePerGreen - 1) * Igreen);
                    blueCol = (int)Math.Floor((double)(valuePerBlue - 1) * Iblue);

                    double red = (valuePerRed - 1) * Ired - redCol;
                    double green = (valuePerGreen - 1) * Igreen - greenCol;
                    double blue = (valuePerBlue - 1) * Iblue - blueCol;

                    double tmp = (double)BayerMatrices[mapPos][i % mapSize, j % mapSize] / divisor;
                    if (red >= tmp)
                        ++redCol;
                    if (green >= tmp)
                        ++greenCol;
                    if (blue >= tmp)
                        ++blueCol;

                    data[0] = (byte)(redLevels[redCol] * 255);
                    data[1] = (byte)(greenLevels[greenCol] * 255);
                    data[2] = (byte)(blueLevels[blueCol] * 255);
                }
            }
            bmp.UnlockBits(bData);
        }

        public WriteableBitmap Ordered_Dithering_Gray(BitmapSource grayImage, int mapSize, int valuePerChannel)
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
            int mapPos = Get_Map_Position(mapSize);
            double divisor = mapSize * mapSize + 1;
            double delta = 1 / (double)(valuePerChannel - 1);
            double[] grayLevels = new double[valuePerChannel];

            for(int i = 0; i < valuePerChannel; i++)
            {
                grayLevels[i] = i * delta;
            }

            for (int i = 0; i < wb.PixelHeight; i++)
            {
                for (int j = 0; j < wb.PixelWidth; j++)
                {
                    position = ((j + (wb.PixelWidth * i)) * (wb.Format.BitsPerPixel / 8));
                    int val = (int)imageCopy[position];

                    double I = ((double)imageCopy[position]) / 255;
                    int grayCol = (int)Math.Floor((valuePerChannel - 1) * I);
                    double gray = (double)(valuePerChannel - 1) * I - grayCol;

                    double tmp = (double)BayerMatrices[mapPos][i % mapSize, j % mapSize] / divisor;
                    if (gray >= tmp)
                        ++grayCol;
                  
                    pixels[position] = (byte)(grayLevels[grayCol] * 255);
                }
            }
            wb.WritePixels(rect, pixels, stride, 0);
            return wb;
        }

        public unsafe void Ordered_Dithering_YCrCb(Bitmap bmp, int mapSize, int valueY, int valueCr, int valueCb)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();
            byte bitsPerPixel = (byte)(Image.GetPixelFormatSize(bData.PixelFormat));

            int i, YCol, CrCol, CbCol;
            int mapPos = Get_Map_Position(mapSize);
            double divisor = mapSize * mapSize + 1;
            double[] YLevels = new double[valueY];
            double[] CrLevels = new double[valueCr];
            double[] CbLevels = new double[valueCb];

            for (i = 0; i < valueY; i++)
            {
                double delta = 1 / ((double)(valueY - 1));
                YLevels[i] = i * delta;
            }
            for (i = 0; i < valueCr; i++)
            {
                double delta = 1 / ((double)(valueCr - 1));
                CrLevels[i] = i * delta;
            }
            for (i = 0; i < valueCb; i++)
            {
                double delta = 1 / ((double)(valueCb - 1));
                CbLevels[i] = i * delta;
            }

            for (i = 0; i < bData.Height; i++)
            {
                for (int j = 0; j < bData.Width; j++)
                {
                    byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    int yVal = (int)Clamp_YCrCB((int)(16 + (65.738 * data[0] + 129.057 * data[1] + 25.064 * data[2]) / (double)256), 235);
                    int CrVal = (int)Clamp_YCrCB((int)(128 + (-37.945 * data[0] - 74.494 * data[1] + 112.439 * data[2] ) / (double)256),240);
                    int CbVal = (int)Clamp_YCrCB((int)(128 + (112.439 * data[0] - 94.154 * data[1] - 18.285 * data[2]) / (double)256),240);

                    double IY = ((double)(yVal - 16)) / 219;
                    double ICr = ((double)(CrVal - 16)) / 224;
                    double ICb = ((double)(CbVal - 16)) / 224;

                    YCol = (int)Math.Floor((double)(valueY - 1) * IY);
                    CrCol = (int)Math.Floor((double)(valueCr - 1) * ICr);
                    CbCol = (int)Math.Floor((double)(valueCb - 1) * ICb);

                    double finalY = (valueY - 1) * IY - YCol;
                    double finalCr = (valueCr - 1) * ICr - CrCol;
                    double finalCb = (valueCb - 1) * ICb - CbCol;

                    double tmp = (double)BayerMatrices[mapPos][i % mapSize, j % mapSize] / divisor;
                    if (finalY >= tmp)
                        ++YCol;
                    if (finalCr >= tmp)
                        ++CrCol;
                    if (finalCb >= tmp)
                        ++CbCol;

                    //if (YCol >= valueY) YCol = valueY - 1;
                    //if (CrCol >= valueCr) CrCol = valueCr - 1;
                    //if (CbCol >= valueCb) CbCol = valueCb - 1;
                    int Y = (byte)(16 + (YLevels[YCol] * 219));
                    int Cr = (byte)(16 + (CrLevels[CrCol] * 224));
                    int Cb = (byte)(16 + (CbLevels[CbCol] * 224));

                    data[0] = (byte)Clamp((int)((298.082 * Y + 408.583 * Cr) / 256 - 222.921));
                    data[1] = (byte)Clamp((int)((298.082 * Y - 100.291 * Cb - 208.120 * Cr) / 256 + 135.576));
                    data[2] = (byte)Clamp((int)((298.082 * Y + 516.412 * Cb) / 256 - 276.836));
                }
            }
            bmp.UnlockBits(bData);
        }

    }
}
