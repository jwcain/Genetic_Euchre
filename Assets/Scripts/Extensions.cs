using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions  {
	
	public static Card.Suit SameColorSuit(this Card.Suit s) {
		switch (s) {
			case Card.Suit.Hearts:
				return Card.Suit.Diamonds;
			case Card.Suit.Diamonds:
				return Card.Suit.Hearts;
			case Card.Suit.Spades:
				return Card.Suit.Clubs;
			case Card.Suit.Clubs:
				return Card.Suit.Spades;



			default:
				return Card.Suit.Clubs;
		}
	}
	public static string Shortname(this Card.Suit s) {
		switch (s) {
			case Card.Suit.Hearts:
				return "H";
			case Card.Suit.Diamonds:
				return "D";
			case Card.Suit.Spades:
				return "S";
			case Card.Suit.Clubs:
				return "C";
			default:
				return "?";
		}
	}


	public static string Shortname(this Card.Value s) {
		switch (s) {
			case Card.Value.Two:
				return "2";
			case Card.Value.Three:
				return "3";
			case Card.Value.Four:
				return "4";
			case Card.Value.Five:
				return "5";
			case Card.Value.Six:
				return "6";
			case Card.Value.Seven:
				return "7";
			case Card.Value.Eight:
				return "8";
			case Card.Value.Nine:
				return "9";
			case Card.Value.Ten:
				return "10";
			case Card.Value.Jack:
				return "J";
			case Card.Value.Queen:
				return "Q";
			case Card.Value.King:
				return "K";
			case Card.Value.Ace:
				return "A";
			default:
				return "!";
		}
	}
}
