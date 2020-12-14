// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Linq

type Point(x, y) =  
    member __.X = x
    member __.Y = y   

[<EntryPoint>]
let main argv =
    let lines = System.IO.File.ReadAllLines("iotest/kmeans/points.txt");
    let points: Point array = Array.zeroCreate lines.Length
    for i in 0 .. lines.Length-1 do
        let split = lines.[i].Split(':')
        points.[i] <- Point(Double.Parse(split.[0]), Double.Parse(split.[1]))

    let c = points.Length
    printfn "%i" c

    0 // return an integer exit code
