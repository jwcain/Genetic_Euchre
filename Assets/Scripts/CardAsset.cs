using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "Card", order = 1)]
public class CardAsset : ScriptableObject {

	[SerializeField]
	public Card.Suit suit;

	[SerializeField]
	public Card.Value value;

	[SerializeField]
	public Sprite sprite;
}
