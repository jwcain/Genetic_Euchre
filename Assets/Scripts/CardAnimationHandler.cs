using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAnimationHandler : MonoBehaviour {

	/// <summary>
	/// Animates the movement of a card
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public IEnumerator FlyTo(Vector3 pos, Card target, bool animated, bool audio = false) {
		pos.z = 0;
		if (GameManager.AnimateGame)
			while (target.animating)
				yield return null;
		target.animating = true;

		if (animated && GameManager.AnimateGame) {
			float speed = 0.35f;
			while ((target.transform.position - pos).magnitude > 0.05f) {
				Vector3 dir = (pos - target.transform.position);
				if (dir.magnitude > speed)
					dir = dir.normalized * speed;

				target.transform.position += dir;
					yield return null;
			}
		}

		if (audio && GameManager.PlayAudio)
			FMODUnity.RuntimeManager.PlayOneShot("event:/cardPlace" + Random.Range(1, 4), target.transform.position);

		target.transform.position = pos;
		if (GameManager.AnimateGame)
			yield return null;
		target.animating = false;
	}

	/// <summary>
	/// Draws a number of cards from the game deck and animates their arrival to the player's hand
	/// </summary>
	/// <param name="playerName"></param>
	/// <param name="deckName"></param>
	/// <param name="amt"></param>
	/// <returns></returns>
	public IEnumerator Deal(string playerName, string deckName, int amt, bool animated, StateMachineSystem.StateMachine targetMachine) {
		if (amt == 0)
			yield break;

		Card[] drawn = targetMachine.Memory.GetData<Deck>(deckName).Draw(amt);
		Player p = targetMachine.Memory.GetData<Player>(playerName);
		
		for (int i = 0; i < drawn.Length; i++) {
			Card card = drawn[i];
			StartCoroutine(Orient(card, p.gameObject.name, animated, targetMachine));

			card.gameObject.transform.rotation = Quaternion.Euler(0.0f, 180.0f, p.gameObject.transform.rotation.eulerAngles.z + 90.0f);
			p.AddCard(card);
			
		}
		p.AssertHandSpriteOrdering();
		for (int i = 0; i < drawn.Length; i++) {
			if (p.isHuman || GameManager.ShowAllCards)
				StartCoroutine(Flip(drawn[i], animated));
		}
		if (GameManager.AnimateGame)
			foreach (Card c in drawn) 
				while (c.animating)
					yield return null;
		
		if (GameManager.PlayAudio)
			FMODUnity.RuntimeManager.PlayOneShot("event:/cardSlide" + Random.Range(1,4), drawn[0].transform.position);

		//This handles cards moving to the player's hand from the deck as well
		yield return AdjustHand(playerName, animated, targetMachine);
	}

	/// <summary>
	/// Orients a card to line up with a player's hand. if string is empty or null it orients with the screen.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="playerName"></param>
	/// <returns></returns>
	public IEnumerator Orient(Card target, string playerName, bool animated, StateMachineSystem.StateMachine targetMachine) {
		if (GameManager.AnimateGame)
			while (target.animating)
				yield return null;
		target.animating = true;

		float orientation = (string.IsNullOrEmpty(playerName)) ? 0.0f : targetMachine.Memory.GetData<Player>(playerName).gameObject.transform.eulerAngles.z + 90.0f;
		Quaternion goalRot = Quaternion.Euler(0.0f, target.transform.eulerAngles.y, orientation);

		if (animated && GameManager.AnimateGame) { 
			while ((goalRot.eulerAngles - target.transform.rotation.eulerAngles).magnitude > 0.1f) {
				target.transform.rotation = Quaternion.Lerp(target.transform.rotation, goalRot, 0.4f);
				yield return null;
			}
		}
		
		//Enforce final position
		target.transform.rotation = goalRot;
		//End animation
		target.animating = false;
	}


	/// <summary>
	/// Adjusts the cards within the player's hand to match calucalted positions
	/// </summary>
	/// <param name="playerName"></param>
	/// <returns></returns>
	public IEnumerator AdjustHand(string playerName, bool animated, StateMachineSystem.StateMachine targetMachine) {
		Player p = targetMachine.Memory.GetData<Player>(playerName);
		p.AssertHandSpriteOrdering();
		Vector3[] pos = GetCardPlacementOffsets(p.gameObject, p.GetHand().Count);

		//Move all cards
		for (int i = 0; i < p.GetHand().Count; i++) {
			p.GetHand()[i].goalPosition = pos[i];
			StartCoroutine(FlyTo(pos[i], p.GetHand()[i], animated));
		}

		//Wait for all sub animations to finish
		if (GameManager.AnimateGame)
			for (int i = 0; i < p.GetHand().Count; i++) 
				while (p.GetHand()[i].animating)
					yield return null;
			
	}

	/// <summary>
	/// Animates the flipping of a card and chances its face-position value
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public IEnumerator Flip(Card target, bool animated) {
		if (GameManager.AnimateGame)
			while (target.animating)
				yield return null;

		target.animating = true;
		Quaternion originalRotation = target.transform.rotation;
		if (animated && GameManager.AnimateGame) {
			float step = 10;
			for (float i = 0.0f; i < 180.0f; i += step) {
				target.transform.Rotate(0.0f, step, 0.0f);
				yield return null;
			}
		}
		else
			target.transform.rotation = Quaternion.Euler(originalRotation.eulerAngles.x, originalRotation.eulerAngles.y + 180.0f, originalRotation.eulerAngles.z);

		target.faceDown = !target.faceDown;
		if (GameManager.AnimateGame)
			yield return null;
		target.animating = false;
	}


	/// <summary>
	/// Calculates placement and rotation for a set of cards in a location. (Z is rotation)
	/// </summary>
	/// <returns></returns>
	public Vector3[] GetCardPlacementOffsets(GameObject handObject, int amt) {
		Vector3[] ret = new Vector3[amt];
		//How wide we assume a card is
		float cardWidth = 1.0f;
		//Center point of this hand
		Vector3 point = handObject.transform.position;
		//Euler angle this hand rests on at the 'circular table'
		float eulerDir = handObject.transform.rotation.eulerAngles.z;
		
		//Calculate the normal facing away from the circle
		Vector3 normal = new Vector3(Mathf.Cos(eulerDir * Mathf.Deg2Rad), Mathf.Sin(eulerDir* Mathf.Deg2Rad),0.0f);
		//Make it face inward
		normal *= -1f;
		Vector3 horizAxis = Vector3.Cross(new Vector3(0,0,1), normal);

		float step = 1.0f / (float)amt; 
		Vector3 offset = horizAxis * cardWidth * (amt / 2.0f);

		for (int i = 0; i < amt; i++) {
			ret[i] = Vector3.Lerp(point + offset, point - offset, step * i) - (horizAxis * (cardWidth / 2.0f));// - ((amt % 2 != 0) ?  : Vector3.zero);
			ret[i].z = 0.0f;
			Debug.DrawRay(ret[i], normal, Color.green);
		}

		return ret;
	}
}
