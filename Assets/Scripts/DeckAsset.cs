using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "Deck", order = 1)]
public class DeckAsset : ScriptableObject {

	[SerializeField]
	public CardAsset[] cards;

}
