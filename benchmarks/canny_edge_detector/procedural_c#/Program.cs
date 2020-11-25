using System;
using System.Drawing;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var image = new Bitmap("benchmarks/canny_edge_detector/download.jpg");

			var imageArrGray = toGrayScale(image);

			var gauFilt = GaussianFilter(5, 1.0);
			var gau = Convolve(imageArrGray, gauFilt);

			var (intensity, theta) = computeIntensity(gau);

			var nonMax = nonMaxSuppresion(intensity, theta);

			var doubleTh = doubleThreshold(nonMax);
			var (imageFinal, numWhite) = hysteresis(doubleTh);

			Console.WriteLine("White: " + numWhite);
		}

		public static int weak = 100;
		public static int black = 0;
		public static int white = 255;

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

		public static int[,] Convolve(int[,] image, double[,] filter)
		{
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			//Kernel has to be an odd number
			var halfKernel = filter.GetLength(0) / 2;

			var convolvedImage = new int[width, height];
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
							if (!(posX < 0 || posX > width - 1 || posY < 0 || posY > height - 1))
							{
								sum += ((float)(image[posX, posY]) * (filter[kx + halfKernel, ky + halfKernel]));
							}
						}
					}
					convolvedImage[x, y] = (int)sum;
				}
			}
			return convolvedImage;
		}

		public static int hyp(int num1, int num2)
		{
			return (int)Math.Sqrt((num1 * num1) + (num2 * num2));
		}

		public static (int[,], int[,]) computeIntensity(int[,] image)
		{
			var Ix = Convolve(image, kernelHor);
			var Iy = Convolve(image, kernelVer);

			var g = new int[image.GetLength(0), image.GetLength(1)];
			var thetaWidth = Iy.GetLength(0);
			var thetaHeight = Iy.GetLength(1);
			var thetaQ = new int[thetaWidth, thetaHeight];
			for (int x = 0; x < thetaWidth; x++)
			{
				for (int y = 0; y < thetaHeight; y++)
				{
					var color1 = Ix[x, y];
					var color2 = Iy[x, y];
					var hypColor = hyp(color1, color2);
					g[x, y] = hypColor;

					//Calc theta
					var theta = (Math.Atan2((float)Iy[x, y], (float)Ix[x, y]));
					var num = ((int)(Math.Round(theta * (5.0 / Math.PI))) + 5) % 5;
					thetaQ[x, y] = num;
				}
			}

			return (g, thetaQ);
		}

		public static int[,] nonMaxSuppresion(int[,] image, int[,] theta)
		{
			// Non-maximum suppression
			var gradSup = image;
			var width = image.GetLength(0);
			var height = image.GetLength(1);

			for (int r = 0; r < width - 1; r++)
			{
				for (int c = 0; c < height - 1; c++)
				{
					//Suppress pixels at the image edge
					if (r == 0 || r == width - 1 || c == 0 || c == height - 1)
					{
						gradSup[r, c] = image[r, c];
					}
					else
					{
						var tq = (int)(theta[r, c] % 4);
						if ((tq == 0 && (image[r, c] <= image[r, c - 1] || image[r, c] <= image[r, c + 1]))
							|| tq == 1 && (image[r, c] <= image[r - 1, c + 1] || image[r, c] <= image[r + 1, c - 1])
							|| tq == 2 && (image[r, c] <= image[r - 1, c] || image[r, c] <= image[r + 1, c])
							|| tq == 3 && (image[r, c] <= image[r - 1, c - 1] || image[r, c] <= image[r + 1, c + 1]))
						{
							gradSup[r, c] = black;
						}
					}
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

		public static int getMax(int[,] image)
		{
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			var max = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (image[x, y] > max)
					{
						max = image[x, y];
					}
				}
			}
			return max;
		}

		public static int[,] doubleThreshold(int[,] image)
		{
			var highThreshold = getMax(image) * 0.09;
			var lowThreshold = highThreshold * 0.5;
			var width = image.GetLength(0);
			var height = image.GetLength(1);
			var doubleMap = new int[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (image[x, y] <= lowThreshold)
					{
						doubleMap[x, y] = black;
					}
					else if (image[x, y] >= highThreshold)
					{
						doubleMap[x, y] = white;
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
			var strong = 255;
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

		public static (Bitmap, int) hysteresis(int[,] img)
		{
			var width = img.GetLength(0);
			var height = img.GetLength(1);
			var image = new Bitmap(width, height);
			var num = 0;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (img[x, y] == weak)
					{
						if (hasStrongNeighbor(img, x, y))
						{
							image.SetPixel(x, y, Color.White);
							num++;
						}
						else
						{
							image.SetPixel(x, y, Color.Black);
						}
					}
					else
					{
						var value = img[x, y];
						if (value == white)
						{
							num++;
						}
						image.SetPixel(x, y, Color.FromArgb(value, value, value));
					}
				}
			}
			return (image, num);
		}
	}
}
