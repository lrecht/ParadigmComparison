open System

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
    let mutable result: string = ""
    for card in 0 .. (deck.Size-1) do
        result <- result + deck.Cards.[card] + " "
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
    //make a new deck
    let mutable count = 0
    let mutable deck: Deck = { Deck.Size = 0; Deck.Cards = [||] }
    
    for x in 0 .. 1000 do
        createNewDeck deck
        while deck.Size <> 0 do
            //Show
            let deckString = deckToString deck
            
            //shuffle (randomize) the deck
            shuffle deck
            
            //deal from the deck
            dealCard deck |> ignore
            count <- count + deckString.Length
    
    printfn "Count: %i" count
    0 // return an integer exit code

