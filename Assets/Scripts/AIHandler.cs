using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hand = System.Collections.Generic.List<Card>;
public class AIHandler {


	private const bool debugLog = false;

	public static int  ScoreingValue(Card c, StateMachineSystem.StateMachine targetMachine) {
		Card.Suit trump = targetMachine.Memory.GetData<Card.Suit>("Trump");
		Card lead = targetMachine.Memory.GetData<List<Card>>("Plays")[0];
		//Intial value is as printed on the card
		int val = (int)c.value;
		//If on suit, add value
		if (c.suit == lead.suit) val += 20;
		//If on trump, add value
		if (c.suit == trump) val += 50;
		//Override on-color Jack
		if (c.suit == trump.SameColorSuit() && c.value == Card.Value.Jack) val = 200;
		//Override on-trump Jack
		if (c.suit == trump && c.value == Card.Value.Jack) val = 250;
		return val;
	}


	public static System.Func<Hand, Hand> CloneListOfCards = (Hand input) => { Hand cloned = new List<Card>(); foreach (var c in input) cloned.Add(c); return cloned; };

	private static int CardValue(Card c, Card.Suit trump, PointSpread pointSpread) {
		int value = 0;
		if (c.suit == trump) {
			//On suit cards
			switch (c.value) {
				case Card.Value.Nine:
					value = pointSpread.Nine;
					break;
				case Card.Value.Ten:
					value = pointSpread.Ten;
					break;
				case Card.Value.Queen:
					value = pointSpread.Queen;
					break;
				case Card.Value.King:
					value = pointSpread.King;
					break;
				case Card.Value.Ace:
					value = pointSpread.Ace;
					break;
				case Card.Value.Jack:
					value = pointSpread.RightBower;
					break;
			}
		}
		else {
			//Off suit ace
			if (c.value == Card.Value.Ace)
				value = pointSpread.OffSuitAce;
			//Left Bower
			if (c.value == Card.Value.Jack && c.suit == trump.SameColorSuit())
				value = pointSpread.Ace;
		}
		//Debug.Log("C:" + c.Shortname() + " T:" + trump + " P: " + value);
		//Default to worth of 0
		return value;
	}

	public static bool isValidPlay(Card c, Player p, StateMachineSystem.StateMachine targetMachine) {
		Card.Suit trump = targetMachine.Memory.GetData<Card.Suit>("Trump");
		List<Card> plays = targetMachine.Memory.GetData<List<Card>>("Plays");

		if (plays.Count > 0) {
			bool hasLeadSuitInHand = false;

			//Figure out the lead suit
			Card.Suit leadSuit = plays[0].suit;
			//overide the lead suit for offsuit Trump jack
			if (plays[0].value == Card.Value.Jack && plays[0].suit == trump.SameColorSuit())
				leadSuit = trump;

			//Figure out the suit of what we playes
			Card.Suit playedSuit = c.suit;
			//Override the played suit for offsuit trump jack
			if (c.value == Card.Value.Jack && c.suit == trump.SameColorSuit())
				playedSuit = trump;

			//Check if the hand has a card that is valid suit
			foreach (Card card in p.GetHand()) {
				Card.Suit cardsSuit = card.suit;
				//Overrride the offsuit Trump jack
				if (card.value == Card.Value.Jack && card.suit == trump.SameColorSuit())
					cardsSuit = trump;

				if (cardsSuit == leadSuit)
					hasLeadSuitInHand = true;
			}

			//If they have an on suit play at the played card isn't, invalidate the play
			if (hasLeadSuitInHand && playedSuit != leadSuit)
				return false;
			else
				return true;
		}
		else
			return true;
	}

	private static int CalculateHandPoints(List<Card> hand, Card upCard, Card.Suit suitInQuestion, bool dealerOnTeam, PointSpread pointSpread) {

		int ret = 0;
		//Calulate the face up card going to a player
		ret += CardValue(upCard, suitInQuestion, pointSpread) * ((dealerOnTeam) ? 1 : -1 );
		List<Card.Suit> uniqueSuits = new List<Card.Suit>();
		foreach (Card card in hand) {
			if (uniqueSuits.Contains(card.suit) == false)
				uniqueSuits.Add(card.suit);
			ret += CardValue(card, suitInQuestion, pointSpread);
		}

		if (uniqueSuits.Count <= 2)
			ret += pointSpread.TwoSuited;

		{
			string debugPrint = "Suit: " + suitInQuestion.Shortname() + " Points: " + ret + "\n Hand:";
			foreach (Card card in hand) {
				debugPrint += card.Shortname();
				debugPrint += ",";
			}
			//Debug.Log(debugPrint);
		}


		return ret;
	}

	public static IEnumerator MakeTrumpOrderingDecision(int playerID, PointSpread pointSpread, StateMachineSystem.StateMachine targetMachine) {
		//Default to passing
		bool pass = true;
		Player player = targetMachine.Memory.GetData<Player>("Player"+playerID);
		Card revealedCard = targetMachine.Memory.GetData<Card>("RevealedCardFromKittie");
		Card.Suit? callSuit = (revealedCard.faceDown) ? null : (Card.Suit?)revealedCard.suit;
		int dealerID = targetMachine.Memory.GetData<int>("Dealer");
		bool dealerOnTeam = (dealerID % 2) == (playerID % 2);
		//Debug.Log("Player" + playerID+" team w/ Dealer: " + dealerOnTeam);


		if (callSuit != null) {
			//Handle deciding on just this card
			int points = CalculateHandPoints(player.GetHand(), revealedCard, (Card.Suit)callSuit, dealerOnTeam, pointSpread);
			//Debug.Log("Player" + playerID + " Points: " + points);
			if (points >= pointSpread.callThreshold)
				pass = false;
			//Debug.Log("Player" + playerID + ": " + points + " for " + ((Card.Suit)callSuit).Shortname());
			//Call a loner if we are confident in our hand, unless we have more than seven points
			if (points >= pointSpread.lonerThreshold && targetMachine.Memory.GetData<int>("Team"+ (playerID % 2) + "Points") <= 7) {
				//If we are the dealer, calling loner is fine. If we are not the dealer, and it is the right bower, don't go alone.
				if (dealerID == playerID || revealedCard.value != Card.Value.Jack)
					targetMachine.Memory.GetData<TrumpSelector>("TrumpSelector").aloneToggle.isOn = true;
			}

		}
		else {
			//If screw-the-dealer is enabled, disable passing
			if (GameManager.ScrewTheDealer && dealerID == playerID)
				pass = false;

			List<Card.Suit> suits = new List<Card.Suit>(new Card.Suit[] { Card.Suit.Clubs, Card.Suit.Diamonds, Card.Suit.Hearts, Card.Suit.Spades });
			suits.Remove(revealedCard.suit);
			int maxPoints = int.MinValue;
			foreach (Card.Suit suit in suits) {
				int points = CalculateHandPoints(player.GetHand(), revealedCard, suit, dealerOnTeam, pointSpread);
				if (points > maxPoints) {
					maxPoints = points;
					callSuit = suit;
				}
			}
			if (maxPoints >= pointSpread.callThreshold)
				pass = false;

			//If this is a strong call, go alone
			if (maxPoints >= 9 && targetMachine.Memory.GetData<int>("Team" + (playerID % 2) + "Points") <= 7)
				targetMachine.Memory.GetData<TrumpSelector>("TrumpSelector").aloneToggle.isOn = true;

				//Debug.Log("Player" + playerID + ": " + maxPoints + " for " + ((Card.Suit)callSuit).Shortname());
			}

		yield return null;

		if (pass)
			player.PostNotification("Pass" + targetMachine.UID);
		else
			player.PostNotification("OrderUp" + targetMachine.UID, (Card.Suit)callSuit);
	}


	public static IEnumerator MakeTrumpDiscardDecision(int playerID, PointSpread pointSpread, StateMachineSystem.StateMachine targetMachine) {
		//Discard the lowest value card or a card that makes the hand two suited (aside from a trump)
		yield return null;
		Player p = targetMachine.Memory.GetData<Player>("Player" + playerID);
		Card.Suit trump = targetMachine.Memory.GetData<Card.Suit>("Trump");
		Card discard = null;
		Hand cardsRemaining = CloneListOfCards(p.GetHand());

		System.Func<Hand> SortByLowestValue = () => {
			Hand temp = CloneListOfCards(cardsRemaining);
			temp.Sort((Card x, Card y) => {
				if (CardValue(x, trump, pointSpread) < CardValue(y, trump, pointSpread))
					return -1;
				else
					return 1;
			});
			return temp;
		};

		System.Func<Hand> RemoveTrump = () => {
			Hand temp = CloneListOfCards(cardsRemaining);
			temp.RemoveAll((Card c) => { return c.suit == trump; });
			return temp;
		};
		System.Func<Hand> RemoveAces = () => {
			Hand temp = CloneListOfCards(cardsRemaining);
			temp.RemoveAll((Card c) => { return c.value == Card.Value.Ace; });
			return temp;
		};
		System.Func<Hand> LowerSuitCount = () => {
			Hand temp = CloneListOfCards(cardsRemaining);
			Dictionary<Card.Suit, int> suitCount = new Dictionary<Card.Suit, int>();
			foreach (Card card in temp) {
				if (suitCount.ContainsKey(card.suit) == false)
					suitCount.Add(card.suit, 0);
				suitCount[card.suit]++;
			}
			int lowestCount = int.MaxValue;
			Card.Suit fewestSuit = Card.Suit.Clubs;
			foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit))) {
				if (suitCount.ContainsKey(suit) && suitCount[suit] > 0 && suitCount[suit] < lowestCount)
					fewestSuit = suit;
			}
			//Don't remove anything if it is our only suit
			if (suitCount[fewestSuit] != temp.Count)
				//Otherwise, remove all suits that do not match the smallest one.
				temp.RemoveAll((Card c) => { return c.suit != fewestSuit; });

			return temp;
		};

		System.Func<Hand>[] actions = new System.Func<Hand>[] { RemoveTrump, RemoveAces, LowerSuitCount, SortByLowestValue };
		//Attempt to try all of these actions, and if they leave us with 
		for (int i = 0; i < actions.Length; i++) {
			if (discard != null) break;
			//Perform a reduction on the hand
			Hand newPerspectives = actions[i]();
			//Check what that leaves us with
			if (newPerspectives.Count == 1) {
				//If this criteria left us with only one card, we discard that.
				discard = newPerspectives[0];
			}
			else if (newPerspectives.Count == 0) {
				//If this action resulted in an empty list, it is not a valid criteria and we need to use our most recent intact version, sort it, and pick the lowest value from it.
				discard = SortByLowestValue()[0];
			}
			else {
				//Otherwise, update the remaining cards after this reduction
				cardsRemaining = newPerspectives;
			}
		}
		if (discard == null)
			discard = cardsRemaining[0];

		p.PostNotification("CardPlayedInZone" + targetMachine.UID, new object[] { GameObject.FindGameObjectWithTag(GameManager.Tags.PlayZone), discard });
	}


	public static IEnumerator MakePlayDecision(int playerID, PointSpread pointSpread, StateMachineSystem.StateMachine targetMachine) {
		int trumpCaller = targetMachine.Memory.GetData<int>("TrumpCaller");
		Player player = targetMachine.Memory.GetData<Player>("Player"+ playerID);
		Card.Suit trump = targetMachine.Memory.GetData<Card.Suit>("Trump");
		yield return null;
		Hand handViewframe = CloneListOfCards(player.GetHand());
		//Remove all legal plays
		handViewframe.RemoveAll((Card c) => { return !isValidPlay(c, player, targetMachine); });
		Card playCard = null;
		List<Card> plays = targetMachine.Memory.GetData<List<Card>>("Plays");
		//Calculate this player's team is winning

		//Debug.LogWarning("Player" + playerID);



		if (plays.Count == 0) {
			//Debug.Log(handViewframe.Count);

			Hand trumpHand = CloneListOfCards(handViewframe);
			trumpHand.RemoveAll((Card c) => { return c.suit != trump; });
			trumpHand.Sort((Card x, Card y) => {
				if (CardValue(x, trump, pointSpread) < CardValue(y, trump, pointSpread))
					return -1;
				else
					return 1;
			});
			Dictionary<Card.Suit, int> singletonSuits = new Dictionary<Card.Suit, int>();
			foreach (Card card in player.GetHand()) {
				if (singletonSuits.ContainsKey(card.suit) == false)
					singletonSuits.Add(card.suit, 0);
				singletonSuits[card.suit]++;
			}
			//Limit suit count to only those that have one in them
			foreach (Card.Suit item in System.Enum.GetValues(typeof(Card.Suit))) {
				if (singletonSuits.ContainsKey(item) && singletonSuits[item] > 1)
					singletonSuits.Remove(item);
			}

			//If we have 3+ trump in hand, lead with our strongest trump
			if (playCard == null) {
				if (trumpHand.Count >= 3) {
					//Debug.Log("Leading Strong Trump");
					playCard = trumpHand[trumpHand.Count - 1];
				}
			}

			//If we have just an Ace of an OffSuit, play that
			if (playCard == null) {
				foreach (Card.Suit item in singletonSuits.Keys) {
					foreach (Card card in player.GetHand()) {
						if (card.suit == item && card.value == Card.Value.Ace) {
							//Debug.Log("Leading offsuit Ace");
							playCard = card;
							break;
						}
					}
					if (playCard != null)
						break;
				}
			}

			//If we have the right bower, play that if its our only trump
			if (playCard == null && trumpHand.Count == 1) {
				playCard = handViewframe.Find((Card c) => { return c.suit == trump && c.value == Card.Value.Jack; });
				//if (playCard != null) Debug.Log("Leading Singleton Right Bower");
			}

			//If we have a singleton card, play that.
			if (playCard == null) {
				foreach (Card.Suit item in singletonSuits.Keys) {
					foreach (Card card in player.GetHand()) {
						if (card.suit == item) {
							//Debug.Log("Leading Singleton");
							playCard = card;
							break;
						}
					}
					if (playCard != null)
						break;
				}
			}

			//Play our lowest value card.
			if (playCard == null) {
				//Debug.Log("Leading Lowest Card");
				handViewframe.Sort((Card x, Card y) => { return (CardValue(x, trump, pointSpread) < CardValue(y, trump, pointSpread)) ? -1 : 1; });
				playCard = handViewframe[0];
			}

			//Pick a random card
			if (playCard == null)
				playCard = handViewframe[Random.Range(0, handViewframe.Count)];
		}
		else if (handViewframe.Count == 1) {
			//Debug.Log("Only one valid play");
			//If there is only one card, play it
			playCard = handViewframe[0];
		}
		else {
			bool teamWinningHand;
			
			List<Card> sortedPlays = CloneListOfCards(plays);
			sortedPlays.Sort((Card x, Card y) => {
				if (ScoreingValue(x, targetMachine) < ScoreingValue(y, targetMachine))
					return -1;
				else
					return 1;
			});
			//Make it so the best card is actually in the first position
			sortedPlays.Reverse();
			{
				string ret = "SortedPlays: ";
				foreach (Card item in sortedPlays) {
					ret += item.Shortname() + ", ";
				}
				//Debug.Log(ret);
			}
			int currentWinner = targetMachine.Memory.GetData<Dictionary<Card, int>>("PlaysMap")[sortedPlays[0]];
			teamWinningHand = currentWinner % 2 == playerID % 2;
			

			System.Func<Card, bool> isWinningPlay = (Card c) => {
				Card winningCard = sortedPlays[0];
				if (ScoreingValue(c, targetMachine) > ScoreingValue(winningCard, targetMachine))
					return true;
				else
					return false;
			};

			//Calcualte all cards that can win a play
			Hand winningPlays = CloneListOfCards(handViewframe);
			//Remove things that are not winning plays
			winningPlays.RemoveAll((Card c) => { return !isWinningPlay(c); });
			//Sort them by CardValue
			winningPlays.Sort((Card x, Card y) => { return (CardValue(x, trump, pointSpread) < CardValue(y, trump, pointSpread)) ? -1 : 1; });
			//Calculate all cards that can lose a play
			Hand losingPlays = CloneListOfCards(handViewframe);
			//Remove things that are winning plays
			losingPlays.RemoveAll((Card c) => { return isWinningPlay(c); });
			//Sort them by value
			losingPlays.Sort((Card x, Card y) => { return (CardValue(x, trump, pointSpread) < CardValue(y, trump, pointSpread)) ? -1 : 1; });

			{
				string ret = "Losers: ";
				foreach (Card card in losingPlays)
					ret += card.Shortname() + ", ";
				ret += "\nWinners:";
				foreach (Card card in winningPlays)
					ret += card.Shortname() + ", ";
				//Debug.Log(ret);
			}

			//If we have no winning plays, or we are already winning
			if ((winningPlays.Count == 0 || teamWinningHand) && losingPlays.Count > 0) {
				//Debug.Log("Playing worst card:(" + (winningPlays.Count == 0) + " || " + teamWinningHand + ") && " + (losingPlays.Count > 0) );
				//Play the lowest value losing play
				playCard = losingPlays[0];
			}
			//If we have no losing plays, or we are not winning
			else if ((losingPlays.Count == 0 || teamWinningHand == false) && winningPlays.Count > 0) {
				//Debug.Log("Playing best card:(" + (losingPlays.Count == 0) + " || " + (teamWinningHand == false) + ") && " + (winningPlays.Count > 0));
				//Play the lowest value winning play
				playCard = winningPlays[0];
			}
		}

		//do { playCard = player.GetHand()[Random.Range(0, player.GetHand().Count)]; } while (isValidPlay(playCard, player) == false);
		player.PostNotification("CardPlayedInZone" + targetMachine.UID, new object[] { GameObject.FindGameObjectWithTag(GameManager.Tags.PlayZone), playCard });
	}
}
