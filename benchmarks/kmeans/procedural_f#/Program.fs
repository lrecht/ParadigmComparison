// Learn more about F# at http://fsharp.org

open System

let numKlusters = 10
let rand = Random(2)
let numValues = 200000

type Point = {
    mutable Data: (float * float);
    mutable Kluster: int;
}

let mutable allData: Point array = Array.create numValues ({ Point.Kluster = 0; Point.Data = (0.0, 0.0) })
let klusters: (float * float) array = Array.create numKlusters (0.0, 0.0)

let generateData() =
    let lines = System.IO.File.ReadAllLines("../points.txt")
    let mutable i = 0
    for line in lines do
        let split = line.Split(':')
        let num1 = split.[0]
        let num2 = split.[1]
        allData.[i] <- { Point.Kluster = 1; Point.Data = ((float)num1, (float)num2) }
        i <- i+1

let printKlusters() = 
    for i in 0..numKlusters-1 do
        let (x, y) = klusters.[i]
        printfn "Kluster %i: (%f, %f)" i x y

let setKlustersRandom() =
    for i in 0..numKlusters-1 do
        let randPointIndex = rand.Next(0, allData.Length-1)
        let point = allData.[randPointIndex]
        klusters.[i] <- point.Data

let distance ((xa, ya): (float*float)) ((xb, yb): (float*float)) = 
    Math.Sqrt(Math.Pow((xb-xa), 2.0) + Math.Pow((yb-ya), 2.0))

let assignPointsToKluster() =
    for i in 0..allData.Length-1 do
        let mutable nearest = 0
        let mutable length = Double.PositiveInfinity
        for j in 0..numKlusters-1 do
            let tempDist = distance allData.[i].Data klusters.[j]
            if tempDist < length then
                length <- tempDist
                nearest <- j
        allData.[i].Kluster <- nearest


let setCenter() = 
    let test: ((float * float) * int) array = Array.create numKlusters ((0.0, 0.0), 0)
    
    for point in allData do
        let (x, y) = point.Data
        let mutable ((totalX, totalY),num) = test.[point.Kluster]
        totalX <- totalX + x
        totalY <- totalY + y
        num <- num + 1
        test.[point.Kluster] <- ((totalX, totalY), num)
    
    let mutable hasMoved: bool = false
    for i in 0..numKlusters-1 do
        let mutable ((totalX, totalY),num) = test.[i]
        let (oldX, oldY) = klusters.[i]
        let (newX, newY) = (totalX/(float)num, totalY/(float)num)
        
        if oldX <> newX || oldY <> newY then
            hasMoved <- true

        klusters.[i] <- (newX, newY)
    hasMoved

[<EntryPoint>]
let main argv =
    generateData()
    setKlustersRandom()
    let mutable hasMoved = true
    
    while hasMoved do
        assignPointsToKluster()
        hasMoved <- setCenter()

    printKlusters()

    0 // return an integer exit code
