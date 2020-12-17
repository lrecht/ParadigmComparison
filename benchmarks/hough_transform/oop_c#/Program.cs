using System;
using System.Drawing;
using System.Linq;
using benchmark;

namespace oop_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);

			var initialState = new Bitmap("benchmarks/hough_transform/Pentagon.png");
			bm.Run(() =>
			{
				var hough = new HoughTransform(initialState, 480, 640);
				var output = hough.ComputeTransformation();
				return output.Cast<int>().Sum();
			}, (res) =>
			{
				System.Console.WriteLine(res);
			});
		}
	}

	public class HoughTransform
	{
		int[,] transformed { get; set; }
		PictureData pictureData { get; set; }
		int rhoAxisSize { get; }
		int thetaAxisSize { get; }
		public HoughTransform(Bitmap initBit, int rhoAxisSize, int thetaAxisSize)
		{
			pictureData = new PictureData(initBit);
			this.rhoAxisSize = rhoAxisSize;
			this.thetaAxisSize = thetaAxisSize;
			transformed = new int[thetaAxisSize, rhoAxisSize];
		}

		public int[,] ComputeTransformation()
		{
			int width = pictureData.Width, height = pictureData.Height;
			int diagonal = (int)(Math.Ceiling(MyMath.Hypotenuse(width, height)));
			int halfRhoAxisSize = rhoAxisSize / 2;
			(double[] sinTable, double[] cosTable) = MyMath.CosSinRadianTables(thetaAxisSize);


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
	}

	public class PictureData
	{
		Bitmap original { get; }
		public int Width { get; }
		public int Height { get; }
		public PictureData(Bitmap initBit)
		{
			original = initBit;
			Width = original.Width;
			Height = original.Height;
		}

		public Color GetPixelColor(int x, int y) => original.GetPixel(x, y);
	}

	public static class MyMath
	{
		public static double Hypotenuse(int width, int height) => Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

		public static (double[], double[]) CosSinRadianTables(int thetaAxisSize)
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
}
