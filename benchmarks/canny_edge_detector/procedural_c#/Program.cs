using System;
using System.Drawing;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var image = new Bitmap("benchmarks/canny_edge_detector/download.jpg");
			var grayImg = toGrayScale(image);

			var gauFilt = GaussianFilter(5, 1.0);
			var gau = Convolve(grayImg, gauFilt);

			var (intensity, theta) = computeIntensity(gau);

			var nonMax = nonMaxSuppresion(intensity, theta);

			var doubleT = doubleThreashold(nonMax);

			var final = hysteresis(doubleT);
			final.Save("Final.png");
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

		public static int[,] Convolve(int[,] image, double[,] kernel)
		{
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			var test = new int[width, height];
			var halfKernel = kernel.GetLength(0) / 2;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var sum = 0.0;
					for (int kx = -halfKernel; kx <= halfKernel; kx++)
					{
						for (int ky = -halfKernel; ky <= halfKernel; ky++)
						{
							var posX = x + kx;
							var posY = y + ky;
							// not edges
							if (!(posX <= 0 || posX >= width - 1 || posY <= 0 || posY >= height - 1))
							{
								sum += (image[posX, posY] * (kernel[kx + halfKernel, ky + halfKernel]));
							}
						}
					}

					test[x, y] = (int)sum;
				}
			}
			return test;
		}

		public static int hyp(int num1, int num2)
		{
			var hyp = (Math.Sqrt((num1 * num1) + (num2 * num2)));
			return (int)hyp;
		}

		public static int[,] hypot(int[,] image1, int[,] image2)
		{
			var width = image1.GetLength(0);
			var height = image1.GetLength(1);
			var result = new int[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var color1 = image1[x, y];
					var color2 = image2[x, y];

					var hypColor = hyp(color1, color2);
					result[x, y] = hypColor;
				}
			}
			return result;
		}

		public static double[,] arctan(int[,] image1, int[,] image2)
		{
			var width = image1.GetLength(0);
			var height = image1.GetLength(1);
			var result = new double[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var color1 = image1[x, y];
					var color2 = image2[x, y];
					result[x, y] = Math.Atan2(color1, color2);
				}
			}
			return result;
		}

		public static (int[,], int[,]) computeIntensity(int[,] image)
		{
			var Ix = Convolve(image, kernelHor);
			var Iy = Convolve(image, kernelVer);

			var g = hypot(Ix, Iy);

			var theta = arctan(Iy, Ix);
			var thetaWidth = theta.GetLength(0);
			var thetaHeight = theta.GetLength(1);
			var thetaQ = new int[thetaWidth, thetaHeight];
			for (int i = 0; i < thetaWidth; i++)
			{
				for (int j = 0; j < thetaHeight; j++)
				{
					var num = ((int)(Math.Round(theta[i, j] * (5.0 / Math.PI))) + 5) % 5;
					thetaQ[i, j] = num;
				}
			}

			return (g, thetaQ);
		}

		public static int[,] nonMaxSuppresion(int[,] image, int[,] theta)
		{
			// Non-maximum suppression
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			var gradSup = image;

			for (int r = 1; r < width-1; r++)
			{
				for (int c = 1; c < height-1; c++)
				{
					var tq = (int)(theta[r, c] % 4);
					if (tq == 0) //0 is E-W (horizontal)
						if (image[r, c] <= image[r, c - 1] || image[r, c] <= image[r, c + 1])
							gradSup[r, c] = 0;
					if (tq == 1) //1 is NE-SW
						if (image[r, c] <= image[r - 1, c + 1] || image[r, c] <= image[r + 1, c - 1])
							gradSup[r, c] = 0;
					if (tq == 2) //2 is N-S (vertical)
						if (image[r, c] <= image[r - 1, c] || image[r, c] <= image[r + 1, c])
							gradSup[r, c] = 0;
					if (tq == 3) //#3 is NW-SE
						if (image[r, c] <= image[r - 1, c - 1] || image[r, c] <= image[r + 1, c + 1])
							gradSup[r, c] = 0;
				}
			}
			return gradSup;
		}

		public static int[,] toGrayScale(Bitmap image)
		{
			var width = image.Width;
			var height = image.Height;
			var result = new int[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var c = image.GetPixel(x, y);
					var grayScale = (int)(((float)c.R * 0.3) + ((float)c.G * 0.59) + ((float)c.B * +0.11));
					result[x, y] = grayScale;
				}
			}
			return result;
		}

		public static int[,] doubleThreashold(int[,] image)
		{
			var highThreshold = 255 * 0.09;
			var lowThreshold = highThreshold * 0.05;
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			var doubleMap = new int[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (image[x, y] <= lowThreshold)
					{
						doubleMap[x, y] = 0;
					}
					else if (image[x, y] >= highThreshold)
					{
						doubleMap[x, y] = 255;
					}
					else
					{
						doubleMap[x, y] = weak;
					}
				}
			}

			return doubleMap;
		}

		public static bool hasStrongNeighbor(int[,] image, int x, int y)
		{
			var strong = 0;
			var width = image.GetLength(0);
			var height = image.GetLength(1);

			var result = false;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					var posX = x + i;
					var posY = y + j;
					// not edges or itself
					if (!((i == 1 && j == 1) || posX <= 0 || posX >= width - 1 || posY <= 0 || posY >= height - 1))
					{
						result = result || image[posX, posY] == strong;
					}
				}
			}
			return result;
		}

		public static Bitmap hysteresis(int[,] img)
		{
			var width = img.GetLength(0);
			var height = img.GetLength(1);
			var image = new Bitmap(width, height);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (img[x, y] == weak)
					{
						if (hasStrongNeighbor(img, x, y))
							image.SetPixel(x, y, black);
						else
							image.SetPixel(x, y, white);
					}
					else
					{
						var value = img[x, y];
						image.SetPixel(x, y, Color.FromArgb(value, value, value));
					}
				}
			}
			return image;
		}
	}
}
