using System;
using System.Collections.Generic;
using System.Text;

namespace oop_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            for (int i = 0; i < 1000; i++)
            {
                Deck d = new Deck();
                while (d.Count > 0)
                {
                    string deck = d.ShowDeck();
                    d.Shuffle();
                    d.Deal();
                    // Count variable is to make sure ShowDeck() is not optimised away
                    count += deck.Length;
                }
            }
            System.Console.WriteLine(count);
        }
    }

    public enum Value { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public enum Suit { Diamonds, Spades, Hearts, Clubs }

    public class Card
    {
        Suit suit { get; }
        Value value { get; }
        public Card(Suit s, Value v) => (suit, value) = (s,v);
        public override string ToString() => $"{value} of {suit}";
    }

    public class Deck
    {
        List<Card> deck = new List<Card>();
        Random random { get; set; }
        public Deck()
        {
            random = new Random();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Value v in Enum.GetValues(typeof(Value)))
                    deck.Add(new Card(s, v));
        }
        public int Count
        {
            get => deck.Count;
            set { }
        }
        public void Shuffle()
        {
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
