using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public bool isHuman = false;
	public bool playingRound = true;
	private List<Card> hand = new List<Card>();


	public void AddCard(Card c) {
		c.owner = this;
		hand.Add(c);
	}

	public void RemoveCard(Card c) {
		c.owner = null;
		hand.Remove(c);
	}

	public List<Card> GetHand() {
		return hand;
	}

	public void ResetHand() {
		hand = new List<Card>();
	}

	public void AssertHandSpriteOrdering() {
		for (int i = 0; i < hand.Count; i++) {
			hand[i].SetOrdering(3+i);
		}
	}
}
