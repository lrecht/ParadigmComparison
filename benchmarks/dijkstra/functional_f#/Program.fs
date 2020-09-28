// Learn more about F# at http://fsharp.org

open System


let graph = [("a", "b", 7);  ("a", "c", 9);  ("a", "f", 14); ("b", "c", 10);
               ("b", "d", 15); ("c", "d", 11); ("c", "f", 2);  ("d", "e", 6);
               ("e", "f", 9)]

let rec merge xs ys res = 
  match xs,ys with
  | [],l | l,[] -> List.append res l
  | (a,b,x)::xs', (a',b',y)::ys' -> 
     if x < y then merge xs' ys (List.append res [(a,b,x)])
     else merge xs ys' (List.append res [(a',b',y)])

let getNewMoves graph (visited:Map<string,string>) position cost =
    List.filter (fun (a,b,c) -> not (visited.ContainsKey b)) 
        (List.filter (fun (a,b,c) -> a = position) 
            (List.map(fun (a,b,c: int) -> (a,b,c+cost)) graph))

let rec backtrack (visited:Map<string,string>) curr res start =
    if curr = start then (curr::res)
    else backtrack visited (visited.[curr]) (curr::res) start

let rec findPath graph (moves:(string * string * int) list) (visited:Map<string,string>) dest =
    let from,target,cost = moves.Head in
        if target = dest then
            visited.Add(target,from)
            else
            findPath graph (merge (moves.Tail) (getNewMoves graph visited target cost) []) (visited.Add(target,from)) dest


let dijkstraPath graph start dest = 
    if start = dest then [start] else
        backtrack (findPath graph (getNewMoves graph Map.empty start 0) Map.empty dest) dest [] start


[<EntryPoint>]
let main argv =
    printfn "%A" (dijkstraPath (List.sortBy (fun (a,b,c) -> c) graph) "a" "e")
    0 // return an integer exit code
