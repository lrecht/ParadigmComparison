using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            (Bitmap detectedEdges, int whiteCount) = new Canny("../download.jpg").CannyEdges();
            System.Console.WriteLine(whiteCount);
            ImageUtils.PlotBitmap(detectedEdges, "canny_edge_detection.jpg");
        }
    }
    public enum Direction
    {
        Vertical = 0,
        DiagonalRL = 45,
        Horizontal = 90,
        DiagonalLR = 135
    }

    public enum Colors
    {
        Black = 0,
        White = 255
    }

    public class Canny
    {
        // Canny parameters
        private static double HIGH_THRESHOLD_VOODOO = 0.12;
        private static double LOW_THRESHOLD_VOODOO = 0.07;

        // Gaussian parameters
        private static int GAUSSIAN_LENGTH = 5;
        private static double GAUSSIAN_INTENSITY = 1;

        Bitmap originalImage;
        public Canny(string filename)
        {
            originalImage = new Bitmap(filename);
        }

        public (Bitmap, int) CannyEdges()
        {
            // 0) Make greyscale
            int[,] output = ImageUtils.ToGreyScaleArray(originalImage);

            // 1) Reduce noise using gaussian blur
            output = Gaussian.BlurGreyscale(output, GAUSSIAN_LENGTH, GAUSSIAN_INTENSITY);

            // 2) Compute intensity gradient using Sobel operators.
            // The Gradient calculation step detects the edge intensity and 
            // direction by calculating the gradient of the image using edge 
            // detection operators.
            Direction[,] direction;
            (output, direction) = Sobel.IntensityGradient(output);

            // 3) Non-max suppresion
            output = nonMaxSuppresion(output, direction);

            // 4) Tracing edges with hysteresis
            (Bitmap detectedEdges, int whiteCount) = hysteresis(output, HIGH_THRESHOLD_VOODOO, LOW_THRESHOLD_VOODOO);
            return (detectedEdges, whiteCount);
        }

        private int[,] nonMaxSuppresion(int[,] image, Direction[,] direction)
        {
            (int width, int height) = (image.GetLength(0), image.GetLength(1));

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int magnitude = image[x, y];
                    Direction dir = direction[x, y];
                    //If vertical: Check left and right neighbors
                    if ((dir == Direction.Vertical && (magnitude < image[x - 1, y] || magnitude < image[x + 1, y])) ||
                        //If DiagonalRL: Check diagonal (upper left and lower right) neighbors
                        (dir == Direction.DiagonalRL && (magnitude < image[x - 1, y + 1] || magnitude < image[x + 1, y - 1])) ||
                        //If horizontal: Check top and bottom neighbors
                        (dir == Direction.Horizontal && (magnitude < image[x, y - 1] || magnitude < image[x, y + 1])) ||
                        //If DiagonalLR: Check diagonal (upper right and lower left) neighbors
                        (dir == Direction.DiagonalLR && (magnitude < image[x + 1, y + 1] || magnitude < image[x - 1, y - 1])))
                        image[x, y] = (int)Colors.Black;
                }
            }
            return image;
        }

        private (Bitmap,int) hysteresis(int[,] image, double highVoodoo, double lowVoodoo)
        {
            (int width, int height) = (image.GetLength(0), image.GetLength(1));
            var arr = image.Cast<int>();
            double thresholdHigh = arr.Max() * highVoodoo;
            double thresholdLow = thresholdHigh * lowVoodoo;
            Bitmap output = new Bitmap(width, height);
            List<(int, int)> weak = new List<(int, int)>();

            int count = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int magnitude = image[x, y];
                    if (magnitude >= thresholdHigh) // strong
                    {
                        count++;
                        output.SetPixel(x, y, Color.White);
                    }
                    else if (magnitude < thresholdLow) // zero
                        output.SetPixel(x, y, Color.Black);
                    else // weak
                        weak.Add((x, y));
                }
            }
            foreach (var (x, y) in weak)
            {
                bool connected = hasStrongNeighbour(image, thresholdHigh, x, y, width, height);
                if (connected) count++;
                output.SetPixel(x, y, connected ? Color.White : Color.Black);
            }
            return (output,count);
        }

        private bool hasStrongNeighbour(int[,] image, double thresholdHigh, int x, int y, int width, int height)
        {
            bool connected = false;
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    var posX = x + i;
                    var posY = y + j;
                    // not edges or itself
                    if(i != j && posX >= 0 && posX < width && posY >= 0 && posY < height)
                        connected |= image[x + i, y + j] > thresholdHigh;
                }
            return connected;
        }
    }

    public static class Gaussian
    {
        // Send this method a grayscale image, an int radius, and a double intensity 
        // to blur the image with a Gaussian filter of that radius and intensity.
        public static int[,] BlurGreyscale(int[,] image, int length = 5, double intensity = 1)
        {
            (int width, int height) = (image.GetLength(0), image.GetLength(1));
            int[,] output = new int[width - length, height - length];

            //Create Gaussian kernel
            double[,] kernel = initialiseKernel(length, intensity);

            //Convolve image with kernel horizontally
            output = Convolver.Convolve(image, kernel);
            return output;
        }

        public static double[,] initialiseKernel(int length, double intensity)
        {
            int radius = (int)length / 2;
            double[,] kernel = new double[length, length];
            double sumTotal = 0;
            double distance = 0;
            double calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(intensity, 2));

            for (int filterX = -radius; filterX <= radius; filterX++)
            {
                for (int filterY = -radius; filterY <= radius; filterY++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (intensity * intensity));
                    kernel[filterX + radius, filterY + radius] = calculatedEuler * Math.Exp(-distance);
                    sumTotal += kernel[filterX + radius, filterY + radius];
                }
            }

            for (int x = 0; x < length; x++)
                for (int y = 0; y < length; y++)
                    kernel[x, y] = kernel[x, y] * (1.0 / sumTotal);
            return kernel;
        }
    }


    public static class Sobel
    {
        //The masks for each Sobel convolution
        private static double[,] KERNEL_H = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        private static double[,] KERNEL_V = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        public static (int[,], Direction[,]) IntensityGradient(int[,] image)
        {
            // Compute intensity
            // the derivatives Ix and Iy w.r.t. x and y are calculated. It can be 
            // implemented by convolving I with Sobel kernels KERNEL_H and KERNEL_V
            int[,] horizontalIntensity = Convolver.Convolve(image, KERNEL_H); // Ix
            int[,] verticalIntensity = Convolver.Convolve(image, KERNEL_V); // Iy

            // Compute magnitude G as  sqrt(Ix^2+Iy^2) and
            // direction: slope of the gradient as arctan(Iy/Ix) converted to degrees and then to compass directions.
            (int[,] gradient, Direction[,] direction) = magnitude(horizontalIntensity, verticalIntensity);
            return (gradient, direction);
        }

        private static (int[,], Direction[,]) magnitude(int[,] image1, int[,] image2)
        {
            (int width, int height) = (image1.GetLength(0), image1.GetLength(1));
            int[,] output = new int[width, height];
            Direction[,] direction = new Direction[width, height];
            // N/S,              NE/SW,                E/W,                  NW/SE
            var compassDirection = new Direction[] { Direction.Vertical, Direction.DiagonalRL, Direction.Horizontal, Direction.DiagonalLR };
            double piRad = 180 / Math.PI;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int color1 = image1[x, y];
                    int color2 = image2[x, y];
                    output[x, y] = (int)(Math.Sqrt((color1 * color1) + (color2 * color2)));

                    // For each pixel compute the orientation of the intensity gradient vector:
                    double angle = Math.Atan2(color1, color2) * piRad;
                    direction[x, y] = compassDirection[(int)Math.Abs(Math.Round(angle / 45) % 4)];
                }
            }
            return (output, direction);
        }
    }

    public static class Convolver
    {
        public static int[,] Convolve(int[,] image, double[,] kernel)
        {
            (int width, int height) = (image.GetLength(0), image.GetLength(1));
            int halfKernel = kernel.GetLength(0) / 2;
            int[,] output = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double sum = 0;
                    for (int kernelX = -halfKernel; kernelX <= halfKernel; kernelX++)
                        for (int kernelY = -halfKernel; kernelY <= halfKernel; kernelY++)
                        {
                            (int posX, int posY) = (kernelX + x, kernelY + y);
                            if (posX >= 0 && posX < width && posY >= 0 && posY < height)
                                sum += kernel[kernelX + halfKernel, kernelY + halfKernel] * image[posX, posY];
                        }
                    output[x, y] = (int)sum;
                }
            }
            return output;
        }
    }

    public static class ImageUtils
    {
        public static int[,] ToGreyScaleArray(Bitmap img)
        {
            int height = img.Height;
            int width = img.Width;
            int[,] output = new int[width, height];

            if (!(height > 0 && width > 0))
                throw new Exception("Somethings not good");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int grayColor = ToGrayscaleColor(img.GetPixel(x, y)).R;
                    output[x, y] = grayColor;
                }
            }
            return output;
        }
        static Color ToGrayscaleColor(Color color)
        {
            var level = (int)(color.R * 0.3 + color.G * 0.59 + color.B * +0.11);
            var result = Color.FromArgb(level, level, level);
            return result;
        }

        public static void PlotArrayAsBitmap(int[,] image, string filename)
        {
            (int width, int height) = (image.GetLength(0), image.GetLength(1));
            Bitmap output = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = Math.Min(255, image[x, y]);
                    output.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }
            PlotBitmap(output, filename);
        }

        public static void PlotBitmap(Bitmap image, string filename)
        {
            image.Save(filename);
        }
    }
}