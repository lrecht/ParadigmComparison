// Learn more about F# at http://fsharp.org

open System
open System.IO

[<EntryPoint>]
let main argv =
    let stateMap = File.ReadAllText("iotest/game_of_life/state256.txt")
    let c = stateMap.Length
    printfn "%i" c
    0 // return an integer exit code
