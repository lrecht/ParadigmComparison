using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using BenchmarkInterface;

namespace CBenchmarks.PlayingCards
{
    public class Functional : IBenchmark
    {
        public void Preprocess() { } //Nothing to Preprocess

        public int Run()
        {
            return performPlayingCards(1000, 0);
        }

        private enum Suit { Diamonds, Spades, Hearts, Clubs }
        private enum Value { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

        private int performPlayingCards(int runs, int count)
        {
            if (runs < 1)
                return count;

            return performPlayingCards(runs - 1, playingCardsOnDeck(getNewDeck(), count));
        }

        private int playingCardsOnDeck(ImmutableList<(Suit, Value)> deck, int count)
        {
            if (deck.Count < 1)
                return count;

            var deckStrSize = showDeck(deck).Count();
            var shuffledDeck = shuffleDeck(deck, new Random());
            var dealedDeck = shuffledDeck.Remove(shuffledDeck.Last());

            return playingCardsOnDeck(dealedDeck, deckStrSize + count);
        }


        private string showDeck(ImmutableList<(Suit, Value)> deck) =>
            string.Join("\n", deck.Select(x => $"{x.Item2} of {x.Item1}"));

        private ImmutableList<(Suit, Value)> shuffleDeck(ImmutableList<(Suit, Value)> deck, Random rng) =>
            deck.OrderBy(x => rng.Next()).ToImmutableList();


        private ImmutableList<(Suit, Value)> getNewDeck()
        {
            var suits = ImmutableList<Suit>.Empty.AddRange((Suit[])Enum.GetValues(typeof(Suit)));
            var values = ImmutableArray<Value>.Empty.AddRange((Value[])Enum.GetValues(typeof(Value)));

            return suits
                .SelectMany(x => values.Select(y => (x, y)))
                .ToImmutableList();
        }
    }
}