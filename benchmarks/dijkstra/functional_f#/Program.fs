// Learn more about F# at http://fsharp.org

open System

let graph = [('a', 'b', 7);  ('a', 'c', 9);  ('a', 'f', 14); ('b', 'c', 10);
               ('b', 'd', 15); ('c', 'd', 11); ('c', 'f', 2);  ('d', 'e', 6);
               ('e', 'f', 9)]

let rec merge xs ys res = 
  match xs,ys with
  | [],l | l,[] -> List.append res l
  | (a,b,x)::xs', (a',b',y)::ys' -> 
     if x < y then merge xs' ys (List.append res [(a,b,x)])
     else merge xs ys' (List.append res [(a',b',y)])

let getNewMoves (edgeMap:Map<char,(char*char*int) list>) (visited:Map<char,char>) position cost =
    List.sortBy 
        (fun (a,b,c) -> c)
        (List.map 
            (fun (a,b,c: int) -> (a,b,c+cost)) 
            (List.filter 
                (fun (a,b,c) -> not (visited.ContainsKey b)) 
                (if edgeMap.ContainsKey position then edgeMap.[position] else [])))

let rec backtrack (visited:Map<char,char>) curr res start =
    if curr = start then (curr::res)
    else backtrack visited (visited.[curr]) (curr::res) start

let rec findPath edgeMap (moves:(char * char * int) list) (visited:Map<char,char>) dest =
    let from,target,cost = moves.Head in
        if target = dest then
            visited.Add(target,from)
        elif (visited.ContainsKey target) then
            findPath edgeMap moves.Tail visited dest
        else findPath 
                edgeMap 
                (merge 
                    (moves.Tail) 
                    (getNewMoves edgeMap visited target cost)
                    []) 
                (visited.Add(target,from))
                dest

let dijkstraPath edgeMap start dest = 
    if start = dest then [start] else
        backtrack (findPath edgeMap 
                            (getNewMoves edgeMap Map.empty start 0) 
                            Map.empty dest)
                  dest [] start

[<EntryPoint>]
let main argv =
    let edgeMap = List.fold (fun (acc:Map<char,(char*char*int) list>) (from,dest,cost) -> 
        if acc.ContainsKey from then acc.Add(from,((from,dest,cost)::(acc.[from])))
        else acc.Add(from,[from,dest,cost])) Map.empty graph in
    printfn "%A" (dijkstraPath edgeMap 'a' 'e')
    0 // return an integer exit code
