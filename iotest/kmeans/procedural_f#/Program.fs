// Learn more about F# at http://fsharp.org

open System

let numValues = 200000

type Point = {
    mutable Data: (float * float);
    mutable Cluster: int;
}

let mutable allData: Point array = Array.create numValues ({ Point.Cluster = 0; Point.Data = (0.0, 0.0) })

let generateData() =
    let lines = System.IO.File.ReadAllLines("iotest/kmeans/points.txt")
    let mutable i = 0
    for line in lines do
        let split = line.Split(':')
        let num1 = split.[0]
        let num2 = split.[1]
        allData.[i] <- { Point.Cluster = 1; Point.Data = ((float)num1, (float)num2) }
        i <- i+1

[<EntryPoint>]
let main argv =
    generateData()
    let c = allData.Length
    printfn "%i" c

    0 // return an integer exit code
