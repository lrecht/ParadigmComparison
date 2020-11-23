using System;
using System.Drawing;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var stop = System.Diagnostics.Stopwatch.StartNew();
			//let image: Bitmap = new Bitmap("benchmarks/canny_edge_detector/download.jpg")
			var image = new Bitmap("../download.jpg");
			image = toGrayScale(image);

			var gauFilt = GaussianFilter(5, 1.0);
			var gau = Convolve(image, gauFilt);

			var (intensity, theta) = computeIntensity(gau);

			var nonMax = nonMaxSuppresion(intensity, theta);

			var doubleT = doubleThreashold(nonMax);

			var final = hysteresis(doubleT);
			final.Save("Final.png");

			stop.Stop();
			Console.WriteLine("Time: " + stop.ElapsedMilliseconds);
		}

		public static int weak = 100;
		public static Color black = Color.FromArgb(0, 0, 0);
		public static Color white = Color.FromArgb(255, 255, 255);
		public static int getPixel(Bitmap image, int x, int y) => image.GetPixel(x, y).R;

		public static double[,] kernelHor = {
			{-1.0, 0.0, 1.0},
			{-2.0, 0.0, 2.0},
			{-1.0, 0.0, 1.0}};
		public static double[,] kernelVer = {
			{1.0, 2.0, 1.0},
			{0.0, 0.0, 0.0},
			{-1.0, -2.0, -1.0}};

		public static double[,] GaussianFilter(int length, double weight)
		{
			var kernel = new double[length, length];
			var kernelSum = 0.0;
			var foff = (length - 1) / 2;
			var distance = 0.0;
			var constant = 1.0 / (2.0 * Math.PI * weight * weight);
			for (int x = -foff; x <= foff; x++)
			{
				for (int y = -foff; y <= foff; y++)
				{
					distance = ((y * y) + (x * x)) / (2.0 * (weight * weight));
					kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
					kernelSum += kernel[y + foff, x + foff];
				}
			}

			for (int x = 0; x < length; x++)
			{
				for (int y = 0; y < length; y++)
				{
					kernel[y, x] = kernel[y, x] * 1.0 / kernelSum;
				}
			}

			return kernel;
		}

		public static Bitmap Convolve(Bitmap image, double[,] kernel)
		{
			var test = new Bitmap(image.Width, image.Height);
			var halfKernel = kernel.GetLength(0) / 2;
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					var sum = 0.0;
					for (int kx = -halfKernel; kx <= halfKernel; kx++)
					{
						for (int ky = -halfKernel; ky <= halfKernel; ky++)
						{
							var posX = x + kx;
							var posY = y + ky;
							// not edges
							if (!(posX <= 0 || posX >= image.Width - 1 || posY <= 0 || posY >= image.Height - 1))
							{
								sum += (((double)image.GetPixel(posX, posY).R) * (kernel[kx + halfKernel, ky + halfKernel]));
							}
						}
					}
					if (sum > 255.0)
					{
						sum = 255.0;
					}
					else if (sum < 0.0)
					{
						sum = 0.0;
					}

					test.SetPixel(x, y, Color.FromArgb((int)sum, (int)sum, (int)sum));
				}
			}
			return test;
		}

		public static int hyp(int num1, int num2)
		{
			var hyp = (Math.Sqrt((num1 * num1) + (num2 * num2)));
			return Math.Min(255, (int)hyp);
		}

		public static Bitmap hypot(Bitmap image1, Bitmap image2)
		{
			var result = new Bitmap(image1.Width, image1.Height);

			for (int x = 0; x < image1.Width; x++)
			{
				for (int y = 0; y < image1.Height; y++)
				{
					var color1 = getPixel(image1, x, y);
					var color2 = getPixel(image2, x, y);

					var hypColor = hyp(color1, color2);
					var newColor = Color.FromArgb(hypColor, hypColor, hypColor);
					result.SetPixel(x, y, newColor);
				}
			}
			return result;
		}

		public static double[,] arctan(Bitmap image1, Bitmap image2)
		{
			var result = new double[image1.Width, image1.Height];

			for (int x = 0; x < image1.Width; x++)
			{
				for (int y = 0; y < image1.Height; y++)
				{
					var color1 = image1.GetPixel(x, y);
					var color2 = image2.GetPixel(x, y);
					result[x, y] = (Math.Atan2((float)color1.R, (float)color2.R));
				}
			}
			return result;
		}

		public static (Bitmap, int[,]) computeIntensity(Bitmap image)
		{
			var Ix = Convolve(image, kernelHor);
			var Iy = Convolve(image, kernelVer);

			var g = hypot(Ix, Iy);

			var theta = arctan(Iy, Ix);
			var thetaQ = new int[theta.GetLength(0), theta.GetLength(1)];
			for (int i = 0; i < theta.GetLength(0); i++)
			{
				for (int j = 0; j < theta.GetLength(1); j++)
				{
					var num = ((int)(Math.Round(theta[i, j] * (5.0 / Math.PI))) + 5) % 5;
					thetaQ[i, j] = num;
				}
			}

			return (g, thetaQ);
		}

		public static Bitmap nonMaxSuppresion(Bitmap image, int[,] theta)
		{
			// Non-maximum suppression
			var gradSup = image;

			for (int r = 0; r < image.Width; r++)
			{
				for (int c = 0; c < image.Height; c++)
				{
					//Suppress pixels at the image edge
					if (r == 0 || r == image.Width - 1 || c == 0 || c == image.Height - 1)
					{
						gradSup.SetPixel(r, c, black);
					}

					else
					{
						var tq = (int)(theta[r, c] % 4);
						if (tq == 0) //0 is E-W (horizontal)
							if (image.GetPixel(r, c).R <= image.GetPixel(r, c - 1).R || image.GetPixel(r, c).R <= image.GetPixel(r, c + 1).R)
								gradSup.SetPixel(r, c, black);
						if (tq == 1) //1 is NE-SW
							if (image.GetPixel(r, c).R <= image.GetPixel(r - 1, c + 1).R || image.GetPixel(r, c).R <= image.GetPixel(r + 1, c - 1).R)
								gradSup.SetPixel(r, c, black);
						if (tq == 2) //2 is N-S (vertical)
							if (image.GetPixel(r, c).R <= image.GetPixel(r - 1, c).R || image.GetPixel(r, c).R <= image.GetPixel(r + 1, c).R)
								gradSup.SetPixel(r, c, black);
						if (tq == 3) //#3 is NW-SE
							if (image.GetPixel(r, c).R <= image.GetPixel(r - 1, c - 1).R || image.GetPixel(r, c).R <= image.GetPixel(r + 1, c + 1).R)
								gradSup.SetPixel(r, c, black);
					}
				}
			}
			return gradSup;
		}

		public static Bitmap toGrayScale(Bitmap image)
		{

			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					var c = image.GetPixel(x, y);
					var grayScale = (int)(((float)c.R * 0.3) + ((float)c.G * 0.59) + ((float)c.B * +0.11));
					image.SetPixel(x, y, Color.FromArgb(grayScale, grayScale, grayScale));
				}
			}
			return image;
		}

		public static int maxValue(Bitmap image)
		{
			var max = 0;
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					if (image.GetPixel(x, y).R > max)
					{
						max = image.GetPixel(x, y).R;
					}
				}
			}
			return max;
		}

		public static Bitmap doubleThreashold(Bitmap image)
		{
			var highThreshold = maxValue(image) * 0.09;
			var lowThreshold = highThreshold * 0.05;
			var doubleMap = new Bitmap(image.Width, image.Height);
			int no = 0, low = 0, high = 0;
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					if ((float)(image.GetPixel(x, y).R) <= lowThreshold)
					{
						doubleMap.SetPixel(x, y, black);
						no++;
					}
					else if ((float)(image.GetPixel(x, y).R) >= highThreshold)
					{
						doubleMap.SetPixel(x, y, white);
						high++;
					}
					else
					{
						low++;
						doubleMap.SetPixel(x, y, Color.FromArgb(weak, weak, weak));
					}
				}
			}

			return doubleMap;
		}

		public static bool hasStrongNeighbor(Bitmap image, int x, int y)
		{
			var strong = 0;

			var result = false;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					var posX = x + i;
					var posY = y + j;
					// not edges or itself
					if (!((i == 1 && j == 1) || posX <= 0 || posX >= image.Width - 1 || posY <= 0 || posY >= image.Height - 1))
						result = result || (getPixel(image, posX, posY) == strong);
				}
			}
			return result;
		}

		public static Bitmap hysteresis(Bitmap img)
		{
			var image = new Bitmap(img.Width, img.Height);
			for (int x = 0; x < img.Width; x++)
			{
				for (int y = 0; y < img.Height; y++)
				{
					if (img.GetPixel(x, y).R == weak)
					{
						if (hasStrongNeighbor(img, x, y))
							image.SetPixel(x, y, black);
						else
							image.SetPixel(x, y, white);
					}
					else
						image.SetPixel(x, y, img.GetPixel(x, y));
				}
			}
			return image;
		}
	}
}
