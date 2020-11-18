using System;
using System.Drawing;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var hough = new HoughTransform("../Pentagon.png", 480, 640);
            var output = hough.ComputeTransformation();
            System.Console.WriteLine(output.Cast<int>().Sum());
        }
    }

    public class HoughTransform
    {

        int[,] transformed { get; set; }
        PictureData pictureData { get; set; }
        int rhoAxisSize { get; }
        int thetaAxisSize { get; }
        public HoughTransform(string filename, int rhoAxisSize, int thetaAxisSize)
        {
            pictureData = new PictureData(filename);
            this.rhoAxisSize = rhoAxisSize;
            this.thetaAxisSize = thetaAxisSize;
            transformed = new int[thetaAxisSize, rhoAxisSize];
        }

        public int[,] ComputeTransformation()
        {
            int width = pictureData.Width, height = pictureData.Height;
            int diagonal = (int)(Math.Ceiling(hypotenusis(width, height)));
            int halfRhoAxisSize = rhoAxisSize / 2;
            (double[] sinTable, double[] cosTable) = fillTables(thetaAxisSize);


            // Scanning through each (x,y) pixel of the image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = pictureData.GetPixelColor(x, y);
                    
                    // If a pixel is white, skip it.
                    if (color.Name == "ffffffff")
                        continue;

                    // If it is black (an edge pixel), loop through all possible values of θ, 
                    // calculate the corresponding ρ, find the θ and ρ index in the 
                    // accumulator, and increment the accumulator base on those index pairs.
                    for (int theta = 0; theta < thetaAxisSize; theta++)
                    {
                        // Distance from the origin to the closest point on the straight line
                        double rho = cosTable[theta] * x + sinTable[theta] * y;
                        int rScaled = (int)Math.Round(rho * halfRhoAxisSize / diagonal) + halfRhoAxisSize;
                        
                        // Accumulate
                        transformed[theta, rScaled] += 1;
                    }
                }
            }
            return transformed;
        }

        private double hypotenusis(int width, int height) => Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

        private (double[], double[]) fillTables(int thetaAxisSize)
        {
            double[] sinTable = new double[thetaAxisSize], cosTable = new double[thetaAxisSize];
            for (int theta = 0; theta < thetaAxisSize; theta++)
            {
                double thetaRadians = theta * Math.PI / thetaAxisSize;
                sinTable[theta] = Math.Sin(thetaRadians);
                cosTable[theta] = Math.Cos(thetaRadians);
            }
            return (sinTable, cosTable);
        }
    }

    public class PictureData
    {
        Bitmap original { get; }
        public int Width { get; }
        public int Height { get; }
        public PictureData(string filename)
        {
            original = new Bitmap(filename);
            Width = original.Width;
            Height = original.Height;
        }

        public Color GetPixelColor(int x, int y) => original.GetPixel(x, y);
    }
}