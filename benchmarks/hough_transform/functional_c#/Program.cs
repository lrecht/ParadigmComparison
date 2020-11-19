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
            System.Console.WriteLine(res.Select(x => x.Item3).Sum());

        }

        private static ImmutableArray<(int,int,int)> computeHoughTransformation(Bitmap pic)

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

            return colouredPixelsCordinates
                .GroupBy(p => p)
                .Select(x => (x.First().theta, x.First().rScaled, x.Count())).ToImmutableArray();
        }


        private static (ImmutableArray<double> sinTable, ImmutableArray<double> cosTable) cosSinRadianTables(int thetaAxisSize)
        {
            var thetaRadians = Enumerable
                .Range(0, thetaAxisSize)
                .Select(theta => theta * Math.PI / thetaAxisSize);

            return (thetaRadians.Select(t => Math.Sin(t)).ToImmutableArray(), thetaRadians.Select(t => Math.Cos(t)).ToImmutableArray());
        }

    }
}
