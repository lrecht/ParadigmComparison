using System;
using System.Drawing;
using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static readonly int thetaAxisSize = 640;
        static readonly int rhoAxisSize = 480;
        static void Main(string[] args)
        {
            var pic = new Bitmap("benchmarks/hough_transform/Pentagon.png");
            var res = computeHoughTransformation(pic);
            System.Console.WriteLine(res.Sum());
            ToBitmap(res.ToImmutableList());
        }

        private static ImmutableArray<int> computeHoughTransformation(Bitmap pic)
        {
            var width = pic.Width;
            var height = pic.Height;
            var diagonal = (int) Math.Ceiling(Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2)));
            var halfRhoAxisSize = rhoAxisSize / 2;

            var tables = cosSinRadianTables(thetaAxisSize);

            var pixelCordinates = Enumerable.Range(0, height).SelectMany(y => Enumerable.Range(0, width).Select(x => (x, y)));
            var transformedPixelCoordinates = Enumerable.Range(0, rhoAxisSize).SelectMany(y => Enumerable.Range(0, thetaAxisSize).Select(x => (x, y)));
            var thetaRange = Enumerable.Range(0, thetaAxisSize);

            var validPixels = pixelCordinates.Where(p => pic.GetPixel(p.x, p.y).Name != "ffffffff");
            var colouredPixelsCordinates = validPixels.SelectMany(p => 
                thetaRange.Select(theta => {
                    var rho = tables.cosTable[theta] * p.x + tables.sinTable[theta] * p.y;
                    var rScaled = (int)Math.Round(rho * halfRhoAxisSize / diagonal) + halfRhoAxisSize;

                    return (theta, rScaled);
                })
            );

            var transformedPixelDict = colouredPixelsCordinates
                .GroupBy(p => p)
                .Select(x => (x.First().theta, x.First().rScaled, x.Count()))
                .ToImmutableDictionary(x => (x.theta, x.rScaled), elementSelector: x => x.Item3);

            var transformed = transformedPixelCoordinates.Select(p => transformedPixelDict.ContainsKey(p) ? transformedPixelDict[p] : 0);
            return transformed.ToImmutableArray();
        }

        private static (ImmutableArray<double> sinTable, ImmutableArray<double> cosTable) cosSinRadianTables(int thetaAxisSize)
        {
            var thetaRadians = Enumerable
                .Range(0, thetaAxisSize)
                .Select(theta => theta * Math.PI / thetaAxisSize);

            return (thetaRadians.Select(t => Math.Sin(t)).ToImmutableArray(), thetaRadians.Select(t => Math.Cos(t)).ToImmutableArray());
        }

        private static void ToBitmap(ImmutableList<int> transformed)	
        {	
            (int width, int height) = (thetaAxisSize, rhoAxisSize);	
            Bitmap newBitMap = new Bitmap(width, height);	
            for (int x = 0; x < transformed.Count; x++)	
            {	
                    if (transformed[x] <= 255)	
                    {	
                        var num = 255 - transformed[x];	
                        newBitMap.SetPixel(x % thetaAxisSize, x / thetaAxisSize, Color.FromArgb(num, num, num));	
                    }	
                    else	
                        newBitMap.SetPixel(x % thetaAxisSize, x / thetaAxisSize, Color.FromArgb(0, 0, 0));	
            }	
            newBitMap.Save("HoughSpace.png");	
        }
    }
}
