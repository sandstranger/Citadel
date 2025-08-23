using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using UnityEngine;

// Registers touch from a player and displays a message about the barrier to
// indicate that player should do something to remove this barrier as opposed
// to just normal cyber walls.  Expected to still be used with a normal
// CyberWall attached on the func_door_cyber.
public class CyberDoor : MonoBehaviour {
	public bool isDoor = false;
	public int messageIndex = 600; // Power routed to door lock.

	[Inject] private Const _consts;
	
	void OnCollisionEnter (Collision other) {
		if (isDoor && other.gameObject.CompareTag("Player")) {
            string msg = (_consts.stringTable[messageIndex] + "  "
                + _consts.stringTable[601]); // Reroute power to remove barrier.
			_consts.sprint(msg); 
		}
	}
}
