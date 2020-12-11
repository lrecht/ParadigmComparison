// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic
open System.Text

//------ HEAP
type Heap = {
    //Type help:   (id, (freqency, Dictionary<char, encodeing>) array)
    mutable array: (int*Dictionary<char,string>) array
    mutable maxSize: int
    mutable size: int
}

let heap: Heap = { Heap.array = (Array.zeroCreate 1024); Heap.maxSize = 1024; Heap.size = 0 }


let swap (index1: int) (index2: int) =
    let swap = heap.array.[index1]
    heap.array.[index1] <- heap.array.[index2]
    heap.array.[index2] <- swap

let smallerThan ((freq1, nodesDict1): (int*Dictionary<char,string>)) ((freq2, nodesDict2): (int*Dictionary<char,string>)) =
    (freq1 < freq2)

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

let insert (element: (int*Dictionary<char,string>)) =
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
    let (id, rest) = heap.array.[0]
    heap.array.[0] <- heap.array.[heap.size-1]
    heap.size <- heap.size - 1
    heapify 0
    (id, rest)

//------ End of heap

let convertToHeap (sym2Freq: Dictionary<char, int>) =
    for leaf in sym2Freq do
        let newDic = new Dictionary<char, string>()
        newDic.Add(leaf.Key, "")
        insert (leaf.Value, newDic)

let combineNodes (nodesDict1: Dictionary<char, string>) (nodesDict2: Dictionary<char, string>) =
    let mutable combinedNodes = new Dictionary<char, string>()
    for elm in nodesDict1 do
        combinedNodes.Add(elm.Key, ("0"+ elm.Value))
    for elm in nodesDict2 do
        combinedNodes.Add(elm.Key, ("1"+ elm.Value))
    combinedNodes

let createMappings (sym2Freq: Dictionary<char,int>) = 
    convertToHeap sym2Freq
    while heap.size > 1 do
        let mutable (freq1, nodesDict1) = pop() //Right
        let mutable (freq2, nodesDict2) = pop() //Left
        let combinedNodes = combineNodes nodesDict1 nodesDict2
        insert ((freq1+freq2), combinedNodes)
    
    let (freq, mappings) = pop()
    mappings //Dictionary<char, string>

let encode (mappings: Dictionary<char, string>) (text: string) =
    let mutable encodedString: StringBuilder = StringBuilder();
    for c in text do
        encodedString <- encodedString.Append(mappings.[c])
    encodedString.ToString()

[<EntryPoint>]
let main argv =
    let text = System.IO.File.ReadAllText "benchmarks/huffman_coding/lines.txt"
    let frequencies: Dictionary<char, int> = new Dictionary<char, int>()
    
    for c in text do
        if frequencies.ContainsKey(c) then
            frequencies.[c] <- frequencies.[c]+1
        else
            frequencies.Add(c, 1)

    let mappings = createMappings frequencies
    let encoded: string = encode mappings text
    printfn "Length: %i" encoded.Length
    
    0 // return an integer exit code
