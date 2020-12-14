// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Collections.Generic
open System.IO

type Edge(startVertex, endVertex, cost) =
    member __.Start = startVertex
    member __.End = endVertex
    member __.Weight = cost
    member val Id = 0 with get,set
    static member FromCsv (csvLine: String) id =
        let values = csvLine.Split(',')
        let n1 = Convert.ToInt32(values.[0])
        let n2 = Convert.ToInt32(values.[1])
        let w = Convert.ToInt32(values.[2])
        let mutable edge = Edge(n1, n2, w)
        edge.Id <- id
        edge

[<EntryPoint>]
let main argv =
    let edgesFile = File.ReadAllLines("iotest/spanning_tree/graph.csv")
    let mutable edges : Edge array = Array.zeroCreate edgesFile.Length
    for index in 0 .. edgesFile.Length - 1 do
        edges.[index] <- Edge.FromCsv edgesFile.[index] (index + 1)
    let c = edges.Length
    printfn "%i" c
    0 // return an integer exit code