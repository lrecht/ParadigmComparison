// Learn more about F# at http://fsharp.org

open System
open System.Text

let height: int = 256
let width: int = 256
let mutable board: bool[,] = Array2D.create width height false


let initilizeBoard() =
    let state = System.IO.File.ReadAllText("iotest/game_of_life/state256.txt")
    for i in 0 .. state.Length-1 do
        board.[(i/width), (i % width)] <- state.[i] = '1'

[<EntryPoint>]
let main argv =
    initilizeBoard()
    let count: int = board.Length
    printfn "%i" count

    0 // return an integer exit code
