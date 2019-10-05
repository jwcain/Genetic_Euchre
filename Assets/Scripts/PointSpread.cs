using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PointSpread  {
	[SerializeField]
	public int RightBower;
	[SerializeField]
	public int LeftBower;
	[SerializeField]
	public int Ace;
	[SerializeField]
	public int King;
	[SerializeField]
	public int Queen;
	[SerializeField]
	public int Ten;
	[SerializeField]
	public int Nine;
	[SerializeField]
	public int OffSuitAce;
	[SerializeField]
	public int TwoSuited;
	[SerializeField]
	public int callThreshold;
	[SerializeField]
	public int lonerThreshold;

	public PointSpread(int rightBower, int leftBower, int ace, int king, int queen, int ten, int nine, int offSuitAce, int twoSuited, int callThreshold, int lonerThreshold) {
		this.RightBower = rightBower;
		this.LeftBower = leftBower;
		this.Ace = ace;
		this.King = king;
		this.Queen = queen;
		this.Ten = ten;
		this.Nine = nine;
		this.OffSuitAce = offSuitAce;
		this.TwoSuited = twoSuited;
		this.callThreshold = callThreshold;
		this.lonerThreshold = lonerThreshold;
	}

	public PointSpread(int[] d) {
		if (d.Length != 11)
			Debug.LogError("Improper int set passed.");

		this.RightBower = d[0];
		this.LeftBower = d[1];
		this.Ace = d[2];
		this.King = d[3];
		this.Queen = d[4];
		this.Ten = d[5];
		this.Nine = d[6];
		this.OffSuitAce = d[7];
		this.TwoSuited = d[8];
		this.callThreshold = d[9];
		this.lonerThreshold = d[10];
	}

	public override string ToString() {
		string ret = "";
		ret += "RB:" + this.RightBower;
		ret += " LB:" + this.LeftBower;
		ret += " A:" + this.Ace;
		ret += " K:" + this.King;
		ret += " Q:" + this.Queen;
		ret += " T:" + this.Ten;
		ret += " N:" + this.Nine;
		ret += " OA:" + this.OffSuitAce;
		ret += " TS:" + this.TwoSuited;
		ret += " CL:" + this.callThreshold;
		ret += " LN:" + this.lonerThreshold;
		return ret;
	}


	public static explicit operator int[](PointSpread d) =>  new int[] { d.RightBower, d.LeftBower, d.Ace, d.King, d.Queen, d.Ten, d.Nine, d.OffSuitAce, d.TwoSuited, d.callThreshold, d.lonerThreshold};

}
