using System;
using System.Collections.Generic;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            Deck d = new Deck();
            while(d.Count() > 0)
            {
                string deck = d.ShowDeck();
                d.Shuffle();
                d.Deal();
                deck = d.ShowDeck();
            }
        }
    }

    public enum Value { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public enum Suit { Diamonds, Spades, Hearts, Clubs }

    public class Card
    {
        Suit suit { get; }
        Value value { get; }
        public Card(Suit s, Value v)
        {
            suit = s;
            value = v;
        }

        public override string ToString() => $"{value} of {suit}";
    }

    public class Deck
    {
        List<Card> deck = new List<Card>();
        public Deck()
        {
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Value v in Enum.GetValues(typeof(Value)))
                    deck.Add(new Card(s, v));
        }
        public int Count() => deck.Count;
        public void Shuffle()
        {
            var random = new Random();
            for (int i = 0; i < deck.Count; i++)
            {
                int r = random.Next(i, deck.Count);
                var temp = deck[i];
                deck[i] = deck[r];
                deck[r] = temp;
            }
        }
        public Card Deal()
        {
            int last = deck.Count - 1;
            Card card = deck[last];
            deck.RemoveAt(last);
            return card;
        }

        public string ShowDeck() => String.Join('\n', deck);
    }
}