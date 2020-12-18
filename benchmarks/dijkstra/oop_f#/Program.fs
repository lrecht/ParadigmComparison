// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic
open benchmark

[<AllowNullLiteral>]
type Vertex(name) =
    member this.name = name 
    member val neighbours = new Dictionary<Vertex, Int32>() with get, set
    member val previous:Vertex = null with get, set
    member val dist = Int32.MaxValue with get, set
    override this.Equals o = (o :?> Vertex).name = this.name
    override this.GetHashCode() = hash this.name
    interface System.IComparable with
        member this.CompareTo o = 
            let v = o :?> Vertex
            if v.dist = this.dist
            then compare this.name v.name
            else compare this.dist v.dist


type Edge(startVertex, endVertex, cost) =
    member this.Start = startVertex
    member this.End = endVertex
    member this.Cost = cost

    // Converts a line from graph CSV to an edge
    static member FromCSV line =
        let values = (line:String).Split(",")
        let cost:Int32 = Convert.ToInt32(values.[2])
        let edge = Edge(values.[0], values.[1], cost)
        edge


type Graph(edges) =
    let mutable _edges = new List<Edge>()
    let graph = new Dictionary<String, Vertex>()
    do
        _edges <- edges

        // Fill the Set with all available verticies
        for e:Edge in edges do
            if not (graph.ContainsKey e.Start) 
            then graph.Add(e.Start, Vertex(e.Start)) |> ignore

            if not (graph.ContainsKey e.End)
            then graph.Add(e.End, Vertex(e.End)) |> ignore
        
        for e:Edge in edges do
            let endV = graph.[e.End]
            graph.[e.Start].neighbours.Add(endV, e.Cost)

    let getPath dest =
        if not (isNull (dest:Vertex).previous) then
            let path = new List<String>()
            let mutable previous = dest
            while not ( isNull previous ) do
                path.Insert(0, previous.name) |> ignore
                previous <- previous.previous
            path
        else new List<String>()

    let visitNeighbours (current: Vertex) (vertexQueue:SortedSet<Vertex>) = 
        for n in current.neighbours do
            let mutable neighbour = n.Key
            let mutable alternativeDist = current.dist + n.Value
            if alternativeDist < neighbour.dist then 
                vertexQueue.Remove(neighbour) |> ignore
                neighbour.dist <- alternativeDist
                neighbour.previous <- current
                vertexQueue.Add(neighbour) |> ignore

    let mutable isDone = false
    member this.Solve startVertex endVertex = 
        if graph.ContainsKey startVertex && graph.ContainsKey endVertex then
            let source = graph.[startVertex]
            source.dist <- 0
            let dest = graph.[endVertex]
            let vertexQueue = new SortedSet<Vertex>()
            vertexQueue.Add(source) |> ignore

            while vertexQueue.Count > 0 && not isDone do
                let mutable current = vertexQueue.Min
                vertexQueue.Remove(current) |> ignore
                if not (current.Equals dest) then
                    visitNeighbours current vertexQueue
                else isDone <- true 
            getPath dest
        else List<String>()
    

[<Literal>]
let START = "257"
let END = "5525"

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let lines = System.IO.File.ReadAllLines("benchmarks/dijkstra/graph.csv")
    
    bm.Run((fun () -> 
        let edges = List<Edge>()
        for line in lines do
            edges.Add(Edge.FromCSV(line))

        let g = Graph(edges)
        g.Solve START END
    ), (fun res -> 
        for i in res do
            printf "%A " i
    ))
    
    0 // return an integer exit code
