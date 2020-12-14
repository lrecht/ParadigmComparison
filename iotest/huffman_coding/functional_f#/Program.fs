// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Collections.Immutable
open System.Collections.Generic

let filePath = "iotest/huffman_coding/lines.txt"
let input = File.ReadAllText filePath


[<EntryPoint>]
let main argv =
    let c = input.Length
    printfn "%i" c
    0 // return an integer exit code
