// Learn more about F# at http://fsharp.org

open System
open System.IO

let filePath = "../soc-sign-bitcoinotc.csv"
let lines = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}
let graph = Seq.map 
                (fun (a:string) -> 
                    let words = (a.Split ',') 
                    in (words.[0],words.[1],(words.[2] |> int))) 
                lines

let rec merge xs ys res = 
  match xs,ys with
  | [],l | l,[] -> List.append res l
  | (a,b,x)::xs', (a',b',y)::ys' -> 
     if x <= y then merge xs' ys (List.append res [(a,b,x)])
     else merge xs ys' (List.append res [(a',b',y)])

let getNewMoves (edgeMap:Map<string,(string*string*int) list>) (visited:Map<string,string>) position cost =
    List.sortBy 
        (fun (a,b,c) -> c)
        (List.map 
            (fun (a,b,c: int) -> (a,b,c+cost)) 
            (List.filter 
                (fun (a,b,c) -> not (visited.ContainsKey b)) 
                (if edgeMap.ContainsKey position then edgeMap.[position] else [])))

let rec backtrack (visited:Map<string,string>) curr res start =
    if curr = start then (curr::res)
    else backtrack visited (visited.[curr]) (curr::res) start

let rec findPath edgeMap (moves:(string * string * int) list) (visited:Map<string,string>) dest =
    let from,target,cost = moves.Head in
        if target = dest then
            printfn "%A" cost
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
    let edgeMap = Seq.fold (fun (acc:Map<string,(string*string*int) list>) (from,dest,cost) -> 
        if acc.ContainsKey from then acc.Add(from,((from,dest,cost)::(acc.[from])))
        else acc.Add(from,[from,dest,cost])) Map.empty graph in
    let meh = List.map (fun a -> printfn "%O" a) (dijkstraPath edgeMap "257" "5525")
    0 // return an integer exit code
