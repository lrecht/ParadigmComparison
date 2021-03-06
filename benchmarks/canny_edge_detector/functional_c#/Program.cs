﻿using System;
using System.Drawing;
using System.Linq;
using System.Collections.Immutable;
using benchmark;

namespace functional_c_
{
    class Program
    {
        static readonly double[] horr = {-1.0, 0.0, 1.0, -2.0, 0.0, 2.0, -1.0, 0.0, 1.0};
        static readonly double[] verr = {1.0, 2.0, 1.0, 0.0, 0.0, 0.0, -1.0, -2.0, -1.0};
        static readonly ImmutableArray<(int x, int y, double v)> kernelHori = horr.Select((v, i) => (i % 3 - 1, i / 3 - 1, v)).ToImmutableArray();
        static readonly ImmutableArray<(int x, int y, double v)> kernelVert = verr.Select((v, i) => (i % 3 - 1, i / 3 - 1, v)).ToImmutableArray();
        static readonly int weak = 100;
        
        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);
			var pic = new Bitmap("benchmarks/canny_edge_detector/download.jpg");
            
			bm.Run(() => {
				var res = cannyEdge(pic);
				return res.Count(p => p.w > 0);
			}, (res) => {
				System.Console.WriteLine(res);
			});
        }

        private static ImmutableArray<(int x, int y, int w)> cannyEdge(Bitmap pic)
            => hysteresis(doubleThreshold(nonMaxSupression(intensityGradients(BlurGreyscale(greyScale(pic))))));

        private static ImmutableArray<(int, int, int)> hysteresis(ImmutableArray<(int x, int y, int w)> pic)
        {
            var temp = pic.Last();
            var width = temp.x + 1;
            var height = temp.y + 1;

            return pic.Select(p => {
                if(p.w == weak){
                    if(strongNeighbour(pic, width, height, p.x, p.y))
                        return (p.x, p.y, 255);
                    else
                        return (p.x, p.y, 0);
                }
                else
                    return (p.x, p.y, p.w);
            })
            .ToImmutableArray();
        }

        static readonly ImmutableArray<(int x, int y)> range = Enumerable.Range(-1, 3).SelectMany(x => Enumerable.Range(-1, 3).Select(y => (x, y))).ToImmutableArray();
        private static bool strongNeighbour(ImmutableArray<(int x, int y, int w)> pic, int width, int height, int x, int y)
        {
            return range
                .Any(f => {
                    var posX = x + f.x;
                    var posY = y + f.y;
                    return !((f.x == 1 && f.y == 1) || posX <= 0 || posX >= width - 1 || posY <= 0 || posY >= height - 1) 
                            && pic[posY * width + posX].w == 255;
                });
        }

        private static ImmutableArray<(int, int, int)> doubleThreshold(ImmutableArray<(int x, int y, int w)> pic)
        {
            var max = pic.Max(p => p.w);
            var high = max * 0.09;
            var low = high * 0.5;
            return pic.Select(p => (p.x, p.y, p.w <= low ? 0 : (p.w < high ? weak : 255)))
                    .ToImmutableArray();
        }

        private static ImmutableArray<(int, int, int)> nonMaxSupression((ImmutableArray<(int x, int y, int)> pic, ImmutableArray<(int, int, int)> direction) input)
        {
            var (pic, direction) = input;
            var temp = pic.Last();
            var width = temp.x + 1;
            var height = temp.y + 1;

            return pic.Zip(direction, (g, d) => (g.x, g.y, maxSupressionOne(width, height, pic, g.x, g.y, g.Item3, d.Item3)))
                    .ToImmutableArray();
        }

        private static int maxSupressionOne(int width, int height, ImmutableArray<(int x, int y, int)> pic, int x, int y, int w1, int w2)
        {
            if(x == 0 || x == width - 1 || y == 0 || y == height - 1)
                return w1;

            var tq = w2 % 4;
            if((tq == 0 && (w1 <= pic[x+(y-1)*width].Item3   || w1 <= pic[x+(y+1)*width].Item3)) //0 is E-W (horizontal)
            ||(tq == 1 && (w1 <= pic[x-1+(y+1)*width].Item3 || w1 <= pic[x+1+(y-1)*width].Item3)) //1 is NE-SW
            ||(tq == 2 && (w1 <= pic[x-1+y*width].Item3     || w1 <= pic[x+1+y*width].Item3)) //2 is N-S (vertical)
            ||(tq == 3 && (w1 <= pic[x-1+(y-1)*width].Item3 || w1 <= pic[x+1+(y+1)*width].Item3))) //#3 is NW-SE
                return 0;
            else
                return w1;
        }

        private static (ImmutableArray<(int, int, int)> gradient, ImmutableArray<(int, int, int)> direction) intensityGradients(ImmutableArray<(int x, int y, int colour)> pic)
        {
            var hori = convolve(pic, kernelHori);
            var vert = convolve(pic, kernelVert);

            var gradiant = hori.Zip(vert, (h, v) => (h.x, h.y, hyp(h.Item3, v.Item3))).ToImmutableArray();
            var direction = hori.Zip(vert, (h, v) => (h.x, h.y, arctan(h.Item3, v.Item3))).ToImmutableArray();

            return (gradiant, direction);
        }

        private static int arctan(int x, int y)
            => ((int) Math.Round((Math.Atan2(x, y)) * (5.0 / Math.PI)) + 5) % 5;

        private static int hyp(int x, int y)
            => (int)Math.Sqrt(x*x + y*y);

        private static ImmutableArray<(int x, int y, int colour)> BlurGreyscale(ImmutableArray<(int x, int y, int colour)> pic)
        {
            var filter = createGaussianFilter(5, 1);
            return convolve(pic, filter);
        }

        private static ImmutableArray<(int x, int y, int)> convolve(ImmutableArray<(int x, int y, int colour)> pic, ImmutableArray<(int x, int y, double w)> filter)
        {
            var temp = pic.Last();
            var width = temp.x + 1;
            var height = temp.y + 1;

            return pic.Select(p => (p.x, p.y, convolveOne(pic, width, height, filter, p))).ToImmutableArray();
        }

        private static int convolveOne(ImmutableArray<(int x, int y, int colour)> pic, int width, int height, ImmutableArray<(int x, int y, double w)> filter, (int x, int y, int) pixel) //TODO: double or int? TODO: index in stead of pixel?
        {
            return (int)filter.Aggregate(0.0, (acc, f) => {
                var xIndex = pixel.x + f.x;
                var yIndex = pixel.y + f.y;

                if (xIndex < 0 || yIndex < 0 || xIndex >= width || yIndex >= height)
                    return acc;
                else
                    return acc + (f.w * (pic[yIndex * width + xIndex].colour));
            });
        }

        private static ImmutableArray<(int x, int y, double w)> createGaussianFilter(int length, int weight)
        {
            var foff = length / 2;
            var calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(weight, 2));
            var filter = Enumerable.Range(0, length * length)
                        .Select(i => {
                            var offX = (i % length) - foff;
                            var offY = (i / length) - foff;
                            return (offX, offY, calculatedEuler * Math.Exp(-(offY * offY + (offX * offX)) / (2.0 * weight*weight)));
                        });

            var sum = filter.Sum(t => t.Item3);
            return filter.Select(x => (x.offX, x.offY, x.Item3 / sum)).ToImmutableArray();
        }

        private static ImmutableArray<(int x, int y, int colour)> greyScale(Bitmap pic)
        {
            return Enumerable.Range(0, pic.Height).SelectMany(y => Enumerable.Range(0, pic.Width).Select(x => (x, y)))
				.Select(p =>
				{
					var pixel = pic.GetPixel(p.x, p.y);
					return (p.x, p.y, (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11));
				})
				.ToImmutableArray();
        }
    }
}
