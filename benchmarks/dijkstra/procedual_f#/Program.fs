// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic

let initVertices (edges: Dictionary<char, (char*int) array>) =
    let vertices = new Dictionary<char, int32>()
    for edge in edges.Keys do
        vertices.Add(edge, Int32.MaxValue)
    vertices

let initPrevDic (edges: Dictionary<char, (char*int) array>) =
    let prevDic = new Dictionary<char, Nullable<char>>()
    for edge in edges.Keys do
        prevDic.Add(edge, Nullable())
    prevDic

let initEdges =
    let edges = new Dictionary<char, (char*int) array>()
    edges.Add('a', [|('b', 7);('c', 9);('f', 14)|])
    edges.Add('b', [|('c', 10);('d', 15)|])
    edges.Add('c', [|('d', 11);('f', 2)|])
    edges.Add('d', [|('e', 6)|])
    edges.Add('e', [|('f', 9)|])
    edges.Add('f', [| |])
    edges

let remove (array: char array) (elm: char) =
    let rec loop i =
        if i >= (array.Length) then
            i
        else if array.[i] = elm then
            i
        else 
            loop (i+1)
    let index = loop 0
    Array.append array.[0 .. (index-1)] array.[(index+1) .. (array.Length-1)]

let insertSort (array: char array) (costMap: Dictionary<char, int32>) (newValue: char) =
    let rec loop i =
        if i >= array.Length then
            i
        else if costMap.[newValue] < costMap.[array.[i]] then
            i
        else 
            loop (i+1)
    let index = loop 0
    
    if index = (array.Length-1) then
        Array.append array [| newValue |]
    else if index = 0 then
        Array.append [| newValue |] array
    else
        let start = Array.append array.[0 .. (index-1)] [| newValue |]
        Array.append start array.[index .. (array.Length-1)]

[<EntryPoint>]
let main argv =
    let mutable edges = initEdges
    let mutable vertices = initVertices edges
    let mutable prevDic = initPrevDic edges

    let source = 'a'
    let mutable destination: Nullable<char> = Nullable('e')

    vertices.[source] <- 0
    
    let mutable vertex_queue: char array = [| source |]
    
    while vertex_queue.Length > 0 do 
        let current = vertex_queue.[0]
        vertex_queue <- vertex_queue.[1 .. (vertex_queue.Length-1)]
        let mutable continueNow = false
        
        //Destination found
        if current = destination.Value then
            vertex_queue <- [|  |]
        else
            let neighbors = edges.[current]
            for (key, value) in neighbors do
                let alt: int32 = vertices.[current] + value
                if alt < vertices.[key] then
                    vertex_queue <- remove vertex_queue key
                    vertices.[key] <- alt
                    prevDic.[key] <- Nullable(current)
                    vertex_queue <- insertSort vertex_queue vertices key
    
    //Create the shortest path
    let mutable shortestPath: (char array) = [| |]
    let mutable previous = destination
    while previous <> Nullable() do
        shortestPath <- Array.append [| previous.Value |] shortestPath
        previous <- prevDic.[previous.Value]

    for part in shortestPath do
        printf "%c " part
    printfn ""

    0 // return an integer exit code


