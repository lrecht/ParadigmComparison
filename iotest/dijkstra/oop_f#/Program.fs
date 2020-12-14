// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic

type Edge(startVertex, endVertex, cost) =
    member this.Start = startVertex
    member this.End = endVertex
    member this.Cost = cost

    // Converts a line from graph CSV to an edge
    static member FromCSV line =
        let values = (line:String).Split(",")
        let cost:Int32 = Convert.ToInt32(values.[2])
        let edge = Edge(values.[0], values.[1], cost)
        edge

[<EntryPoint>]
let main argv =
    let mutable edges = List<Edge>()

    for line in System.IO.File.ReadAllLines("benchmarks/dijkstra/graph.csv") do
        edges.Add(Edge.FromCSV(line))
    printfn "‰i" edges.Count
    0 // return an integer exit code
