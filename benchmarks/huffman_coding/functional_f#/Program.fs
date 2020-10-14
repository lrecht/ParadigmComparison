// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Immutable
open System.Collections.Generic

let filePath = "benchmarks/huffman_coding/lines.txt"
let input = File.ReadAllText filePath

type Tree = Node of Weight:int * Left:Tree * Right:Tree 
          | Leaf of Weight:int * Value:char

let Compare t1 t2 =
    match t1,t2 with
        | Node (w,t,_), Node (w1,t1,_) -> if w < w1 then -1 elif w > w1 then 1 else compare t t1
        | Leaf (w,_), Node (w1,_,_) -> if w < w1 then -1  elif w > w1 then 1 else 1
        | Node (w,_,_), Leaf (w1,_) -> if w < w1 then -1 elif w > w1 then 1 else -1
        | Leaf (w,c), Leaf (w1,c1) -> if w < w1 then -1 elif w > w1 then 1 else c.CompareTo c1

let Weight t1 =
    match t1 with
        | Node (w,_,_) -> w
        | Leaf (w,_) -> w

// Returns a map with the frequencies of each char in the file.
let frequencies input = 
    Seq.fold (fun (acc:Map<char,int>) symbol -> 
        if acc.ContainsKey symbol 
            then acc.Add (symbol,(acc.[symbol] + 1))
            else acc.Add (symbol,1))
            Map.empty
            input

let sort frq =
    ImmutableSortedSet.ToImmutableSortedSet<Tree> 
        ((Seq.map (fun (c,w) -> Leaf (w,c)) (Map.toSeq frq)),
         (Comparer<Tree>.Create(fun t1 t2 -> Compare t1 t2)))

let rec makeTree (sortedset:ImmutableSortedSet<Tree>) =
    if sortedset.Count = 1 then sortedset.[0]
    else let first,second = sortedset.[0],sortedset.[1] in
            makeTree 
                (sortedset.Remove(first).Remove(second)
                   .Add(Node (((Weight first) + (Weight second)),first,second)))


let rec makeTable' tree path = 
    match tree with
        | Node (w,l,r) -> Map.fold 
                            (fun acc key value -> Map.add key value acc)
                            (makeTable' l (path+"0"))
                            (makeTable' r (path+"1"))
        | Leaf (w,c) -> Map.empty.Add (c,path)

let makeTable tree = 
    makeTable' tree ""

let huffman str =
    let frq = frequencies str
    let ss = sort frq
    let tree = makeTree ss
    let table = makeTable tree
    let encoded = String.collect (fun c -> table.[c]) str
    encoded

[<EntryPoint>]
let main argv =
    printfn "%i" (String.length (huffman input))
    0 // return an integer exit code
