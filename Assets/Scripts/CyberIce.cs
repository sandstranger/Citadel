using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberIce : MonoBehaviour {
    void  OnTriggerEnter (Collider col) {
		if (col.gameObject.CompareTag("PlayerBullet")) {
			col.gameObject.layer = 24; // Set to NPCBullet layer
			Rigidbody rbody = col.gameObject.GetComponent<Rigidbody>();
			if (rbody != null) {
				Vector3 flip = rbody.linearVelocity * -1f;
				rbody.linearVelocity = flip;
			}
		}
	}
}
