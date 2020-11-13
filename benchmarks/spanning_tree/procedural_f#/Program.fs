// Learn more about F# at http://fsharp.org

open System

type Edge = 
    {
        Start: int;
        End: int;
        Weight: int;
    }

//------ HEAP
type Heap = {
    //Type help:   (id, (freqency, Dictionary<char, encodeing>) array)
    mutable array: Edge array
    mutable maxSize: int
    mutable size: int
}

let heap: Heap = { Heap.array = (Array.zeroCreate 1024); Heap.maxSize = 1024; Heap.size = 0 }


let swap (index1: int) (index2: int) =
    let swap = heap.array.[index1]
    heap.array.[index1] <- heap.array.[index2]
    heap.array.[index2] <- swap

let smallerThan (edge1: Edge) (edge2: Edge) =
    (edge1.Weight < edge2.Weight)

let rec heapifyNode (index: int) =
    // Find parent 
    let parent: int = (index - 1) / 2; 

    // For Min-Heap 
    // If current node is smaller than its parent 
    // Swap both of them and call heapify again 
    // for the parent 
    if (smallerThan heap.array.[index] heap.array.[parent]) then
        swap index parent

        // Recursively heapify the parent node 
        heapifyNode parent

let insert (element: Edge) =
    if(heap.size = heap.maxSize) then
        heap.array <- Array.append heap.array (Array.zeroCreate heap.maxSize)
        heap.maxSize <- heap.maxSize * 2

    heap.array.[heap.size] <- element
    heap.size <- (heap.size + 1)
    heapifyNode (heap.size-1)

let rec heapify (index: int) =
    // Code from https://www.geeksforgeeks.org/heap-sort/
    let mutable smallest: int = index; // Initialize smallest as root 
    let l: int = 2*index + 1; // left = 2*i + 1 
    let r: int = 2*index + 2; // right = 2*i + 2 

    // If left child is smaller than root 
    if (l < heap.size && (smallerThan heap.array.[l] heap.array.[smallest])) then
        smallest <- l

    // If right child is smaller than smallest so far 
    if (r < heap.size && (smallerThan heap.array.[r] heap.array.[smallest])) then
        smallest <- r

    // If smallest is not root 
    if (smallest <> index) then
        swap index smallest

        // Recursively heapify the affected sub-tree 
        heapify smallest

let pop ()=
    let edge = heap.array.[0]
    heap.array.[0] <- heap.array.[heap.size-1]
    heap.size <- heap.size - 1
    heapify 0
    edge

//------ End of heap

let readFileToHeap() =
    let lines = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv");
    for line in lines do
        let elms = line.Split(',')
        insert { Edge.Start = Int32.Parse(elms.[0]); Edge.End = Int32.Parse(elms.[1]); Edge.Weight = Int32.Parse(elms.[2]) }



let vertexGrups: int array = Array.create (6005 + 1) -1
let rec uniounFind (node: int) =
    if vertexGrups.[node] < 0 then
        node
    else 
        vertexGrups.[node] <- uniounFind vertexGrups.[node]
        vertexGrups.[node]

let unioun (startNode: int) (endNode: int) = 
    let group1Root: int = uniounFind startNode
    let group2Root: int = uniounFind endNode
    if group1Root = group2Root then
        false
    else
        vertexGrups.[group2Root] <- group1Root
        true

let computeMinSpanTree() = 
    let theMagicNumber: int = (5877-1)
    let result: Edge array = Array.zeroCreate theMagicNumber
    let mutable size = 0
    let mutable totalWeight = 0
    let mutable totalEdges = 0

    while size < theMagicNumber do
        let currentEdge = pop()
        if (unioun currentEdge.Start currentEdge.End) then
            result.[size] <- currentEdge
            size <- size + 1
            totalWeight <- totalWeight + currentEdge.Weight
            totalEdges <- totalEdges + 1

    (totalWeight, totalEdges)

[<EntryPoint>]
let main argv =
    readFileToHeap()
    let (totalWeight, totalEdges) = computeMinSpanTree()

    printfn "Total weight: %i" totalWeight
    printfn "Total Edges: %i" totalEdges
    0 // return an integer exit code
