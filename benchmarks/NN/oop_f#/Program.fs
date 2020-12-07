// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Collections.Generic
open System.IO


type IActivationStrategy =
    abstract member Activate : double[] -> double[] -> double


type IAccuracyMetric =
    abstract member ComputeAccuracy : double[] -> int[] -> double


type SigmoidActivation() =
    interface IActivationStrategy with
        member __.Activate weights inputs =
            let length = weights.Length - 1
            let mutable activation = weights.[length]
            for i in 0 .. length - 1 do
                activation <- activation + (weights.[i] * inputs.[i])
            1.0 / (1.0 + Math.Exp(- activation))


type AccuracyPercentage() =
    interface IAccuracyMetric with
        member __.ComputeAccuracy actual predictions =
            let mutable correct = 0
            let numPredictions = predictions.Length
            for i in 0 .. numPredictions - 1 do
                if int(actual.[i]) = predictions.[i] then correct <- correct + 1
            (double(correct) / double(numPredictions)) * 100.0


type Neuron(nInputs, activation : IActivationStrategy) as this =
    let weights : double[] = Array.zeroCreate nInputs
    let rnd = Random()
    do
        for i in 0 .. nInputs - 1 do
            weights.[i] <- rnd.NextDouble()

    member val Weights : double[] = weights with get, set
    member val Output = 0.0 with get, set
    member val Delta = 0.0 with get, set
    member __.Derivative = this.Output * (1.0 - this.Output)
    member __.Activate inputs =
        this.Output <- activation.Activate this.Weights inputs
        this.Output



[<AllowNullLiteralAttribute>]
type Layer(nNeurons, nNeuronInputConnections, previous : Layer, activation : IActivationStrategy) as this =
    let neurons : Neuron[] = Array.zeroCreate nNeurons
    do
        for i in 0 .. nNeurons - 1 do
            neurons.[i] <- Neuron(nNeuronInputConnections, activation)

    member val Neurons : Neuron[] = neurons
    member __.ForwardPropagate inputs =
        let newInputs = List<double>()
        for neuron in this.Neurons do
            newInputs.Add(neuron.Activate inputs)
        newInputs.ToArray()

    member __.UpdateWeights (row : double[]) learningRate =
        let lastElement = row.GetLength(0)
        let mutable inputs = row.[0 .. lastElement]
        if not (isNull previous)
        then
            let neuronsInPreviousLayer = previous.Neurons
            inputs <- Array.zeroCreate neuronsInPreviousLayer.Length
            for neuron in 0 .. neuronsInPreviousLayer.Length - 1 do
                inputs.[neuron] <- neuronsInPreviousLayer.[neuron].Output

        for neuron in this.Neurons do
            for j in 0 .. inputs.Length - 1 do
                neuron.Weights.[j] <- neuron.Weights.[j] + learningRate * neuron.Delta * inputs.[j]
            let last = neuron.Weights.Length - 1
            neuron.Weights.[last] <- neuron.Weights.[last] + learningRate * neuron.Delta


type NeuralNetwork(accuracyMetric : IAccuracyMetric) as this =
    let predictRow (row) =
        let outputs : double[] = this.ForwardPropagate row
        let mutable max : double = outputs.[0]
        let mutable index = 0
        for j in 1 .. outputs.Length - 1 do
            if outputs.[j] > max
            then
                max <- outputs.[j]
                index <- j
        index

    let sumSquaredError (outputs : double[]) (expected : double[]) =
        let mutable res = 0.0
        for p in 0 .. this.nOutput - 1 do
            res <- res + (expected.[p] - outputs.[p])**2.0
        res

    let updateWeights (row : double[]) (learning_rate : double) =
        for layer : Layer in this.Layers do
            layer.UpdateWeights row learning_rate

    member val nOutput = 0 with get, set
    member val Layers = List<Layer>()

    member __.InitialiseLayers nInputs nHidden nOutputs =
        this.nOutput <- nOutputs
        let hidden = Layer(nHidden, nInputs + 1, null, SigmoidActivation())
        this.Layers.Add hidden
        this.Layers.Add (Layer(nOutputs, nHidden + 1, hidden, SigmoidActivation()))
        this

    member __.Predict (testData : double[,]) testActual =
        let numTestRows = testData.GetLength(0)
        let mutable predictions = Array.zeroCreate numTestRows
        for i in 0 .. numTestRows - 1 do
            predictions.[i] <- predictRow testData.[i, *]
        let accuracy = accuracyMetric.ComputeAccuracy testActual predictions
        (predictions, accuracy)

    member this.ForwardPropagate inputs =
        let mutable inputs = inputs
        for layer in this.Layers do
            inputs <- layer.ForwardPropagate inputs
        inputs

    member __.BackPropagateError (expected : double[]) =
        let length = this.Layers.Count - 1
        for i in 0 .. length do
            let len = length - i
            let layer = this.Layers.[len]
            let errors = List<double>()
            let numNeurons = layer.Neurons.Length
            for j in 0 .. numNeurons - 1 do
                let n = layer.Neurons.[j]
                if len = length
                then errors.Add (expected.[j] - n.Output)
                else
                    let mutable error = 0.0
                    for neuron in this.Layers.[len + 1].Neurons do
                        error <- error + neuron.Weights.[j] * neuron.Delta
                    errors.Add error
                n.Delta <- errors.[j] * n.Derivative

    member __.Train (trainData : double[,]) (trainActual : double[]) learningRate nEpochs =
        for _ in 0 .. nEpochs - 1 do
            let mutable sumError = 0.0
            for j in 0 .. trainData.GetLength(0) - 1 do
                let features = trainData.[j, *]
                let actual = trainActual.[j]
                let outputs = this.ForwardPropagate features
                let mutable expected : double[] = Array.zeroCreate this.nOutput
                expected.[int(actual)] <- 1.0
                sumError <- sumError + (sumSquaredError outputs expected)
                this.BackPropagateError expected
                updateWeights features learningRate


type Utils private() =
    static let getOutermost array =
        let mutable min = Double.MaxValue
        let mutable max = Double.MinValue
        for elem in array do
            if elem < min
            then min <- elem
            else if elem > max
            then max <- elem
        (min, max)

    static let shuffle (array : double[,]) = 
        let mutable shuffledArray = Array2D.zeroCreate (array.GetLength(0)) (array.GetLength(1))
        let numRows = array.GetLength(0)
        let rnd = Random()
        for i in 0 .. numRows - 1 do
            let randIndex = rnd.Next(0, numRows)
            let tempCol = array.[i, *]
            shuffledArray.[i, *] <- array.[randIndex, *]
            shuffledArray.[randIndex, *] <- tempCol
        shuffledArray


    static member LoadCSV filepath =
        let file = File.ReadAllLines(filepath)
        let mutable dataset: double[,] = Array2D.zeroCreate file.Length (file.[0].Split(',').Length)
        for i in 0 .. file.Length - 1 do
            let values = file.[i].Split(',')
            for j in 0 .. values.Length - 1 do
                if j = values.Length - 1
                then dataset.[i, j] <- Double.Parse(values.[j]) - 1.0
                else dataset.[i, j] <- Double.Parse(values.[j])
        dataset

    static member NormaliseColumns (dataset : double[,]) =
        let numColumns = dataset.GetLength(1) - 1
        for i in 0 .. numColumns - 1 do
            let column : double[] = dataset.[*, i]
            let (min, max) = getOutermost column
            let diff = max - min
            for j in 0 .. column.Length - 1 do
                dataset.[j, i] <- (dataset.[j, i] - min) / diff
        dataset

    static member GetTestTrainSplit (dataset : double[,]) (percent : double) =
        let dataset : double[,] = shuffle dataset
        let (rows : int, columns : int) = dataset.GetLength(0), dataset.GetLength(1)
        let numTest = int(double(rows) * percent)
        let numTrain = rows - numTest
        let test = Array2D.zeroCreate numTest columns
        let train = Array2D.zeroCreate numTrain columns
        let mutable testindex = 0
        let mutable trainindex = 0
        for i in 0 .. rows - 1 do
            if i < numTest
            then 
                test.[testindex, *] <- dataset.[i, *]
                testindex <- testindex + 1
            else
                train.[trainindex, *] <- dataset.[i, *]
                trainindex <- trainindex + 1
        (test, train)

    static member CountDistinct array =
        let dict = new Dictionary<double, int>()
        for i in array do
            if dict.ContainsKey(i)
            then dict.[i] <- dict.[i] + 1
            else dict.Add(i, 0)
        dict.Keys.Count


[<Literal>]
let TRAIN_TEST_SPLIT = 0.1
[<Literal>]
let LEARNING_RATE = 0.3
[<Literal>]
let N_EPOCHS = 500
[<Literal>]
let N_HIDDEN = 5

[<EntryPoint>]
let main argv =
    let mutable dataset = Utils.LoadCSV("benchmarks/NN/wheat-seeds.csv")
    dataset <- Utils.NormaliseColumns(dataset)
    let (test : double[,], train : double[,]) = Utils.GetTestTrainSplit dataset TRAIN_TEST_SPLIT

    let column = test.GetLength(1) - 1
    let (testData, testActual) = test.[*, 0 .. column], test.[*, column]
    let (trainData, trainActual) = train.[*, 0 .. column], train.[*, column]

    let nInputs = train.GetLength(1)
    let nOutputs = Utils.CountDistinct(trainActual)

    let network = NeuralNetwork(AccuracyPercentage()).InitialiseLayers nInputs N_HIDDEN nOutputs
    network.Train train trainActual LEARNING_RATE N_EPOCHS
    let (predictions, accuracy) = network.Predict test testActual
    printfn "%f" accuracy
    0 // return an integer exit code