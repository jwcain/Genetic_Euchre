using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
	public Vector3 eulers;
    // Update is called once per frame
    void Update() {
		this.transform.Rotate(eulers * Time.deltaTime);
    }
}
