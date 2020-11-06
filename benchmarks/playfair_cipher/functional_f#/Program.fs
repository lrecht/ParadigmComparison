open System
open System.IO

let filePath = "benchmarks/playfair_cipher/lines.txt"
let input = File.ReadAllText filePath
let keyword = "this is a great keyword"
let alph = "ABCDEFGHIKLMNOPQRSTUVWXYZ"
let rare = 'X'

let contains letter text =
    (List.exists (fun c -> c = letter) (text |> Seq.toList))

let rec prepKey' key (used:char list) =
    match key with
        | x::xs -> if contains x used
                        then prepKey' xs used
                   else prepKey' xs (x::used)
        | _ -> List.rev used

let prepKey key = 
    prepKey' (Seq.toList (String.collect 
                (fun c -> 
                    if c = 'j' || c = 'J'
                        then "I" 
                    elif not (contains (Char.ToUpper c) alph) 
                        then "" 
                    else string (Char.ToUpper c)) key+alph))
                []

let rec computePos char (values:char []) res =
    if char = 'J' then -1 elif char = values.[res] then res else computePos char values (res+1)

let createTable keyword =
    let key = prepKey keyword
    let values = Seq.toArray key
    let positions = Array.init 26 (fun index -> computePos (char (index + (int 'A'))) values 0)
    (positions,values)

let findPos letter (positions:int [],_) =
    let p = positions.[(int letter - int 'A')]
    (p%5,p/5)

let findVal col row (_,values:char []) =
    values.[(col+row*5)]

let rec pairHelper adjust first second table =
    let col1,row1 = findPos first table in
    let col2,row2 = findPos second table in

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
    pairHelper (fun x -> x%5) first second table

let decodePair first second table =
    pairHelper (fun x -> (x-1+5)%5) first second table

let rec prepInput' input res = 
    match input with
    | x::xs ->  if x = 'J'
                    then prepInput' xs ('I'::res)
                elif contains x alph
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
