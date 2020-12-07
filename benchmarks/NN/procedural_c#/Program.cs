using System;

namespace procedural_c_
{
	class Program
	{
		//global field stuff
		public struct Neuron
		{
			public float[] Weights;
			public float Delta;
			public float Output;
		}

		public struct NetLayer
		{
			public Neuron[] Layer;
		}

		public static Random rand = new Random(2);

		static void Main(string[] args)
		{
			var dataset = getDataset();
			var nHidden = 5;
			var learningRate = 0.3f;
			var epochs = 500;

			normalizeDataset(dataset);

			var score = evaluateAlgorithm(dataset, learningRate, epochs, nHidden);
			Console.WriteLine("Score:" + score);
		}

		// ---- Initilizers
		public static float[] randomInitArray(float[] array)
		{
			for (int weightIndex = 0; weightIndex < array.Length; weightIndex++)
				array[weightIndex] = (float)rand.NextDouble();
			return array;
		}

		// network
		public static NetLayer[] getNetwork(int[] layers, int inputs)
		{
			var num = inputs;
			var network = new NetLayer[layers.Length];
			for (int i = 0; i < layers.Length; i++)
			{
				network[i] = new NetLayer { Layer = new Neuron[layers[i]] };
				for (int j = 0; j < layers[i]; j++)
				{
					network[i].Layer[j] = new Neuron { Delta = 0.0f, Output = 0.0f, Weights = (randomInitArray(new float[num + 1])) };
				}
				num = layers[i];
			}

			return network;
		}

		// ---- Forward propagation
		public static float activate(Neuron neuron, float[] inputs)
		{
			var activation = neuron.Weights[(neuron.Weights.Length - 1)];
			for (int i = 0; i < neuron.Weights.Length - 1; i++)
			{
				activation += ((neuron.Weights[i]) * inputs[i]);
			}
			return activation;
		}

		//Uses the sigmoid activation function
		public static float transfer(float activation)
		{
			return (float)(1.0 / (1.0 + Math.Exp(-activation)));
		}

		// Forward propagate input to a network output
		public static float[] forwardPropagate(NetLayer[] network, float[] data)
		{
			var inputs = data;

			for (int layer = 0; layer < network.Length; layer++)
			{
				var lay = network[layer].Layer;
				var input = lay.Length;
				var newInputs = new float[input];
				for (int neuron = 0; neuron < input; neuron++)
				{
					var activation = activate(lay[neuron], inputs);
					newInputs[neuron] = transfer(activation);
					lay[neuron].Output = newInputs[neuron];
				}
				inputs = newInputs;
			}
			return inputs;
		}

		// ---- Back propagation
		//Sigmoid trasfer function
		public static float transferDerivative(float output)
		{
			return output * (1.0f - output);
		}

		public static void backwardPropagateError(NetLayer[] network, float[] expected)
		{
			for (int num = 0; num < network.Length; num++)
			{
				var i = network.Length - 1 - num;
				var layer = network[i].Layer;
				var inputNeurons = layer.Length;
				var errors = new float[inputNeurons];

				for (int j = 0; j < inputNeurons; j++)
				{
					if (i != network.Length - 1)
					{
						//Hidden layers
						var error = 0.0f;
						foreach (var neuron in network[(i + 1)].Layer)
						{
							error += (neuron.Weights[j] * neuron.Delta);
						}
						errors[j] = error;
					}
					else
						//Output layer
						errors[j] = expected[j] - layer[j].Output;
					layer[j].Delta = errors[j] * (transferDerivative(layer[j].Output));
				}
			}
		}

		// ---- Training
		public static void updateWeights(NetLayer[] network, float[] inputs, float learningRate)
		{
			for (int i = 0; i < network.Length; i++)
			{
				var layer = network[i].Layer;
				var input = inputs[0..(inputs.Length - 2)]; //Removes the last input, as this is the expected answer

				//If it is not the first layer, then the inputs should be set to the outputs in the previous layer.
				if (i != 0)
				{
					var previousLayer = network[i - 1].Layer;
					input = new float[previousLayer.Length];
					for (int n = 0; n < previousLayer.Length; n++)
						input[n] = previousLayer[n].Output;
				}
				foreach (var neuron in layer)
				{
					for (int j = 0; j < input.Length; j++)
						neuron.Weights[j] += learningRate * neuron.Delta * input[j];
					neuron.Weights[neuron.Weights.Length - 1] = neuron.Weights[neuron.Weights.Length - 1] + learningRate * neuron.Delta;
				}
			}
		}

		public static void trainNetwork(NetLayer[] network, float[,] data, float learningRate, int epochs, int numOutPuts)
		{
			var rows = data.GetLength(0);
			var cols = data.GetLength(1);
			for (int e = 0; e < epochs; e++)
			{
				var sumError = 0.0;
				for (int rowIndex = 0; rowIndex < rows; rowIndex++)
				{
					var row = (data.GetRow(rowIndex))[0..cols];
					var outputs = forwardPropagate(network, row);
					var expected = new float[numOutPuts];
					var index = (int)row[row.Length - 1];
					expected[index] = 1.0f;
					for (int i = 0; i < numOutPuts; i++)
					{
						sumError += Math.Pow((expected[i] - outputs[i]), 2.0);
					}

					backwardPropagateError(network, expected);
					updateWeights(network, row, learningRate);
				}
			}
		}

		public static int predict(NetLayer[] network, float[] data)
		{
			var output = forwardPropagate(network, data);
			var index = 0;
			var max = output[0];
			for (int i = 1; i < output.Length; i++)
			{
				if (output[i] > max)
				{
					index = i;
					max = output[i];
				}
			}
			return index;
		}

		public static int getNumOutpus(float[,] dataset)
		{
			var outputValues = new int[10];
			var rows = dataset.GetLength(0);
			var cols = dataset.GetLength(1);
			for (int x = 0; x < rows; x++)
			{
				var test = (int)(dataset[x, cols - 1]);
				outputValues[test] = 1;
			}
			var numOut = 0;
			for (int o = 0; o < outputValues.Length; o++)
			{
				if (outputValues[o] == 1)
					numOut++;
			}
			return numOut;
		}

		public static float[,] getDataset()
		{
			//Create a dataset from the file
			var dataStrings = System.IO.File.ReadAllLines("benchmarks/NN/wheat-seeds.csv");
			var dataset = new float[dataStrings.Length, (dataStrings[0].Split(',').Length)];
			for (int line = 0; line < dataStrings.Length; line++)
			{
				var elms = dataStrings[line].Split(',');
				//Insert all values except for the last
				for (int elm = 0; elm < elms.Length - 2; elm++)
				{
					dataset[line, elm] = float.Parse(elms[elm]);
				}
				//The last value (the result)
				dataset[line, (elms.Length - 1)] = float.Parse(elms[elms.Length - 1]) - 1.0f;
			}
			return dataset;
		}

		public static void normalizeDataset(float[,] dataset)
		{
			var width = dataset.GetLength(0);
			var height = dataset.GetLength(1);
			for (int x = 0; x < width; x++)
			{
				var (min, max) = (float.MaxValue, 0.0f);
				for (int y = 0; y < height - 1; y++)
				{
					if (dataset[x, y] < min)
						min = dataset[x, y];
					if (dataset[x, y] > max)
						max = dataset[x, y];
				}

				for (int y = 0; y < height - 1; y++)
				{
					dataset[x, y] = (dataset[x, y] - min) / (max - min);
				}
			}
		}

		public static void shuffleDataset(float[,] dataset)
		{
			var l1 = dataset.GetLength(0) - 1;
			var l2 = dataset.GetLength(1) - 1;
			for (int i = 0; i <= l1; i++)
			{
				var r = rand.Next(0, l1);
				var temp = dataset.GetRow(i);
				dataset.SetRow(i, dataset.GetRow(r));
				dataset.SetRow(r, temp);
			}
		}

		// ---- Evaluation
		public static float accuracyMetric(float[] actual, float[] predicted)
		{
			var correct = 0;
			for (int a = 0; a < actual.Length; a++)
			{
				if (actual[a] == predicted[a])
					correct++;
			}
			return (float)correct / ((float)actual.Length) * 100.0f;
		}

		public static float[] backPropagation(float[,] train, float[,] test, float learningRate, int epocs, int nHidden)
		{
			var nInputs = train.GetLength(1);
			var nOutputs = getNumOutpus(train);

			var network = getNetwork(new int[] { nHidden, nOutputs }, nInputs);

			trainNetwork(network, train, learningRate, epocs, nOutputs);
			var testRows = test.GetLength(0);
			var testCols = test.GetLength(1);
			var predictions = new float[testRows];
			for (int x = 0; x < testRows; x++)
			{
				var prediction = predict(network, test.GetRow(x));
				predictions[x] = prediction;
			}
			return predictions;
		}

		public static float evaluateAlgorithm(float[,] dataset, float learningRate, int epocs, int nHidden)
		{
			shuffleDataset(dataset);
			var tenPercent = dataset.GetLength(0) / 10;

			//Create the training data
			float[,] trainSet = dataset.GetRows((tenPercent + 1), (dataset.GetLength(0) - 1));

			//Create the testing data
			float[,] testSet = dataset.GetRows(0, tenPercent);
			var prediction = backPropagation(trainSet, testSet, learningRate, epocs, nHidden);
			float[] actual = testSet.GetCol(testSet.GetLength(1) - 1);

			return accuracyMetric(actual, prediction);
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

		public static T[,] GetRows<T>(this T[,] matrix, int start, int end)
		{
			var colLength = matrix.GetLength(1);
			var rows = new T[end - start, colLength];

			for (int i = start; i < end; i++)
				for (var j = 0; j < colLength; j++)
					rows[i - start, j] = matrix[i, j];
			return rows;
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
