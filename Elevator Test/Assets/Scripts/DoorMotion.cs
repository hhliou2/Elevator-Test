using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMotion : MonoBehaviour {

	public GameObject leftDoor;
	public GameObject rightDoor;

	public void OpenDoors(float speed) {
		leftDoor.transform.Translate (new Vector3 (speed, 0));
		rightDoor.transform.Translate (new Vector3 (-speed, 0));
	}

	public void CloseDoors(float speed) {
		leftDoor.transform.Translate (new Vector3 (-speed, 0));
		rightDoor.transform.Translate (new Vector3 (speed, 0));
	}
}
