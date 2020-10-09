open System

let suits: string array = [| "H"; "R"; "S"; "K" |] //The suits: Hjerter, Ruder, Spar, Klør
let numbers: string array = [| "A"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "10"; "J"; "Q"; "K" |]

type Deck = {
    mutable Size: int
    Cards: string array
}

let createNewDeck =
    let deck: Deck = { Deck.Size=52; Deck.Cards=(Array.create 52 null) }

    for suit in 0 .. suits.Length-1 do
        for num in 0 .. numbers.Length-1 do
            deck.Cards.[(num+(suit*numbers.Length))] <- suits.[suit] + "_" + numbers.[num]
    deck

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
    if (deck.Size = 0) then
        raise (Exception("There are no cards in the deck"))
    else
        let card = deck.Cards.[deck.Size-1]
        deck.Size <- deck.Size-1
        card

[<EntryPoint>]
let main argv =
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    
    //make a new deck
    let mutable deck: Deck = createNewDeck

    while deck.Size <> 0 do
        //Show
        deckToString deck |> ignore
        
        //shuffle (randomize) the deck
        shuffle deck
        
        //deal from the deck
        dealCard deck |> ignore

    stopWatch.Stop()
    printfn "Time: %i" stopWatch.ElapsedMilliseconds
    0 // return an integer exit code

