open System.Collections.Immutable

let compare (from,dest,w) (from1,dest1,w1) =
    if w < w1 then -1
    elif w > w1 then 1
    elif from < from1 then -1
    elif from > from1 then 1
    elif dest < dest1 then -1
    elif dest > dest1 then 1
    else 0

let rec find node (uf:ImmutableArray<int32>) changes =
    if uf.[node] < 0
    then node,(List.fold (fun (acc:ImmutableArray<int32>) n -> acc.SetItem(n,node)) uf changes)
    else 
        find uf.[node] uf (node::changes)

let rec spanningTree' (edges:(int32*int32*int32) list) (uf:ImmutableArray<int32>) weightSum edgeNumber vCount =
    if vCount = 0 then (weightSum,edgeNumber)
    else 
        let ((f,d,w)::xs) = edges
        let root1,uf1 = find f uf []
        let root2,uf2 = find d uf1 []
        if root1 = root2 then spanningTree' xs uf weightSum edgeNumber vCount
        else spanningTree' xs (uf2.SetItem(root1,root2)) (weightSum+w) (edgeNumber+1) (vCount-1)

let spanningTree (edges:(int32*int32*int32) list) =
    spanningTree' edges ([|for i in 0 .. 6006 do -1|].ToImmutableArray()) 0 0 5876

[<EntryPoint>]
let main argv =
    let res = System.IO.File.ReadAllLines("benchmarks/spanning_tree/graph.csv")
    let full = Array.map (fun (s:string) -> 
        let sarr = s.Split ',' in ((int32 sarr.[0]), (int32 sarr.[1]), (int32 sarr.[2]))) res
    let weight,edge = spanningTree (List.ofArray (Array.sortWith compare full))
    printfn "(%i,%i)" weight edge
    0 // return an integer exit code
