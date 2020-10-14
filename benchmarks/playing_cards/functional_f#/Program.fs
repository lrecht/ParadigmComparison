open System

type Value = Two=0 | Three=1 | Four=2 | Five=3 | Six=4 | Seven=5 | Eight=6 | Nine=7 | Ten=8 | Jack=9 | Queen=10 | King=11 | Ace=12
type Suit = Diamonds=0 | Spades=1 | Hearts=2 | Clubs=3

let newDeck = seq {for x in Enum.GetValues(typeof<Value>) do for y in Enum.GetValues(typeof<Suit>) -> x,y }

let rand = Random()
let shuffle xs = Seq.sortBy (fun x -> rand.Next()) xs

let showDeck deck = 
    String.concat "\n" (Seq.map (fun (v,s) -> sprintf "%O of %O" v s) deck)

let deal deck =
    (Seq.head deck, Seq.tail deck)

let rec run' deck runs c count =
    match runs,count with 
        | 0, _ -> c
        | _, 0 -> run' newDeck (runs-1) c 52
        | _, _ -> run' ((snd (deal (shuffle deck)))) runs ((String.length (showDeck deck)) + c) (count-1)

let run runs =
    run' newDeck runs 0 52

[<EntryPoint>]
let main argv =
    printfn "%O" (run 1000)
    0 // return an integer exit code