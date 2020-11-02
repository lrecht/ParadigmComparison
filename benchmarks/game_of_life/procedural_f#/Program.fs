﻿// Learn more about F# at http://fsharp.org

open System

let runs: int = 2800
let height: int = 50
let width: int = 220
let mutable board: bool[,] = Array2D.create width height false

let countLiveNeighbors (x: int) (y: int) = 
    // The number of live neighbors.
    let mutable value = 0
    // This nested loop enumerates the 9 cells in the specified cells neighborhood.
    for j in -1 .. 1 do
        // Loop around the edges if y+j is off the board.
        let k = (y + j + height) % height

        for i in -1 .. 1 do
           // Loop around the edges if x+i is off the board.
            let h = (x + i + width) % width;

            // Count the neighbor cell at (h,k) if it is alive.
            if (board.[h, k]) then
                value <- value + 1
    // Subtract 1 if (x,y) is alive since we counted it as a neighbor.
    if (board.[x, y]) then
        value <- value - 1
    value

let updateBord() = 
    let newBoard = Array2D.create width height false
    for y in 0 .. height-1 do
        for x in 0 .. width-1 do
            let n: int = countLiveNeighbors x y
            let c: bool = board.[x, y]
            newBoard.[x, y] <- c && (n = 2 || n = 3) || not c && n = 3
    board <- newBoard

let printBoard() =
    for i in 0 .. width do
        printf "-"
    printfn ""
    for y in 0 .. height-1 do
        printf "|"
        for x in 0 .. width-1 do
            if board.[x, y] then
                printf "o"
            else
                printf " "
        printfn "|"
    for i in 0 .. width do
        printf "-"
    printfn ""

let initilizeBoard() =
    let rand: Random = Random(2)
    for y in 0 .. height-1 do
        for x in 0 .. width-1 do
            if rand.Next(0, 2) = 1 then
                board.[x, y] <- true

[<EntryPoint>]
let main argv =
    initilizeBoard()
    for i in 0 .. runs do
        updateBord()
    printBoard()
    0 // return an integer exit code
