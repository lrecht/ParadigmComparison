// Learn more about F# at http://fsharp.org

open System

type Edge = 
    {
        Start: int;
        End: int;
        Weight: int;
    }

let readFileToArr() =
    let lines = System.IO.File.ReadAllLines("iotest/spanning_tree/graph.csv");
    let c = lines.Length
    let mutable arr : Edge array = Array.zeroCreate c
    for i in 0 .. (c-1) do
        let elms = lines.[i].Split(',')
        arr.[i] <- { Edge.Start = Int32.Parse(elms.[0]); Edge.End = Int32.Parse(elms.[1]); Edge.Weight = Int32.Parse(elms.[2]) }
    arr

[<EntryPoint>]
let main argv =
    let arr = readFileToArr()
    let c = arr.Length
    printfn "%i" c
    0 // return an integer exit code
