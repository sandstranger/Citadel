using UnityEngine;
using System.Collections;
using Zenject;

public class Ladder : MonoBehaviour {	
	
	[Inject] private PlayerMovement _playerMovement;
	
	void  OnTriggerEnter (Collider other){
		if (other.CompareTag("Player")) {
			_playerMovement.ladderState++;
			if (_playerMovement.ladderState < 1) _playerMovement.ladderState = 1;
		}
	}
	
	void  OnTriggerExit (Collider other){
		if (other.CompareTag("Player")) {
			_playerMovement.ladderState--;
			if (_playerMovement.ladderState < 0) _playerMovement.ladderState = 0;
		}
	}
}
