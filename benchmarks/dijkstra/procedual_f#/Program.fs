// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic

let initVertices =
    let vertices = new Dictionary<char, int32>()
    vertices.Add('a', Int32.MaxValue)
    vertices.Add('b', Int32.MaxValue)
    vertices.Add('c', Int32.MaxValue)
    vertices.Add('d', Int32.MaxValue)
    vertices.Add('e', Int32.MaxValue)
    vertices.Add('f', Int32.MaxValue)
    vertices

let initPrevDic =
    let prevDic = new Dictionary<char, Nullable<char>>()
    prevDic.Add('a', Nullable())
    prevDic.Add('b', Nullable())
    prevDic.Add('c', Nullable())
    prevDic.Add('d', Nullable())
    prevDic.Add('e', Nullable())
    prevDic.Add('f', Nullable())
    prevDic

let initEdges =
    let edges = new Dictionary<char, (char*int) array>()
    edges.Add('a', [|('b', 7);('c', 9);('f', 14)|])
    edges.Add('b', [|('c', 10);('d', 15)|])
    edges.Add('c', [|('d', 11);('f', 2)|])
    edges.Add('d', [|('e', 6)|])
    edges.Add('e', [|('f', 9)|])
    edges

let findMin (vertices: Dictionary<char, int32>) =
    let mutable min = Int32.MaxValue
    let mutable minElm = 'x'
    for vertex in vertices do
        if (vertex.Value < min) then
            min <- vertex.Value
            minElm <- vertex.Key
    minElm

let remove (array: char array) (elm: char) =
    let rec loop i (newArray: char array) =
        if i >= (array.Length) then
            newArray
        else if array.[i] = elm then
            let rest = array.[(i+1) .. (array.Length-1)]
            Array.append newArray rest
        else 
            loop (i+1) (Array.append newArray [| array.[i] |])
    loop 0 [|  |]

let insertSort (array: char array) (costMap: Dictionary<char, int32>) (newValue: char) =
    let rec loop i newArray =
        if i >= array.Length then
            Array.append newArray [| newValue |]
        else if costMap.[newValue] < costMap.[array.[i]] then
            let rest = Array.append [| newValue |] array.[i .. (array.Length-1)]
            Array.append newArray rest
        else 
            loop (i+1) (Array.append newArray [| array.[i] |])
    loop 0 [|  |]

[<EntryPoint>]
let main argv =
    let mutable vertices = initVertices
    let mutable edges = initEdges
    let mutable prevDic = initPrevDic

    let source = 'a'
    let mutable destination: Nullable<char> = Nullable('e')

    vertices.[source] <- 0
    
    let mutable vertex_queue: char array = [| source |]
    let mutable current: char = 'x'

    while vertex_queue.Length > 0 do 
        current <- vertex_queue.[0] //findMin vertices
        vertex_queue <- remove vertex_queue current
        let mutable fail = false
        if not (edges.ContainsKey current) then
            fail <- true
        if not fail then
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


