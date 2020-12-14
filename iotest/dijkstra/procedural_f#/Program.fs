// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Generic

let toDictionary (map : Map<_, _>) : Dictionary<_, _> = Dictionary(map)

let initEdges =
    let filePath = "iotest/dijkstra/graph.csv"
    let lines = seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    }

    let graph = 
        Seq.map 
            (fun (a:string) -> 
                let words = (a.Split ',') 
                in (words.[0],words.[1],(words.[2] |> int)))
            lines
    
    let edgeMap = Seq.fold (fun (acc:Map<string,(string*int) array>) (from,dest,cost) -> 
        if acc.ContainsKey from then 
            acc.Add(from,(Array.append (acc.[from]) [| (dest,cost)|] ))
        else acc.Add(from,[|dest,cost|])) Map.empty graph in

    toDictionary edgeMap

[<EntryPoint>]
let main argv =
    let edges = initEdges
    let c = edges.Count
    printfn "%i" c
    0 // return an integer exit code
