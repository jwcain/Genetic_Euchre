using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreLinks : MonoBehaviour {
	public TMPro.TMP_Text playerScore = null, opponentScore = null;

	public void UpdateScore(int team, int score) {
		if (team == 0)
			playerScore.text = "" + score;
		else
			opponentScore.text = "" + score;

	}
}
