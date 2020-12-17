open System
open benchmark

let Value = [ "Two";"Three";"Four";"Five";"Six";"Seven";"Eight";"Nine";"Ten";"Jack";"Queen";"King";"Ace" ]
let Suit = [ "Diamonds";"Spades";"Hearts";"Clubs" ]

let newDeck () = [for x in Value do
                    for y in Suit -> x,y] 

let rand = Random()
let shuffle xs = List.sortBy (fun x -> rand.Next()) xs

let showDeck deck = 
    String.concat "\n" (List.map (fun (v,s) -> v + " of " + s) deck)

let deal deck = (List.head deck, List.tail deck)

let rec run' deck runs c count =
    match runs,count with 
        | 0, _ -> c
        | _, 0 -> run' (newDeck()) (runs-1) c 52
        | _, _ -> run' ((snd (deal (shuffle deck)))) runs ((String.length (showDeck deck)) + c) (count-1)

let run runs =
    run' (newDeck()) runs 0 52

[<EntryPoint>]
let main argv =
    var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
    var bm = new Benchmark(iterations);
    
    bm.Run((fun () ->
        run 1000
    ), (fun (res) ->
        printfn "%O" res
    ))
    0 // return an integer exit code