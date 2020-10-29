// Learn more about F# at http://fsharp.org

open System
open System.Text

let alphabet: string = "abcdefghijklmnopqrstuvwxyz"
let mutable dimension1 = 0
let mutable dimension2 = 0

let substitute (value: char) =
    if value = 'j' then
        'i'
    else
        value

let applySubstitutions (text: string) = 
    let mutable result = ""
    for i in 0 .. (text.Length-1) do
        result <- result + (substitute text.[i]).ToString()
    result

let existsInTable (table: char[,]) (value: char) =
    let mutable result = false
    for i in 0 .. (dimension1-1) do
        for j in 0 .. (dimension2-1) do
            if table.[i, j] = value then
                result <- true
    result

let populateKeyTable (table: char[,]) (keyword: string) = 
    //Apply substitutions
    let newKeyWord = applySubstitutions (keyword + alphabet)
    
    //Add the keyword to the table
    let mutable pos = 0
    for key in newKeyWord do
        if not (existsInTable table key) then
            table.[(pos/dimension1), (pos % dimension1)] <- Char.ToLower key
            pos <- pos + 1

let getCoordinates (value: char) (table: char[,]) =
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
    let mutable (col1, row1) = getCoordinates input1 table
    let mutable (col2, row2) = getCoordinates input2 table
    if (row1 = row2) then
        col1 <- addOne col1 dimension1
        col2 <- addOne col2 dimension1
    else if (col1 = col2) then
        row1 <- addOne row1 dimension2
        row2 <- addOne row2 dimension2
    else
        let temp = row1
        row1 <- row2
        row2 <- temp

    (table.[col1, row1], table.[col2, row2])

let decryptPair (input1: char) (input2: char) (table: char[,]) =
    let mutable (col1, row1) = getCoordinates input1 table
    let mutable (col2, row2) = getCoordinates input2 table
    //The four rules:
    if (row1 <> row2 && col1 <> col2) then
        let temp = row1
        row1 <- row2
        row2 <- temp
    else if (col1 = col2) then
        row1 <- subtractOne row1 dimension2
        row2 <- subtractOne row2 dimension2
    else if (row1 = row2) then
        col1 <- subtractOne col1 dimension1
        col2 <- subtractOne col2 dimension1
    
    (table.[col1, row1], table.[col2, row2])

let takeNext (table: char[,]) (text: StringBuilder) (pos: int) =
    let mutable newPos = pos
    let textLength = text.Length
    while newPos < textLength && (not (existsInTable table (substitute (Char.ToLower text.[newPos])))) do
        newPos <- newPos+1
    
    if (newPos >= textLength) then
        ('x', newPos+1)
    else 
        (substitute (Char.ToLower text.[newPos]), (newPos+1))

let addEncryption (initialPos: int) ((char1, pos1): (char*int)) ((char2, pos2): (char*int)) table (text:StringBuilder) (encrypt:bool) = 
    if encrypt then
        let (encrypted1, encrypted2) = encryptPair char1 char2 table
        // This is to keep the white-space and special caracters
        // result <- result.Append (text.[initialPos .. (pos1-2)] + encrypted1.ToString())
        // result <- result.Append (text.[pos1 .. (pos2-2)] + encrypted2.ToString())
        (encrypted1.ToString() + encrypted2.ToString())
    else
        let (decrypted1, decrypted2) = decryptPair char1 char2 table
        // This is to keep the white-space and special caracters
        // result <- result.Append (text.[initialPos .. (pos1-2)] + decrypted1.ToString())
        // result <- result.Append (text.[pos1 .. (pos2-2)] + decrypted2.ToString())
        (decrypted1.ToString() + decrypted2.ToString())

let iterateOnPairs (table: char[,]) (inputText: string) (encrypt: bool) =
    let mutable i: int = 0
    let mutable result = StringBuilder()
    let mutable text: StringBuilder = StringBuilder inputText
    
    let mutable textLength = text.Length
    while i <= textLength do
        let (char1, pos1) = takeNext table text i
        let (char2, pos2) = takeNext table text pos1
        
        if (pos1 >= textLength) then
            i <- pos1
        else 
            if (char1 = char2 && char1 <> 'x' && encrypt) then
                text <- text.Insert(pos1, "x")
                textLength <- textLength+1
            else
                result <- result.Append (addEncryption i (char1, pos1) (char2, pos2) table text encrypt)
                i <- pos2
    result.ToString()

[<EntryPoint>]
let main argv =
    let text = System.IO.File.ReadAllText("benchmarks/playfair_cipher/lines.txt")
    let keyword: string = "thisisagreatkeyword"
    
    let mutable keyTable: char[,] = Array2D.create 5 5 Char.MaxValue
    dimension1 <- keyTable.GetLength(0)
    dimension2 <- keyTable.GetLength(1)
    
    populateKeyTable keyTable keyword
    
    let encryption = iterateOnPairs keyTable text true
    let decryption = iterateOnPairs keyTable encryption false
    
    System.IO.File.WriteAllText("../encrypted.txt", encryption)
    System.IO.File.WriteAllText("../decrypted.txt", decryption)
    
    printfn "Length Encryption: %i" encryption.Length
    printfn "Length Decryption: %i" decryption.Length
    0 // return an integer exit code
