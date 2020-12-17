open System
open System.Text
open benchmark

let suits: string array = [| "Clubs"; "Diamonds"; "Hearts"; "Spades" |]
let numbers: string array = [| "Two"; "Three"; "Four"; "Five"; "Six"; "Seven"; "Eight"; "Nine"; "Ten"; "Jack"; "Queen"; "King"; "Ace" |]

type Deck = {
    mutable Size: int
    mutable Cards: string array
}

let createNewDeck deck =
    deck.Size <- 52
    deck.Cards <- (Array.create 52 null)
    let mutable i = 0
    for suit in 0 .. suits.Length-1 do
        for num in 0 .. numbers.Length-1 do
            deck.Cards.[i] <- numbers.[num] + " of " + suits.[suit]
            i <- i+1
    

let deckToString (deck: Deck) =
    let mutable result: StringBuilder = StringBuilder()
    for card in 0 .. (deck.Size-1) do
        result <- result.Append(deck.Cards.[card] + " ")
    result

let shuffle (deck: Deck) =
    // using Knuth Shuffle (see at http://rosettacode.org/wiki/Knuth_shuffle)
    let random = new Random();
    for i in 0 .. (deck.Size-1) do
        let r: int = random.Next(i, deck.Size)
        let temp = deck.Cards.[i];
        deck.Cards.[i] <- deck.Cards.[r];
        deck.Cards.[r] <- temp;

let dealCard (deck: Deck) = 
    let card = deck.Cards.[deck.Size-1]
    deck.Size <- deck.Size-1
    card

[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    bm.Run((fun () ->
        let mutable count = 0
        let mutable deck: Deck = { Deck.Size = 0; Deck.Cards = [||] }
        
        for x in 0 .. 999 do
            createNewDeck deck
            while deck.Size <> 0 do
                //Show
                let deckString = deckToString deck
                
                //shuffle (randomize) the deck
                shuffle deck
                
                //deal from the deck
                dealCard deck |> ignore
                count <- count + deckString.Length
        count
    ), (fun (res) ->
        printfn "Count: %i" res
    ))
    
    0 // return an integer exit code
