using System;
using System.Drawing;
using System.Linq;
using System.Collections.Immutable;

namespace functional_c_
{
    class Program
    {
        static readonly double[] horr = {-1.0, 0.0, 1.0, -2.0, 0.0, 2.0, -1.0, 0.0, 1.0};
        static readonly double[] verr = {1.0, 2.0, 1.0, 0.0, 0.0, 0.0, -1.0, -2.0, -1.0};
        static readonly ImmutableList<(int x, int y, double v)> kernelHori = horr.Select((v, i) => (i % 3 - 1, i / 3 - 1, v)).ToImmutableList();
        static readonly ImmutableList<(int x, int y, double v)> kernelVert = verr.Select((v, i) => (i % 3 - 1, i / 3 - 1, v)).ToImmutableList();
        static readonly int weak = 100;
        static void Main(string[] args)
        {
            var pic = new Bitmap("benchmarks/canny_edge_detector/download.jpg");
            var res = cannyEdge(pic);
            System.Console.WriteLine(res.Count(p => p.w > 0));
        }

        private static ImmutableList<(int x, int y, int w)> cannyEdge(Bitmap pic)
        {
            var pixelCords = Enumerable.Range(0, pic.Height).SelectMany(y => Enumerable.Range(0, pic.Width).Select(x => (x, y))).ToImmutableList();

            var greyscaled = greyScale(pic, pixelCords);
            //saveComputedPic(greyscaled, "greyscaled");

            var gaussianPic = BlurGreyscale(greyscaled);
            //saveComputedPic(gaussianPic, "blurred");

            var (intensityGradientsPic, direction) = intensityGradients(gaussianPic);
            //saveComputedPic(intensityGradientsPic, "intensity");

            var nonMaxSupressedPic = nonMaxSupression(intensityGradientsPic, direction);
            //saveComputedPic(nonMaxSupressedPic, "nonMaxSupressed");

            var doubleThresholdedPic = doubleThreshold(nonMaxSupressedPic);
            var hysteresisedPic = hysteresis(doubleThresholdedPic);
            //saveComputedPic(hysteresisedPic, "hysteresis");

            return hysteresisedPic;
        }

        private static ImmutableList<(int, int, int)> hysteresis(ImmutableList<(int x, int y, int w)> pic)
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
            .ToImmutableList();
        }

        static readonly ImmutableList<(int x, int y)> range = Enumerable.Range(-1, 3).SelectMany(x => Enumerable.Range(-1, 3).Select(y => (x, y))).ToImmutableList();
        private static bool strongNeighbour(ImmutableList<(int x, int y, int w)> pic, int width, int height, int x, int y)
        {
            return range
                .Select(f => {
                    var posX = x + f.x;
                    var posY = y + f.y;
                    return (f.x, f.y, posX, posY);
                })
                .Where(f => !((f.x == 1 && f.y == 1) || f.posX <= 0 || f.posX >= width - 1 || f.posY <= 0 || f.posY >= height - 1))
                .Any(f => pic[f.posY * width + f.posX].w == 255);
        }

        private static ImmutableList<(int, int, int)> doubleThreshold(ImmutableList<(int x, int y, int w)> pic)
        {
            var max = pic.Max(p => p.w);
            var high = max * 0.09;
            var low = high * 0.5;
            return pic.Select(p => (p.x, p.y, p.w <= low ? 0 : (p.w < high ? weak : 255)))
                    .ToImmutableList();
        }

        private static ImmutableList<(int, int, int)> nonMaxSupression(ImmutableList<(int x, int y, int)> pic, ImmutableList<(int, int, int)> direction)
        {
            var temp = pic.Last();
            var width = temp.x + 1;
            var height = temp.y + 1;

            return pic.Zip(direction, (g, d) => (g.x, g.y, maxSupressionOne(width, height, pic, g.x, g.y, g.Item3, d.Item3)))
                    .ToImmutableList();
        }

        private static int maxSupressionOne(int width, int height, ImmutableList<(int x, int y, int)> pic, int x, int y, int w1, int w2)
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

        private static (ImmutableList<(int, int, int)> gradient, ImmutableList<(int, int, int)> direction) intensityGradients(ImmutableList<(int x, int y, int colour)> pic)
        {
            var hori = convolve(pic, kernelHori);
            var vert = convolve(pic, kernelVert);

            //saveComputedPic(hori, "hori"); //TODO: Remove
            //saveComputedPic(vert, "vert");

            var gradiant = hori.Zip(vert, (h, v) => (h.x, h.y, hyp(h.Item3, v.Item3))).ToImmutableList();
            var direction = hori.Zip(vert, (h, v) => (h.x, h.y, arctan(h.Item3, v.Item3))).ToImmutableList();

            return (gradiant, direction);
        }

        private static int arctan(int x, int y)
            => ((int) Math.Round((Math.Atan2(x, y)) * (5.0 / Math.PI)) + 5) % 5;

        private static int hyp(int x, int y)
            => (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

        private static ImmutableList<(int x, int y, int colour)> BlurGreyscale(ImmutableList<(int x, int y, int colour)> pic)
        {
            var filter = createGaussianFilter(5, 1);  //TODO: REMOVE HARDCODED STUFFS
            return convolve(pic, filter);
        }

        private static ImmutableList<(int x, int y, int)> convolve(ImmutableList<(int x, int y, int colour)> pic, ImmutableList<(int x, int y, double w)> filter)
        {
            var temp = pic.Last();
            var width = temp.x + 1;
            var height = temp.y + 1;

            return pic.Select(p => (p.x, p.y, convolveOne(pic, width, height, filter, p))).ToImmutableList();
        }

        private static int convolveOne(ImmutableList<(int x, int y, int colour)> pic, int width, int height, ImmutableList<(int x, int y, double w)> filter, (int x, int y, int) pixel) //TODO: double or int? TODO: index in stead of pixel?
        {
            return (int)filter.Select(f => {
                var xIndex = pixel.x + f.x;
                var yIndex = pixel.y + f.y;

                if (xIndex < 0 || yIndex < 0 || xIndex >= width || yIndex >= height) //TODO: Potential performance by having const values
                    return 0;
                else
                    return f.w * (pic[yIndex * width + xIndex].colour);
            })
            .Sum(); //TODO: PErformance by using aggregate?
        }

        private static ImmutableList<(int x, int y, double w)> createGaussianFilter(int length, int weight) //TODO: SHould weight be double or int?
        {
            var foff = length / 2;
            var calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(weight, 2));
            var filter = Enumerable.Range(0, length * length)
                        .Select(i => {
                            var offX = (i % length) - foff;
                            var offY = (i / length) - foff;
                            return (offX, offY, calculatedEuler * Math.Exp(-(offY * offY + (offX * offX)) / (2.0 * Math.Pow(weight, 2))));
                        });

            var sum = filter.Sum(t => t.Item3);
            return filter.Select(x => (x.offX, x.offY, x.Item3 / sum)).ToImmutableList();
        }

        private static ImmutableList<(int x, int y, int colour)> greyScale(Bitmap pic, ImmutableList<(int x, int y)> pixelCords)
        {
            return pixelCords.Select(p => (p.x, p.y, pic.GetPixel(p.x, p.y)))
                    .Select(t => (t.x, t.y, (int)(t.Item3.R * 0.3 + t.Item3.G * 0.59 + t.Item3.B * 0.11)))
                    .ToImmutableList(); //TODO: More performance here by combining into one select?
        }


        //TODO: Remove
        public static void saveComputedPic(ImmutableList<(int x, int y, int colour)> pixels, string name){
            var newPic = new Bitmap(pixels.Last().x + 1, pixels.Last().y + 1);
            pixels.Select(p => (p.x, p.y, p.colour <= 255 ? (p.colour >= 0 ? p.colour : 0) : 255))
                .ToList()
                .ForEach(p => newPic.SetPixel(p.x, p.y, Color.FromArgb(p.Item3, p.Item3, p.Item3)));
            newPic.Save(name + ".jpg");
        }
    }
}
