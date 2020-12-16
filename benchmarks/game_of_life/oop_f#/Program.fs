// Learn more about F# at http://fsharp.org

open System
open System.IO
open benchmark

type IRules =
    abstract member Apply : bool -> int -> bool


type GameRules() =
    interface IRules with
        member __.Apply cellValue liveNeighbourCount = 
            cellValue && (liveNeighbourCount = 2 || liveNeighbourCount = 3) || (not cellValue) && liveNeighbourCount = 3


type Board(size) =
    let mutable _board: bool[,] = Array2D.zeroCreate 0 0
    member val Size = size with get
    member __.GetCell x y = _board.[x, y]
    member __.Update newBoard = _board <- newBoard
    member __.GetLiveCount = 
        let mutable count = 0
        for i in 0 .. (size - 1) do
            for j in 0 .. (size - 1) do
                if _board.[i, j] then count <- count + 1
        count

    member __.Initialize (initState: bool[,])=
        _board <- Array2D.zeroCreate size size
        _board <- initState

type Life (gameRules: IRules, boardSize, initState: bool[,]) =
    let board = Board(boardSize)
    let (%%) x y = (x % y + y) % y // The real mod
    let countLiveNeighbours x y =
        let mutable value = 0
        for row in -1 .. 1 do
            let k = (y + row) %% boardSize 
            for column in -1 .. 1 do
                let h = (x + column) %% boardSize
                value <- value + if board.GetCell h k then 1 else 0
        value - (if board.GetCell x y then 1 else 0)
    do
        board.Initialize initState

    member __.GetLiveCount = board.GetLiveCount
    member __.NextGeneration =
        let newBoard: bool[,] = Array2D.zeroCreate boardSize boardSize
        for x in 0 .. (boardSize - 1) do
            for y in 0 .. (boardSize - 1) do
                let neighbours = countLiveNeighbours x y
                let cell = board.GetCell x y
                newBoard.[x, y] <- gameRules.Apply cell neighbours
        board.Update newBoard


[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)
    
    let size = 256
    let initState = Array2D.zeroCreate size size
    let stateMap = File.ReadAllText("benchmarks/game_of_life/state256.txt")
    for index in 0 .. (stateMap.Length - 1) do
        initState.[index / size, index % size] <- stateMap.[index] = '1'
    
    bm.Run((fun () ->
        let gameOf = Life(GameRules(), size, initState)
        for i in 0 .. 99 do
            gameOf.NextGeneration
        gameOf.GetLiveCount
    ), (fun(res) ->
        printfn "%d" res
    ))
    
    0 // return an integer exit code
