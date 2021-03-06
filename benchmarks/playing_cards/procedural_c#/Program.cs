﻿using System;
using benchmark;

namespace procedural_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            var iterations = args.Length > 0 ? int.Parse(args[0]) : 1;
			var bm = new Benchmark(iterations);

			bm.Run(() => {
				PlayingCards p = new PlayingCards();
				return p.Start();
			}, (res) => {
				System.Console.WriteLine(res);
			});
        }
    }

    public class PlayingCards
    {
        string[] suits = { "Diamonds", "Spades", "Hearts", "Clubs" };
        string[] values = { "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "Ace" };
        string[] deck = new string[52];
        int deckCount = 0;

        public PlayingCards(){}

		public int Start(){
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
            var random = new Random(2);
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
