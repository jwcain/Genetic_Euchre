using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : IEnumerable {

	/// <summary>
	/// The position the deck is considered to be for animation purposes.
	/// </summary>
	public Vector3 basePosition = Vector3.zero;

	/// <summary>
	/// Card-sprites within the deck will max out at this ordering depth
	/// </summary>
	public int orderingBase = 0;

	[SerializeField]
	private List<Card> cards = new List<Card>();

	public int Count => cards.Count;


	public bool Contains(Card c) {
		return cards.Contains(c);
	}


	/// <summary>
	/// Shuffles the decks with the Fisher-Yates shuffle
	/// </summary>
	public void Shuffle() {
		int n = cards.Count;
		while (n > 1) {
			n--;
			int k = Random.Range(0, n+1);
			Card value = cards[k];
			cards[k] = cards[n];
			cards[n] = value;
		}
	}

	/// <summary>
	/// Turns all of the cards in the deck towards the specific location and returns cards to the base position
	/// </summary>
	/// <param name="playerNameOrNull"></param>
	/// <returns></returns>
	public IEnumerator Orient(string playerNameOrNull) {
		foreach (Card card in cards) {
			card.StartCoroutine(GameManager.cardAnimator.Orient(card, playerNameOrNull, GameManager.AnimateGame));
			card.StartCoroutine(GameManager.cardAnimator.FlyTo(basePosition, card, GameManager.AnimateGame));
		}
		//Wait for all the cards to finish animating
		foreach (Card card in cards) {
			while (card.animating)
				yield return null;
		}
	}

	/// <summary>
	/// Takes amt cards from the top of the deck and returns them as an array
	/// </summary>
	/// <param name="amt"></param>
	/// <returns></returns>
	public Card[] Draw(int amt) {
		Card[] t = new Card[amt];
		for (int i = 0; i < amt; i++) {
			//Pop off the 0th position every time.
			t[i] = cards[0];
			cards.RemoveAt(0);
			t[i].SetOrdering(orderingBase+1);
		}
		return t;
	}

	/// <summary>
	/// Places an array of cards in the deck
	/// </summary>
	/// <param name="cards"></param>
	/// <param name="index"></param>
	public void Place(Card[] cards, int index = 0) {
		this.cards.InsertRange(index, cards);
	
		AssertOrdering();
	}

	/// <summary>
	/// More aggressive form of orientation, this is effectively a full reset on cards within the deck.
	/// </summary>
	/// <param name="animated"></param>
	public void EnforceCardLocationAndOrientation(bool animated) {
		basePosition.z = 0;
		AssertOrdering();
		if (animated)
			for (int i = 0; i < cards.Count; i++) {
				cards[i].StartCoroutine(GameManager.cardAnimator.FlyTo(basePosition, cards[i], GameManager.AnimateGame));
				if (cards[i].faceDown == false)
					cards[i].StartCoroutine(GameManager.cardAnimator.Flip(cards[i], GameManager.AnimateGame));
				cards[i].StartCoroutine(GameManager.cardAnimator.Orient(cards[i], null, GameManager.AnimateGame));
			}
		else
			for (int i = 0; i < cards.Count; i++) {
				cards[i].transform.position = basePosition;
				cards[i].transform.rotation = Quaternion.Euler(0.0f, cards[i].transform.rotation.eulerAngles.y, 0.0f);
			}
	}

	/// <summary>
	/// Assigns a sprite ordering on cards based on their position in the deck
	/// </summary>
	public void AssertOrdering() {
		for (int i = 0; i < cards.Count; i++) {
			cards[i].SetOrdering(orderingBase - i);
		}
	}

	public IEnumerator GetEnumerator() {
		return ((IEnumerable)this.cards).GetEnumerator();
	}
}
