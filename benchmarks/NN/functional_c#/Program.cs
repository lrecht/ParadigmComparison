using System;
using System.Linq;
using System.Collections.Immutable;


namespace functional_c_
{
    using Layer = ImmutableArray<Neuron>;
    struct Neuron
    {
        public readonly ImmutableArray<double> weights;
        public readonly double output;
        public readonly double bias;
        public readonly double delta;
        public Neuron(ImmutableArray<double> w, double o, double b, double d){
            weights = w;
            output = o;
            bias = b;
            delta = d;
        }
    }

    class Program
    {
        static Random rand = new Random(2);

        static readonly ImmutableArray<ImmutableArray<double>> rawData = System.IO.File.ReadAllLines("../wheat-seeds.csv").Select(l => l.Split(',').Select(n => double.Parse(n)).ToImmutableArray()).ToImmutableArray();
        static readonly ImmutableArray<ImmutableArray<double>> wheatData = rawData.Select(row => row.SetItem(row.Length - 1, row[^1] - 1)).ToImmutableArray();

        static void Main(string[] args)
        {
            var nInput = wheatData[0].Length;
            var nHidden = 5;
            var nOutput = wheatData.Select(row => row[row.Length - 1]).Distinct().Count();
            var learningRate = 0.3;
            var epochs = 500;

            var dataset = normaliseDataset(wheatData, datasetMinMax(wheatData))
                        .OrderBy(_ => rand.Next())
                        .ToImmutableArray();
            
            var splitIndex = dataset.Length / 10;
            var testData = dataset.Take(splitIndex).ToImmutableArray();
            var trainData = dataset.TakeLast(dataset.Length - splitIndex).ToImmutableArray();

            var init = initialiseNetwork(nInput, nHidden, nOutput);

            var trainedNetwork = trainNetwork(init, trainData, learningRate, epochs, nOutput);
 
            var results = testData.Select(row => (prediction: predict(trainedNetwork, row), actual: (int)row[^1])).ToImmutableArray();
            var correctPredictions = results.Where(res => res.prediction == res.actual).Count();
            var accuracy = correctPredictions / (double)results.Length * 100.0;
            
            System.Console.WriteLine(accuracy);
        }

        private static ImmutableArray<Layer> backwardsPropagateError(ImmutableArray<Layer> network, ImmutableArray<double> expected)
        {
            var firstLayer = network[0];
            var errors = firstLayer.Zip(expected, (neuron, exp) => exp - neuron.output).ToImmutableArray();
            
            Func<double, double> derivative = (neuronOutput) => neuronOutput * (1.0 - neuronOutput);
            Func<double, Neuron, Neuron> updateNeuronDelta = (error, neuron) => new Neuron(neuron.weights, neuron.output, neuron.bias, error * derivative(neuron.output));
            Func<ImmutableArray<double>, Layer, Layer> delta = (errors, layer) => errors.Zip(layer, (error, neuron) => updateNeuronDelta(error, neuron)).ToImmutableArray();
            Func<Layer, int, double> neuronError = (layer, i) => layer.Select(neuron => neuron.weights[i] * neuron.delta).Sum();
            Func<ImmutableArray<Layer>, Layer, ImmutableArray<Layer>> backProp = (res, layer) => {
                var errors = layer.Select((neuron, i) => neuronError(res[0], i)).ToImmutableArray();
                var newLayer = delta(errors, layer);
                return res.Add(newLayer);
            };

            return network.RemoveAt(0).Aggregate(ImmutableArray<Layer>.Empty.Add(delta(errors, firstLayer)), (acc, r) => backProp(acc, r)).Reverse().ToImmutableArray();
        }

        private static (ImmutableArray<Layer>, ImmutableArray<double>) forwardPropagate(ImmutableArray<double> row, ImmutableArray<Layer> network)
        {
            Func<ImmutableArray<double>, ImmutableArray<double>, double, double> activate = (weights, inputs, bias) => bias + Enumerable.Zip(weights, inputs, (w, i) => w * i).Sum();
            Func<double, double> transfer = (activation) => 1.0 / (1.0 + Math.Exp(-activation));
            Func<ImmutableArray<double>, Neuron, Neuron> activateNeuron = (inputs, neuron) => new Neuron(neuron.weights, transfer(activate(neuron.weights, inputs, neuron.bias)), neuron.bias, neuron.delta);
            Func<Layer, ImmutableArray<double>> getOutput = (layer) => layer.Select(neuron => neuron.output).ToImmutableArray();

            var propagatedLayersTemp = network.Aggregate((row, layers: ImmutableArray<Layer>.Empty), (acc, layer) => {
                var newLayer = layer.Select(neuron => activateNeuron(acc.Item1, neuron)).ToImmutableArray();
                return (getOutput(newLayer), acc.layers.Add(newLayer));
            });
            var propagatedLayers = (propagatedLayersTemp.row, layers: propagatedLayersTemp.layers.Reverse().ToImmutableArray());

            return (propagatedLayers.layers, getOutput(propagatedLayers.layers[0]));
        }

        private static ImmutableArray<Layer> initialiseNetwork(int nInput, int nHidden, int nOutput){
            Layer initNeuronsWithWeights(int numNeurons, int numWeights){
                return Enumerable.Range(0, numNeurons)
                    .Select(i => new Neuron(Enumerable.Range(0, numWeights).Select(_ => rand.NextDouble()).ToImmutableArray(), 0, rand.NextDouble(), 0))
                    .ToImmutableArray();
            }

            return ImmutableArray<Layer>.Empty.Add(initNeuronsWithWeights(nHidden, nInput)).Add(initNeuronsWithWeights(nOutput, nHidden));
        }

        private static ImmutableArray<Layer> updateWeights(ImmutableArray<Layer> network, ImmutableArray<double> dataRow, double learningRate){
            var inputs = network.Select((l, i) => {
                if(i != 0)
                    return network[i - 1].Select(neuron => neuron.output).ToImmutableArray();
                else
                    return dataRow.Take(dataRow.Length - 1).ToImmutableArray();
            }).ToImmutableArray();

            return network.Select((l, i) => {
                    var layerInputs = inputs[i];
                    var range = Enumerable.Range(0, layerInputs.Length);
                    return l.Select(neuron => {
                        var updatedWeights = range.Select(j => neuron.weights[j] + (learningRate * neuron.delta * layerInputs[j])).ToImmutableArray();
                        var updatedBias = neuron.bias + learningRate * neuron.delta;
                        return new Neuron(updatedWeights, neuron.output, updatedBias, neuron.delta);
                        }).ToImmutableArray();
                }).ToImmutableArray();
        }

        private static ImmutableArray<Layer> trainNetwork(ImmutableArray<Layer> network, ImmutableArray<ImmutableArray<double>> train, double learningRate, int nEpoch, int nOutput){
            var epochRange = Enumerable.Range(0, nEpoch);
            var outputRange = Enumerable.Range(0, nOutput);
            return epochRange.Aggregate(network, (accNetwork, epoch) => {
                var res = train.Aggregate((accNetwork, error: 0.0), (acc, row) => {
                    var (forwardNet, outputs) = forwardPropagate(row, acc.accNetwork);
                    var expected = outputRange.Select(i => 0.0).ToImmutableArray().SetItem((int)row[^1], 1.0);
                    var sumError = acc.error + outputRange.Select(i => Math.Pow((expected[i] - outputs[i]), 2)).Sum();
                    return (updateWeights(backwardsPropagateError(forwardNet, expected), row, learningRate), sumError);
                });
                return res.accNetwork;
            });
        }

        private static int predict(ImmutableArray<Layer> network, ImmutableArray<double> row){
            var (_, output) = forwardPropagate(row, network);
            return output.IndexOf(output.Max());
        }

        private static ImmutableArray<ImmutableArray<double>> normaliseDataset(ImmutableArray<ImmutableArray<double>> dataset, ImmutableArray<(double columnMin, double columnMax)> minmax){
            var lastColumnIndex = dataset[0].Length - 1;
            return dataset.Select(row => row.Select((num, i) => i < lastColumnIndex ? ((num - minmax[i].columnMin) / (minmax[i].columnMax - minmax[i].columnMin)) : num)
                                        .ToImmutableArray())
                        .ToImmutableArray();
        }

        private static ImmutableArray<(double columnMin, double columnMax)> datasetMinMax(ImmutableArray<ImmutableArray<double>> dataset)
            => dataset.SelectMany(row => row.Select((elem, i) => (elem, i)))
                .GroupBy(t => t.i)
                .Select(g => (g.Min(t => t.elem), g.Max(t => t.elem)))
                .ToImmutableArray(); 
    }
}
