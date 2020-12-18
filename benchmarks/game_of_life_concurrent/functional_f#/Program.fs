open benchmark

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
    Array.Parallel.mapi (fun index cell -> (rules cell (count index arr))) arr

let readFile file =
    (Seq.map (fun c -> c = '1') (file) |> Seq.toArray)
    
let rec run arr number =
    if number > 0 
    then run (updateCells arr) (number-1)
    else arr |> Seq.cast |> Seq.filter id |> Seq.length

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let file = System.IO.File.ReadAllText "benchmarks/game_of_life_concurrent/state256.txt";
    bm.Run(( fun () ->
        let arr = readFile file
        run arr 100
    ),( fun(res) -> 
        printfn "%i" res
    ))

    0 // return an integer exit code
