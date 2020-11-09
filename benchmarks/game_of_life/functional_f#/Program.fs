let size = 256
let neighbours = List.except [(0,0)] [for x in [-1;0;1] do for y in [-1;0;1] do (x,y)]

let wrap x =
    ((x % size) + size) % size

let count index (arr:bool []) =
    List.fold (fun count (xdif,ydif) -> 
                if arr.[wrap ((index % size)+xdif) + (wrap ((index / size)+ydif)) * size]
                then count + 1 
                else count) 0 neighbours

let rules cell count =
    match cell,count with
    | true,(2|3) -> true
    | false,3 -> true
    | _ -> false
    
let updateCells arr =
    Array.mapi (fun index cell -> (rules cell (count index arr))) arr

let readFile file =
    (Seq.map (fun c -> c = '1') (System.IO.File.ReadAllText file) |> Seq.toArray)
    
let rec run arr number =
    if number > 0 
    then run (updateCells arr) (number-1)
    else arr |> Seq.cast |> Seq.filter id |> Seq.length

[<EntryPoint>]
let main argv =
    let arr = readFile "benchmarks/game_of_life/state256.txt"

    printfn "%i" (run arr 100)

    0 // return an integer exit code
