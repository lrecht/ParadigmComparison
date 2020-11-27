// Learn more about F# at http://fsharp.org

open System

let rand = Random(2)

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

[<EntryPoint>]
let main argv =
    let init = initialiseNetwork 2 1 2
    let net,out = forwardPropagate [|1.0;0.0|] init
    let backNet = backwardPropagateError net [|0.0;1.0|]
    0 // return an integer exit code
