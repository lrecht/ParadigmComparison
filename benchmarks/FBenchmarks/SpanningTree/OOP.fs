namespace FBenchmarks.SpanningTree

open System
open System.Collections.Generic
open System.IO

module OOP =
    open BenchmarkInterface
    type ISpanningTree =
        abstract member ComputeSpanningTree : unit -> int * int


    type Edge(startVertex, endVertex, cost) =
        member __.Start = startVertex
        member __.End = endVertex
        member __.Weight = cost
        member val Id = 0 with get,set
        static member FromCsv (csvLine: String) id =
            let values = csvLine.Split(',')
            let n1 = Convert.ToInt32(values.[0])
            let n2 = Convert.ToInt32(values.[1])
            let w = Convert.ToInt32(values.[2])
            let mutable edge = Edge(n1, n2, w)
            edge.Id <- id
            edge

        override __.Equals obj = 
            let other = obj :?> Edge
            __.Start.Equals(other.Start) && __.End.Equals(other.End)

        override __.GetHashCode() =
            __.Id.GetHashCode()

        override __.ToString() =
            sprintf "%d, %d, with weight %d" __.Start __.End __.Weight

        interface IComparable<Edge> with
            member __.CompareTo other =
                if __.Weight = other.Weight
                then __.Id.CompareTo(other.Id)
                else __.Weight.CompareTo(other.Weight)


        type UnionFind() =
            let mutable vertexGroup = Array.create (6005 + 1) -1
            member __.Find node =
                if vertexGroup.[node] < 0 then node
                else 
                    vertexGroup.[node] <- __.Find(vertexGroup.[node])
                    vertexGroup.[node]
        
            member __.Union n1 n2 =
                let group1Root = __.Find(n1)
                let group2Root = __.Find(n2)
                if group1Root = group2Root then false
                else
                    vertexGroup.[group2Root] <- group1Root
                    true
            

    type Graph(edges:Edge[], vertexCount) =
        do Array.Sort edges
        let edges = edges
        let mutable totalWeight : int = 0
        let mutable totalEdges : int = 0
        member this.ComputeSpanningTree() = (this :> ISpanningTree).ComputeSpanningTree()
        interface ISpanningTree with
            member __.ComputeSpanningTree() = 
                let res = List<Edge>()
                let uf = UnionFind()
                let mutable i = 0
                while res.Count < vertexCount - 1 do
                    let mutable currentEdge = edges.[i]
                    i <- i+1
                    if uf.Union currentEdge.Start currentEdge.End
                    then
                        res.Add(currentEdge)
                        totalWeight <- totalWeight + currentEdge.Weight
                        totalEdges <- totalEdges + 1
                (totalWeight, totalEdges)
    
    type Benchmark() =
        [<DefaultValue>] val mutable input : string []
        interface IBenchmark with
            member this.Preprocess() =
                this.input <- System.IO.File.ReadAllLines("benchmarks/graph.csv")

            member this.Run() =
                let mutable edges : Edge array = Array.zeroCreate this.input.Length
                for index in 0 .. this.input.Length - 1 do
                    edges.[index] <- Edge.FromCsv this.input.[index] (index + 1)
                let graph = Graph(edges, 5877)
                let (totalW, totalE) = graph.ComputeSpanningTree()
                totalE+totalW