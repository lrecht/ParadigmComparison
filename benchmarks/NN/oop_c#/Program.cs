using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace oop_c_
{
    class Program
    {
        static double TRAIN_TEST_SPLIT = 0.1;
        static double LEARNING_RATE = 0.3;
        static int N_EPOCHS = 500;
        static int N_HIDDEN = 5;
        static void Main(string[] args)
        {
            var dataset = Utils.LoadCSV("benchmarks/NN/wheat-seeds.csv");
            dataset = Utils.NormaliseColumns(dataset);
            (double[,] test, double[,] train) = Utils.GetTestTrainSplit(dataset, TRAIN_TEST_SPLIT);
            (double[,] testData, double[] testActual) = (test.GetCols(0, test.GetLength(1) - 2), test.GetCol(test.GetLength(1) - 1));
            (double[,] trainData, double[] trainActual) = (train.GetCols(0, train.GetLength(1) - 2), train.GetCol(train.GetLength(1) - 1));

            int nInputs = train.GetLength(1);
            int nOutputs = trainActual.Distinct().Count();

            NeuralNetwork network = new NeuralNetwork(new AccuracyPercentage()).InitialiseLayers(nInputs, N_HIDDEN, nOutputs);
            network.Train(train, trainActual, LEARNING_RATE, N_EPOCHS);
            (int[] predictions, double accuracy) = network.Predict(test, testActual);

            System.Console.WriteLine(accuracy);
        }
    }

    public static class Utils
    {
        public static double[,] LoadCSV(string filepath)
        {
            var file = File.ReadAllLines(filepath);
            double[,] dataset = new double[file.Length, file[0].Split(',').Length];
            for (int i = 0; i < file.Length; i++)
            {
                var values = file[i].Split(',');
                for (int j = 0; j < values.Length; j++)
                {
                    if (j == values.Length - 1)
                        dataset[i, j] = double.Parse(values[j]) - 1;
                    else
                        dataset[i, j] = double.Parse(values[j]);
                }
            }
            return dataset;
        }

        public static double[,] NormaliseColumns(double[,] dataset)
        {
            int numColumns = dataset.GetLength(1) - 1;
            for (int i = 0; i < numColumns; i++)
            {
                double[] column = dataset.GetCol(i);
                double min = column.Min();
                double max = column.Max();
                double diff = max - min;
                for (int j = 0; j < column.Length; j++)
                    dataset[j, i] = (dataset[j, i] - min) / diff;
            }
            return dataset;
        }

        public static (double[,], double[,]) GetTestTrainSplit(double[,] dataset, double percent)
        {
            dataset = shuffle(new Random(), dataset);
            (int rows, int columns) = (dataset.GetLength(0), dataset.GetLength(1));
            int numTest = (int)(rows * percent);
            int numTrain = rows - numTest;
            double[,] test = new double[numTest, columns];
            double[,] train = new double[numTrain, columns];
            int testindex = 0;
            int trainindex = 0;
            for (int i = 0; i < rows; i++)
            {
                if (i < numTest)
                    test.SetRow(testindex++, dataset.GetRow(i));
                else
                    train.SetRow(trainindex++, dataset.GetRow(i));
            }
            return (test, train);
        }

        private static T[,] shuffle<T>(T[,] array)

        {
            T[,] shuffledArray = new T[array.GetLength(0), array.GetLength(1)];
            int numRows = array.GetLength(0);
            int[] indicies = new int[numRows];
            Random rnd = new Random();
            for (int i = 0; i < numRows; i++)
            {
                int randIndex = rnd.Next(0, numRows);
                // swap procedure
                var h = array.GetRow(i);
                shuffledArray.SetRow(i, array.GetRow(randIndex));
                shuffledArray.SetRow(randIndex, h);
            }
            return shuffledArray;
        }
    }

    public class NeuralNetwork
    {
        int nOutputs { get; set; }
        List<Layer> layers { get; set; }
        IAccuracyMetric accuracyMetric { get; set; }

        public NeuralNetwork(IAccuracyMetric accuracyMetric)
        {
            this.accuracyMetric = accuracyMetric;
        }

        public NeuralNetwork InitialiseLayers(int nInputs, int nHidden, int nOutputs)
        {
            this.nOutputs = nOutputs; // Output neurons
            layers = new List<Layer>();
            Layer hidden = new Layer(nHidden, nInputs + 1, null, new SigmoidActivation());
            Layer output = new Layer(nOutputs, nHidden + 1, hidden, new SigmoidActivation());
            layers.Add(hidden);
            layers.Add(output);
            return this;
        }

        public (int[], double) Predict(double[,] testData, double[] testActual)
        {
            int numTestRows = testData.GetLength(0);
            int[] predictions = new int[numTestRows];
            for (int i = 0; i < numTestRows; i++)
                predictions[i] = PredictRow(testData.GetRow(i));
            double accuracy = accuracyMetric.ComputeAccuracy(testActual, predictions);
            return (predictions, accuracy);
        }

        private int PredictRow(double[] row)
        {
            double[] outputs = ForwardPropagate(row);
            double max = outputs[0];
            int index = 0;
            for (int j = 1; j < outputs.Length; j++)
                if (outputs[j] > max)
                {
                    max = outputs[j];
                    index = j;
                }
            return index;
        }

        public double[] ForwardPropagate(double[] inputs)
        {
            foreach (Layer layer in layers)
                inputs = layer.ForwardPropagate(inputs);
            return inputs;
        }

        public void BackPropagateError(double[] expected)
        {
            int length = layers.Count() - 1;
            // Iterating through layers, staring with the ouput layer
            // This ensures that the neurons in the output layer have ‘delta’ 
            // values calculated first that neurons in the hidden layer can use 
            // in the subsequent iteration. 
            for (int i = length; i >= 0; i--)
            {
                Layer layer = layers[i];
                List<double> errors = new List<double>();
                // Iterating through all neurons in the layer
                int numNeurons = layer.Neurons.Length;
                for (int j = 0; j < numNeurons; j++)
                {
                    Neuron n = layer.Neurons[j];
                    // If output layer
                    if (i == length)
                        errors.Add(expected[j] - n.Output);
                    // If hidden layer
                    else
                    {
                        double error = 0.0;
                        // Accumulate error based on neurons in the next layer
                        foreach (Neuron neuron in layers[i + 1].Neurons)
                            error += neuron.Weights[j] * neuron.Delta;
                        errors.Add(error);
                    }
                    n.Delta = errors[j] * n.Derivative;
                }
            }
        }
        public void Train(double[,] trainData, double[] trainActual, double learningRate, int nEpochs)
        {
            for (int i = 0; i < nEpochs; i++)
            {
                double sumError = 0;
                // For each row in the training data
                for (int j = 0; j < trainData.GetLength(0); j++)
                {
                    double[] features = trainData.GetRow(j);
                    double actual = trainActual[j];

                    double[] outputs = ForwardPropagate(features);
                    double[] expected = new double[nOutputs];
                    expected[(int)actual] = 1;

                    // Calculate Error
                    sumError += SumSquaredError(outputs, expected);

                    // Calculate Gradient and Delta
                    BackPropagateError(expected);

                    // Adjust weights and biases
                    updateWeights(features, learningRate);
                }
            }
        }

        private double SumSquaredError(double[] outputs, double[] expected)
        {
            double res = 0;
            for (int p = 0; p < nOutputs; p++)
                res += Math.Pow(expected[p] - outputs[p], 2);
            return res;
        }

        private void updateWeights(double[] row, double learning_rate)
        {
            foreach (Layer layer in layers)
                layer.UpdateWeights(row, learning_rate);
        }
    }

    public class Layer
    {
        public Neuron[] Neurons { get; set; }
        int numNeurons { get; set; }
        IActivationStrategy activation { get; set; }
        Layer previousLayer { get; set; }
        public Layer(int nNeurons, int nNeuronInputConnections, Layer previous, IActivationStrategy activation)
        {
            numNeurons = nNeurons;
            previousLayer = previous;
            this.activation = activation;
            Neurons = new Neuron[nNeurons];
            // Adds specified neurons
            for (int i = 0; i < nNeurons; i++)
                Neurons[i] = new Neuron(nNeuronInputConnections);
        }

        public double[] ForwardPropagate(double[] inputs)
        {
            List<double> newInputs = new List<double>();
            foreach (Neuron neuron in Neurons)
            {
                double activation = this.activation.Activate(neuron.Weights, inputs);
                neuron.Output = activation;
                newInputs.Add(activation);
            }
            return newInputs.ToArray();
        }

        public void UpdateWeights(double[] row, double learningRate)
        {
            double[] inputs = row[0..^1];
            if (previousLayer != null)
            {
                Neuron[] neuronsInPreviousLayer = previousLayer.Neurons;
                inputs = new double[neuronsInPreviousLayer.Length];
                for (int neuron = 0; neuron < neuronsInPreviousLayer.Length; neuron++)
                    inputs[neuron] = neuronsInPreviousLayer[neuron].Output;
            }
            foreach (Neuron n in Neurons)
            {
                for (int j = 0; j < inputs.Length; j++)
                    n.Weights[j] += learningRate * n.Delta * inputs[j];
                n.Weights[^1] += learningRate * n.Delta;
            }
        }
    }

    public class Neuron
    {
        static Random rnd = new Random(2);
        public double[] Weights { get; set; }
        public double Output { get; set; }
        public double Delta { get; set; }
        public double Derivative
        {
            // Calculate the derivative of an neuron output
            // using the sigmoid transfer function
            get => Output * (1 - Output);
        }
        public Neuron(int nInputs)
        {
            Weights = new double[nInputs];
            Delta = 0;
            // Random weights for each input to the neuron
            for (int i = 0; i < nInputs; i++)
            {
                double rand = rnd.NextDouble();
                Weights[i] = rand;
            }
        }
    }

    public interface IActivationStrategy
    {
        double Activate(double[] weights, double[] inputs);
    }

    public class SigmoidActivation : IActivationStrategy
    {
        public double Activate(double[] weights, double[] inputs)
        {
            int length = weights.Length - 1;
            // This is bias
            double activation = weights[length];
            for (int i = 0; i < length; i++)
                activation += weights[i] * inputs[i];
            return 1.0 / (1.0 + Math.Exp(-activation));
        }
    }

    public interface IAccuracyMetric
    {
        double ComputeAccuracy(double[] actual, int[] predictions);
    }

    public class AccuracyPercentage : IAccuracyMetric
    {
        public double ComputeAccuracy(double[] actual, int[] predictions)
        {
            int correct = 0;
            int numPredictions = predictions.Length;
            for (int i = 0; i < numPredictions; i++)
                if (actual[i] == predictions[i])
                    correct++;
            return ((double)correct / numPredictions) * 100;
        }
    }
    public static class MatrixExtensions
    {
        /// <summary>
        /// Returns the row with number 'row' of this matrix as a 1D-Array.
        /// </summary>
        public static T[] GetRow<T>(this T[,] matrix, int row)
        {
            var rowLength = matrix.GetLength(1);
            var rowVector = new T[rowLength];

            for (var i = 0; i < rowLength; i++)
                rowVector[i] = matrix[row, i];

            return rowVector;
        }

        /// <summary>
        /// Sets the row with number 'row' of this 2D-matrix to the parameter 'rowVector'.
        /// </summary>
        public static void SetRow<T>(this T[,] matrix, int row, T[] rowVector)
        {
            var rowLength = matrix.GetLength(1);

            for (var i = 0; i < rowLength; i++)
                matrix[row, i] = rowVector[i];
        }

        /// <summary>
        /// Returns the column with number 'col' of this matrix as a 1D-Array.
        /// </summary>
        public static T[] GetCol<T>(this T[,] matrix, int col)
        {
            var colLength = matrix.GetLength(0);
            var colVector = new T[colLength];

            for (var i = 0; i < colLength; i++)
                colVector[i] = matrix[i, col];

            return colVector;
        }

        public static T[,] GetCols<T>(this T[,] matrix, int start, int end)
        {
            var rowLength = matrix.GetLength(0);
            var cols = new T[rowLength, end - start];

            for (int i = start; i < end; i++)
                for (var j = 0; j < rowLength; j++)
                    cols[j, i - start] = matrix[j, i];
            return cols;
        }
    }
}
