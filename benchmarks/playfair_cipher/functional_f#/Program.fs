open System
open System.IO

let filePath = "benchmarks/huffman_coding/lines.txt"
let input = File.ReadAllText filePath
let keyword = "this is a great keyword"
let alph = "ABCDEFGHIKLMNOPQRSTUVWXYZ" 
let rare = 'X'

let inAlph letter alph =
    (List.fold (fun acc c -> c = letter || acc) false (alph |> Seq.toList))

let rec prepKey' key (used:char list) =
    match key with
        | x::xs -> if inAlph x used
                        then prepKey' xs used
                   else prepKey' xs (x::used)
        | _ -> List.rev used

let prepKey key = 
    prepKey' (Seq.toList (String.collect 
                (fun c -> 
                    if c = 'j' || c = 'J'
                        then "I" 
                    elif not (inAlph (Char.ToUpper c) alph) 
                        then "" 
                    else string (Char.ToUpper c)) key+alph))
                []

let rec createTable' keyword rowIndex colIndex table =
    match keyword with
        | x::xs -> createTable' xs 
                                ((rowIndex%5)+1) 
                                (colIndex+rowIndex/5) 
                                ((rowIndex,colIndex,x)::table)
        | _ -> table

let createTable keyword =
    createTable' (prepKey keyword) 1 1 []

let findPos letter table =
    List.find (fun (_,_,c) -> c = letter) table

let third (_,_,x) = x
let findVal x y table =
    third (List.find (fun (x1,y1,_) -> x1 = x && y1 = y) table)

let rec pairHelper adjust first second table =
    let col1,row1,_ = findPos first table in
    let col2,row2,_ = findPos second table in

    if first = second
        then (pairHelper adjust first rare table)
    elif row1 = row2 
        then sprintf "%c%c" 
                (findVal (adjust col1) row1 table) 
                (findVal (adjust col2) row2 table)
    elif col1 = col2
        then sprintf "%c%c"
                (findVal col1 (adjust row1) table)
                (findVal col2 (adjust row2) table)
    else sprintf "%c%c"
                (findVal col2 row1 table)
                (findVal col1 row2 table)

let encodePair first second table =
    pairHelper (fun x -> x%5+1) first second table

let decodePair first second table =
    pairHelper (fun x -> if x = 1 then 5 else x - 1) first second table

let rec prepInput' input res = 
    match input with
    | x::xs ->  if x = 'j' || x = 'J'
                    then prepInput' xs ('I'::res)
                elif inAlph x alph
                    then prepInput' xs (x::res)
                else prepInput' xs res
    | _ -> List.rev res

let prepInput input =
    prepInput' input []

let rec codeHelper codeFunc input table res = 
    match input with
    | (c1::c2::rest) -> if c1 = c2 
                            then codeHelper codeFunc (c2::rest) table ((codeFunc c1 rare table)::res)
                        else codeHelper codeFunc rest table ((codeFunc c1 c2 table)::res)
    | (c::_) -> String.concat "" (List.rev ((codeFunc c rare table)::res))
    | _ -> String.concat "" (List.rev res)

let encode input table = 
    let prep = prepInput (Seq.toList (Seq.map (Char.ToUpper) input))
    codeHelper encodePair prep table []

let decode input table =
    let prep = prepInput (Seq.toList (Seq.map (Char.ToUpper) input))
    codeHelper decodePair prep table []

[<EntryPoint>]
let main argv =
    let table = createTable keyword
    let encoded = encode input table
    let decoded = decode encoded table
    printfn "%i" (String.length encoded)
    printfn "%i" (String.length decoded)
    0 // return an integer exit code
