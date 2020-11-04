let size = 256
let neighbours = List.except [(0,0)] [for x in [-1;0;1] do for y in [-1;0;1] do (x,y)]

let rec count' coorX coorY (map:Map<int*int,bool>) coorList res =
    match coorList with
    | (head::tail) -> 
                let x = ((coorX + (fst head) % size) + size) % size
                let y = ((coorY + (snd head) % size) + size) % size
                count' coorX coorY map tail (if map.[(x,y)] then res+1 else res)
    | _ -> res

let count (coorX,coorY) map =
    count' coorX coorY map neighbours 0

let rules cell count =
    match cell,count with
    | true,(2|3) -> true
    | false,3 -> true
    | _ -> false

let updateCells (map:Map<int*int,bool>) =
    Map.fold (fun accMap coor cell -> 
                Map.add coor (rules cell (count coor map)) accMap) Map.empty map

let readFile file =
    List.zip ([ for x in [0..size-1] do for y in [0..size-1] do (x,y) ]) 
             (Seq.map (fun c -> c = '1') (System.IO.File.ReadAllText file) |> Seq.toList)

let rec run map number =
    if number > 0 
    then run (updateCells map) (number-1)
    else Map.fold (fun count _ b -> if b then count+1 else count) 0 map

[<EntryPoint>]
let main argv =
    let assocList = readFile "benchmarks/game_of_life/state256.txt"
    let map = Map.ofList assocList

    printfn "%i" (run map 100)

    0 // return an integer exit code
