using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorMotion : MonoBehaviour {

	float bottomHeight = 0;
	float topHeight = 5;
	float speed = 0.01f;

	// Use this for initialization
	void Start () {
		StartCoroutine (waiter ());
	}

	IEnumerator waiter() {
		while (this.transform.position.y < topHeight) {
			this.transform.Translate (new Vector3(0, speed));
			yield return new WaitForFixedUpdate ();
		} 

		yield return new WaitForSeconds (3);

		while (this.transform.position.y > bottomHeight) {
			this.transform.Translate (new Vector3(0, -speed));
			yield return new WaitForFixedUpdate ();
		}
	}
}
