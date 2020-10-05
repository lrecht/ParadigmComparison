// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Generic

let initVertices (edges: Dictionary<string, (string*int) array>) =
    let vertices = new Dictionary<string, int32>()
    for edge in edges.Keys do
        vertices.Add(edge, Int32.MaxValue)
    vertices

let initPrevDic (edges: Dictionary<string, (string*int) array>) =
    let prevDic = new Dictionary<string, string>()
    for edge in edges.Keys do
        prevDic.Add(edge, null)
    prevDic

let toDictionary (map : Map<_, _>) : Dictionary<_, _> = Dictionary(map)

let initEdges =
    let filePath = "../graph.csv"
    let lines = seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    }

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

let remove (array: string array) (elm: string) =
    let arrayLength = array.Length
    let rec loop i =
        if i >= arrayLength || array.[i] = elm then
            i
        else 
            loop (i+1)
    let index = loop 0
    Array.append array.[0 .. (index-1)] array.[(index+1) .. (array.Length-1)]

let insertSort (array: string array) (costMap: Dictionary<string, int32>) (newValue: string) =
    let arrayLength = array.Length 
    let costNew = costMap.[newValue]
    let rec loop i =
        if i >= arrayLength || costNew < costMap.[array.[i]] then
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

    let source = "257"
    let mutable destination: string = "5525"

    vertices.[source] <- 0
    
    let mutable vertex_queue: string array = [| source |]
    
    while vertex_queue.Length > 0 do 
        let current = vertex_queue.[0]
        vertex_queue <- vertex_queue.[1 .. (vertex_queue.Length-1)]
        
        //Destination found
        if current = destination then
            vertex_queue <- [|  |]
        else if vertices.ContainsKey(current) then
            let neighbors = edges.[current]
            
            for (key, value) in neighbors do   
                //If the neighbor does not have another neighbor, then it is not added to the vertices, and cannot work
                //This fix however, will do so that we cannot find distinations if it has no edges going out...
                if vertices.ContainsKey(key) then
                    let alt: int32 = vertices.[current] + value
                    if alt < vertices.[key] then
                        vertex_queue <- remove vertex_queue key
                        vertices.[key] <- alt
                        prevDic.[key] <- current
                        vertex_queue <- insertSort vertex_queue vertices key
    
    //Create the shortest path
    let mutable shortestPath: (string array) = [| |]
    let mutable previous = destination
    while not (isNull previous) do
        shortestPath <- Array.append [| previous |] shortestPath
        previous <- prevDic.[previous]

    for part in shortestPath do
        printf "%s " part
    printfn ""
    printfn "Steps: %i" shortestPath.Length
    printfn "End weigth: %i" vertices.[destination]
    0 // return an integer exit code


