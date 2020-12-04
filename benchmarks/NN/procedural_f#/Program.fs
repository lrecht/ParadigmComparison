// Learn more about F# at http://fsharp.org

open System
let rand = Random(1)

//global field stuff
type Neuron = {
    mutable Weights: float[];
    mutable Delta: float;
    mutable Output: float
}

type Layer = {
    mutable Layer: Neuron[]
}

// ---- Initilizers

let randomInitArray (array: float[]) = 
    for weightIndex in 0 .. array.Length-1 do
        array.[weightIndex] <- rand.NextDouble()
    array 

// network
let getNetwork (layers: int[]) (inputs: int) = 
    let mutable num = inputs;
    let network: Layer[] = Array.zeroCreate layers.Length
    for i in 0 .. layers.Length - 1 do
        network.[i] <- { Layer.Layer = Array.zeroCreate layers.[i] }
        for j in 0 .. layers.[i] - 1 do
            network.[i].Layer.[j] <- { Neuron.Delta = 0.0; Neuron.Output = 0.0; Neuron.Weights = (randomInitArray (Array.zeroCreate (num + 1))) }
        num <- layers.[i]
    network

// ---- Forward propagation
let activate (neuron: Neuron) (inputs: float[]) =
    let mutable activation = neuron.Weights.[(neuron.Weights.Length - 1)]
    for i in 0 .. (neuron.Weights.Length - 2) do
        activation <- activation + ((neuron.Weights.[i]) * inputs.[i])
    activation

//Uses the sigmoid activation function
let transfer (activation: float) =
    1.0 / (1.0 + Math.Exp(-activation))

// Forward propagate input to a network output
let forwardPropagate (network: Layer[]) (data: float[]) =
    let mutable inputs: float[] = data
    
    for layer in 0 .. network.Length-1 do
        let lay = network.[layer].Layer
        let input = lay.Length
        let newInputs: float[] = Array.zeroCreate input
        for neuron in 0 .. input-1 do
            let activation = activate lay.[neuron] inputs
            newInputs.[neuron] <- transfer(activation)
            lay.[neuron].Output <- newInputs.[neuron]
        inputs <- newInputs
    inputs

// ---- Back propagation
//Sigmoid trasfer function
let transferDerivative (output: float) =
    output * (1.0 - output)

let backwardPropagateError (network: Layer[]) (expected: float[]) =
    for num in 0..network.Length-1 do
        let i = network.Length-1 - num
        let layer = network.[i].Layer
        let inputNeurons = layer.Length
        let errors: float[] = Array.zeroCreate inputNeurons
        
        for j in 0 .. inputNeurons-1 do
            if i <> network.Length - 1 then
                //Hidden layers
                let mutable error = 0.0
                for neuron in network.[(i + 1)].Layer do
                    error <- error + (neuron.Weights.[j] * neuron.Delta)
                errors.[j] <- error
            
            else 
                //Output layer
                errors.[j] <- expected.[j] - layer.[j].Output
            layer.[j].Delta <- errors.[j] * (transferDerivative layer.[j].Output)

// ---- Training
let updateWeights (network: Layer[]) (inputs: float[]) (learningRate: float) =
    for i in 0 .. network.Length-1 do
        let layer = network.[i].Layer
        let mutable input = inputs.[0 .. inputs.Length-2] //Removes the last input, as this is the expected answer
        
        //If it is not the first layer, then the inputs should be set to the outputs in the previous layer.
        if i <> 0 then
            let previousLayer = network.[i - 1].Layer
            input <- Array.zeroCreate previousLayer.Length
            for n in 0 .. previousLayer.Length-1 do
                input.[n] <- previousLayer.[n].Output

        for neuron in layer do
            for j in 0 .. input.Length-1 do
                neuron.Weights.[j] <- neuron.Weights.[j] + learningRate * neuron.Delta * input.[j]
            neuron.Weights.[neuron.Weights.Length-1] <- neuron.Weights.[neuron.Weights.Length-1] + learningRate * neuron.Delta


let trainNetwork (network: Layer[]) (data: float[,]) (learningRate: float) (epochs: int) (numOutputs: int) =
    let rows = data.GetLength(0)-1
    let cols = data.GetLength(1)-1
    for epoch in 1 .. epochs do
        let mutable sumError = 0.0
        for rowIndex in 0 .. rows do
            let row = data.[rowIndex, 0 .. cols]
            let outputs = forwardPropagate network row
            let expected: float[] = Array.zeroCreate numOutputs
            expected.[(int)row.[row.Length-1]] <- 1.0
            for i in 0 .. numOutputs-1 do
                sumError <- sumError + Math.Pow( (expected.[i] - outputs.[i]) , 2.0)
            
            backwardPropagateError network expected
            updateWeights network row learningRate


let predict (network: Layer[]) (data: float[]) =
    let output = forwardPropagate network data
    let mutable index = 0
    let mutable max = output.[0]
    for i in 1 .. output.Length-1 do
        if output.[i] > max then
            index <- i
            max <- output.[i]
    index


// ---- Dataset stuff
let getNumOutpus (dataset: float[,]) = 
    let outputValues: int[] = Array.zeroCreate 10
    let rows = dataset.GetLength(0)-1
    let cols = dataset.GetLength(1)-1
    for x in 0 .. rows do
        outputValues.[(int) (dataset.[x, cols])] <- 1
    let mutable numOut: int = 0
    for out in 0 .. outputValues.Length-1 do
        if outputValues.[out] = 1 then
            numOut <- numOut + 1
    numOut

let getDataset =
    //Create a dataset from the file
    let dataStrings = System.IO.File.ReadAllLines("benchmarks/NN/wheat-seeds.csv")
    let dataset = Array2D.zeroCreate dataStrings.Length (dataStrings.[0].Split(',').Length)
    for line in 0 .. dataStrings.Length-1 do
        let elms = dataStrings.[line].Split(',')
        //Insert all values except for the last
        for elm in 0..elms.Length-2 do
            dataset.[line, elm] <- (float)elms.[elm]
        //The last value (the result)
        dataset.[line, (elms.Length - 1)] <- (float)elms.[elms.Length-1] - 1.0
    dataset

let normalizeDataset (dataset: float[,]) = 
    let width = dataset.GetLength(0)
    let height = dataset.GetLength(1)
    for x in 0 .. width-1 do //Dont normalize the output
        let mutable (min, max) = (Double.MaxValue, 0.0)
        for y in 0 .. height-2 do
            if (dataset.[x, y] < min) then
                min <- dataset.[x, y]
            if (dataset.[x, y] > max) then
                max <- dataset.[x, y]
        
        for y in 0 .. height-2 do
            dataset.[x, y] <- (dataset.[x, y] - min) / (max - min)

let shuffleDataset (dataset: float[,]) = 
    let l1 = dataset.GetLength(0) - 1
    let l2 = dataset.GetLength(1) - 1
    for i in 0 .. l1 do
        let r = rand.Next(0, l1)
        let temp = dataset.[i, 0 .. l2]
        dataset.[i, 0 .. l2] <- dataset.[r, 0 .. l2]
        dataset.[r, 0 .. l2] <- temp

// ---- Evaluation
let accuracyMetric (actual: float[]) (predicted: float[]) = 
    let mutable correct = 0
    for a in 0 .. actual.Length - 1 do
        if (actual.[a] = predicted.[a]) then
            correct <- correct + 1
    (float) correct / ((float) actual.Length) * 100.0

let backPropagation (train: float[,]) (test: float[,]) (learningRate: float) (epocs: int) (nHidden: int) = 
    let nInputs = train.GetLength(1)
    let nOutputs = getNumOutpus train
    
    let network = getNetwork [| nHidden; nOutputs |] nInputs
    
    trainNetwork network train learningRate epocs nOutputs
    let testRows = test.GetLength(0)
    let testCols = test.GetLength(1)
    let predictions: float[] = Array.zeroCreate (testRows)
    for x in 0 .. testRows-1 do
        let prediction = predict network test.[x, 0 .. testCols-1]
        predictions.[x] <- (float)prediction
    predictions

let evaluateAlgorithm (dataset: float[,]) (learningRate: float) (epocs: int) (nHidden: int) = 
    shuffleDataset dataset
    let tenPercent = dataset.GetLength(0) / 10
    //Create the training data
    let trainSet: float[,] = dataset.[tenPercent+1 .. dataset.GetLength(0) - 1, 0 .. dataset.GetLength(1) - 1]
    
    //Create the testing data
    let testSet: float[,] = dataset.[0 .. tenPercent, 0 .. dataset.GetLength(1)-1]
    
    let prediction = backPropagation trainSet testSet learningRate epocs nHidden
    let actual = testSet.[0 .. testSet.GetLength(0)-1, testSet.GetLength(1)-1]
    
    accuracyMetric actual prediction

// ---- Main (great comment this is)
[<EntryPoint>]
let main argv =
    let dataset = getDataset
    let nHidden = 5
    let learningRate = 0.3
    let epochs = 500
    
    normalizeDataset dataset
    
    let score = evaluateAlgorithm dataset learningRate epochs nHidden
    
    printfn "Score: %f" score
    0 // return an integer exit code


