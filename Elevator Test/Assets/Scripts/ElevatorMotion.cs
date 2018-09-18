using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorMotion : MonoBehaviour {

	float bottomHeight = 0;
	float topHeight = 5;
	float speed = 0.05f;

	// Use this for initialization
		void Start () {
		GameObject doors = GameObject.Find ("Doors");
		DoorMotion doorMotion = doors.GetComponent<DoorMotion>();
		StartCoroutine (waiter (doors, doorMotion));
	}

	IEnumerator waiter(GameObject doors, DoorMotion doorMotion) {

		while (this.transform.position.y < topHeight) {
			this.transform.Translate (new Vector3(0, speed));
			yield return new WaitForFixedUpdate ();
		} 

		while (doors.transform.GetChild (0).position.x < 0.745) {
			doorMotion.OpenDoors(speed);
			yield return new WaitForFixedUpdate ();
		}

		yield return new WaitForSeconds (2);

		while (doors.transform.GetChild (0).position.x > 0.245) {
			doorMotion.CloseDoors(speed);
			yield return new WaitForFixedUpdate ();
		}

		while (this.transform.position.y > bottomHeight) {
			this.transform.Translate (new Vector3(0, -speed));
			yield return new WaitForFixedUpdate ();
		}
	}
}
