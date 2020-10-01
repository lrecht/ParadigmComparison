// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic

type Vertex = 
    {
        Key: char
        Dist: int32
    }

type Edge = 
    { 
        Start: Vertex
        End: Vertex
        Cost: int
    }


let setVertexDist (array: Vertex array) (key: char) (value: int) =
    let rec loop i = 
        if i >= array.Length then
            raise (System.Exception("The given element could not be found in the array"))
        else if array.[i].Key = key then
            Array.set array i { Vertex.Key=key; Vertex.Dist=value }
            array
        else
            loop (i+1)
    loop 0

let remove (array: Vertex array) index =
    let rec loop i newArray =
        if i >= array.Length then 
            newArray
        else if i = index then
            loop (i+1) newArray
        else
            let extra = [| array.[i] |]
            loop (i+1) (Array.append newArray extra)

    loop 0 [| |]

let findNeighbors (array: Edge array) (elm: Vertex) =
    let rec loop i neighbors = 
        if (i >= array.Length) then
            neighbors
        else if (array.[i]).Start.Key = elm.Key then
            let extra = [| array.[i] |]
            loop (i+1) (Array.append neighbors extra)
        else
            loop (i+1) neighbors

    loop 0 [|  |]

let includes (array: Vertex array) (key: char) =
    let rec loop i = 
        if array.Length <= i then
            false
        else if array.[i].Key = key then
            true
        else loop (i+1)
    loop 0

[<EntryPoint>]
let main argv =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let mutable vertices: Vertex array = [| 
        {Vertex.Key='a'; Vertex.Dist=Int32.MaxValue }
        {Vertex.Key='b'; Vertex.Dist=Int32.MaxValue }
        {Vertex.Key='c'; Vertex.Dist=Int32.MaxValue }
        {Vertex.Key='d'; Vertex.Dist=Int32.MaxValue }
        {Vertex.Key='e'; Vertex.Dist=Int32.MaxValue }
        {Vertex.Key='f'; Vertex.Dist=Int32.MaxValue }
        |]
    let edges = [|
        { Edge.Start = vertices.[0]; End = vertices.[1]; Cost = 7 }
        { Edge.Start = vertices.[0]; End = vertices.[2]; Cost = 9 }
        { Edge.Start = vertices.[0]; End = vertices.[5]; Cost = 14 }
        { Edge.Start = vertices.[1]; End = vertices.[2]; Cost = 10 }
        { Edge.Start = vertices.[1]; End = vertices.[3]; Cost = 15 }
        { Edge.Start = vertices.[2]; End = vertices.[3]; Cost = 11 }
        { Edge.Start = vertices.[2]; End = vertices.[5]; Cost = 2 }
        { Edge.Start = vertices.[3]; End = vertices.[4]; Cost = 6 }
        { Edge.Start = vertices.[4]; End = vertices.[5]; Cost = 9 }
        |]
    
    let mutable prevDic = new Dictionary<char, Char>()
    let source = 'a'
    let destination = 'e'

    //Set start vertex dist to 0
    vertices <- setVertexDist vertices source 0
    let mutable Q = [| vertices.[0] |]

    while Q.Length > 0 do
        let u = Q.[0]
        Q <- remove Q 0
        
        //Foreach neighbor
        let neighbors = findNeighbors edges u
        for neighbor in neighbors do
            let alt = u.Dist + neighbor.Cost
            if alt < neighbor.End.Dist then
                vertices <- setVertexDist vertices neighbor.End.Key alt
                if not (includes Q neighbor.End.Key ) then
                    Q <- Array.append Q [| neighbor.End |]
                
                if prevDic.ContainsKey(neighbor.End.Key) then
                    prevDic.Item neighbor.End.Key <- u.Key
                else 
                    prevDic.Add(neighbor.End.Key, u.Key)

    let mutable s: char array = [| |]
    let mutable u = destination
    while u <> source && u <> 'z' do
        s <- Array.append s [| u |]
        u <- prevDic.GetValueOrDefault u
    s <- Array.append s [| u |]

    printfn "Route:"
    for place in s do
        printf "%c" place
    printfn ""

    stopWatch.Stop()
    printfn "Milliseconds: %f" stopWatch.Elapsed.TotalMilliseconds

    0 // return an integer exit code


