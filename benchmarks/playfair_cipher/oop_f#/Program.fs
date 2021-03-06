﻿open System
open System.Text
open System.Drawing
open System.Text.RegularExpressions
open benchmark

type Table() =
    let mutable charTable = Array2D.zeroCreate<Char> 5 5
    let mutable positions = Array.empty<Point>
    let mutable currentPos = Point.Empty
    let getPos x = int x - int 'A'
    do
        positions <- Array.create 26 Point.Empty

    member __.AddNext c =
        charTable.[currentPos.X, currentPos.Y] <- c
        positions.[getPos c] <- currentPos
        if currentPos.Y = 4 && currentPos.X <> 4
        then 
            currentPos.X <- currentPos.X + 1
            currentPos.Y <- 0
        else if currentPos.X = 5
        then raise (IndexOutOfRangeException())
        else currentPos.Y <- currentPos.Y + 1

    member __.ContainsChar c =
        (positions.[getPos c] <> Point.Empty) || charTable.[0,0] = c
    member __.GetPositionFromChar c = positions.[getPos c]
    member __.GetCharFromPosition x y = charTable.[x, y]


type PlayFairCipher(key) =
    let alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    let charTable = Table()
    let preprocessText (text:String) =
        let txt = text.ToUpper().Replace("J", "I")
        Regex.Replace(txt, "[^A-Z]", "")

    let createTable key = 
        for c in key do
            if not (charTable.ContainsChar c) then charTable.AddNext c

    let addKeyMod5 value key = (value + key) % 5
    let sameRow (aCord : Point) (bCord : Point) key = [|
            charTable.GetCharFromPosition aCord.X (addKeyMod5 bCord.Y key);
            charTable.GetCharFromPosition bCord.X (addKeyMod5 aCord.Y key)
        |]

    let sameColumn (aCord : Point) (bCord : Point) key = [|
            charTable.GetCharFromPosition (addKeyMod5 aCord.X key) aCord.Y;
            charTable.GetCharFromPosition (addKeyMod5 bCord.X key) bCord.Y
        |]

    let differentRowColumn (aCord : Point) (bCord : Point) = [|
            charTable.GetCharFromPosition aCord.X bCord.Y;
            charTable.GetCharFromPosition bCord.X aCord.Y
        |]

    let cipher (text : String) encipher = 
        let length = text.Length
        let returnValue = StringBuilder()
        let cipherKey = if encipher then 1 else 4
        for i in 0 .. 2 .. (length - 1) do
            let aCord = charTable.GetPositionFromChar text.[i]
            let bCord = charTable.GetPositionFromChar text.[i + 1]
            if (aCord.X = bCord.X) then returnValue.Append(sameRow aCord bCord cipherKey) |> ignore
            else if aCord.Y = bCord.Y
            then returnValue.Append(sameColumn aCord bCord cipherKey) |> ignore
            else returnValue.Append(differentRowColumn aCord bCord) |> ignore
        returnValue.ToString()
    do
        createTable(preprocessText(key + alphabet))

    member __.Decrypt text = cipher text false
    member __.Encrypt text =
        let mutable sb = StringBuilder(preprocessText text)
        let mutable i = 0
        while i < sb.Length do
            if i = sb.Length - 1 then sb.Append('X') |> ignore
            else if sb.[i] = sb.[i + 1] then sb.Insert(i + 1, 'X') |> ignore
            i <- i + 2
        cipher (sb.ToString()) true

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    let testString = System.IO.File.ReadAllText "benchmarks/playfair_cipher/lines.txt"
    
    bm.Run((fun () ->
        let p = PlayFairCipher("this is a great keyword")
        let encrypt = p.Encrypt(testString)
        let decrypt = p.Decrypt(encrypt)
        (encrypt.Length, decrypt.Length)
    ), (fun (en, de) ->
        printfn "%d" en
        printfn "%d" de
    ))
    0
