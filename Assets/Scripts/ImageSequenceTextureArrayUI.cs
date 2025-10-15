using UnityEngine;
using System.Collections;
using Citadel.Game;
using UnityEngine.UIElements;
using Zenject;
using Image = UnityEngine.UI.Image;

public class ImageSequenceTextureArrayUI : MonoBehaviour {
	private Image goImage;
	private int frameCounter = 0;
	public float frameDelay = 0.35f; // frameDelay = 0.09f; for Vmail
	public bool stopAtEnd = false; // True for Vmail
	private bool playDone = false;
	public bool replayOnEnable = false; // True for Vmail
	public bool playOnMenu = false; // False for Vmail
	public bool deactivateAtEnd = false;
	
	[Inject] 
	private readonly PauseScript _pauseScript;
	[Inject] private readonly ITexturesStorage _texturesStorage;

	void Awake() {
		goImage = this.GetComponent<Image>();
	}
	
	void OnEnable() {
		if (replayOnEnable) {
			playDone = false;
			frameCounter = 0;
		}
	}
	
	void Update() {
		if (!_pauseScript.Paused() || playOnMenu) {
			if (!_pauseScript.MenuActive() || playOnMenu) {
				if (deactivateAtEnd && playDone) gameObject.SetActive(false);
				if (stopAtEnd && playDone) return;

				if (stopAtEnd && !playDone) {
					StartCoroutine("Play", frameDelay);
				} else {
					StartCoroutine("PlayLoop", frameDelay); // Call the 'PlayLoop' method as a coroutine with a float delay.
				}

				// Set the material's texture to current value of frameCounter.
				if (frameCounter < _texturesStorage.BlockedBySecuritySprites.Count && frameCounter >= 0) {
					goImage.overrideSprite = _texturesStorage.BlockedBySecuritySprites[frameCounter];
				}
			}
		}
	}

	IEnumerator PlayLoop(float delay) {
		yield return new WaitForSeconds(delay); // Wait for the time defined at the delay parameter.
TryAgain:
		if (_pauseScript.Paused() && !playOnMenu) {
			yield return null;
			goto TryAgain;
		}
		
		frameCounter = (++frameCounter)%_texturesStorage.BlockedBySecuritySprites.Count; // Advance one frame
		StopCoroutine("PlayLoop"); // Stop this coroutine
	}  

	IEnumerator Play(float delay) {
		yield return new WaitForSeconds(delay); // Wait for the time defined at the delay parameter.
		
		// If the frame counter isn't at the last frame.
		if(frameCounter < _texturesStorage.BlockedBySecuritySprites.Count-1) {
			++frameCounter; // Advance one frame.
			if (frameCounter >= _texturesStorage.BlockedBySecuritySprites.Count) playDone = true;
		} else {
			playDone = true;
		}
		StopCoroutine("Play"); //Stop this coroutine
	} 
}
