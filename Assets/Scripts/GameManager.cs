using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	#region Public Members
	public static CardAnimationHandler cardAnimator => instance.cardAnimationHandler;
	public static DeckAsset DeckAsset => instance._deckAsset;
	public static LayerMasks Masks => instance._masks;
	//public static DynamicContainer Memory => instance._dynamicContainer;
	public static Color OpponentColor => instance._opponentColor;
	public static Color PlayerColor => instance._playerColor;
	public static ScoreLinks Scoreboard => instance._scoreLinks;
	public static Vector2 ScreenBounds => instance._screenBounds;
	public static bool AnimateGame => instance._animateGame;
	public static bool ShowAllCards => instance._showAllCards;
	public static bool ScrewTheDealer => instance._screwDealer;
	public static bool HumanPlayer => instance._humanPlayer;
	public static bool PlayAudio => instance._playAudio;
	public static bool RunGenetics => instance._runGenetics;
	public static PointSpread DefaultPointSpread => instance.defaultPointSpread;
	public static GeneticHandler GeneticHandler => instance.geneHandler;
	#endregion


	#region Private Members
	private static GameManager instance = null;

	private StateMachineSystem.StateMachine[] stateMachines;
	#endregion

	#region Serialized Members
	[SerializeField]
	private bool _animateGame = true;
	[SerializeField]
	private bool _showAllCards = false;
	[SerializeField]
	private bool _screwDealer = false;
	[SerializeField]
	private bool _humanPlayer = true;
	[SerializeField]
	private bool _playAudio = false;
	[SerializeField]
	private bool _runGenetics = false;
	[SerializeField]
	private CardAnimationHandler cardAnimationHandler = null;

	[SerializeField]
	private GameObject cardHolder = null;
	[SerializeField]
	private GameObject trumpSelectorUI = null;

	#region Static Linked
	[SerializeField]
	private Vector2 _screenBounds = Vector2.zero;
	[SerializeField]
	private LayerMasks _masks = null;

	[SerializeField]
	private Color _playerColor = Color.white, _opponentColor = Color.white;

	[SerializeField]
	private ScoreLinks _scoreLinks = null;

	[SerializeField]
	private DeckAsset _deckAsset = null;

	//[SerializeField]
	//[HideInInspector]
	//private DynamicContainer _dynamicContainer = new DynamicContainer();
	#endregion

	#region Spawned Objects
	[SerializeField]
	private GameObject cardPrefab = null;
	[SerializeField]
	private GameObject playerPrefab = null;
	[SerializeField]
	private GameObject trickDot = null;
	[SerializeField]
	private GameObject[] trumpStamps = null;
	[SerializeField]
	private GameObject passSticker = null;
	[SerializeField]
	private GameObject callSticker = null;
	[SerializeField]
	private GameObject aloneSticker = null;
	[SerializeField]
	private PointSpread defaultPointSpread = default;
	[SerializeField]
	private GeneticHandler geneHandler = new GeneticHandler();
	[SerializeField]
	private int geneticGames = 10;
	#endregion

	#endregion

	// Start is called before the first frame update
	void Start() {
		if (instance != null) {
			Debug.Log("Multiple game managers in scene.");
			Debug.Break();
		}
		instance = this;
		if (GameManager.RunGenetics == false)
			geneticGames = 1;
		stateMachines = new StateMachineSystem.StateMachine[geneticGames];
		for (int i = 0; i < geneticGames; i++) {
			stateMachines[i] = new StateMachineSystem.StateMachine();
			stateMachines[i].UID = i;
			stateMachines[i].Transition<StateMachineSystem.EuchreStates.SetupGame>();
		}
	}



	/// <summary>
	/// Returns locations to spawn player hands at. (Z is euler angle)
	/// </summary>
	/// <param name="playerAmt"></param>
	/// <returns></returns>
	public static Vector3[] GetHandLocations(int playerAmt) {
		Vector3[] ret = new Vector3[playerAmt]; 
		float circleSize = 4.6f;
		float step = (360.0f / (float)playerAmt);
		for (int i = playerAmt - 1; i >= 0; i--) {
			float angle = ((step * i));
			//Bound angle in the positive
			if (angle < 0.0f) angle += 360.0f;
			ret[i] = new Vector3(circleSize * Mathf.Cos(angle * Mathf.Deg2Rad), circleSize * Mathf.Sin(angle * Mathf.Deg2Rad), angle);
		}
		return ret;
	}

	public static Player SpawnPlayerPrefab() {
		return Instantiate(instance.playerPrefab).GetComponent<Player>();
	}

	public static Card SpawnCardPrefab(CardAsset cardAsset) {
		Card spawnedCard = Instantiate(instance.cardPrefab).GetComponent<Card>();
		spawnedCard.transform.parent = instance.cardHolder.transform;
		spawnedCard.Initialize(cardAsset);
		return spawnedCard;

	}

	public static TrumpSelector GetTrumpSelector() {
		return instance.trumpSelectorUI.GetComponent<TrumpSelector>();
	}


	public static GameObject SpawnTrumpIndicator(Card.Suit suit, int team) {
		GameObject spawnedObject = Instantiate(instance.trumpStamps[(int)suit]);
		spawnedObject.GetComponent<SpriteRenderer>().color = (team == 0) ? PlayerColor : OpponentColor ;
		return spawnedObject;
	}

	public static GameObject SpawnTrickIndicator(int team) {
		GameObject spawnedObject = Instantiate(instance.trickDot);
		spawnedObject.GetComponent<SpriteRenderer>().color = (team == 0) ? PlayerColor : OpponentColor;
		return spawnedObject;
	}

	/// <summary>
	/// types 1==Passing, 2==Calling, 3==Alone
	/// </summary>
	/// <param name="playerID"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	public static GameObject SpawnAIText(int playerID, int type, StateMachineSystem.StateMachine targetMachine) {
		GameObject spawnedOject;
		if (type == 1)
			spawnedOject = Instantiate(instance.passSticker);
		else if (type == 2)
			spawnedOject = Instantiate(instance.callSticker);
		else
			spawnedOject = Instantiate(instance.aloneSticker);

		spawnedOject.transform.position = Vector3.Lerp(Vector3.zero, targetMachine.Memory.GetData<Player>("Player"+playerID).gameObject.transform.position,0.5f);
		if (type == 3)
			spawnedOject.transform.position += Vector3.down * 0.65f;

		spawnedOject.gameObject.GetComponent<SpriteRenderer>().color = (playerID % 2 == 0) ? PlayerColor : OpponentColor;
		foreach (TMPro.TMP_Text text in spawnedOject.GetComponentsInChildren<TMPro.TMP_Text>()) {
			text.color = (playerID % 2 == 0) ? PlayerColor : OpponentColor;
		}
		return spawnedOject;
	}

	public static GameManager AccessInstance() {
		return instance;
	}


	[System.Serializable]
	public class LayerMasks {
		public LayerMask notCards;
	}
	public static class Tags {
		public static string PlayZone = "PlayZone";
		public static string PlayerHand = "PlayerHand";
	}
}
