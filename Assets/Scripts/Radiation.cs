using Zenject;
using System.Text;
using UnityEngine;

public class Radiation : MonoBehaviour {
	public float radiationAmount = 11f;
	public float intervalTime = 1f;
	public float radFinished = 0f;
	private static StringBuilder s1 = new StringBuilder();

	[Inject] private PauseScript _pauseScript;
	[Inject] private PlayerHealth _playerHealth;

	void Start() {
		radFinished = _pauseScript.relativeTime + (intervalTime * 2);
	}

	void OnTriggerEnter (Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			if (_playerHealth.hm.health > 0f) {
				_playerHealth.radiationArea = true;
				_playerHealth.GiveRadiation(radiationAmount);
				radFinished = _pauseScript.relativeTime + (intervalTime*Random.Range(1f,1.5f));
			}
		}
	}

	void  OnTriggerStay (Collider col) {
		if (col.gameObject.CompareTag("Player")) {
			if (_playerHealth.hm.health > 0f && (radFinished < _pauseScript.relativeTime)) {
				_playerHealth.radiationArea = true;
				_playerHealth.GiveRadiation(radiationAmount);
				radFinished = _pauseScript.relativeTime + (intervalTime*Random.Range(1f,1.5f));
			}
		}
	}

	void OnTriggerExit (Collider col) {
		if (col.gameObject.CompareTag("Player")) { 
			if (_playerHealth.hm.health > 0f) {
				_playerHealth.radiationArea = false;
				radFinished = _pauseScript.relativeTime;  // reset so re-triggering is instant
			}
		}
	}
	
	void OnDisable() {
		_playerHealth.radiationArea = false;
	}
	
	public static string Save(GameObject go) {
		Radiation rad = go.GetComponent<Radiation>();
		s1.Clear();
		s1.Append(Utils.FloatToString(rad.radiationAmount,"radiationAmount"));
		s1.Append(Utils.splitChar);	
		s1.Append(Utils.FloatToString(rad.intervalTime,"intervalTime"));
		s1.Append(Utils.splitChar);	
		s1.Append(Utils.SaveRelativeTimeDifferential(rad._pauseScript,rad.radFinished,"radFinished"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		Radiation rad = go.GetComponent<Radiation>();
		rad.radiationAmount = Utils.GetFloatFromString(entries[index],"radiationAmount"); index++;
		rad.intervalTime = Utils.GetFloatFromString(entries[index],"intervalTime"); index++;
		rad.radFinished = Utils.LoadRelativeTimeDifferential(rad._pauseScript,entries[index],"radFinished"); index++;
		return index;
	}
}
