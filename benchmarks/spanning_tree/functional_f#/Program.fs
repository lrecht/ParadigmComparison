open System.Collections.Immutable
open System.Collections.Generic

let compare (from,dest,w) (from1,dest1,w1) =
    if w < w1 then 1
    elif w > w1 then -1
    elif from < from1 then 1
    elif from > from1 then -1
    elif dest < dest1 then 1
    elif dest > dest1 then -1
    else 0

let rec find node (uf:ImmutableArray<int32>) changes =
    if uf.[node] < 0 || List.contains node changes
    then (node,(List.fold (fun (uf:ImmutableArray<int32>) change -> uf.Insert(node,change)) uf changes))
    else 
        find uf.[node] uf (node::changes)

let rec spanningTree' (edges:ImmutableSortedSet<int*int*int>) (uf:ImmutableArray<int32>) weightSum edgeNumber vCount =
    if vCount = 0 then (weightSum,edgeNumber)
    elif edges.IsEmpty then (-1,-1)
    else 
        let (f,d,w) = edges.Min
        let root1,uf1 = find f uf []
        let root2,uf2 = find d uf1 []
        if root1 = root2 then spanningTree' (edges.Remove (f,d,w)) uf2 weightSum edgeNumber vCount
        else spanningTree' (edges.Remove (f,d,w)) (uf2.Insert(root1,root2)) (weightSum+w) (edgeNumber+1) (vCount-1)

let spanningTree (edges:ImmutableSortedSet<int*int*int>) =
    spanningTree' edges ([|for i in 0 .. 6006 do -1|].ToImmutableArray()) 0 0 6004

[<EntryPoint>]
let main argv =
    let iss = ImmutableSortedSet.Empty.WithComparer (Comparer<int*int*int>.Create (fun x1 x2 -> compare x1 x2))
    let res = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv")
    let full = Array.fold (fun (acc:ImmutableSortedSet<int*int*int>) (s:string) -> 
        let sarr = s.Split ',' in acc.Add((int32 sarr.[0]), (int32 sarr.[1]), (int32 sarr.[2]))) iss res
    let weight,edge = spanningTree full
    printfn "(%i,%i)" weight edge
    0 // return an integer exit code
