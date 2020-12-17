using System;
using System.Drawing;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var image = new Bitmap("benchmarks/hough_transform/Pentagon.png");

			var thetaAxisSize = 640;
			var rhoAxisSize = 480;

			var (sinTable, cosTable) = createCosSinTables(thetaAxisSize);

			var outputData = makeHoughSpaceData(cosTable, sinTable, image, thetaAxisSize, rhoAxisSize);

			var sum = 0;
			(int width, int height) = (outputData.GetLength(0), outputData.GetLength(1));
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					sum += outputData[x, y];
				}
			}
			Console.WriteLine("Sum: " + sum);
		}

		static int[,] makeHoughSpaceData(double[] cosTable, double[] sinTable, Bitmap image, int thetaAxisSize, int rhoAxisSize)
		{
			var width = image.Width;
			var height = image.Height;
			var diagonal = (int)(Math.Ceiling(Math.Sqrt(Math.Pow((float)width, 2.0) + Math.Pow((float)height, 2.0)))); //Max radius
			var halfRAxisSize = rhoAxisSize / 2;
			var outputData = new int[thetaAxisSize, rhoAxisSize];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var pixel = image.GetPixel(x, y);
					if (pixel.Name != "ffffffff")
					{
						for (int theta = 0; theta < thetaAxisSize; theta++)
						{
							var r = cosTable[theta] * (float)x + sinTable[theta] * (float)y;
							var rScaled = (int)(Math.Round(r * (float)halfRAxisSize / (float)diagonal) + (float)halfRAxisSize);
							outputData[theta, rScaled]++;
						}
					}
				}
			}
			return outputData;
		}

		static (double[], double[]) createCosSinTables(int thetaAxisSize)
		{
			var sinTable = new double[thetaAxisSize];
			var cosTable = new double[thetaAxisSize];
			for (int theta = 0; theta < thetaAxisSize; theta++)
			{
				var thetaRadians = (double)theta * Math.PI / (double)thetaAxisSize;
				sinTable[theta] = Math.Sin(thetaRadians);
				cosTable[theta] = Math.Cos(thetaRadians);
			}
			return (sinTable, cosTable);
		}
	}
}
