using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBroadcaster : MonoBehaviour {
	public string notificationText;
	public Card.Suit suit;

	public void SimpleBroadcast() {
		this.PostNotification(notificationText);
	}

	public void SuitBroadcast() {
		this.PostNotification(notificationText, suit);
	}
}
