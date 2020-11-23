using System;
using System.Drawing;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap b = new Canny("../download.jpg").CannyEdges();
        }
    }

    public class Canny
    {
        //Canny parameters
        private static double CANNY_THRESHOLD_RATIO = .2; //Suggested range .2 - .4
        private static double CANNY_STD_DEV = 1;          //Range 1-3

        // Gaussian parameters
        private static int GAUSSIAN_RADIUS = 5;
        private static double GAUSSIAN_INTENSITY = 1;
        Bitmap originalImage;
        public Canny(string filename)
        {
            originalImage = new Bitmap(filename);
        }

        public Bitmap CannyEdges()
        {
            // 0) Make greyscale
            Bitmap output = ImageUtils.ToGreyScale(originalImage);
            ImageUtils.PlotBitmap(output, "greyscale.jpg");

            // 1) Reduce noise using gaussian blur
            output = Gaussian.BlurGreyscale(output, GAUSSIAN_RADIUS, GAUSSIAN_INTENSITY);
            ImageUtils.PlotBitmap(output, "blur.jpg");

            // 2) Compute intensity gradient using Sobel operators.
            // The Gradient calculation step detects the edge intensity and 
            // direction by calculating the gradient of the image using edge 
            // detection operators.
            double[,] direction;
            (output, direction) = Sobel.IntensityGradient(output);
            ImageUtils.PlotBitmap(output, "Sobel.jpg");

            // 3) Non-max suppresion
            output = nonMaxSuppresion(output, direction);
            ImageUtils.PlotBitmap(output, "nonmax.jpg");

            // 4) Tracing edges with hysteresis
            output = hysteresis(output, CANNY_THRESHOLD_RATIO, CANNY_STD_DEV);
            ImageUtils.PlotBitmap(output, "hysteresis.jpg");


            return output;
        }

        private Bitmap nonMaxSuppresion(Bitmap image, double[,] direction)
        {
            int height = image.Height;
            int width = image.Width;

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int magnitude = image.GetPixel(x, y).R;
                    switch (direction[x, y])
                    {
                        case 0:
                            if (magnitude <= image.GetPixel(x - 1, y - 1).R || magnitude <= image.GetPixel(x + 1, y + 1).R)
                                image.SetPixel(x, y, Color.Black);
                            break;
                        case 45:
                            if (magnitude <= image.GetPixel(x + 1, y - 1).R || magnitude <= image.GetPixel(x - 1, y + 1).R)
                                image.SetPixel(x, y, Color.Black);
                            break;
                        case 90:
                            if (magnitude <= image.GetPixel(x, y - 1).R || magnitude <= image.GetPixel(x, y + 1).R)
                                image.SetPixel(x, y, Color.Black);
                            break;
                        case 135:
                            if (magnitude <= image.GetPixel(x - 1, y - 1).R || magnitude <= image.GetPixel(x - 1, y - 1).R)
                                image.SetPixel(x, y, Color.Black);
                            break;
                    }
                }
            }
            return image;
        }

        private Bitmap hysteresis(Bitmap image, double numberDeviations, double fract)
        {
            int height = image.Height;
            int width = image.Width;
            double thresholdHigh = 255 * numberDeviations;
            double thresholdLow = thresholdHigh * fract;

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int magnitude = image.GetPixel(x, y).R;
                    if (magnitude >= thresholdHigh) // strong
                        image.SetPixel(x, y, Color.White);
                    else if (magnitude < thresholdLow) // zero
                        image.SetPixel(x, y, Color.Black);
                    else // weak
                    {
                        bool connected = hasStrongNeighbour(image, thresholdHigh, x, y);
                        image.SetPixel(x, y, (connected) ? Color.White : Color.Black);
                    }
                }
            }
            return image;
        }

        private bool hasStrongNeighbour(Bitmap image, double thresholdHigh, int x, int y)
        {
            bool connected = false;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    (int posX, int posY) = (i + x, j + y);
                    if (!((i == 1 && j == 1) || posX <= 0 || posX >= image.Width - 1 || posY <= 0 || posY >= image.Height - 1))
                        connected = connected || image.GetPixel(posX, posY) == Color.White;
                }
            }
            return connected;
        }
    }

    public static class Gaussian
    {
        enum Direction
        {
            Horizontally,
            Vertically
        }

        // Send this method a grayscale image, an int radius, and a double intensity 
        // to blur the image with a Gaussian filter of that radius and intensity.
        public static Bitmap BlurGreyscale(Bitmap image, int radius = 7, double intensity = 1.5)
        {
            int height = image.Height;
            int width = image.Width;
            Bitmap output = new Bitmap(width - (2 * radius), height - (2 * radius));

            //Create Gaussian kernel
            double[,] kernel = initialiseKernel(radius, intensity);

            //Convolve image with kernel horizontally
            output = Convolver.Convolve(image, kernel);
            return output;
        }

        public static double[,] initialiseKernel(int radius, double weight)
        {
            int length = 2 * radius + 1;
            double[,] kernel = new double[length, length];
            double sumTotal = 0;
            double distance = 0;
            double calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(weight, 2));

            for (int filterX = -radius; filterX <= radius; filterX++)
            {
                for (int filterY = -radius; filterY <= radius; filterY++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (weight * weight));
                    kernel[filterX + radius, filterY + radius] = calculatedEuler * Math.Exp(-distance);
                    sumTotal += kernel[filterX + radius, filterY + radius];
                }
            }

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    kernel[x, y] = kernel[x, y] * (1.0 / sumTotal);
                }
            }
            return kernel;
        }
    }


    public static class Sobel
    {
        //The masks for each Sobel convolution
        private static double[,] KERNEL_H = { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
        private static double[,] KERNEL_V = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

        public static (Bitmap, double[,]) IntensityGradient(Bitmap image)
        {
            // Compute intensity
            // the derivatives Ix and Iy w.r.t. x and y are calculated. It can be 
            // implemented by convolving I with Sobel kernels KERNEL_H and KERNEL_V
            Bitmap horizontalIntensity = Convolver.Convolve(image, KERNEL_H); // Ix
            Bitmap verticalIntensity = Convolver.Convolve(image, KERNEL_V); // Iy

            // Compute magnitude G as  sqrt(Ix^2+Iy^2) and
            // direction: slope of the gradient as arctan(Iy/Ix) converted to degrees.
            (Bitmap gradient, double[,] direction) = magnitude(horizontalIntensity, verticalIntensity);
            return (gradient, direction);


        }

        private static (Bitmap, double[,]) magnitude(Bitmap image1, Bitmap image2)
        {
            (int width, int height) = (image1.Width, image1.Height);
            Bitmap output = new Bitmap(width, height);
            double[,] direction = new double[width, height];
            double piRad = 180 / Math.PI;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int color1 = image1.GetPixel(x, y).R;
                    int color2 = image2.GetPixel(x, y).R;
                    int mag = (int)Math.Min(255, (Math.Sqrt((color1 * color1) + (color2 * color2))));
                    Color newColor = Color.FromArgb(mag, mag, mag);
                    output.SetPixel(x, y, newColor);

                    // For each pixel compute the orientation of the intensity gradient vector:
                    double rad = Math.Atan2(color1, color2);
                    direction[x, y] = computeDirection(rad * piRad);
                }
            }
            return (output, direction);
        }

        private static int computeDirection(double angle)
        {
            //Each pixels ACTUAL angle is examined and placed in 1 of four groups (for the four searched 45-degree neighbors)
            if (angle <= 22.5 || (angle >= 157.5 && angle <= 202.5) || angle >= 337.5)
                return 0;      //Check left and right neighbors
            else if ((angle >= 22.5 && angle <= 67.5) || (angle >= 202.5 && angle <= 247.5))
                return 45;     //Check diagonal (upper right and lower left) neighbors
            else if ((angle >= 67.5 && angle <= 112.5) || (angle >= 247.5 && angle <= 292.5))
                return 90;     //Check top and bottom neighbors
            else
                return 135;    //Check diagonal (upper left and lower right) neighbors
        }
    }

    public static class Convolver
    {
        public static Bitmap Convolve(Bitmap image, double[,] kernel)
        {
            int height = image.Height;
            int width = image.Width;
            int halfKernel = kernel.GetLength(0) / 2;
            Bitmap output = new Bitmap(width - halfKernel, height - halfKernel);
            for (int x = 1; x < width - halfKernel; x++)
            {
                for (int y = 1; y < height - halfKernel; y++)
                {
                    double sum = 0;
                    for (int kernelX = -halfKernel; kernelX <= halfKernel; kernelX++)
                    {
                        for (int kernelY = -halfKernel; kernelY <= halfKernel; kernelY++)
                        {
                            int posX = x + kernelX;
                            int posY = y + kernelY;
                            if (!(posX <= 0 || posX >= width - 1 || posY <= 0 || posY >= height - 1))
                            {
                                int color = image.GetPixel(x + kernelX, y + kernelY).R;
                                sum += kernel[kernelX + halfKernel, kernelY + halfKernel] * color;
                            }
                        }
                    }
                    sum = sum > 255 ? 255 : sum < 0 ? 0 : sum;
                    Color newColor = Color.FromArgb((int)sum, (int)sum, (int)sum);
                    output.SetPixel(x - 1, y - 1, newColor);
                }
            }
            return output;
        }
    }


    public static class ImageUtils
    {
        public static Bitmap ToGreyScale(Bitmap img)
        {
            int height = img.Height;
            int width = img.Width;

            if (!(height > 0 && width > 0))
                throw new Exception("Somethings not good");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var grayColor = ToGrayscaleColor(img.GetPixel(x, y));
                    img.SetPixel(x, y, grayColor);
                }
            }
            return img;
        }
        static Color ToGrayscaleColor(Color color)
        {
            var level = (byte)((color.R + color.G + color.B) / 3);
            var result = Color.FromArgb(level, level, level);
            return result;
        }

        public static void PlotBitmap(Bitmap image, string filename)
        {
            image.Save(filename);
        }
    }
}