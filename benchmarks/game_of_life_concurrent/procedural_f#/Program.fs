// Learn more about F# at http://fsharp.org

open System
open System.Threading
open System.Threading.Tasks
open benchmark

let runs: int = 100
let height: int = 256
let width: int = 256
let mutable board: bool[,] = Array2D.create width height false

let countLiveNeighbors (x: int) (y: int) = 
    // The number of live neighbors.
    let mutable value = 0
    // This nested loop enumerates the 9 cells in the specified cells neighborhood.
    for j in -1 .. 1 do
        // Loop around the edges if y+j is off the board.
        let k = ((((y + j + height) % height) + height) % height)

        for i in -1 .. 1 do
           // Loop around the edges if x+i is off the board.
            let h = ((((x + i + width) % width) + width) % width);

            // Count the neighbor cell at (h,k) if it is alive.
            if (board.[h, k]) then
                value <- value + 1
    // Subtract 1 if (x,y) is alive since we counted it as a neighbor.
    if (board.[x, y]) then
        value <- value - 1
    value

let updateBoadAt (x: int) (y: int) (newBoard: bool[,]) = 
    let n: int = countLiveNeighbors x y
    let c: bool = board.[x, y]
    newBoard.[x, y] <- c && (n = 2 || n = 3) || not c && n = 3

let updateBord() = 
    let newBoard = Array2D.create width height false
    
    Parallel.For(0, width, fun(x) -> 
        Parallel.For(0, height, fun(y) -> 
            updateBoadAt x y newBoard
        ) |> ignore
    ) |> ignore

    board <- newBoard

let initilizeBoard (state: string) =
    let initState = Array2D.zeroCreate height width
    for i in 0 .. state.Length-1 do
        initState.[(i/width), (i % width)] <- state.[i] = '1'
    initState

let countAlive () =
    let mutable count = 0
    for i in 0 .. height-1 do
        for j in 0 .. width-1 do
            if (board.[i, j]) then
                count <- count + 1
    count

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let file = System.IO.File.ReadAllText("benchmarks/game_of_life_concurrent/state256.txt");
    bm.Run((fun () -> 
        board <- initilizeBoard file
        for i in 0 .. runs-1 do
            updateBord()
        countAlive()
    ), (fun (res) ->
        printfn "Alive: %i" res
    ))
    
    0 // return an integer exit code
