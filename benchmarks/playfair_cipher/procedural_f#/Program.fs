// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic
open System.Text

// The stupid rule: i = j
let substitutions: Dictionary<char, char> = new Dictionary<char, char>()
substitutions.Add('j', 'i')

let alphabet: string = "abcdefghijklmnopqrstuvwxyz"

let substitute (value: char) =
    if substitutions.ContainsKey(value) then
        substitutions.[value]
    else
        value

let applySubstitutions (text: string) = 
    let mutable result = ""
    for i in 0 .. (text.Length-1) do
        result <- result + (substitute text.[i]).ToString()
    result

let existsInTable (table: char[,]) (value: char) =
    let dimension1 = table.GetLength(0)
    let dimension2 = table.GetLength(1)
    let mutable result = false
    for i in 0 .. (dimension1-1) do
        for j in 0 .. (dimension2-1) do
            if table.[i, j] = value then
                result <- true
    result

let addKeyword (table: char[,]) (keyword: string) (startPosition: int) =
    let mutable pos = startPosition
    let dimension1 = table.GetLength(0)
    //Apply substitutions
    let newKeyWord = applySubstitutions keyword
    
    //Add the keyword to the table
    for key in newKeyWord do
        if not (existsInTable table key) then
            table.[(pos/dimension1), (pos % dimension1)] <- Char.ToLower key
            pos <- pos + 1
    pos

let populateKeyTable (table: char[,]) (keyword: string) = 
    let mutable posistion: int = 0
    posistion <- addKeyword table keyword posistion
    //Add the remainder of the alphabet
    addKeyword table alphabet posistion |> ignore

let getCoordinates (value: char) (table: char[,]) =
    let dimension1 = table.GetLength(0)
    let dimension2 = table.GetLength(1)
    let mutable result: (int * int) = (Int32.MaxValue, Int32.MaxValue)
    for i in 0 .. (dimension1-1) do
        for j in 0 .. (dimension2-1) do
            if table.[i, j] = value then
                result <- (i, j)
    result

let addOne (value: int) (max: int) =
    if (value+1 >= max) then
        0
    else
        value+1

let subtractOne (value: int) (max: int) =
    if (value-1 < 0) then
        max-1
    else
        value-1

let encryptPair (input1: char) (input2: char) (table: char[,]) =
    let mutable val1 = input1
    let mutable val2 = input2
    let dimension1 = table.GetLength(0)
    let dimension2 = table.GetLength(1)
    //The four rules:
    if (val1 = val2) then
        val2 <- 'x'

    let mutable (row1, col1) = getCoordinates val1 table
    let mutable (row2, col2) = getCoordinates val2 table
    if (row1 = row2) then
        row1 <- addOne row1 dimension1
        row2 <- addOne row2 dimension1
    else if (col1 = col2) then
        col1 <- addOne col1 dimension2
        col2 <- addOne col2 dimension2
    else
        let temp = col1
        col1 <- col2
        col2 <- temp

    (table.[row1, col1], table.[row2, col2])

let decryptPair (input1: char) (input2: char) (table: char[,]) =
    let mutable val1 = input1
    let mutable val2 = input2
    let dimension1 = table.GetLength(0)
    let dimension2 = table.GetLength(1)
    let mutable (row1, col1) = getCoordinates val1 table
    let mutable (row2, col2) = getCoordinates val2 table
    //The four rules:
    if (row1 <> row2 && col1 <> col2) then
        let temp = col1
        col1 <- col2
        col2 <- temp
    else if (col1 = col2) then
        col1 <- subtractOne col1 dimension2
        col2 <- subtractOne col2 dimension2
    else if (row1 = row2) then
        row1 <- subtractOne row1 dimension1
        row2 <- subtractOne row2 dimension1
    
    (table.[row1, col1], table.[row2, col2])

let takeNext (table: char[,]) (text: string) (pos: int) =
    let mutable newPos = pos
    let textLength = text.Length
    while newPos < textLength && (not (existsInTable table (substitute (Char.ToLower text.[newPos])))) do
        newPos <- newPos+1
    
    if (newPos >= textLength) then
        ('x', newPos)
    else 
        (substitute (Char.ToLower text.[newPos]), (newPos+1))

let itterateOnPairs (table: char[,]) (inputText: string) (encrypt: bool) =
    let mutable i: int = 0
    let mutable result = StringBuilder()
    let mutable text: string = inputText
    
    let textLength: int = text.Length
    while i < textLength do
        let (char1, pos1) = takeNext table text i
        let (char2, pos2) = takeNext table text pos1
        if encrypt then
            let (encrypted1, encrypted2) = encryptPair char1 char2 table
            result <- result.Append (text.[i .. (pos1-2)] + encrypted1.ToString())
            result <- result.Append (text.[pos1 .. (pos2-2)] + encrypted2.ToString())
        else
            let (decrypted1, decrypted2) = decryptPair char1 char2 table
            result <- result.Append (text.[i .. (pos1-2)] + decrypted1.ToString())
            result <- result.Append (text.[pos1 .. (pos2-2)] + decrypted2.ToString())
        
        i <- pos2
    result.ToString()

[<EntryPoint>]
let main argv =
    let text = System.IO.File.ReadAllText("benchmarks/playfair_cipher/lines.txt")
    let keyword: string = "thisisagreatkeyword"
    let mutable keyTable: char[,] = Array2D.create 5 5 Char.MaxValue
    
    populateKeyTable keyTable keyword
    let encryption = itterateOnPairs keyTable text true
    let decryption = itterateOnPairs keyTable encryption false
    
    System.IO.File.WriteAllText("../encrypted.txt", encryption)
    System.IO.File.WriteAllText("../decrypted.txt", decryption)

    printfn "Encryption: %i" encryption.Length
    printfn "Decryption: %i" decryption.Length
    0 // return an integer exit code
