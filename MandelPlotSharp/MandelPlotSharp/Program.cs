using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MandelPlotSharp
{
    internal class Program
    {
        private const string FileName = "./mandelbrot.BMP";

        private const int Height = 8192, Width = 8192;

        private const int Iterations = 100, BreakoutValue = 4;

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DrawMandelbrotSingleThreaded();

            DrawMandelbrotMultiThreaded();
        }

        private static unsafe void DrawMandelbrotSingleThreaded()
        {
            const PixelFormat pixelFormat = PixelFormat.Format24bppRgb;

            Bitmap bmp = new Bitmap(Width, Height, pixelFormat);

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);

            const int heightShift = Height / 2, widthShift = Width / 2;
            const double heightScale = 4.0 / Height, widthScale = 4.0 / Width;

            // format: RRGGBB
            const byte bitsPerPixel = 24;
            byte* rootBmpPixelPart = (byte*)data.Scan0.ToPointer();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < Width; i++)  // for each column
            {
                for (int j = 0; j < Height; j++)  // for each row
                {
                    double real = (i - widthShift) * widthScale, imaginary = (j - heightShift) * heightScale;  // map set into centre

                    /* Calculate mandelbrot set */
                    double x = 0, y = 0;
                    int k;
                    for (k = 0; x * x + y * y <= BreakoutValue && k < Iterations; k++)
                    {
                        double newX = x * x - y * y + real;

                        y = 2 * x * y + imaginary;
                        x = newX;
                    }

                    /* Paint into bmp */
                    byte* pixel = rootBmpPixelPart + (i * bitsPerPixel / 8) + (j * data.Stride);

                    if (k < Iterations)  // we broke out early, colour some fancy colour ;)
                    {
                        pixel[0] = 255;
                        pixel[1] = 255;
                        pixel[2] = 255;
                    }
                    else  // point is in set, colour a boring black :(
                    {
                        pixel[0] = 0;
                        pixel[1] = 0;
                        pixel[2] = 0;
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms ({stopwatch.ElapsedTicks} ticks) to draw a {Width}x{Height} mandelbrot set!");

            bmp.UnlockBits(data);

            bmp.Save(Path.GetFullPath(FileName));
        }
    }
}
