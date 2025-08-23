using UnityEngine;
using System.Collections;
using Zenject;

public class TeleportFXStatic : MonoBehaviour {
	public float intervalTime = 0.08f;
	public float activeTime = 1f;
	public Texture2D tempCursorTexture;
	[HideInInspector] public Texture2D cursorTexture;
	private float effectFinished;
	private float flipTime;
	private float randHolder;
	private bool xFlipped = false;
	private bool yFlipped = false;
	private RectTransform rect;

	[Inject] private MouseCursor _mouseCursor;
	[Inject] private PauseScript _pauseScript;

	void OnEnable () {
		cursorTexture = _mouseCursor.cursorImage; //store correct cursor
		_mouseCursor.cursorImage = tempCursorTexture; //give dummy cursor to hide it
		effectFinished = _pauseScript.relativeTime + activeTime;
		rect = GetComponent<RectTransform>();
		flipTime = _pauseScript.relativeTime + intervalTime;
	}

	void FlipX () {
		if (xFlipped) {
			xFlipped = false;
			rect.localScale = new Vector3(1f, 1f, 1f);
		} else {
			xFlipped = true;
			rect.localScale = new Vector3(-1f, 1f, 1f);
		}
	}

	void FlipY () {
		if (yFlipped) {
			yFlipped = false;
			rect.localScale = new Vector3(1f, 1f, 1f);
		} else {
			yFlipped = true;
			rect.localScale = new Vector3(1f, -1f, 1f);
		}
	}

	void Deactivate () {
		_mouseCursor.cursorImage = cursorTexture; //return to previous cursor
		gameObject.SetActive(false);
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive()) {
			if (effectFinished < _pauseScript.relativeTime) Deactivate();
			if (flipTime < _pauseScript.relativeTime) {
				flipTime = _pauseScript.relativeTime + intervalTime;
				randHolder = Random.Range(0f,1f);
				if (randHolder < 0.5) {
					FlipX();
				} else {
					FlipY();
				}
			}
		}
	}
}
