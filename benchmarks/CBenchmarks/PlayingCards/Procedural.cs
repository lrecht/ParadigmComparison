using System;
using BenchmarkInterface;

namespace CBenchmarks.PlayingCards
{
    public class Procedural : IBenchmark
    {
        public void Preprocess() { }
        public int Run()
        {
            int count = 0;
            for (int i = 0; i < 1000; i++)
            {
                createDeck();
                while (deckCount > 0)
                {
                    string deck = showDeck();
                    shuffle();
                    dealCard();
                    // Count variable is to make sure ShowDeck() is not optimised away
                    count += deck.Length;
                }
            }
            return count;
        }

        string[] suits = { "Diamonds", "Spades", "Hearts", "Clubs" };
        string[] values = { "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "Ace" };
        string[] deck = new string[52];
        int deckCount = 0;

        void createDeck()
        {
            int i = 0;
            foreach (string suit in suits)
                foreach (string value in values)
                    deck[i++] = cardString(value, suit);
            deckCount = deck.Length;
        }

        string cardString(string value, string suit) => $"{value} of {suit}";

        string dealCard()
        {
            string card = deck[deckCount - 1];
            deckCount--;
            return card;
        }

        void shuffle()
        {
            var random = new Random();
            for (int i = 0; i < deckCount; i++)
            {
                int r = random.Next(i, deckCount);
                var temp = deck[i];
                deck[i] = deck[r];
                deck[r] = temp;
            }
        }

        string showDeck()
        {
            string deckString = "";
            for (int i = 0; i < deckCount; i++)
                deckString += '\n' + deck[i];
            return deckString;
        }

    }
}