using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour {
	[Inject] private Const _consts;
	[Inject] private MainMenuHandler _mainMenuHandler;
	[Inject] private PauseScript _pauseScript;

	public void Targetted(UseData ud) {
		Debug.Log("Game finished!");
		_consts.gameFinished = true; // YAY WE DID IT!!!!
		_pauseScript.PauseEnable(); // Pauses game, no more to do
		_pauseScript.NoSavePauseQuit(); // quit to and enable main menu (exits the game to menu)
		_mainMenuHandler.PlayCredits(); // Play credits and set page in menu handler
	}
}