using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class ConfigKeybindButton : MonoBehaviour {
	// Externally set per instance
	public int index;

	// Internal references
	private Button self;
	private bool enterMode;
	private bool firstFrame;
	private Text selfText;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	[Inject] private GetInput _getInput;
	[Inject] private MainMenuHandler _mainMenuHandler;

	void Start() { // Wait for _consts. to initialize.
		Initialize();
	}

	void OnEnable() {
		Initialize();
	}

	void Initialize() {
		if (index < 0 || index >= 40) {
			Debug.Log("BUG: ConfigKeybindButton index out of range [0,40]");
			index = 0;
		}

		enterMode = false;
		firstFrame = true;
		if (self == null) self = GetComponent<Button>();
		if (self == null) {
			Debug.Log("BUG: ConfigKeybindButton missing component for self.");
		}

		if (selfText == null) selfText = GetComponentInChildren<Text>(true);

		UpdateText();
	}

	public void KeybindButtonClick() {
		if (_mainMenuHandler.PresetConfirmDialog.activeSelf) return;

		if (!enterMode) {
			self.GetComponentInChildren<Text>(true).text = "...";
			enterMode = true;
		}
	}

	void CheckAndHandleConflicts(int checkVal) {
		for (int i=0;i<_consts.InputCodeSettings.Length;i++) {
			if (i == index) continue; // We already know this one is us.

			if (_consts.InputCodeSettings[i] == checkVal) {
				_consts.InputCodeSettings[i] = 109;
				_consts.sprint(_consts.stringTable[1018] // "Found and unbound conflict with "
							 + _consts.InputCodes[i],_consts.Player);
				break;
			}
		}
	}

	void Update() {
		if (enterMode) {
			// Prevent capturing the click in input check when first clicking
			// on the button to enter the entry mode.
			if (firstFrame) {firstFrame = false; return; } 

			bool goodkey = false;
			// Prevent checking keys Unity doesn't recognize in GetKeyUp/GetKey
			for (int i=0;i<159;i++) {
				if (i == 139) {
					if (Input.GetKeyDown(KeyCode.CapsLock)) goodkey = true;
				} else if (i == 153) {
					if (_getInput.MouseWheelUp()) goodkey = true;
				} else if (i == 154) {
					if (_getInput.MouseWheelDn()) goodkey = true;
				} else {
					if (Input.GetKeyUp(_consts.InputValues[i])) goodkey = true;
				}

				if (goodkey) {
					selfText.text = _consts.InputConfigNames[i];
					_consts.InputCodeSettings[index] = i;
					_config.WriteConfig();
					enterMode = false;
					firstFrame = true;
					CheckAndHandleConflicts(i);
					return;
				}
			}
		} else {
			UpdateText();
		}
	}

	public void UpdateText() { // Called by Start also.
		if (selfText != null) {
			selfText.text = _consts.InputConfigNames[_consts.InputCodeSettings[index]];
			if (_consts.InputCodeSettings[index] == 109) selfText.color = _consts.ssRedText;
			else selfText.color = _consts.ssGreenText;
		}
	}
}
