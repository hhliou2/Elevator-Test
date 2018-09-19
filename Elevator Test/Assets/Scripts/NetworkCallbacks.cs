using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCallbacks : Bolt.GlobalEventListener {

	[BoltGlobalBehaviour]
	public override void SceneLoadLocalDone (string map) {
		var pos = new Vector3 (0, 0, -5.757f);
		BoltNetwork.Instantiate (BoltPrefabs.PlayerObject, pos, Quaternion.identity);
	}

}
