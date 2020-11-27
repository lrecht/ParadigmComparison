open System

let rand = Random(2)

let readFile file =
    System.IO.File.ReadAllLines(file)
    |> Array.map (fun line -> 
                let elms = line.Split(',')
                let len = elms.Length
                Array.mapi (fun i (e:string) -> if i = len-1 then float e-1.0 else float e) elms)

let initialiseNetwork nInput nHidden nOutput =
    let initMapsWithWeights maps weights =
        Array.map (fun _ -> Map.empty) [|for i in 1 .. maps do i|]
        |> Array.map (fun (dict:Map<string,float[]>) -> 
            dict.Add("weights",[|for i in 1 .. weights do rand.NextDouble()|])
                .Add("bias",[|rand.NextDouble()|]))
    [(initMapsWithWeights nHidden nInput);
     (initMapsWithWeights nOutput nHidden)]

let forwardPropagate row network =
    let activate weights inputs bias = bias + (Array.map2 ( * ) weights inputs |> Array.sum)
    let transfer activation = 1.0 / (1.0 + Math.Exp (-activation))
    let propagateNeuron inputs (neuron:Map<string,float[]>) =
        let out = activate neuron.["weights"] inputs neuron.["bias"].[0] |> transfer
        neuron.Add("output",[|out|])
    let getOutput layer = Array.map (fun (neuron:Map<string,float[]>) -> neuron.["output"].[0]) layer
    let rec propagateLayer rest inputs res =
        match rest with
          | (layer::xs) -> let newLayer = Array.map (propagateNeuron inputs) layer
                           propagateLayer xs (getOutput newLayer) (newLayer::res)
          | _ -> res,(getOutput (List.head res))
    propagateLayer network row []

let backwardPropagateError network expected =
    let derivative out = out * (1.0 - out)
    let error err (neuron:Map<string,float[]>) = 
        neuron.Add("delta",[|(err * (derivative neuron.["output"].[0]))|])
    let delta errors layer = Array.map2 error errors layer

    let neuronError previousLayer index _ = 
        Array.map (fun (n:Map<string,float[]>) -> 
            n.["weights"].[index] * n.["delta"].[0]) previousLayer |> Array.sum

    let backProp res layer =
        let errors = Array.mapi (neuronError (List.head res)) layer
        let newLayer = delta errors layer
        newLayer::res

    let (out::rest) = network
    let errors = Array.map2 (fun (neuron:Map<string,float[]>) exp -> exp - neuron.["output"].[0]) 
                         out expected
    List.fold backProp [(delta errors out)] rest

let updateWeights network row lRate =
    let outputs layer = Array.map (fun (neuron:Map<string,float[]>) -> 
                                        neuron.["output"].[0]) layer
    let update layer inputs =
        let updateNeuron (neuron:Map<string,float[]>) =
            let newWeights = Array.map2 (fun i w -> w+i*lRate*neuron.["delta"].[0]) inputs neuron.["weights"]
            neuron.Add("weights",newWeights)
                  .Add("bias",[|lRate*neuron.["delta"].[0]+neuron.["bias"].[0]|])
        Array.map updateNeuron layer
    List.fold (fun res layer -> (update layer (outputs (List.head res)))::res)
              [(update (List.head network) row)] 
              (List.tail network)
    |> List.rev

let train network trainData lRate epochs nOutputs =
    let trainOnce net i =
        let trainOnData (net,err) (row:float[]) =
            let input = row.[..(Array.length row)-2]
            let fnet,output = forwardPropagate input net
            let expIndex = (int)row.[Array.length row-1]
            let expected = [|for i in 0 .. nOutputs-1 do if i = expIndex then 1.0 else 0.0|]
            let error = err + (Array.map2 (fun exp out -> (exp-out)**2.0) expected output |> Array.sum)
            let backNet = backwardPropagateError fnet expected
            let res = updateWeights backNet input lRate
            res,error
        let res,err = Array.fold trainOnData (net,0.0) trainData
        printfn ">epoch=%2i, lrate=%1.2f, error=%1.3f" i lRate err
        res
    List.fold trainOnce network [1 .. epochs]

[<EntryPoint>]
let main argv =
    let init = initialiseNetwork 2 2 2
    //printfn "%A" init
    let data = readFile "benchmarks/NN/wheat-seeds.csv"
     
    let dataset = [|
        [|2.7810836;2.550537003;0.0|]
        [|1.465489372;2.362125076;0.0|]
        [|3.396561688;4.400293529;0.0|]
        [|1.38807019;1.850220317;0.0|]
        [|3.06407232;3.005305973;0.0|]
        [|7.627531214;2.759262235;1.0|]
        [|5.332441248;2.088626775;1.0|]
        [|6.922596716;1.77106367;1.0|]
        [|8.675418651;-0.242068655;1.0|]
        [|7.673756466;3.508563011;1.0|]|]
    let learnNet = train init dataset 0.5 20 2
    //printfn "%A" learnNet
    0 // return an integer exit code
