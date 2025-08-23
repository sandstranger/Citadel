using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

public class Email : MonoBehaviour {
	public int emailIndex;
	public bool autoPlayEmail = false;

	[Inject] private Const _consts;
	[Inject] private Inventory _inventory;
	
    public void Targetted() {
		// Give email.
		if (_inventory.hasLog[emailIndex]) return; // Already have it.

		_inventory.hasLog[emailIndex] = true;
		_inventory.hasNewEmail = true;
		_inventory.lastAddedIndex = emailIndex;
		if (_consts.audioLogType[emailIndex] == AudioLogType.Email) {
			_inventory.beepDone = true;
		}

		if (autoPlayEmail) _inventory.PlayLastAddedLog(emailIndex);
	}
}
