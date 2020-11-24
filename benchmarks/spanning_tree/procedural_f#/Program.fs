// Learn more about F# at http://fsharp.org

open System

type Edge = 
    {
        Start: int;
        End: int;
        Weight: int;
    }

// Made by https://www.w3resource.com/csharp-exercises/searching-and-sorting-algorithm/searching-and-sorting-algorithm-exercise-9.php

let Partition (arr:Edge[]) l r =
    let pivot = arr.[l].Weight;
    let mutable left = l
    let mutable right = r
    let mutable løøp = true
    while løøp do
        while (arr.[left].Weight < pivot) do
            left <- left + 1

        while (arr.[right].Weight > pivot) do
            right <- right - 1

        if (left < right) then
            if arr.[left].Weight = arr.[right].Weight then
                left <- left + 1
                right <- right - 1
            else
                let temp = arr.[left];
                arr.[left] <- arr.[right];
                arr.[right] <- temp;
        else løøp <- false
    right

let rec QuickSort (arr: Edge []) (left:int) (right:int) =
    if (left < right) then
        let pivot = Partition arr left right

        if (pivot > 1) then
            QuickSort arr left (pivot - 1)

        if (pivot + 1 < right) then
            QuickSort arr (pivot + 1) right

let readFileToArr() =
    let lines = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv");
    let c = lines.Length
    let mutable arr : Edge array = Array.zeroCreate c
    for i in 0 .. (c-1) do
        let elms = lines.[i].Split(',')
        arr.[i] <- { Edge.Start = Int32.Parse(elms.[0]); Edge.End = Int32.Parse(elms.[1]); Edge.Weight = Int32.Parse(elms.[2]) }
    arr

let vertexGroup : int array = Array.create (6005 + 1) -1
let rec unionFind (node: int) =
    if vertexGroup .[node] < 0 then
        node
    else 
        vertexGroup .[node] <- unionFind vertexGroup .[node]
        vertexGroup .[node]

let union (startNode: int) (endNode: int) = 
    let group1Root: int = unionFind startNode
    let group2Root: int = unionFind endNode
    if group1Root = group2Root then
        false
    else
        vertexGroup .[group2Root] <- group1Root
        true

let computeMinSpanTree (arr: Edge array) = 
    let theMagicNumber: int = (5877-1)
    let result: Edge array = Array.zeroCreate theMagicNumber
    let mutable size = 0
    let mutable totalWeight = 0
    let mutable totalEdges = 0
    let mutable i = 0

    while size < theMagicNumber do
        let currentEdge = arr.[i]
        i <- i + 1
        if (union currentEdge.Start currentEdge.End) then
            result.[size] <- currentEdge
            size <- size + 1
            totalWeight <- totalWeight + currentEdge.Weight
            totalEdges <- totalEdges + 1

    (totalWeight, totalEdges)

[<EntryPoint>]
let main argv =
    let arr = readFileToArr()
    QuickSort arr 0 (arr.Length - 1)
    let (totalWeight, totalEdges) = computeMinSpanTree arr

    printfn "Total weight: %i" totalWeight
    printfn "Total Edges: %i" totalEdges
    0 // return an integer exit code
