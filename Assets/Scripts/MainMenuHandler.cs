using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using Citadel.Game;
using Zenject;
using SimpleFileBrowser;

public class MainMenuHandler : MonoBehaviour {
	public GameObject Button1;
	public GameObject Button2;
	public GameObject Button3;
	public GameObject Button4;	
	public GameObject startFXObject;
	public GameObject saltTheFries;
	public GameObject mainCamera;
	public GameObject singleplayerPage;
	public GameObject multiplayerPage;
	public GameObject newgamePage;
	public GameObject frontPage;
	public GameObject loadPage;
	public GameObject savePage;
	public GameObject optionsPage;
	public GameObject creditsPage;
	public GameObject CouldNotFindDialogue;
	public GameObject SuccessBanner;
	public GameObject FailureBanner;
	public GameObject InitialDisplay;
	public InputField dataPathInputText;
	public InputField newgameInputText;
	public StartMenuDifficultyController combat;
	public StartMenuDifficultyController mission;
	public StartMenuDifficultyController puzzle;
	public StartMenuDifficultyController cyber;
	public InputField[] saveNameInputField;
	public GameObject[] saveNameInput;
	public GameObject[] saveNamePlaceholder;
	public Text[] saveButtonText;
	public Text[] loadButtonText;
	public int currentSaveSlot = -1;
	public CreditsScroll credScrollManager;
	public GameObject IntroVideo;
	public GameObject IntroVideoContainer;
	public bool inCutscene = false;
	public GameObject PresetConfirmDialog;
	public Text presetQuestionText;
	public ConfigKeybindButton[] keybindButtons;
	public ConfigToggles ctInvertUpDnLook;
	public ConfigToggles ctInvertUpDnCyberLook;
	public ConfigToggles ctInvertInventoryCyc;
	public ConfigToggles ctQuickItemPickUp;
	public ConfigToggles ctQuickReload;
	public ConfigToggles ctNoShootMode;
	public GameObject introVideoTextGO1;
	public GameObject introVideoTextGO2;
	public GameObject introVideoTextGO3;
	public GameObject introVideoTextGO4;
	public GameObject introVideoTextGO5;
	public GameObject introVideoTextGO6;
	public GameObject introVideoTextGO7;
	public GameObject introVideoTextGO8;
	public GameObject introVideoTextGO9;
	public GameObject introVideoTextGO10;
	public GameObject introVideoTextGO11;
	public GameObject introVideoTextGO12;
	public GameObject introVideoTextGO13;
	public GameObject introVideoTextGO14;
	public GameObject introVideoTextGO15;
	public Text introVideoText1;
	public Text introVideoText2;
	public Text introVideoText3;
	public Text introVideoText4;
	public Text introVideoText5;
	public Text introVideoText6;
	public Text introVideoText7;
	public Text introVideoText8;
	public Text introVideoText9;
	public Text introVideoText10;
	public Text introVideoText11;
	public Text introVideoText12;
	public Text introVideoText13;
	public Text introVideoText14;
	public Text introVideoText15;
	public VideoPlayer introPlayer;

	public GameObject DeathVideo;
	public GameObject DeathVideoContainer;
	public VideoPlayer deathPlayer;
	public GameObject deathVideoTextGO1;
	public GameObject deathVideoTextGO2;
	public Text deathVideoText1;
	public Text deathVideoText2;

	public GameObject GraphicsTab;
	public GameObject InputTab;
	public GameObject AudioTab;
	public Image GraphicsTabButtonImage;
	public Image InputTabButtonImage;
	public Image AudioTabButtonImage;
	public Text GraphicsTabButtonText;
	public Text InputTabButtonText;
	public Text AudioTabButtonText;
	public Sprite OptionsTabDehilited;
	public Sprite OptionsTabHilited;
	public Camera configCamera;
	public AudioSource BackGroundMusic;
	public ConfigurationMenuAAApply aaaApply;
	public ConfigurationMenuShadowsApply shadApply;
	public ConfigurationMenuSSRApply ssrApply;
	public ConfigurationMenuAudioModeApply audModeApply;

	[HideInInspector] public bool returnToPause = false;
	[HideInInspector] public bool fileBrowserOpen = false;
	public bool dataFound = false;
	private enum Pages : byte {fp,sp,mp,np,lp,op,sv,cd};
	private Pages currentPage;
	private bool typingSaveGame = false;
	private string tempSaveNameHolder;
	private int presetQuestionValue = -1;
	private float vidFinished;
	private const float vidLength = 117.5f;
	private const float deathvidLength = 16.8f;
	private float vidStartTime;

	[Inject] private Const _consts;
	[Inject] private Config _config;
	[Inject] private MFDManager _mfdManager;
	[Inject] private Inventory _inventory;
	[Inject] private MissionTimer _missionTimer;
	[Inject] private MouseCursor _mouseCursor;
	[Inject] private MouseLookScript _mouseLookScript;
	[Inject] private Music _music;
	[Inject] private PauseScript _pauseScript;
	[Inject] private DynamicCulling _dynamicCulling;

	private void Awake()
	{
		BackGroundMusic.ignoreListenerPause = true; // Play when paused.
		ResetPages();
		dataFound = false;
		inCutscene = false;
#if UNITY_ANDROID
		dataFound = true;
		_config.SetVolume();
		GoToFrontPage();
		CheckAndPlayIntro();
#else
		_config.SetVolume();
		FileBrowser.SetFilters(false,new FileBrowser.Filter("SHOCK RES Files",
															".RES",".res"));
		FileBrowser.SetDefaultFilter( ".RES" );
		StartCoroutine(CheckDataFiles());
#endif
	}

	// Improve menu performance.
	void DisableCameraDuringMenu() {
		if (_mouseLookScript == null) return;

		_mouseLookScript.playerCamera.enabled = false; 
	}

	void ReEnableCamera() {
		if (_mouseLookScript == null) return;

		_mouseLookScript.playerCamera.enabled = true;
		//UnityEngine.Debug.Log("Camera reenabled");
	}

	void OnEnable() {
		if (_inventory != null) _inventory.HideBioMonitor();
		DisableCameraDuringMenu();
		if (IntroVideoContainer.activeSelf) {
			vidFinished = Time.time + vidLength;
			vidStartTime = Time.time;

			// Setup text.
			introVideoText1.text = _consts.stringTable[613];
			introVideoText2.text = _consts.stringTable[614];
			introVideoText3.text = _consts.stringTable[615];
			introVideoText4.text = _consts.stringTable[616];
			introVideoText5.text = _consts.stringTable[617];
			introVideoText6.text = _consts.stringTable[618];
			introVideoText7.text = _consts.stringTable[619];
			introVideoText8.text = _consts.stringTable[620];
			introVideoText9.text = _consts.stringTable[621];
			introVideoText10.text = _consts.stringTable[622];
			introVideoText11.text = _consts.stringTable[623];
			introVideoText12.text = _consts.stringTable[624];
			introVideoText13.text = _consts.stringTable[625];
			introVideoText14.text = _consts.stringTable[626];
			introVideoText15.text = _consts.stringTable[627];
			Utils.Activate(introVideoTextGO1);
			Utils.Deactivate(introVideoTextGO2);
			Utils.Deactivate(introVideoTextGO3);
			Utils.Deactivate(introVideoTextGO4);
			Utils.Deactivate(introVideoTextGO5);
			Utils.Deactivate(introVideoTextGO6);
			Utils.Deactivate(introVideoTextGO7);
			Utils.Deactivate(introVideoTextGO8);
			Utils.Deactivate(introVideoTextGO9);
			Utils.Deactivate(introVideoTextGO10);
			Utils.Deactivate(introVideoTextGO11);
			Utils.Deactivate(introVideoTextGO12);
			Utils.Deactivate(introVideoTextGO13);
			Utils.Deactivate(introVideoTextGO14);
			Utils.Deactivate(introVideoTextGO15);
		}
	}

	void OnDisable() {
		if (_inventory != null) _inventory.UnHideBioMonitor();
		ReEnableCamera();
	}
	
	void ClearVideoRT() {
		RenderTexture.active = introPlayer.targetTexture;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = null;
	}
	
	void CheckAndPlayIntro() {
        string basePath = Utils.GetAppropriateDataPath();
		string indn = Utils.SafePathCombine(basePath,"introdone.dat");
		if (System.IO.File.Exists(indn)) {
			IntroVideo.SetActive(false);
			ClearVideoRT();
			IntroVideoContainer.SetActive(false);
			BackGroundMusic.clip = _music.titleMusic;
			if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
		} else {
			System.IO.File.Create(indn);
			PlayIntro();
		}
	}

	IEnumerator CheckDataFiles () {
		BackGroundMusic.Stop();
		string basePath = Utils.GetAppropriateDataPath();
		string alogPath = Utils.SafePathCombine(basePath,"CITALOG.RES");
		if (File.Exists(alogPath)) {
			// Go right on into the game, all good here.
			InitialDisplay.SetActive(false);
			dataFound = true;
			_config.SetVolume();
			GoToFrontPage();
			CheckAndPlayIntro();
		} else {
			// Fake like we are checking for the files to be there.
			// It's fake because we already did it and it is instant.
			// This is just to show the user that we did in fact look.
			InitialDisplay.SetActive(true);
			IntroVideo.SetActive(false);
			ClearVideoRT();
			IntroVideoContainer.SetActive(false);
			yield return new WaitForSeconds(0.3f);

			// OK, now show that we didn't find them
			InitialDisplay.SetActive(false);
			CouldNotFindDialogue.SetActive(true);
			dataFound = false;
		}
	}

	void LeaveDeathCutscene() {
		inCutscene = false;
		DeathVideo.SetActive(false);
		DeathVideoContainer.SetActive(false);
		ClearVideoRT();
		BackGroundMusic.clip = _music.titleMusic;
		if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
	}

	void LeaveIntroCutscene() {
		inCutscene = false;
		IntroVideo.SetActive(false);
		ClearVideoRT();
		IntroVideoContainer.SetActive(false);
		_consts.WriteDatForIntroPlayed(false);
		BackGroundMusic.clip = _music.titleMusic;
		if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)
			|| Input.GetMouseButtonUp(1)
			|| Input.GetKeyDown(KeyCode.Escape)
			|| Input.GetKeyDown(KeyCode.JoystickButton0)
			|| Input.GetKeyDown(KeyCode.JoystickButton1)
			|| Input.anyKey) {
			if ((inCutscene || IntroVideoContainer.activeSelf
				|| DeathVideoContainer.activeSelf)
				&& !CouldNotFindDialogue.activeSelf) {
				
				if (IntroVideoContainer.activeSelf && (Time.time - vidStartTime) > 1.5f) {
					LeaveIntroCutscene();
				} else if (DeathVideoContainer.activeSelf && (Time.time - vidStartTime) > 1.5f) {
					LeaveDeathCutscene();
				}

				return;
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape)
			|| Input.GetKeyDown(KeyCode.JoystickButton1)) { // Escape/back
															// button listener
			if (savePage.activeInHierarchy && !newgamePage.activeInHierarchy) {
				if (currentSaveSlot > 0
					&& currentSaveSlot < saveNameInputField.Length) {
					InputField infld = saveNameInputField[currentSaveSlot];
					if (infld != null) infld.DeactivateInputField();
				}
				currentSaveSlot = -1;
				typingSaveGame = false;
				returnToPause = true;
			}

			GoBack();
			return;
		}

		if (IntroVideoContainer.activeSelf) {
			if (vidFinished > 0 && (Time.time - vidStartTime) > 6.7f
				&& introVideoTextGO1.activeSelf
				&& !introVideoTextGO2.activeSelf) {

				Utils.Deactivate(introVideoTextGO1);
				Utils.Activate(introVideoTextGO2);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 9.9f
				&& introVideoTextGO2.activeSelf
				&& !introVideoTextGO3.activeSelf) {

				Utils.Deactivate(introVideoTextGO2);
				Utils.Activate(introVideoTextGO3);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 19.2f
				&& introVideoTextGO3.activeSelf
				&& !introVideoTextGO4.activeSelf) {

				Utils.Deactivate(introVideoTextGO3);
				Utils.Activate(introVideoTextGO4);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 30.7f
				&& introVideoTextGO4.activeSelf
				&& !introVideoTextGO5.activeSelf) {

				Utils.Deactivate(introVideoTextGO4);
				Utils.Activate(introVideoTextGO5);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 37.9f
				&&  introVideoTextGO5.activeSelf
				&& !introVideoTextGO6.activeSelf) {

				Utils.Deactivate(introVideoTextGO5);
				Utils.Activate(  introVideoTextGO6);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 43.7f
				&&  introVideoTextGO6.activeSelf
				&& !introVideoTextGO7.activeSelf) {

				Utils.Deactivate(introVideoTextGO6);
				Utils.Activate(  introVideoTextGO7);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 48.1f
				&&  introVideoTextGO7.activeSelf
				&& !introVideoTextGO8.activeSelf) {

				Utils.Deactivate(introVideoTextGO7);
				Utils.Activate(  introVideoTextGO8);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 59.3f
				&&  introVideoTextGO8.activeSelf
				&& !introVideoTextGO9.activeSelf) {

				Utils.Deactivate(introVideoTextGO8);
				Utils.Activate(  introVideoTextGO9);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 69.1f
				&&  introVideoTextGO9.activeSelf
				&& !introVideoTextGO10.activeSelf) {

				Utils.Deactivate(introVideoTextGO9);
				Utils.Activate(  introVideoTextGO10);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 74.5f
				&&  introVideoTextGO10.activeSelf
				&& !introVideoTextGO11.activeSelf) {

				Utils.Deactivate(introVideoTextGO10);
				Utils.Activate(  introVideoTextGO11);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 81.2f
				&&  introVideoTextGO11.activeSelf
				&& !introVideoTextGO12.activeSelf) {

				Utils.Deactivate(introVideoTextGO11);
				Utils.Activate(  introVideoTextGO12);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 89.2f
				&&  introVideoTextGO12.activeSelf
				&& !introVideoTextGO13.activeSelf) {

				Utils.Deactivate(introVideoTextGO12);
				Utils.Activate(  introVideoTextGO13);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 98.4f
				&&  introVideoTextGO13.activeSelf
				&& !introVideoTextGO14.activeSelf) {

				Utils.Deactivate(introVideoTextGO13);
				Utils.Activate(  introVideoTextGO14);
			}

			if (vidFinished > 0 && (Time.time - vidStartTime) > 105.0f
				&&  introVideoTextGO14.activeSelf
				&& !introVideoTextGO15.activeSelf) {

				Utils.Deactivate(introVideoTextGO14);
				Utils.Activate(  introVideoTextGO15);
			}


			if (vidFinished < Time.time && IntroVideoContainer.activeSelf
				&& vidFinished > 0) {

				vidFinished = 0;
				Utils.Deactivate(IntroVideoContainer);
				Utils.Deactivate(introVideoTextGO1);
				Utils.Deactivate(introVideoTextGO2);
				Utils.Deactivate(introVideoTextGO3);
				Utils.Deactivate(introVideoTextGO4);
				Utils.Deactivate(introVideoTextGO5);
				Utils.Deactivate(introVideoTextGO6);
				Utils.Deactivate(introVideoTextGO7);
				Utils.Deactivate(introVideoTextGO8);
				Utils.Deactivate(introVideoTextGO9);
				Utils.Deactivate(introVideoTextGO10);
				Utils.Deactivate(introVideoTextGO11);
				Utils.Deactivate(introVideoTextGO12);
				Utils.Deactivate(introVideoTextGO13);
				Utils.Deactivate(introVideoTextGO14);
				Utils.Deactivate(introVideoTextGO15);
				ClearVideoRT();
			}
		} else if (DeathVideoContainer.activeSelf) {
			if (vidFinished > 0 && (Time.time - vidStartTime) > 5.53f
				&& deathVideoTextGO1.activeSelf
				&& !deathVideoTextGO2.activeSelf) {

				Utils.Deactivate(deathVideoTextGO1);
				Utils.Activate(deathVideoTextGO2);
				ClearVideoRT();
			}

			if (vidFinished < Time.time && DeathVideoContainer.activeSelf
				&& vidFinished > 0) {

				vidFinished = 0;
				Utils.Deactivate(DeathVideoContainer);
				Utils.Deactivate(deathVideoTextGO1);
				Utils.Deactivate(deathVideoTextGO2);
				ClearVideoRT();
				BackGroundMusic.clip = _music.titleMusic;
				if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
			}
		} else {
			if (!BackGroundMusic.isPlaying
				&& !saltTheFries.activeInHierarchy
				&& gameObject.activeSelf) {
				BackGroundMusic.clip = _music.titleMusic;
				if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
			}
		}

		// Qmaster's cheat
		if ((   (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.P))
			 || (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKey(KeyCode.P)))
			&& !CouldNotFindDialogue.activeInHierarchy) {
			if (string.IsNullOrWhiteSpace(_consts.playerName)) {
				_consts.playerName = "Qmaster";
			}

			StartGame(true);
			return;
		}

		if (typingSaveGame && (Input.GetKeyUp(KeyCode.Return)
							   || Input.GetKeyUp(KeyCode.KeypadEnter)
							   || Input.GetKeyDown(KeyCode.JoystickButton0))
			&& savePage.activeInHierarchy
			&& !newgamePage.activeInHierarchy) {
			if (currentSaveSlot < 0) return;

			InputField infldTemp = saveNameInputField[currentSaveSlot];
			string sname = infldTemp.text;
			if (!string.IsNullOrEmpty(sname)) {
				if (sname == "- unused -") sname = "Savegame: - unused - "
												   + currentSaveSlot.ToString();
				SaveGame(currentSaveSlot,sname);
				saveNameInput[currentSaveSlot].SetActive(false);
				saveNamePlaceholder[currentSaveSlot].SetActive(false);
				saveButtonText[currentSaveSlot].text = sname;
				typingSaveGame = false;
				currentSaveSlot = -1;
			} else {
				GoBack();
				return;
			}
		}

		UpdateConfigTabTextColor();
	}

	public void StartGame (bool isNew) {
		_consts.difficultyCombat = combat.difficultySetting;
		_consts.difficultyMission = mission.difficultySetting;
		_consts.difficultyPuzzle = puzzle.difficultySetting;
		_consts.difficultyCyber = cyber.difficultySetting;
		if (_consts.difficultyMission < 3) {
			_missionTimer.text.text = System.String.Empty;
			_missionTimer.timerTypeText.text = System.String.Empty;
			_mfdManager.overallMissionTimerT.SetActive(false);
			_mfdManager.overallMissionTimer.SetActive(false);
		} else {
			_mfdManager.overallMissionTimerT.SetActive(true);
			_mfdManager.overallMissionTimer.SetActive(true);
		}
		
		if (isNew) {
			string pname = newgamePage.GetComponentInChildren<InputField>(true).text;
			if (string.IsNullOrWhiteSpace(pname)) pname = "Hacker";
			_consts.playerName = pname;
			_consts.NewGame();
		}

		_mouseCursor.mainCamera.enabled = true;
		this.gameObject.SetActive(false);
	}

	void ResetPages() {
		singleplayerPage.SetActive(false);
		multiplayerPage.SetActive(false);
		newgamePage.SetActive(false);
		frontPage.SetActive(false);
		loadPage.SetActive(false);
		savePage.SetActive(false);
		optionsPage.SetActive(false);
		creditsPage.SetActive(false);
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void GoToFrontPage() {
		ResetPages();
		frontPage.SetActive(true);
		currentPage = Pages.fp;
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void GoToSingleplayerSubmenu() {
		ResetPages();
		singleplayerPage.SetActive(true);
		currentPage = Pages.sp;
		Input.ResetInputAxes();
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void GoToMultiplayerSubmenu() {
		ResetPages();
		multiplayerPage.SetActive(true);
		currentPage = Pages.mp;
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void GoToOptionsSubmenu(bool accessedFromPause) {
		ResetPages();
		if (accessedFromPause) IntroVideoContainer.SetActive(false);
		optionsPage.SetActive(true);
		SetOptionsTabGraphics();
		currentPage = Pages.op;
		returnToPause = accessedFromPause;
		RenderConfigView();
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void SetOptionsTabGraphics() {
		GraphicsTab.SetActive(true);
		InputTab.SetActive(false);
		AudioTab.SetActive(false);
		GraphicsTabButtonImage.overrideSprite = OptionsTabHilited;
		InputTabButtonImage.overrideSprite = OptionsTabDehilited;
		AudioTabButtonImage.overrideSprite = OptionsTabDehilited;
		UpdateConfigTabTextColor();
	}

	public void SetOptionsTabInput() {
		GraphicsTab.SetActive(false);
		InputTab.SetActive(true);
		AudioTab.SetActive(false);
		GraphicsTabButtonImage.overrideSprite = OptionsTabDehilited;
		InputTabButtonImage.overrideSprite = OptionsTabHilited;
		AudioTabButtonImage.overrideSprite = OptionsTabDehilited;
		UpdateConfigTabTextColor();
	}

	public void SetOptionsTabAudio() {
		GraphicsTab.SetActive(false);
		InputTab.SetActive(false);
		AudioTab.SetActive(true);
		GraphicsTabButtonImage.overrideSprite = OptionsTabDehilited;
		InputTabButtonImage.overrideSprite = OptionsTabDehilited;
		AudioTabButtonImage.overrideSprite = OptionsTabHilited;
		UpdateConfigTabTextColor();
	}

	void UpdateConfigTabTextColor() {
		if (GraphicsTab.activeInHierarchy) {
			GraphicsTabButtonText.color = _consts.ssYellowText;
			InputTabButtonText.color = _consts.ssGreenText;
			AudioTabButtonText.color = _consts.ssGreenText;
		} else if (InputTab.activeInHierarchy) {
			GraphicsTabButtonText.color = _consts.ssGreenText;
			InputTabButtonText.color = _consts.ssYellowText;
			AudioTabButtonText.color = _consts.ssGreenText;
		} else if (AudioTab.activeInHierarchy) {
			GraphicsTabButtonText.color = _consts.ssGreenText;
			InputTabButtonText.color = _consts.ssGreenText;
			AudioTabButtonText.color = _consts.ssYellowText;
		}
	}

	public IEnumerator RenderConfigViewDelay() {
		yield return null;
		if (!optionsPage.activeInHierarchy) yield break;
		if (!GraphicsTab.activeInHierarchy) yield break;

// 		configCamera.targetTexture.Release();
// 		configCamera.targetTexture.width = Screen.width;
// 		configCamera.targetTexture.height = Screen.height;
		configCamera.fieldOfView = _consts.player1CapsuleMainCameragGO.GetComponent<Camera>().fieldOfView;
		Grayscale gsc = configCamera.gameObject.GetComponent<Grayscale>();
		Grayscale gscMain = _consts.player1CapsuleMainCameragGO.GetComponent<Camera>().GetComponent<Grayscale>();
		if (gsc != null && gscMain != null) gsc.enabled = gscMain.enabled;

		_config.SetSSAO();
		_dynamicCulling.CullCore();
		configCamera.Render();
// 		if (_consts.GraphicsSEGI) {
// 			yield return null;
// 			configCamera.Render();
// 		}
	}

	public void RenderConfigView() {
		StartCoroutine(RenderConfigViewDelay());
	}

	public void GoToNewGameSubmenu() {
		ResetPages();
		newgamePage.SetActive(true);
		newgameInputText.ActivateInputField();
		currentPage = Pages.np;
	}

	string GetSaveName(int index) {
		string savName = "sav" + index.ToString() + ".txt";
		string basePath = Utils.GetAppropriateDataPath();
		string sP = Utils.SafePathCombine(basePath,savName);
		string retval = "! unknown !";
		Utils.ConfirmExistsMakeIfNot(basePath,savName);
		StreamReader sf = new StreamReader(sP);
		if (sf == null) {
			Debug.Log("GetSaveName error! sf null");
			return retval;
		}

		using (sf) {
			retval = sf.ReadLine();
			if (retval == null) {
				Debug.Log("GetSaveName error! retval null");
				return "! unknown !"; // just in case
			}

			sf.Close();
		}

		Debug.Log("GetSaveName retval: " + retval);
		return retval;
	}

	public void GoToLoadGameSubmenu (bool accessedFromPause) {
		ResetPages();
		loadPage.SetActive(true);
		currentPage = Pages.lp;
		returnToPause = accessedFromPause;
		for (int i=0;i<8;i++) {
			loadButtonText[i].text = GetSaveName(i);
		}	
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void GoToSaveGameSubmenu (bool accessedFromPause) {
		ResetPages();
		savePage.SetActive(true);
		currentPage = Pages.sv;
		returnToPause = accessedFromPause;
		for (int i=0;i<8;i++) {
			saveButtonText[i].text = GetSaveName(i);
		}	
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void SaveGameEntry (int index) {
		currentSaveSlot = index;
		typingSaveGame = true;
		saveNameInput[index].SetActive(true);
		saveNamePlaceholder[index].SetActive(true);
		saveButtonText[index].text = System.String.Empty;
		tempSaveNameHolder = saveButtonText[index].text;
		saveButtonText[index].text = System.String.Empty;
		saveNameInputField[index].ActivateInputField();
	}

	public void SaveQuickSaveButton () {
		SaveGame(7,"quicksave");
	}

	public void SaveGame(int index,string savename) {
		_consts.StartSave(index,savename);
		_consts.sprint(_consts.stringTable[28] + index.ToString() + "!",_consts.Player);
		_pauseScript.EnablePauseUI();
		_mouseCursor.mainCamera.enabled = true;
		this.gameObject.SetActive(false);
	}

	public void LoadGame(int index) {
		if (loadButtonText[index].text == "- unused -"
			|| loadButtonText[index].text == "- unused quicksave -") {
			_consts.sprint(_consts.stringTable[1022]); // "No data to load."
		} else _consts.Load(index,false);
	}

	public void GoBack () {
		EventSystem.current.SetSelectedGameObject(null);
		if (typingSaveGame) {
			saveNameInput[currentSaveSlot].SetActive(false);
			saveNamePlaceholder[currentSaveSlot].SetActive(false);
			typingSaveGame = false;
			loadButtonText[currentSaveSlot].text = tempSaveNameHolder;
			currentSaveSlot = -1;
			return;
		}

		if (returnToPause) {
			_pauseScript.ExitSaveDialog();
			ResetPages();
			returnToPause = false;
			this.gameObject.SetActive(false);
			return;
		}

		if (currentPage == Pages.sv) {
			GoToFrontPage();
			return;
		}

		// Go Back to front page
		if (currentPage == Pages.sp || currentPage == Pages.mp || currentPage == Pages.op) {
			GoToFrontPage();
			return;
		}

		// Go Back to singlepayer page
		if (currentPage == Pages.np || currentPage == Pages.lp || currentPage == Pages.cd) {
			if (currentPage == Pages.cd) {
				BackGroundMusic.clip = _music.titleMusic;
				if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
			}
			GoToSingleplayerSubmenu();
			return;
		}
	}

	public void PathSearch() {
		// Open dialogue to search for path to C:\SHOCK\RES\DATA
		StartCoroutine(ShowSelectPathCoroutine());
	}

	IEnumerator ShowSelectPathCoroutine () {
		// Show a select path dialog and wait for a response from user
		// Path folder: folder, Allow multiple selection: false
		// Initial path: default (Documents), Title: "Load File", submit button
		// text: "Load"
		fileBrowserOpen = true;
		yield return FileBrowser.WaitForLoadDialog(true,false,
												   System.String.Empty,
												   "Select Path","Select");
		
		fileBrowserOpen = false;
		// Dialog is closed
		// Print whether the user has selected a folder path or cancelled the
		// operation (FileBrowser.Success).
		if (FileBrowser.Success) {
			// Set the folder path only, e.g. C:\SHOCK\RES\DATA
			dataPathInputText.text = FileBrowser.Result[0];
		}
	}

	// Linked in Inspector to dataPathInputText.
	public void CopyFromPath() {
		// Copy CITALOG.RES and CITBARK.RES from data path if they exist
		string basePath = Utils.GetAppropriateDataPath();
		string fromPath = Utils.SafePathCombine(dataPathInputText.text,"CITALOG.RES");

		// Must have both to get audio logs and SHODAN barks.
		if (File.Exists(fromPath)) {
			dataFound = true;
			string toPath = Utils.SafePathCombine(basePath,"CITALOG.RES");
			File.Copy(fromPath,toPath,true); // Set overwrite to true in case we have 1 and not the other.
		}

		StartCoroutine(CopyPathCheck());
	}

	IEnumerator CopyPathCheck() {
		if (dataFound) {
			SuccessBanner.SetActive(true);
			CouldNotFindDialogue.SetActive(false);
			_config.SetVolume();
			yield return new WaitForSeconds(0.5f);

			GoToFrontPage();
			CheckAndPlayIntro();
		} else {
			CouldNotFindDialogue.SetActive(false);
			FailureBanner.SetActive(true);
			yield return new WaitForSeconds(2f);
			
			FailureBanner.SetActive(false);
			CouldNotFindDialogue.SetActive(true);
		}
	}

	public void CloseDataFileNotification() {
		// Close data file notification without finding sound files CITALOG.RES and CITBARK.RES from data path
		CouldNotFindDialogue.SetActive(false);
		_config.SetVolume(); // probably not needed here, but just in case
		GoToFrontPage();
		CheckAndPlayIntro();
	}

	public void PlayDeathVideo() {
		DeathVideoContainer.SetActive(true);
		DeathVideo.SetActive(true);
		deathPlayer.Play();
		deathPlayer.SetDirectAudioMute(0,true);
		deathVideoText1.text = _consts.stringTable[628];
		deathVideoText2.text = _consts.stringTable[629];
		Utils.Activate(deathVideoTextGO1);
		Utils.Deactivate(deathVideoTextGO2);
		gameObject.SetActive(true);
		BackGroundMusic.clip = _music.levelMusicDeath;
		if (dataFound) BackGroundMusic.Play();
		vidFinished = Time.time + deathvidLength;
		vidStartTime = Time.time;
	}

	public void PlayIntro() {
		_consts.WriteDatForIntroPlayed(false);
		IntroVideoContainer.SetActive(true);
		IntroVideo.SetActive(true);
		introPlayer.Play();
		if (!dataFound) introPlayer.SetDirectAudioMute(0,true);
		else introPlayer.SetDirectAudioMute(0,false);

		inCutscene = true;
		BackGroundMusic.Stop();
		vidFinished = Time.time + vidLength;
		vidStartTime = Time.time;

		// Setup text.
		introVideoText1.text = _consts.stringTable[613];
		introVideoText2.text = _consts.stringTable[614];
		introVideoText3.text = _consts.stringTable[615];
		introVideoText4.text = _consts.stringTable[616];
		introVideoText5.text = _consts.stringTable[617];
		introVideoText6.text = _consts.stringTable[618];
		introVideoText7.text = _consts.stringTable[619];
		introVideoText8.text = _consts.stringTable[620];
		introVideoText9.text = _consts.stringTable[621];
		introVideoText10.text = _consts.stringTable[622];
		introVideoText11.text = _consts.stringTable[623];
		introVideoText12.text = _consts.stringTable[624];
		introVideoText13.text = _consts.stringTable[625];
		introVideoText14.text = _consts.stringTable[626];
		introVideoText15.text = _consts.stringTable[627];
		Utils.Activate(introVideoTextGO1);
		Utils.Deactivate(introVideoTextGO2);
		Utils.Deactivate(introVideoTextGO3);
		Utils.Deactivate(introVideoTextGO4);
		Utils.Deactivate(introVideoTextGO5);
		Utils.Deactivate(introVideoTextGO6);
		Utils.Deactivate(introVideoTextGO7);
		Utils.Deactivate(introVideoTextGO8);
		Utils.Deactivate(introVideoTextGO9);
		Utils.Deactivate(introVideoTextGO10);
		Utils.Deactivate(introVideoTextGO11);
		Utils.Deactivate(introVideoTextGO12);
		Utils.Deactivate(introVideoTextGO13);
		Utils.Deactivate(introVideoTextGO14);
		Utils.Deactivate(introVideoTextGO15);
	}

	public void PlayCredits () {
		ResetPages();
		creditsPage.SetActive(true);
		currentPage = Pages.cd;
		if (_consts.DynamicMusic) {
			BackGroundMusic.clip = _music.creditsMusic;
		} else {
			BackGroundMusic.clip = _music.levelMusicLooped;
		}

		if (gameObject.activeSelf && dataFound) BackGroundMusic.Play();
	}

	public void SetConfigPreset(int index) {
		presetQuestionValue = index;

		if (presetQuestionValue == 1)  presetQuestionText.text = _consts.stringTable[924]; // CHANGE ALL KEYS TO LEGACY PRESET?
		else presetQuestionText.text = _consts.stringTable[923]; // RESET ALL KEYS TO DEFAULT?

		PresetConfirmDialog.SetActive(true);
	}

	public void CancelPresetSet() {
		presetQuestionValue = -1;
		PresetConfirmDialog.SetActive(false);
	}
	
	public void ApplyPreset() {
		switch (presetQuestionValue) {
			case 0: // Default
				_consts.InputCodeSettings[0] = 22; // Forward = w
				_consts.InputCodeSettings[1] = 0; // Strafe Left = a
				_consts.InputCodeSettings[2] = 18; // Backpedal = s
				_consts.InputCodeSettings[3] = 3; // Strafe Right = d
				_consts.InputCodeSettings[4] = 87; // Jump = space
				_consts.InputCodeSettings[5] = 2; // Crouch = c
				_consts.InputCodeSettings[6] = 23; // Prone = x
				_consts.InputCodeSettings[7] = 16; // Lean Left = q
				_consts.InputCodeSettings[8] = 4; // Lean Right = e
				_consts.InputCodeSettings[9] = 46; // Sprint = left shift
				_consts.InputCodeSettings[10] = 139; // Toggle Sprint = capslock
				_consts.InputCodeSettings[11] = 38; // Turn Left = left
				_consts.InputCodeSettings[12] = 39; // Turn Right = right
				_consts.InputCodeSettings[13] = 36; // Look Up = up
				_consts.InputCodeSettings[14] = 37; // Look Down = down
				_consts.InputCodeSettings[15] = 20; // Recent Log = u
				_consts.InputCodeSettings[16] = 26; // Biomonitor = 1
				_consts.InputCodeSettings[17] = 27; // Sensaround = 2
				_consts.InputCodeSettings[18] = 28; // Lantern = 3
				_consts.InputCodeSettings[19] = 29; // Shield = 4
				_consts.InputCodeSettings[20] = 30; // Infrared = 5
				_consts.InputCodeSettings[21] = 31; // Email = 6
				_consts.InputCodeSettings[22] = 32; // Booster = 7
				_consts.InputCodeSettings[23] = 33; // Jumpjets = 8
				_consts.InputCodeSettings[24] = 53; // Attack = mouse 0
				_consts.InputCodeSettings[25] = 54; // Use = mouse 1
				_consts.InputCodeSettings[26] = 86; // Menu/Back = escape
				_consts.InputCodeSettings[27] = 84; // Toggle Mode = tab
				_consts.InputCodeSettings[28] = 17; // Reload = r
				_consts.InputCodeSettings[29] = 153; // Weapon + = mwheel up
				_consts.InputCodeSettings[30] = 154; // Weapon - = mwheel dn
				_consts.InputCodeSettings[31] = 6; // Grenade = g
				_consts.InputCodeSettings[32] = 19; // Grenade + = t
				_consts.InputCodeSettings[33] = 1; // Grenade - = b
				_consts.InputCodeSettings[34] = 21; // Ammo Type = v
				_consts.InputCodeSettings[35] = 109; // Unused
				_consts.InputCodeSettings[36] = 9; // Patch Use = j
				_consts.InputCodeSettings[37] = 8; // Patch + = i
				_consts.InputCodeSettings[38] = 133; // Patch - = ,
				_consts.InputCodeSettings[39] = 12; // Full Map = m
				_consts.NoShootMode = false;
				_consts.InputQuickReloadWeapons = true;
				_consts.InputQuickItemPickup = false;
				break;
			case 1: // Legacy SS1
				_consts.InputCodeSettings[0] = 18; // Forward = s
				_consts.InputCodeSettings[1] = 25; // Strafe Left = z
				_consts.InputCodeSettings[2] = 23; // Backpedal = x
				_consts.InputCodeSettings[3] = 2; // Strafe Right = c
				_consts.InputCodeSettings[4] = 87; // Jump = space
				_consts.InputCodeSettings[5] = 6; // Crouch = g
				_consts.InputCodeSettings[6] = 1; // Prone = b
				_consts.InputCodeSettings[7] = 16; // Lean Left = q
				_consts.InputCodeSettings[8] = 4; // Lean Right = e
				_consts.InputCodeSettings[9] = 46; // Sprint = left shift
				_consts.InputCodeSettings[10] = 139; // Toggle Sprint = capslock
				_consts.InputCodeSettings[11] = 0; // Turn Left = a
				_consts.InputCodeSettings[12] = 3; // Turn Right = d
				_consts.InputCodeSettings[13] = 17; // Look Up = r
				_consts.InputCodeSettings[14] = 21; // Look Down = v
				_consts.InputCodeSettings[15] = 20; // Recent Log = p
				_consts.InputCodeSettings[16] = 26; // Biomonitor = 1
				_consts.InputCodeSettings[17] = 28; // Sensaround = 3
				_consts.InputCodeSettings[18] = 29; // Lantern = 4
				_consts.InputCodeSettings[19] = 30; // Shield = 5
				_consts.InputCodeSettings[20] = 31; // Infrared = 6
				_consts.InputCodeSettings[21] = 33; // Email = 8
				_consts.InputCodeSettings[22] = 34; // Booster = 9
				_consts.InputCodeSettings[23] = 35; // Jumpjets = 0
				_consts.InputCodeSettings[24] = 54; // Use = mouse 1
				_consts.InputCodeSettings[25] = 53; // Attack = mouse 0
				_consts.InputCodeSettings[26] = 86; // Menu/Back = escape
				_consts.InputCodeSettings[27] = 84; // Toggle Mode = tab
				_consts.InputCodeSettings[28] = 19; // Reload = t
				_consts.InputCodeSettings[29] = 153; // Weapon + = mwheel up
				_consts.InputCodeSettings[30] = 154; // Weapon - = mwheel dn
				_consts.InputCodeSettings[31] = 7; // Grenade = h
				_consts.InputCodeSettings[32] = 24; // Grenade + = y
				_consts.InputCodeSettings[33] = 13; // Grenade - = n
				_consts.InputCodeSettings[34] = 10; // Ammo Type = k
				_consts.InputCodeSettings[35] = 109; // Unused
				_consts.InputCodeSettings[36] = 9; // Patch Use = j
				_consts.InputCodeSettings[37] = 8; // Patch + = i
				_consts.InputCodeSettings[38] = 133; // Patch - = ,
				_consts.InputCodeSettings[39] = 12; // Full Map = m
				_consts.NoShootMode = true;
				_consts.InputQuickReloadWeapons = false;
				_consts.InputQuickItemPickup = false;
				break;
		}
		presetQuestionValue = -1;	
		_config.WriteConfig(); // Save config.  Always set to autosave.
		for (int i=0;i<keybindButtons.Length;i++) {
			keybindButtons[i].UpdateText();
		}
		ctInvertUpDnLook.AlignWithConfigFile();
		ctInvertUpDnCyberLook.AlignWithConfigFile();
		ctInvertInventoryCyc.AlignWithConfigFile();
		ctQuickItemPickUp.AlignWithConfigFile();
		ctQuickReload.AlignWithConfigFile();
		ctNoShootMode.AlignWithConfigFile();
		CancelPresetSet();
	}

	public void Quit () {
		EventSystem.current.SetSelectedGameObject(null);
		StartCoroutine(quitFunction());
	}

	IEnumerator quitFunction () { // Handle exiting from menu option
		BackGroundMusic.Stop();
		_consts.WriteDatForIntroPlayed(false);
		saltTheFries.SetActive(true);
		yield return new WaitForSeconds(0.75f);
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#endif
			
		_config.SaveConfigToPlayerPrefs();
		Application.Quit();
	}
	
	void OnApplicationQuit() { // Handle X button close
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#endif
		_consts.WriteDatForIntroPlayed(false);
		_config.SaveConfigToPlayerPrefs();
		Utils.CopyLogFiles(false);
	}
	
	void OnDestroy() {
		Button1 = null;
		Button2 = null;
		Button3 = null;
		Button4 = null;
		startFXObject = null;
		saltTheFries = null;
		mainCamera = null;
		singleplayerPage = null;
		multiplayerPage = null;
		newgamePage = null;
		frontPage = null;
		loadPage = null;
		savePage = null;
		optionsPage = null;
		creditsPage = null;
		CouldNotFindDialogue = null;
		SuccessBanner = null;
		FailureBanner = null;
		InitialDisplay = null;
		dataPathInputText = null;
		newgameInputText = null;
		combat = null;
		mission = null;
		puzzle = null;
		cyber = null;
		saveNameInputField = null;
		saveNameInput = null;
		saveNamePlaceholder = null;
		saveButtonText = null;
		loadButtonText = null;
		credScrollManager = null;
		IntroVideo = null;
		IntroVideoContainer = null;
		PresetConfirmDialog = null;
		presetQuestionText = null;
		keybindButtons = null;
		ctInvertUpDnLook = null;
		ctInvertUpDnCyberLook = null;
		ctInvertInventoryCyc = null;
		ctQuickItemPickUp = null;
		ctQuickReload = null;
		ctNoShootMode = null;
		introVideoTextGO1 = null;
		introVideoTextGO2 = null;
		introVideoTextGO3 = null;
		introVideoTextGO4 = null;
		introVideoTextGO5 = null;
		introVideoTextGO6 = null;
		introVideoTextGO7 = null;
		introVideoTextGO8 = null;
		introVideoTextGO9 = null;
		introVideoTextGO10 = null;
		introVideoTextGO11 = null;
		introVideoTextGO12 = null;
		introVideoTextGO13 = null;
		introVideoTextGO14 = null;
		introVideoTextGO15 = null;
		introVideoText1 = null;
		introVideoText2 = null;
		introVideoText3 = null;
		introVideoText4 = null;
		introVideoText5 = null;
		introVideoText6 = null;
		introVideoText7 = null;
		introVideoText8 = null;
		introVideoText9 = null;
		introVideoText10 = null;
		introVideoText11 = null;
		introVideoText12 = null;
		introVideoText13 = null;
		introVideoText14 = null;
		introVideoText15 = null;
		introPlayer = null;
		DeathVideo = null;
		DeathVideoContainer = null;
		deathPlayer = null;
		deathVideoTextGO1 = null;
		deathVideoTextGO2 = null;
		deathVideoText1 = null;
		deathVideoText2 = null;
		GraphicsTab = null;
		InputTab = null;
		AudioTab = null;
		GraphicsTabButtonImage = null;
		InputTabButtonImage = null;
		AudioTabButtonImage = null;
		GraphicsTabButtonText = null;
		InputTabButtonText = null;
		AudioTabButtonText = null;
		OptionsTabDehilited = null;
		OptionsTabHilited = null;
		configCamera = null;
		BackGroundMusic = null;
		aaaApply = null;
		shadApply = null;
		ssrApply = null;
		audModeApply = null;
	}
}
