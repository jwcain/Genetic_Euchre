using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A collection of events that can be trriggered by a notification for handling cards. This is decoupled from card's themselves so it can be turned off/on easier by the state machine
/// </summary>
public static class CardInteractionHandlers {

	public static bool CanPlayCard = false;

	/// <summary>
	/// Adds observers for all card interaction events
	/// </summary>
	/// <param name="target"></param>
	public static void EnableCardInteraction(object target) {
		target.AddObserver(CardMouseDown, "MouseDownCard");
		target.AddObserver(CardMouseDrag, "MouseDragCard");
		target.AddObserver(CardMouseUp, "MouseUpCard");
		target.AddObserver(CardMouseOver, "MouseOverCard");
		target.AddObserver(CardMouseExit, "MouseExitCard");
		target.AddObserver(CardMouseEnter, "MouseEnterCard");
		target.AddObserver(CardMouseClick, "MouseClickCard") ;
	}

	/// <summary>
	/// Removes observers for card interaction event
	/// </summary>
	/// <param name="target"></param>
	public static void DisableCardInteraction(object target) {
		target.RemoveObserver(CardMouseDown, "MouseDownCard");
		target.RemoveObserver(CardMouseDrag, "MouseDragCard");
		target.RemoveObserver(CardMouseUp, "MouseUpCard");
		target.RemoveObserver(CardMouseOver, "MouseOverCard");
		target.RemoveObserver(CardMouseExit, "MouseExitCard");
		target.RemoveObserver(CardMouseEnter, "MouseEnterCard");
		target.RemoveObserver(CardMouseClick, "MouseClickCard");
	}


	public static void CardMouseDown(object sender, object args) {
		Card card = (Card)args;
		card.storedOrdering = card.GetComponentInChildren<SpriteRenderer>().sortingOrder;
		//Always give the player the clearest shot of the card possible
		card.SetOrdering(25);
	}

	public static void CardMouseDrag(object sender, object args) {
		Card card = (Card)args;

		var v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = Camera.main.ScreenToWorldPoint(v3);

		//Constrain the card to being within the screen
		if (v3.x > GameManager.ScreenBounds.x) {
			v3.x = GameManager.ScreenBounds.x;
		}
		if (v3.x < -GameManager.ScreenBounds.x) {
			v3.x = -GameManager.ScreenBounds.x;
		}
		if (v3.y > GameManager.ScreenBounds.y) {
			v3.y = GameManager.ScreenBounds.y;
		}
		if (v3.y < -GameManager.ScreenBounds.y) {
			v3.y = -GameManager.ScreenBounds.y;
		}

		card.gameObject.transform.position = v3;
	}
	public static void CardMouseUp(object sender, object args) {
		Card card = (Card)args;
		card.SetOrdering(card.storedOrdering);

		GameObject zone = null;

		//Shoot a ray to determine what we have left the card on
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, GameManager.Masks.notCards);
		if (hit) {
			if (hit.collider.gameObject.tag == GameManager.Tags.PlayZone || hit.collider.gameObject.tag == GameManager.Tags.PlayerHand) {
				zone = hit.collider.gameObject;
			}
		}

		if (zone == null || CanPlayCard == false)
			//If the card wasn't played in a zone, return it to its correct position
			card.StartCoroutine(GameManager.cardAnimator.FlyTo(card.goalPosition, card, GameManager.AnimateGame));
		else
			//Otherwise, shout into the void and hope someone is listening to fix this.
			GameManager.cardAnimator.PostNotification("CardPlayedInZone", new object[] { zone, card });
	}

	public static void CardMouseClick(object sender, object args) {
		Card card = (Card)args;
		//GameObject zone = GameObject.FindGameObjectWithTag(GameManager.Tags.PlayZone);
		//GameManager.cardAnimator.PostNotification("CardPlayedInZone", new object[] { zone, card });
	}

	public static void CardMouseOver(object sender, object args) {
		Card card = (Card)args;
		//Nothing.
	}
	public static void CardMouseExit(object sender, object args) {
		Card card = (Card)args;
		card.gameObject.transform.localScale = Vector3.one;
	}
	public static void CardMouseEnter(object sender, object args) {
		Card card = (Card)args;
		card.gameObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
	}
}
