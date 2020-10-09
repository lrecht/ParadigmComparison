using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace functional_c_
{
    class Program
    {
    private enum Suit { Diamonds, Spades, Hearts, Clubs }
    private enum Value { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

        static void Main(string[] args)
        {
            var result = performPlayingCards(1000, 0);
            System.Console.WriteLine(result);
        }

        private static int performPlayingCards(int runs, int count){
            if(runs < 1)
                return count;
            
            return performPlayingCards(runs - 1, playingCardsOnDeck(getNewDeck(), count));
        }

        private static int playingCardsOnDeck(ImmutableList<(Suit, Value)> deck, int count){
            if(deck.Count < 1)
                return count;

            var deckStrSize = showDeck(deck).Count();
            var shuffledDeck = shuffleDeck(deck, new Random(), deck.Count, 0);
            var dealedDeck = shuffledDeck.Remove(getCardFromBackOfDeck(shuffledDeck));

            return playingCardsOnDeck(dealedDeck, deckStrSize + count);
        }


        private static string showDeck(ImmutableList<(Suit, Value)> deck)
        {
            return string.Join('\n', deck.Select(x => $"{x.Item2} of {x.Item1}"));
        }

        private static ImmutableList<(Suit, Value)> shuffleDeck(ImmutableList<(Suit, Value)> deck, Random rng, int deckSize, int i)
        {
            if(i >= deckSize)
                return deck;

            var randomNum = rng.Next(i, deckSize);
            var temp = deck[randomNum];
            return shuffleDeck(
                deck.Replace(deck[randomNum], deck[i]).Replace(deck[i], temp), 
                rng, 
                deckSize, 
                i + 1
                );
        }
        private static (Suit, Value) getCardFromBackOfDeck(ImmutableList<(Suit, Value)> deck)
        {
            return deck.Last();
        }

        private static ImmutableList<(Suit, Value)> getNewDeck(){
            var suits = ImmutableList<Suit>.Empty.AddRange((Suit[])Enum.GetValues(typeof(Suit)));
            var values = ImmutableArray<Value>.Empty.AddRange((Value[])Enum.GetValues(typeof(Value)));

            return suits
                .SelectMany(x => values.Select(y => (x, y)))
                .ToImmutableList();                
        }
    }
}
