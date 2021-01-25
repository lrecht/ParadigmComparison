// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic
open benchmark

// Value is necessary to make it ENUM and not UNION
let Value = [ "Two";"Three";"Four";"Five";"Six";"Seven";"Eight";"Nine";"Ten";"Jack";"Queen";"King";"Ace" ]
let Suit = [ "Diamonds";"Spades";"Hearts";"Clubs" ]


// The type of a single card
type Card(suit, value) =
    member this.Suit = suit
    member this.Value = value
    override this.ToString() = sprintf "%s of %s" this.Value this.Suit


// The type of a whole deck
type Deck() =
    let _cards = new List<Card>()
    let _rand = Random(2)
    do
        for _suit in Suit do
            for _value in Value do
                _cards.Add(Card(_suit, _value))

    member this.Count = _cards.Count

    // Shuffles the deck
    member this.Shuffle =
        for index in 0 .. _cards.Count - 1 do
            let ran = _rand.Next(_cards.Count)
            let temp = _cards.[index]
            _cards.[index] <- _cards.[ran]
            _cards.[ran] <- temp

    // Removes the last card of the deck
    member this.Deal =
        let last = _cards.Count - 1
        let card = _cards.[last]
        _cards.RemoveAt(last)
        card

    // Prints the deck with a line for each card
    member this.ShowDeck = String.Join('\n', _cards)


[<EntryPoint>]
let main argv =
    let iterations = if argv.Length > 0 then int (argv.[0]) else 1
    let bm = Benchmark(iterations)

    bm.Run((fun () ->
        let mutable count = 0
        for index in 0 .. 999 do // 1000 runs - [0 to and including 999]
            let d = Deck()
            while d.Count > 0 do
                let deck = d.ShowDeck
                d.Shuffle
                d.Deal |> ignore
                count <- count + deck.Length
        count
    ), (fun (res) ->
        printf "%d" res
    ))
    
    0
