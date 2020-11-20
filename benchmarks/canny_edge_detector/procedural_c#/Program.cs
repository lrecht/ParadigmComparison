using System;
using System.Drawing;

namespace procedural_c_
{
	class Program
	{
		static void Main(string[] args)
		{
			var image = new Bitmap("blur.png");
			double[,] hoKernel = { { 1.0, 2.0, 1.0 }, { 0.0, 0.0, 0.0 }, { -1.0, -2.0, -1.0 } };

			var test = Convolve(image, hoKernel);
			test.Save("test.png");
		}

		static Bitmap Convolve(Bitmap image, double[,] kernel)
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
	}
}
