﻿open System

let rand = Random(2)

let readFile file =
    System.IO.File.ReadAllLines(file)
    |> Array.map (fun line -> 
                let elms = line.Split(',')
                let len = elms.Length
                Array.mapi (fun i (e:string) -> if i = len-1 then float e-1.0 else float e) elms)

let initialiseNetwork nInput nHidden nOutput =
    let initMapsWithWeights maps weights =
        Array.map ((fun _ -> Map.empty) >> (fun (dict:Map<string,float[]>) -> 
            dict.Add("weights",[|for i in 1 .. weights do rand.NextDouble()|])
                .Add("bias",[|rand.NextDouble()|]))) [|for i in 1 .. maps do i|]
    [(initMapsWithWeights nHidden nInput);
     (initMapsWithWeights nOutput nHidden)]

let activate weights inputs bias = bias + (Array.map2 ( * ) weights inputs |> Array.sum)
let transfer activation = 1.0 / (1.0 + Math.Exp (-activation))
let forwardPropagate row network =
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
        Array.sumBy (fun (n:Map<string,float[]>) -> 
            n.["weights"].[index] * n.["delta"].[0]) previousLayer

    let backProp res layer =
        let prevLayer =  List.head res
        let errors = Array.mapi (neuronError prevLayer) layer
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
        res
    List.fold trainOnce network [1 .. epochs]

let predict network input =
    let computeLayer acc layer =
        Array.map (fun (n:Map<string,float[]>) -> 
            activate n.["weights"] acc n.["bias"].[0] |> transfer) layer
    List.fold computeLayer input network 
    |> Array.fold (fun (acc,i,index) n -> 
        if acc < n then (n,index,index+1) else (acc,i,index+1)) (0.0,0,0)
    |> (fun (_,i,_) -> i)

let normaliseColumns (data:float[][]) =
    let aggr func i = func (fun (e:float[]) -> e.[i]) data |> Array.item i
    let len = data.[0].Length-1
    let minmax = [|for i in 0 .. len do 
                        if i = len then (0.0,1.0) else aggr Array.minBy i,aggr Array.maxBy i|]
    let changeRow data = Array.map2 (fun v (min,max) -> (v-min)/(max-min)) data minmax
    Array.map changeRow data

let accuracy (guess:int[]) (truth:int[]) =
    let corrects = Array.fold2 (fun acc g t -> if g = t then acc+1.0 else acc ) 0.0 guess truth
    corrects/(float)guess.Length * 100.0

[<EntryPoint>]
let main argv =
    let hidden = 5
    let iterations = 500
    let learnRate = 0.3
    let data = readFile "benchmarks/NN/wheat-seeds.csv"
    let ndata = normaliseColumns data |> Array.sortBy (fun _ -> rand.Next()) 
    let splitAt = (int)(ndata.Length / 10)
    let trainData = ndata.[splitAt+1..ndata.Length-1]
    let testData = ndata.[0..splitAt]
    let nInput = Array.length data.[0] - 1
    let nOutput = Array.distinct (Array.map (fun (a:float[]) -> a.[a.Length-1]) data) |> Array.length
    let init = initialiseNetwork nInput hidden nOutput
    let learnNet = train init trainData learnRate iterations nOutput
    let dataNoRes = Array.map (fun (a:float[]) -> a.[0..a.Length-2]) testData
    let res = Array.map (fun (a:float[]) -> (int)a.[a.Length-1]) testData
    printfn "%f" (accuracy (Array.map (predict learnNet) dataNoRes) res)
    0 // return an integer exit code
