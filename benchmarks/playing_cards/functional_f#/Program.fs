open System

let Value = [| "Two"; "Three"; "Four"; "Five"; "Six"; "Seven"; "Eight"; "Nine"; "Ten"; "Jack"; "Queen"; "King"; "Ace" |]
let Suit = [| "Diamonds"; "Spades"; "Hearts"; "Clubs" |]

let newDeck() = 
    Seq.toList (seq {for x in Value do for y in Suit do x,y })
    
let rand = Random()
let shuffle (xs: list<string*string>) = 
    Seq.toList(Seq.sortBy (fun x -> rand.Next()) xs)

let showDeck (deck: list<string*string>) = 
    String.Join("\n", (Seq.map (fun (v,s) -> (v.ToString() + " of " + s.ToString())) deck))

let deal (deck: list<string*string>) =
    let list = Seq.toList deck
    (list.Head, list.Tail)

let rec run' (deck: list<string*string>) runs c count =
    if runs = 0
        then c
    elif count = 0
        then run' (newDeck()) (runs-1) c 52
    else
        run' ((snd (deal (shuffle deck)))) runs ((String.length (showDeck deck)) + c) (count-1)
let run runs =
    run' (newDeck()) runs 0 52

[<EntryPoint>]
let main argv =
    printfn "%i" (run 1000)
    0 // return an integer exit code