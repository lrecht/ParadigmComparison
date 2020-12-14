// Learn more about F# at http://fsharp.org

open System.Collections.Immutable
open System.Collections.Generic
open System.IO
open System.Linq

let filePath = "iotest/dijkstra/graph.csv"
let lines = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}
let graph = Seq.map 
                (fun (a:string) -> 
                    let words = (a.Split ',') 
                    in (words.[0],words.[1],(words.[2] |> int))) 
                lines

[<EntryPoint>]
let main argv =
    let count = graph |> Seq.length
    printfn "%i" count
    0 // return an integer exit code
