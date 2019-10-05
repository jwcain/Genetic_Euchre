using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachineSystem.EuchreStates {
	public class SetupGame : State {
		public override IEnumerator Enter() {
			yield return base.Enter();

			//Reset GameManager's Memory
			owner.Memory.Reset();

			owner.Memory.SetData<float>("StartTime", Time.time);

			//Show 0/0 on the scoreboard
			GameManager.Scoreboard.UpdateScore(0, 0);
			GameManager.Scoreboard.UpdateScore(1, 0);


			//Create a trump selector
			owner.Memory.SetData("TrumpSelector", GameManager.GetTrumpSelector());
			//Hide it
			owner.Memory.GetData<TrumpSelector>("TrumpSelector").gameObject.SetActive(false);
			//Default to not going alone
			owner.Memory.GetData<TrumpSelector>("TrumpSelector").aloneToggle.isOn = false;
			//Get hand positions for 4 players
			Vector3[] positions = GameManager.GetHandLocations(4);

			//Spawn player objects and add them to memory
			for (int i = 0; i < 4; i++) {
				Player p = GameManager.SpawnPlayerPrefab();
				p.gameObject.name = "Player" + i;
				owner.Memory.SetData("Player" + i, p);
				p.gameObject.transform.position = new Vector3(positions[3 - i].x, positions[3 - i].y, 10.0f);
				p.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, positions[3 - i].z);
			}

			//Set the first player as a human
			if (GameManager.HumanPlayer)
				owner.Memory.GetData<Player>("Player0").isHuman = true;

			//Set the scores to 0 all
			owner.Memory.SetData("Team0Score", 0);
			owner.Memory.SetData("Team1Score", 0);

			PointSpread testingPointSpread;
			if (GameManager.RunGenetics) {
				int[] array = null;
				yield return GameManager.GeneticHandler.GetSpread((int[] inn) => { array = inn; });
				testingPointSpread = new PointSpread(array);
			}
			else {
				testingPointSpread = GameManager.DefaultPointSpread;
			}
			//if (GameManager.RunGenetics) Debug.LogWarning("Trial of: " + testingPointSpread);
			owner.Memory.SetData("Player0PointSpread", testingPointSpread);
			owner.Memory.SetData("Player1PointSpread", GameManager.DefaultPointSpread);
			owner.Memory.SetData("Player2PointSpread", testingPointSpread);
			owner.Memory.SetData("Player3PointSpread", GameManager.DefaultPointSpread);

			//Setup holders for trick indicators
			owner.Memory.SetData("Player0TrickIndicators", new List<GameObject>());
			owner.Memory.SetData("Player1TrickIndicators", new List<GameObject>());
			owner.Memory.SetData("Player2TrickIndicators", new List<GameObject>());
			owner.Memory.SetData("Player3TrickIndicators", new List<GameObject>());

			// Pick a random starting dealer
			owner.Memory.SetData("Dealer", Random.Range(0, 4));

			//Generate Deck.
			if (owner.Memory.HasKey<Deck>("GameDeck") == false) {
				Deck playingDeck = new Deck();
				for (int i = 0; i < GameManager.DeckAsset.cards.Length; i++) {
					playingDeck.Place(new Card[] { GameManager.SpawnCardPrefab(GameManager.DeckAsset.cards[i]) });
				}
				playingDeck.EnforceCardLocationAndOrientation(false, owner);
				//Add the deck to memory
				owner.Memory.SetData("GameDeck", playingDeck);
			}

			owner.Transition<SetupHand>();
		}
	}

	public class SetupHand : State {
		public override IEnumerator Enter() {
			yield return base.Enter();

			//Ensure no cards are animating
			if (GameManager.AnimateGame)
				foreach (Card card in owner.Memory.GetData<Deck>("GameDeck"))
					while (card.animating)
						yield return null;

			//Advance the Dealer
			owner.Memory.SetData("Dealer", (owner.Memory.GetData<int>("Dealer") == 3) ? 0 : owner.Memory.GetData<int>("Dealer") + 1);

			//Set active player to right of the dealer.
			owner.Memory.SetData("ActivePlayer", (owner.Memory.GetData<int>("Dealer") == 3) ? 0 : owner.Memory.GetData<int>("Dealer") + 1);

			owner.Memory.SetData("Plays", new List<Card>());
			owner.Memory.SetData("PlaysMap", new Dictionary<Card, int>());

			owner.Memory.SetData("Player0Tricks", 0);
			owner.Memory.SetData("Player1Tricks", 0);
			owner.Memory.SetData("Player2Tricks", 0);
			owner.Memory.SetData("Player3Tricks", 0);


			owner.Memory.GetData<TrumpSelector>("TrumpSelector").aloneToggle.isOn = false;

			owner.Memory.SetData<bool>("Alone", false);
			//Remove trump if this is not our first round.
			owner.Memory.RemoveData<Card.Suit>("Trump");


			Deck playingDeck = owner.Memory.GetData<Deck>("GameDeck");
			playingDeck.basePosition = Vector3.Lerp(Vector3.zero, owner.Memory.GetData<Player>("Player" + owner.Memory.GetData<int>("Dealer")).gameObject.transform.position, 0.55f);

			if (GameManager.AnimateGame)
				//Reorient the deck to the dealer
				yield return playingDeck.Orient("Player" + owner.Memory.GetData<int>("Dealer"), owner);

			//Shuffle the deck
			playingDeck.Shuffle();


			//Deal to each player
			int[] dealSequence = new int[] { 3, 2, 3, 2,
											 2, 3, 2, 3 };
			int player = owner.Memory.GetData<int>("ActivePlayer");
			for (int i = 0; i < dealSequence.Length; i++) {
				//Mark the player as playing this round
				owner.Memory.GetData<Player>("Player" + player).playingRound = true;
				yield return GameManager.cardAnimator.Deal("Player" + player, "GameDeck", dealSequence[i], GameManager.AnimateGame, owner);
				player++;
				if (player > 3)
					player = 0;
			}

			Card revealedCard = owner.Memory.GetData<Deck>("GameDeck").Draw(1)[0];
			owner.Memory.SetData("RevealedCardFromKittie", revealedCard);
			owner.Memory.SetData<List<Card>>("KnownCards", new List<Card>(new Card[] { revealedCard }));
			revealedCard.SetOrdering(1);
			yield return GameManager.cardAnimator.Flip(revealedCard, GameManager.AnimateGame);
			owner.Transition<DetermineTrump>();
		}
	}

	public class DetermineTrump : State {

		protected override void AddListeners() {
			base.AddListeners();
			this.AddObserver(OrderUp, "OrderUp" + owner.UID);
			this.AddObserver(Pass, "Pass" + owner.UID);
		}

		protected override void RemoveListeners() {
			base.RemoveListeners();
			this.RemoveObserver(OrderUp, "OrderUp" + owner.UID);
			this.RemoveObserver(Pass, "Pass" + owner.UID);
		}

		public void OrderUp(object sender, object args) {
			Card.Suit suit = (Card.Suit)args;

			//Hide the trump selector
			owner.Memory.GetData<TrumpSelector>("TrumpSelector").gameObject.SetActive(false);

			//Assign trump
			owner.Memory.SetData("Trump", suit);

			//Remember who called trump
			owner.Memory.SetData<int>("TrumpCaller", owner.Memory.GetData<int>("ActivePlayer"));

			bool goingAlone = owner.Memory.GetData<TrumpSelector>("TrumpSelector").aloneToggle.isOn;
			owner.Memory.SetData("Alone", goingAlone);
			if (goingAlone) {
				int activePlayer = owner.Memory.GetData<int>("ActivePlayer");
				int teamMateID = ((activePlayer + 2 > 3) ? activePlayer - 2 : activePlayer + 2);
				//Debug.LogError("Player" + activePlayer + " is going alone, Player"+teamMateID +" sits out." );
				Player teamMate = owner.Memory.GetData<Player>("Player"+teamMateID);
				teamMate.playingRound = false;
				Deck deck = owner.Memory.GetData<Deck>("GameDeck");
				foreach (Card c in teamMate.GetHand()) {
					if (GameManager.ShowAllCards == false && c.faceDown == false)
						c.StartCoroutine(GameManager.cardAnimator.Flip(c, GameManager.AnimateGame));
					c.StartCoroutine(GameManager.cardAnimator.FlyTo(deck.basePosition, c, GameManager.AnimateGame, GameManager.PlayAudio));
					deck.Place(new Card[] { c }, deck.Count - 1);
				}
				teamMate.ResetHand();
			}
			//Leave the trump round
			owner.Transition<Play>();
		}

		public void Pass(object sender, object args) {
			//Debug.Log("PassCalled");
			//Check if we have made it back to the dealer
			if (owner.Memory.GetData<int>("ActivePlayer") == owner.Memory.GetData<int>("Dealer")) {
				Card revealedCard = owner.Memory.GetData<Card>("RevealedCardFromKittie");
				//Check if thius is the second time.
				if (revealedCard.faceDown) {
					Deck deck = owner.Memory.GetData<Deck>("GameDeck");
					//Shuffle everything back together and transition to start hand
					//Return the revealed card back to the deck.
					deck.Place(new Card[] { revealedCard });
					Player[] players = new Player[] { owner.Memory.GetData<Player>("Player0"), owner.Memory.GetData<Player>("Player1"), owner.Memory.GetData<Player>("Player2"), owner.Memory.GetData<Player>("Player3") };
					foreach (Player p in players) {
						deck.Place(p.GetHand().ToArray());
						p.ResetHand();
					}
					deck.EnforceCardLocationAndOrientation(true, owner);

					//Go back to setup
					owner.Transition<SetupHand>();
				}
				else {
					//Turn the kitty card face down
					revealedCard.StartCoroutine(GameManager.cardAnimator.Flip(revealedCard, GameManager.AnimateGame));
					//Go back into another trump round.
					owner.Transition<DetermineTrump>();
				}
			}
			//Do not stack this one with the one from above. They are separate cases.
			else
				//Go back into another trump round.
				owner.Transition<DetermineTrump>();
		}

		IEnumerator HandleAISpeach(GameObject obj) {

			float t = 1.0f;
			float step = 1.0f / 100;
			while (t > 0) {
				t -= step;
				foreach (SpriteRenderer renderer in obj.GetComponentsInChildren<SpriteRenderer>()) {
					renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, t);
				}
				foreach (TMPro.TMP_Text text in obj.GetComponentsInChildren<TMPro.TMP_Text>()) {
					text.color = new Color(text.color.r, text.color.g, text.color.b, t);
				}

				yield return null;
			}

			yield return null;
			GameObject.Destroy(obj);
		}

		public override IEnumerator Enter() {
			yield return base.Enter();

			//Show the trump selector if the active player is a human player
			if (owner.Memory.GetData<Player>("Player" + owner.Memory.GetData<int>("ActivePlayer")).isHuman) {
				Card revealedCard = owner.Memory.GetData<Card>("RevealedCardFromKittie");
				if (GameManager.AnimateGame)
					//Ensure that other parts of the game are finished animating before we cary on
					while (revealedCard.animating) yield return null;
				//Show Trump Selector
				TrumpSelector tS = owner.Memory.GetData<TrumpSelector>("TrumpSelector");
				tS.gameObject.SetActive(true);

				Dictionary<Card.Suit, UnityEngine.UI.Button> buttons = new Dictionary<Card.Suit, UnityEngine.UI.Button>();
				buttons.Add(Card.Suit.Clubs, tS.clubs);
				buttons.Add(Card.Suit.Hearts, tS.hearts);
				buttons.Add(Card.Suit.Diamonds, tS.diamonds);
				buttons.Add(Card.Suit.Spades, tS.spades);

				//Hide the pass button if we are screwing the dealer
				if (GameManager.ScrewTheDealer && revealedCard.faceDown)
					tS.aloneToggle.gameObject.SetActive(false);
				else
					tS.aloneToggle.gameObject.SetActive(true);

				foreach (var suit in buttons.Keys) {
					if (revealedCard.faceDown) {
						//Only show the ones that do not match the turned down suit
						buttons[suit].interactable = (suit != revealedCard.suit);
					} else {
						//Set only the suit-button for the reveleaved card as showing
						buttons[suit].interactable = (suit == revealedCard.suit);
					}
				}
			} else {
				yield return AIHandler.MakeTrumpOrderingDecision(owner.Memory.GetData<int>("ActivePlayer"), owner.Memory.GetData<PointSpread>("Player" + owner.Memory.GetData<int>("ActivePlayer") + "PointSpread"), owner);
			}
		}

		public override IEnumerator Exit() {
			yield return base.Exit();

			//Hide the trump selector
			owner.Memory.GetData<TrumpSelector>("TrumpSelector").gameObject.SetActive(false);

			//If we are leaving the Trump round and have selected a trump, prepare regular play
			if (owner.Memory.HasKey<Card.Suit>("Trump")) {

				if (owner.Memory.GetData<Player>("Player" + owner.Memory.GetData<int>("ActivePlayer")).isHuman == false && GameManager.AnimateGame) {
					GameManager.AccessInstance().StartCoroutine(HandleAISpeach(GameManager.SpawnAIText(owner.Memory.GetData<int>("ActivePlayer"), 2, owner)));
					if (owner.Memory.GetData<bool>("Alone"))
						GameManager.AccessInstance().StartCoroutine(HandleAISpeach(GameManager.SpawnAIText(owner.Memory.GetData<int>("ActivePlayer"), 3, owner)));
				}

				//Organize AI and player's hands
				Player[] players = new Player[] { owner.Memory.GetData<Player>("Player0"), owner.Memory.GetData<Player>("Player1"), owner.Memory.GetData<Player>("Player2"), owner.Memory.GetData<Player>("Player3") };
				System.Action OrganizeHands = () => {
					foreach (Player p in players) {
						if (p.isHuman) {
							Card.Suit trump = owner.Memory.GetData<Card.Suit>("Trump");
							System.Func<Card, int> calcVal = (Card c) => {
								//Intial value is as printed on the card
								int val = (int)c.value;
								//Clump up same suits
								val += ((int)c.suit) * 30;
								//If on trump, add value
								if (c.suit == trump) val += 500;
								//Override on-color Jack
								if (c.suit == trump.SameColorSuit() && c.value == Card.Value.Jack) val = 99995;
								//Override on-trump Jack
								if (c.suit == trump && c.value == Card.Value.Jack) val = 99999;
								return val;
							};
							p.GetHand().Sort((Card x, Card y) => {
								if (calcVal(x) < calcVal(y))
									return -1;
								else
									return 1;
							});
							p.GetHand().Reverse();

						}
						else {
							System.Action<List<Card>> ShuffleTheList = (List<Card> shuffleList) => {
								int n = shuffleList.Count;
								while (n > 1) {
									n--;
									int k;
									//Shuffle, but don't let things stay in the same spot
									do {
										k = Random.Range(0, n + 1);
									} while (k == n);
									Card value = shuffleList[k];
									shuffleList[k] = shuffleList[n];
									shuffleList[n] = value;
								}
							};
							ShuffleTheList(p.GetHand());
						}
						//Animate the cards where they belong
						p.StartCoroutine(GameManager.cardAnimator.AdjustHand(p.gameObject.name, GameManager.AnimateGame, owner));
					}

				};


				//Create a stamp to track what trump is
				if (GameManager.AnimateGame)
					owner.Memory.SetData<GameObject>("TrumpIndicator", GameManager.SpawnTrumpIndicator(owner.Memory.GetData<Card.Suit>("Trump"), owner.Memory.GetData<int>("ActivePlayer") % 2));
				Deck d = owner.Memory.GetData<Deck>("GameDeck");

				//If the revealed card is still face up, have the dealer switch with it.			
				if (owner.Memory.GetData<Card>("RevealedCardFromKittie").faceDown == false &&
					//The following is some going alone logic. First check if someone went alone.
					//The specific case is when our partner calls a loner and we are the dealer so we need to skip doing the pickup
					//So we check for the specific case, and then compare it to false.
					//First we check for someone having gone alone. Then we check to make sure that was person was on our team, and then we check to see if it was not the dealer.
					(owner.Memory.GetData<bool>("Alone")
					&& owner.Memory.GetData<int>("TrumpCaller") % 2 == owner.Memory.GetData<int>("Dealer") % 2
					&& owner.Memory.GetData<int>("TrumpCaller") != owner.Memory.GetData<int>("Dealer")) == false) 
					{
					//Add the revealed card to the dealer's hand
					Card rCard = owner.Memory.GetData<Card>("RevealedCardFromKittie");
					string dealerName = "Player" + owner.Memory.GetData<int>("Dealer");
					Player dealer = owner.Memory.GetData<Player>(dealerName);
					dealer.AddCard(rCard);
					if (GameManager.AnimateGame)
						yield return GameManager.cardAnimator.Orient(rCard, dealerName, GameManager.AnimateGame, owner);

					//Hide the card
					if (GameManager.AnimateGame)
						if (dealer.isHuman == false && GameManager.ShowAllCards == false)
							yield return GameManager.cardAnimator.Flip(rCard, GameManager.AnimateGame);

					if (GameManager.AnimateGame)
						yield return GameManager.cardAnimator.AdjustHand(dealerName, GameManager.AnimateGame, owner);

					OrganizeHands();
					//Wait for the cards to orient
					if (GameManager.AnimateGame)
						foreach (Player player in players) {
							foreach (Card card in player.GetHand()) {
								while (card.animating)
									yield return null;
							}
						}


					//Add Listeners for Card interaction
					CardInteractionHandlers.EnableCardInteraction(this);
					if (dealer.isHuman)
						CardInteractionHandlers.CanPlayCard = true;

					bool lockout = true;
					System.Action<object, object> func = (object sender, object args) => {
						if (args == null)
							return;

						object[] trueArgs = (object[])args;
						GameObject zone = (GameObject)trueArgs[0];
						Card card = (Card)trueArgs[1];

						if (zone.tag == "PlayZone") {
							//Set the played card as the revealed card
							owner.Memory.SetData("RevealedCardFromKittie", card);

							//Remove the card from the dealers hand.
							dealer.RemoveCard(card);
							//Do exit visuals to signify the card has left hand control
							CardInteractionHandlers.CardMouseExit(this, card);

							//Adjust the hand visuals
							card.StartCoroutine(GameManager.cardAnimator.AdjustHand(dealerName, GameManager.AnimateGame, owner));

							//Advance the coroutine
							lockout = false;
						} else {
							card.StartCoroutine(GameManager.cardAnimator.FlyTo(card.goalPosition, card, GameManager.AnimateGame));
						}
					};
					this.AddObserver(func, "CardPlayedInZone" + owner.UID);
					//Run the AI
					if (owner.Memory.GetData<Player>(dealerName).isHuman == false) {
						yield return AIHandler.MakeTrumpDiscardDecision(owner.Memory.GetData<int>("Dealer"), owner.Memory.GetData<PointSpread>("Player" + owner.Memory.GetData<int>("ActivePlayer") + "PointSpread"), owner);
					}
					if (GameManager.AnimateGame)
						//Yield until the player plays one
						while (lockout) yield return null;
					//Remove listeners for card interaction
					CardInteractionHandlers.CanPlayCard = false;
					CardInteractionHandlers.DisableCardInteraction(this);
					this.RemoveObserver(func, "CardPlayedInZone" + owner.UID);

				}
				else {
					OrganizeHands();
					//Wait for the cards to orient
					if (GameManager.AnimateGame)
						foreach (Player player in players) {
							foreach (Card card in player.GetHand()) {
								while (card.animating)
									yield return null;
							}
						}
				}
				Card revealedCard = owner.Memory.GetData<Card>("RevealedCardFromKittie");

				//Flip the card over if needed
				if (revealedCard.faceDown == false)
					revealedCard.StartCoroutine(GameManager.cardAnimator.Flip(revealedCard, GameManager.AnimateGame));

				//Move card to the deck
				if (GameManager.AnimateGame)
					yield return GameManager.cardAnimator.FlyTo(d.basePosition, revealedCard, GameManager.AnimateGame);
				//Add revealed card back to deck
				d.Place(new Card[] { revealedCard });

				//Slide the deck these cards offscreen
				foreach (Card card in d) {
					card.SetOrdering(-3);
					card.StartCoroutine(GameManager.cardAnimator.FlyTo(Vector3.LerpUnclamped(Vector3.zero, owner.Memory.GetData<Player>("Player" + owner.Memory.GetData<int>("Dealer")).gameObject.transform.position, 3.4f), card, GameManager.AnimateGame));
				}

				//Reset active player to right of dealer
				owner.Memory.SetData("ActivePlayer", (owner.Memory.GetData<int>("Dealer") == 3) ? 0 : owner.Memory.GetData<int>("Dealer") + 1);

				//Game should be good to start, wait a moment before starting play
				if (GameManager.AnimateGame)
					yield return new WaitForSeconds(0.25f);

				//Enable regular card interaction
				CardInteractionHandlers.EnableCardInteraction(this);
			}
			else {
				if (owner.Memory.GetData<Player>("Player"+owner.Memory.GetData<int>("ActivePlayer")).isHuman == false && GameManager.AnimateGame) {
					GameManager.AccessInstance().StartCoroutine(HandleAISpeach(GameManager.SpawnAIText(owner.Memory.GetData<int>("ActivePlayer"), 1, owner)));
					if (GameManager.AnimateGame)
						yield return new WaitForSeconds(0.5f);
				}
				//Move on the active player
				owner.Memory.SetData("ActivePlayer", (owner.Memory.GetData<int>("ActivePlayer") == 3) ? 0 : owner.Memory.GetData<int>("ActivePlayer") + 1);
			}
		}
	}

	public class Play : State {
		public override IEnumerator Enter() {
			yield return base.Enter();
			string activePlayerName = "Player" + owner.Memory.GetData<int>("ActivePlayer");
			Player activePlayer = owner.Memory.GetData<Player>(activePlayerName);
			List<Card> plays = owner.Memory.GetData<List<Card>>("Plays");

			//Check if the active player is playing this hand
			if (activePlayer.playingRound == false) {
				owner.Transition<Play>();
			}
			//Check if we are done playing cards
			else if (plays.Count >= 4 || (owner.Memory.GetData<bool>("Alone") && plays.Count >= 3)) {
				//End the trick
				owner.Transition<EndTrick>();
			}
			else {
				bool lockout = true;
				Card playedCard = null;

				//Wait for the active player to make a play
				System.Action<object, object> func = (object sender, object args) => {
					if (args == null)
						return;

					object[] trueArgs = (object[])args;
					GameObject zone = (GameObject)trueArgs[0];
					playedCard = (Card)trueArgs[1];



					if (zone.tag == "PlayZone" && activePlayer == playedCard.owner && AIHandler.isValidPlay(playedCard, activePlayer, owner)) {
						owner.Memory.GetData<List<Card>>("KnownCards").Add(playedCard);

						//Remove the card from the players hand.
						activePlayer.RemoveCard(playedCard);

						//Ensure card let go visuals are done now
						CardInteractionHandlers.CardMouseExit(this, playedCard);

						//Track the played card
						plays.Add(playedCard);
						owner.Memory.GetData<Dictionary<Card, int>>("PlaysMap").Add(playedCard, owner.Memory.GetData<int>("ActivePlayer"));



					//Move it to a resting position
						if (activePlayer.isHuman == false)
							playedCard.StartCoroutine(GameManager.cardAnimator.FlyTo(Vector3.Lerp(Vector3.zero, activePlayer.gameObject.transform.position, 0.55f), playedCard, GameManager.AnimateGame, true));
						else
							FMODUnity.RuntimeManager.PlayOneShot("event:/cardPlace" + Random.Range(1, 4), playedCard.transform.position);

						//Flip the card up if it is face down
						if (playedCard.faceDown)
							playedCard.StartCoroutine(GameManager.cardAnimator.Flip(playedCard, GameManager.AnimateGame));

						//Adjust the hand visuals
						playedCard.StartCoroutine(GameManager.cardAnimator.AdjustHand(activePlayerName, GameManager.AnimateGame, owner));

						//Advance the coroutine
						lockout = false;
					} else {
						//Debug.Log("Invalid play, returning card");
						playedCard.StartCoroutine(GameManager.cardAnimator.FlyTo(playedCard.goalPosition, playedCard, GameManager.AnimateGame));
					}
				};


				this.AddObserver(func, "CardPlayedInZone" + owner.UID);
				if (activePlayer.isHuman)
					CardInteractionHandlers.CanPlayCard = true;

				if (activePlayer.isHuman == false) {
					if (GameManager.AnimateGame)
						yield return new WaitForSeconds(0.25f);
					yield return AIHandler.MakePlayDecision(owner.Memory.GetData<int>("ActivePlayer"), owner.Memory.GetData<PointSpread>("Player"+ owner.Memory.GetData<int>("ActivePlayer")+"PointSpread"), owner);
				}
				while (lockout) yield return null;
				CardInteractionHandlers.CanPlayCard = false;
				this.RemoveObserver(func, "CardPlayedInZone" + owner.UID);

				owner.Transition<Play>();

			}
			//Advance the active player
			owner.Memory.SetData("ActivePlayer", (owner.Memory.GetData<int>("ActivePlayer") == 3) ? 0 : owner.Memory.GetData<int>("ActivePlayer") + 1);
		}

		public override IEnumerator Exit() {
			yield return base.Exit();
		}

		protected override void AddListeners() {
			base.AddListeners();
		}

		protected override void RemoveListeners() {
			base.RemoveListeners();
		}
	}

	public class EndTrick : State {
		public override IEnumerator Enter() {
			yield return base.Enter();
			//Delay
			if (GameManager.AnimateGame)
				yield return new WaitForSeconds(1.0f);
			//Debug.Log("---End Trick");
			//Determine Winner
			List<Card> originalPlays = owner.Memory.GetData<List<Card>>("Plays");

			List<Card> plays = AIHandler.CloneListOfCards(originalPlays);

			foreach (Card card in plays)
				while (card.animating) yield return null;
			Card.Suit trump = owner.Memory.GetData<Card.Suit>("Trump");
			Card lead = plays[0];



			plays.Sort((Card x, Card y) => {
				if (AIHandler.ScoreingValue(x, owner) < AIHandler.ScoreingValue(y, owner))
					return -1;
				else
					return 1;
			});
			plays.Reverse();
			//Debug.Log("Trump: " + trump + "\nLead:" + lead.suit);
			string winnerName = "Player" + owner.Memory.GetData<Dictionary<Card, int>>("PlaysMap")[plays[0]];
			Player winningPlayer = owner.Memory.GetData<Player>(winnerName);
			int winnerPlayerNum = 0;
			switch (winnerName.ToLower()) {
				case "player0":
					winnerPlayerNum = 0;
					break;
				case "player1":
					winnerPlayerNum = 1;
					break;
				case "player2":
					winnerPlayerNum = 2;
					break;
				case "player3":
					winnerPlayerNum = 3;
					break;
			}




			//Set the active player to the winner
			owner.Memory.SetData("ActivePlayer", winnerPlayerNum);
			//Debug Log Trick
			{
				string t = "Trick Winner: Player" + winnerPlayerNum + " with " + plays[0] +"\nOP:";
				for (int i = 0; i < originalPlays.Count; i++)
					t += originalPlays[i].Shortname() + ",";
				t += "\nP:";
				for (int i = 0; i < plays.Count; i++) 
						t += plays[i].Shortname() + ",";
				//Debug.Log(t);
			}
			//Award trick to the correct team
			owner.Memory.SetData(winnerName + "Tricks", owner.Memory.GetData<int>(winnerName + "Tricks") + 1);

			//Place Trump Indicators
			{
				List<GameObject> indicators = owner.Memory.GetData<List<GameObject>>(winnerName + "TrickIndicators");
				if (GameManager.AnimateGame)
					indicators.Add(GameManager.SpawnTrickIndicator(winnerPlayerNum % 2));
				float r = 1.15f;
				float size = 10.0f;
				float baseAngle = 0.0f;
				switch (winnerName.ToLower()) {
					case "player0":
						baseAngle = 270.0f;
						break;
					case "player1":
						baseAngle = 180.0f;
						break;
					case "player2":
						baseAngle = 90.0f;
						break;
					case "player3":
						baseAngle = 0.0f;
						break;
				}
				float range = size * (indicators.Count / 2.0f);
				float step = 1.0f / indicators.Count;
				//ret[i] = Vector3.Lerp(point + offset, point - offset, step * i) - (horizAxis * (cardWidth / 2.0f));// - ((amt % 2 != 0) ?  : Vector3.zero);
				for (int i = 0; i < indicators.Count; i++) {
					float placeAngle = Mathf.Lerp(baseAngle+range, baseAngle-range, step * i) - (size / 2.0f);
					placeAngle *= Mathf.Deg2Rad;
					indicators[i].transform.position = new Vector3(r * Mathf.Cos(placeAngle), r * Mathf.Sin(placeAngle),0.0f);
				}
			}
			//Animate Cards
			{
				foreach (Card card in plays) {
					card.SetOrdering(-3);
					//Flip down the played cards
					card.StartCoroutine(GameManager.cardAnimator.Flip(card, GameManager.AnimateGame));
					//Orient towards winning player
					card.StartCoroutine(GameManager.cardAnimator.Orient(card, winnerName, GameManager.AnimateGame, owner));
					//Have the cards fly past the winner's hand (from center) as if they are collecting the trick
					card.StartCoroutine(GameManager.cardAnimator.FlyTo(Vector3.LerpUnclamped(Vector3.zero, winningPlayer.gameObject.transform.position, 3.4f), card, GameManager.AnimateGame));
				}
				foreach (Card card in plays)
					while (card.animating) yield return null;
			}
			
			//Return the plays to the deck
			owner.Memory.GetData<Deck>("GameDeck").Place(plays.ToArray());

			//Reset the plays list
			owner.Memory.SetData<List<Card>>("Plays", new List<Card>());

			//Transition
			if (owner.Memory.GetData<int>("Player0Tricks") + owner.Memory.GetData<int>("Player1Tricks") + owner.Memory.GetData<int>("Player2Tricks") + owner.Memory.GetData<int>("Player3Tricks") >= 5) {
				//If we have played 5 tricks, go to end of hand
				owner.Transition<EndHand>();
			}
			else {

				//Else play a new round
				owner.Transition<Play>();
			}
		}
	}

	public class EndHand : State {
		public override IEnumerator Enter() {
			yield return base.Enter();
			CardInteractionHandlers.DisableCardInteraction(this);
			//Debug.Log("End round!");
			Deck d = owner.Memory.GetData<Deck>("GameDeck");
			//Ensure cards are all returned to the deck
			if (d.Count != GameManager.DeckAsset.cards.Length) {
				Debug.Log("Was not able to recover all cards from hand.");
				Debug.Break();
			}
			if (GameManager.AnimateGame)
				GameObject.Destroy(owner.Memory.GetData<GameObject>("TrumpIndicator"));

			Deck deck = owner.Memory.GetData<Deck>("GameDeck");
			deck.basePosition = Vector3.zero;
			deck.EnforceCardLocationAndOrientation(true, owner);
			//Award points based on who ordered up and who won
			int trumpCallerTeam = owner.Memory.GetData<int>("TrumpCaller") % 2;
			int opposingTeam = (trumpCallerTeam + 1) % 2;
			int callerTricks, opponentTricks;
			{ 
				int team0Tricks = owner.Memory.GetData<int>("Player0Tricks") + owner.Memory.GetData<int>("Player2Tricks");
				int team1Tricks = owner.Memory.GetData<int>("Player1Tricks") + owner.Memory.GetData<int>("Player3Tricks");

				if (GameManager.RunGenetics)
					//Log the results of this run. (If someone on our team won this trick)
					yield return GameManager.GeneticHandler.Log((int[])owner.Memory.GetData<PointSpread>("Player0PointSpread"), team0Tricks > team1Tricks);

				callerTricks = (trumpCallerTeam == 0) ? team0Tricks : team1Tricks;
				opponentTricks = (trumpCallerTeam == 0) ? team1Tricks : team0Tricks;
			}




			if (callerTricks > opponentTricks) {
				owner.Memory.SetData("Team" + trumpCallerTeam + "Score", owner.Memory.GetData<int>("Team" + trumpCallerTeam + "Score")
					//Award two points if this team took all 5 (4 alone), otherwise 1
					+ ((callerTricks == 5) ? ((owner.Memory.GetData<bool>("Alone")) ? 4 : 2) : 1));
				//Debug.LogWarning("Caller Team Wins");
			}
			else {
				//Euching team gets 2 points
				owner.Memory.SetData("Team" + opposingTeam + "Score", owner.Memory.GetData<int>("Team" + opposingTeam + "Score") + 2);
				//Debug.LogWarning("Euched!");
			}
			
			//Debug.Log("Scores\tTeam0: " + owner.Memory.GetData<int>("Team0Score") + "\tTeam1: " + owner.Memory.GetData<int>("Team1Score"));

			GameManager.Scoreboard.UpdateScore(0, owner.Memory.GetData<int>("Team0Score"));
			GameManager.Scoreboard.UpdateScore(1, owner.Memory.GetData<int>("Team1Score"));
			if (GameManager.AnimateGame) {
				//Destroy all the trick indicators for all players
				for (int i = 0; i < 4; i++) {
					//Get the list from memory
					List<GameObject> indicators = owner.Memory.GetData<List<GameObject>>("Player" + i + "TrickIndicators");
					//Destroy all the indicators (loop backwards to avoid modification during iteration issues)
					for (int k = indicators.Count - 1; k >= 0; k--) {
						GameObject.Destroy(indicators[k]);
						indicators.RemoveAt(k);
					}
				}
			}

			if (owner.Memory.GetData<int>("Team0Score") >= 10 || owner.Memory.GetData<int>("Team1Score") >= 10)
				owner.Transition<EndGame>();
			else
				owner.Transition<SetupHand>();
		}
	}

	public class EndGame : State {
		public override IEnumerator Enter() {
			yield return base.Enter();
			
			//Delete the Deck
			//owner.Memory.GetData<Deck>("GameDeck").Destroy();

			int winningTeamID = owner.Memory.GetData<int>("Team0Score") >= 10 ? 0 : 1;
			
			for (int i = 0; i < 4; i++) {
				//Destroy the player object
				GameObject.Destroy(owner.Memory.GetData<Player>("Player"+i).gameObject);
			}
			/*
			float timeSpent = Time.time - owner.Memory.GetData<float>("StartTime");
			Debug.Log("Game Time: " + timeSpent);
			Debug.Log("SetupGame Time: " + owner.timeSpent[typeof(SetupGame)]);
			Debug.Log("SetupHand Time: " + owner.timeSpent[typeof(SetupHand)]);
			Debug.Log("DetermineTrump Time: " + owner.timeSpent[typeof(DetermineTrump)]);
			Debug.Log("Play Time: " + owner.timeSpent[typeof(Play)]);
			Debug.Log("EndTrick Time: " + owner.timeSpent[typeof(EndTrick)]);
			Debug.Log("EndHand Time: " + owner.timeSpent[typeof(EndHand)]);
			*/
			owner.timeSpent = new Dictionary<System.Type, float>();
			//Reset the GameManager's Game Memory
			owner.Memory.Reset();
			//Debug.Log("\n");
			//Start a new game.
			owner.Transition<SetupGame>();
		}
	}
}
