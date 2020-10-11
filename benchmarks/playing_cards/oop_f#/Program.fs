// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic


// Value is necessary to make it ENUM and not UNION
type Value = Two=0 | Three=1 | Four=2 | Five=3 | Six=4 | Seven=5 | Eight=6 | Nine=7 | Ten=8 | Jack=9 | Queen=10 | King=11 | Ace=12
type Suit = Diamonds=0 | Spades=1 | Hearts=2 | Clubs=3


// The type of a single card
type Card(suit:Suit, value:Value) =
    member this.Suit = suit
    member this.Value = value
    override this.ToString() = sprintf "%A of %A" this.Value this.Suit


// The type of a whole deck
type Deck() =
    let _cards = new List<Card>()
    let _rand = Random()
    do
        for _suit in Enum.GetValues(typeof<Suit>) do
            for _value in Enum.GetValues(typeof<Value>) do
                _cards.Add(Card(_suit :?> Suit, _value :?> Value))

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
    let mutable count = 0
    for index in 0 .. 999 do // 1000 runs - [0 to and including 999]
        let d = Deck()
        while d.Count > 0 do
            let deck = d.ShowDeck
            d.Shuffle
            d.Deal |> ignore
            count <- count + (d.ShowDeck).Length
    printf "%d" count
    0