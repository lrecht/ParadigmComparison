// Learn more about F# at http://fsharp.org

open System
open System.Text
open System.Text.RegularExpressions

let alphabet: string = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
let dimension1: int = 5
let dimension2: int = 5
let table: char[,] = Array2D.zeroCreate dimension1 dimension2
let positions: (int*int)[] = Array.create 26 (0, 0)

let charValue (value: char) =
    ((int value) - (int 'A'))

let populateTable (text: string) =
    let textLength = text.Length
    let mutable place = 0
    
    for i in 0 .. textLength-1 do
        let (col, row) = (place / dimension1, place % dimension1)
        let character = text.[i]
        let charPos = charValue character
        if (positions.[charPos] = (0, 0) && table.[0, 0] <> character) then
            table.[col, row] <- character
            positions.[charPos] <- (col, row)
            place <- place + 1

let preprocessText (text: string) =
    Regex.Replace(text.ToUpper().Replace("J", "I"), "[^A-Z]", "")

let cipher (item1: (int*int)) (item2: (int*int)) (direction: int) =
    let mutable (col1, row1) = item1
    let mutable (col2, row2) = item2
    if (row1 = row2) then
        col1 <- (col1 + direction) % 5
        col2 <- (col2 + direction) % 5
    else if (col1 = col2) then
        row1 <- (row1 + direction) % 5
        row2 <- (row2 + direction) % 5
    else
        let tmp: int = col1
        col1 <- col2
        col2 <- tmp

    let value1 = table.[col1, row1]
    let value2 = table.[col2, row2]
    value1.ToString() + value2.ToString()

let iterateOnPairs (input: string) (direction: int) =
    let mutable text: string = input
    
    let mutable result = StringBuilder()
    let mutable i: int = 0
    
    while (i < text.Length)do
        let first = positions.[(charValue text.[i])]
        let second = positions.[(charValue text.[i + 1])]
        result <- result.Append( (cipher first second direction) )
        i <- i + 2
    result.ToString()

let encrypt (text: string) =
    let mutable sb: StringBuilder = StringBuilder(text)
    let mutable i: int = 0
    
    while i < sb.Length do
        if (i = sb.Length - 1) then
            let mutable ending: string = ""
            if (sb.Length % 2 = 1) then
                ending <- "X"
            sb <- sb.Append(ending)
        else if (sb.[i] = sb.[i + 1]) then
            sb <- sb.Insert((i + 1), "X")
        i <- i + 2
    iterateOnPairs (sb.ToString()) 1

let decrypt (text: string) =
    iterateOnPairs text 4

[<EntryPoint>]
let main argv =
    let stop = System.Diagnostics.Stopwatch.StartNew()
    let text = System.IO.File.ReadAllText("../lines.txt")
    let keyword = "This is a great keyword"

    populateTable (preprocessText (keyword + alphabet))
    let processedText = preprocessText text
    
    let encryption: string = encrypt processedText
    let decryption: string = decrypt encryption
    
    printfn "%i" encryption.Length
    printfn "%i" decryption.Length
    printfn "Time: %i" stop.ElapsedMilliseconds
    0 // return an integer exit code
