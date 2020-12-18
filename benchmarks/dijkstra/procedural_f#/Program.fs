// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Generic
open benchmark

type Heap = {
    mutable array: (string*int) array
    mutable maxSize: int
    mutable size: int
}

// Global variables
let mutable heap: Heap = { Heap.array = (Array.zeroCreate 1024); Heap.maxSize = 1024; Heap.size = 0 }
let mutable positions: Dictionary<string,int> = null

let swap (index1: int) (index2: int) =
    // Maintains dictionary to find items later
    let (key1, value1) = heap.array.[index1]
    let (key2, value2) = heap.array.[index2]
    positions.[key1] <- index2
    positions.[key2] <- index1

    let temp = heap.array.[index1]
    heap.array.[index1] <- heap.array.[index2]
    heap.array.[index2] <- temp

let smallerThan ((item11, item12): (string*int)) ((item21, item22): (string*int)) =
    if (item12 < item22) then
        true
    else
        (item12 = item22 && item11.CompareTo(item21) = -1)

let rec heapifyNode (index: int) =
    // Find parent 
    let parent: int = (index - 1) / 2; 

    // For Min-Heap 
    // If current node is less than its parent 
    // Swap both of them and call heapify again 
    // for the parent 
    if (smallerThan heap.array.[index] heap.array.[parent]) then
        swap index parent

        // Recursively heapify the parent node 
        heapifyNode parent

let heapInsert (element: (string*int)) =
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

let heapPop ()=
    let (key, cost) : (string*int) = heap.array.[0]
    heap.array.[0] <- heap.array.[heap.size-1]
    heap.size <- heap.size - 1
    heapify 0

    //Maintain dict
    positions.Remove(key) |> ignore

    (key, cost)

let heapReplace (pos: string) (newDist: int) =
    if (positions.ContainsKey(pos)) then
        let index: int = positions.[pos]
        swap index 0
        heapPop ()
    else
        positions.[pos] <- heap.size
        heapInsert (pos,newDist)
        (pos,newDist)

let mutable distances: Dictionary<string,int> = new Dictionary<string, int>()
let mutable backtrack = new Dictionary<string, string>()

let initVertices (edges: Dictionary<string, (string*int) array>) =
    let vertices = new Dictionary<string, int32>()
    for edge in edges.Keys do
        vertices.Add(edge, Int32.MaxValue)
    vertices

let toDictionary (map : Map<_, _>) : Dictionary<_, _> = Dictionary(map)

let initEdges lines =
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
    
let printPath (shortestPath: string array) =
    for part in shortestPath do
        printf "%s " part

let doBacktrack (source:string) (destination:string) =
    let mutable shortestPath: (string array) = [| destination |]
    let mutable position = destination
    
    while position <> source do
        position <- backtrack.[position]
        shortestPath <- Array.append [| position |] shortestPath
    shortestPath

let replacePath (nKey:string) (key:string) (dist:int) =
    backtrack.[nKey] <- key
    distances.[nKey] <- dist
    heapReplace nKey dist |> ignore

let addToPath (nKey:string) (key:string) (dist:int) =
    distances.Add(nKey,dist)
    backtrack.Add(nKey, key)
    positions.Add(nKey,heap.size)
    heapInsert ((nKey,dist))

let visitNeighbours (key:string) (cost:int) (edges: Dictionary<string,(string*int) array>) =
    let neighbors = edges.[key]
    for (nKey, nCost) in neighbors do   
        let alternateDist = cost + nCost
        if not (distances.ContainsKey(nKey)) then
            addToPath nKey key alternateDist |> ignore
        else if alternateDist < distances.[nKey] then
            replacePath nKey key alternateDist |> ignore

let solveDijkstra (source:string) (destination:string) (edges: Dictionary<string, (string * int) array>) =
    while heap.size > 0 do 
        let (key, cost) = heapPop()
        if key = destination then
            heap.size <- 0
        else if edges.ContainsKey(key) then
            visitNeighbours key cost edges |> ignore

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let filePath = "benchmarks/dijkstra/graph.csv"
    let lines = seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    }

    bm.Run((fun () -> 
        positions <- Dictionary<string, int>()
        heap <- { Heap.array = (Array.zeroCreate 1024); Heap.maxSize = 1024; Heap.size = 0 }
        distances <- new Dictionary<string, int>()
        backtrack <- new Dictionary<string, string>()
        let source: string = "257"
        let destination: string = "5525"
        heapInsert (source, 0)
        let mutable edges = initEdges lines

        solveDijkstra source destination edges |> ignore
        doBacktrack source destination
    ), (fun(res) -> 
        printPath res |> ignore
    ))
    
    0 // return an integer exit code
