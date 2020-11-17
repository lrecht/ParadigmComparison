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
            hough.ComputeTransformation();
            hough.ToBitmap();
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
            transformed = new int[rhoAxisSize, thetaAxisSize];
        }

        public int[,] ComputeTransformation()
        {
            int width = pictureData.Original.Width, height = pictureData.Original.Height;
            int maxRadius = (int)Math.Ceiling(hypotenusis(width, height));
            int halfRAxisSize = rhoAxisSize / 2;
            (double[] sinTable, double[] cosTable) = fillTables(rhoAxisSize, rhoAxisSize);


            // Scanning through each (x,y) pixel of the image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = pictureData.Original.GetPixel(x, y);
                    // If a pixel is white, skip it.
                    if (c == Color.White)
                        continue;

                    // If it is an edge pixel, loop through all possible values of θ, 
                    // calculate the corresponding ρ, find the θ and ρ index in the 
                    // accumulator, and increment the accumulator base on those index pairs.
                    for (int theta = 0; theta < rhoAxisSize - 1; theta++)
                    {
                        // Distance from the origin to the closest point on the straight line
                        double rho = cosTable[theta] * x + sinTable[theta] * y;
                        int rScaled = (int)Math.Round(rho * halfRAxisSize / maxRadius) + halfRAxisSize;
                        
                        // Accumulate
                        transformed[theta, rScaled] += 1;
                    }
                }
            }
            return transformed;
        }

        public Bitmap ToBitmap(bool shouldSave = true)
        {
            (int width, int height) = (transformed.GetLength(0), transformed.GetLength(1));
            Bitmap newBitMap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (transformed[x, y] <= 255)
                    {
                        var num = 255 - transformed[x, y];
                        newBitMap.SetPixel(x, y, Color.FromArgb(num, num, num));
                    }
                    else
                        newBitMap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
            if(shouldSave)
                newBitMap.Save("HoughSpace.png");
            return newBitMap;

        }

        private double hypotenusis(int width, int height) => Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
        private (double[], double[]) fillTables(int thetaAxisSize, int numRhos)
        {
            double[] sinTable = new double[numRhos];
            double[] cosTable = new double[numRhos];
            for (int theta = thetaAxisSize - 1; theta >= 0; theta--)
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
        public Bitmap Original { get; }
        public PictureData(string filename)
        {
            Original = new Bitmap(filename);
        }
    }
}

