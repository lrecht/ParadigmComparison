// Learn more about F# at http://fsharp.org

open System.Collections.Immutable
open System.Collections.Generic
open System.IO
open benchmark

let graph (lines: seq<string>) = 
    Seq.map 
        (fun (a:string) -> 
            let words = (a.Split ',') 
            in (words.[0],words.[1],(words.[2] |> int))) 
        lines

let getNewMoves (edgeMap:Map<string,(string*string*int) list>) (moves:ImmutableSortedSet<(string*string*int)>) position cost =
    List.fold 
        (fun (m:ImmutableSortedSet<(string*string*int)>) (f,d,w) -> m.Add((f,d,w+cost)))
        moves
        (if edgeMap.ContainsKey position then Map.find position edgeMap else [])

let rec backtrack (visited:Map<string,string>) curr res start =
    if curr = start then (curr::res)
    else backtrack visited (Map.find curr visited) (curr::res) start

let rec findPath edgeMap (moves:ImmutableSortedSet<(string*string*int)>) (visited:Map<string,string>) dest =
    let from,target,cost = moves.Min in
        if target = dest then
            visited.Add(target,from)
        elif (visited.ContainsKey target) then
            findPath edgeMap (moves.Remove (from,target,cost)) visited dest
        else findPath 
                edgeMap 
                (getNewMoves edgeMap (moves.Remove (from,target,cost)) target cost) 
                (visited.Add(target,from))
                dest


let dijkstraPath edgeMap start dest = 
    if start = dest then [start] else
        backtrack (findPath edgeMap 
                            (getNewMoves 
                                edgeMap 
                                ((List.empty).ToImmutableSortedSet 
                                    (Comparer<(string*string*int)>.Create 
                                        (fun (f,d,w) (f1,d1,w1) -> if w > w1 then 1 elif w < w1 then -1 else d.CompareTo(d1))))
                                start 
                                0)
                            Map.empty 
                            dest)
                  dest [] start

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
        let input = graph lines
        let edgeMap = Seq.fold (fun (acc:Map<string,(string*string*int) list>) (from,dest,cost) -> 
            if acc.ContainsKey from then acc.Add(from,((from,dest,cost)::(acc.[from])))
            else acc.Add(from,[from,dest,cost])) Map.empty input
        dijkstraPath edgeMap "257" "5525"
    ), (fun res ->        
        List.map (fun a -> printfn "%O" a) res |> ignore)
    )
    0 // return an integer exit code
