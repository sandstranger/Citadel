using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citadel.Game;
using Citadel.SceneManagement;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
	public const int MaxLevelsCount = 14;
	public const int NewGameLevelIndex = 1;

	public int[] levelSecurity;
	public int[] levelCameraCount;
	public int[] levelSmallNodeCount;
	public int[] levelLargeNodeCount;
	public int[] levelCameraDestroyedCount;
	public int[] levelSmallNodeDestroyedCount;
	public int[] levelLargeNodeDestroyedCount;
	public Transform[] ressurectionLocation;
	public bool[] ressurectionActive;
	public Door[] ressurectionBayDoor;
	public GameObject sky;
	public GameObject sun;
	public GameObject sunSprite;
	public bool superoverride = false;
	public GameObject saturn;
	public GameObject exterior;
	public GameObject exterior_shield;
	public MeshRenderer skyMR;
	public bool[] showSkyForLevel;
	public bool[] showExteriorForLevel;
	public bool[] showSaturnForLevel;
	public NPCSubManager npcsm;
	public Level levelScript;
	public GameObject geometryContainer;
	public GameObject npcContainer;
	public Material rtxEmissive;
	public Mesh sphereMesh;
	public SkyRotate skyRotate;
	public Material pipe_maint2_3_coolant;
	
	private bool getValparsed;
	private bool _levelDataLoaded = false;
	private int getValreadInt;
	private float getValreadFloat;
	private static readonly StringBuilder s1 = new(200*1024);
	private GameObject _dummyGameObject;

	[SerializeField]
	private Vector3[] _elevatorDestinations = {
		new (48.38161f, -45.00156f, -18.16852f),
		new (51.20589f, -28.30795f, -25.9704f),
		new (5.32389f, -28.38895f, 33.28318f),
		new (2.56813f, -15.496f, 13.00925f),
		new (5.066132f, -15.496f, -20.70875f),
		new (15.29313f, -15.496f, -10.29f),
		new (-1.289999f, 1.04f, 5.099999f),
		new (8.96f, 12.585f, -6.4383f),
		new (-16.534f, 13.873f, -39.7983f),
		new (-1.9688f, 33.6865f, -46.261f),
		new (-14.7078f, 33.7015f, -30.939f),
		new (59.5022f, 31.8125f, 35.773f),
		new (-58.3108f, 33.7345f, -38.3f),
		new (54.27659f, 33.7345f, -61.49423f),
		new (0.611199f, 35.6205f, 43.71f),
		new (-4.5088f, 35.6205f, 43.71f),
		new (-1.9628f, 33.7225f, -69.18999f),
		new (25.113f, 48.44443f, -10.649f),
		new (17.48f, 50.98643f, 56.21f),
		new (4.889f, 58.727f, 19.975f),
		new (9.983f, 96.922f, -41.9f),
		new (2.303f, 106.77f, -38.554f),
		new (42.453f, 136.007f, -6.534f),
		new (11.214f, 168.558f, -23.302f),
		new (17.77f, 195.747f, 18.103f),
		new (11.2683f, -55.45489f, 39.70428f)
	};
	
	[Inject]
	private readonly ConsoleEmulator _consoleEmulator;
	[Inject] 
	private readonly PlayerReferenceManager _playerReference;
	[Inject] private readonly Const _consts;
	[Inject] private readonly Config _config;
	[Inject] private readonly MFDManager _mfdManager;
	[Inject] private readonly Automap _automap;
	[Inject] private readonly GUIState _guiState;
	[Inject] private readonly Inventory _inventory;
	[Inject] private readonly Music _music;
	[Inject] private readonly PauseScript _pauseScript;
	[Inject] private readonly PlayerHealth _playerHealth;
	[Inject] private readonly PlayerMovement _playerMovement;
	[Inject] private readonly DynamicCulling _dynamicCulling;
	[Inject] private readonly QuestLogNotesManager _questLogNotesManager;
	[Inject] private readonly LightDistanceCuller _lightDistanceCuller;
	[Inject] private readonly IResourcesLoader _resourcesLoader;

	public static bool LoadLevelAfterSceneChanges { get; private set; }
	public static Vector3 TargetPosition { get; private set; } = Vector3.zero;
	public static SaveableObjectStringsStorage StaticObjectsSaveStrings { get; } = new();
	public static SaveableObjectStringsStorage DynamicObjectsSavestrings { get; } = new();
	public static int CurrentLevel = NewGameLevelIndex;

	void Awake () {
		if (CurrentLevel < 0) {
			if (_consts == null) return;
			if (_consts.player1CapsuleMainCameragGO == null) return;

			Camera cam = _consts.player1CapsuleMainCameragGO.GetComponent<Camera>();
			if (cam == null) return;

			cam.useOcclusionCulling = false; // For debug whiteroom
			return;
		}
		if (CurrentLevel < 0 || CurrentLevel > 12) return; // 12 because I don't think I support starting in cyberspace, 13, for testing.

		if (sky == null) Debug.Log("BUG: LevelManager missing manually assigned reference for sky.");
		else sky.SetActive(true);

		SetSkyVisible(1);
		if (ressurectionBayDoor.Length != 8) Debug.Log("BUG: LevelManager ressurectionBayDoor array length not equal to 8.");
		Time.timeScale = Const.defaultTimeScale;
		_levelDataLoaded = false;

		if (Const.StartingNewGame)
		{
			StaticObjectsSaveStrings.ResetSaveStrings();
			LoadDynamicObjectsSavestrings();
		}
	}

	public static bool LevNumInBounds(int levnum) {
		return (levnum >=0 && levnum < MaxLevelsCount); // 14 levels
	}

	public static bool LevNumIsNonCyber(int levnum) {
		return (levnum >=0 && levnum < 13); // 13 non-cyber levels
	}

	public static void ResetSaveStrings() {
		DynamicObjectsSavestrings.ResetSaveStrings();
	}
	
	// Used in a couple places, bit slow to return list but it's only part of
	// loads and transitions between levels.
	private static List<string> ReadDynamicObjectFileList(int lev) {
		List<string> readFileList = new List<string>();
		if (lev > (MaxLevelsCount - 1)) return readFileList;
		if (!LevNumInBounds(lev)) return readFileList;

		string dynName = "CitadelScene_dynamics_level"+lev.ToString()+".txt";
		StreamReader sf = Utils.ReadStreamingAsset(dynName);
		if (sf == null) { UnityEngine.Debug.Log("Dynamic objects filepath invalid"); return readFileList; }

		string readline;
		using (sf) {
			do {
				readline = sf.ReadLine();
				if (readline != null) {
					readFileList.Add(readline);
				}
			} while (!sf.EndOfStream);
			sf.Close();
		}
		
		return readFileList;
	}

	private static void LoadDynamicObjectsSavestrings() {
		ResetSaveStrings();		
		for (int i=0;i<MaxLevelsCount;i++) {			
			List<string> readFileList = ReadDynamicObjectFileList(i);
			for (int j=0;j<readFileList.Count;j++) {
				DynamicObjectsSavestrings[i].Add(readFileList[j]);
			}
		}
	}
	
	public void SetSkyVisible(int on) {
		// 0 = Sunlight only
		// 1 = Sky + Sun + exterior + saturn
		// -1 = Nothin much
		skyMR.enabled = (on > 0 && showSkyForLevel[CurrentLevel]);
		saturn.SetActive(on > 0 && showSaturnForLevel[CurrentLevel]);
		exterior.SetActive(on > 0 && showExteriorForLevel[CurrentLevel]);
		if (on == 1) Debug.Log("SkyVisible passed a 1, sky + sunlight");
		if (on == 0) Debug.Log("SkyVisible passed a 0, sunlight only");
		if (on == -1) Debug.Log("SkyVisible passed a -1, nope");
		sun.SetActive(_consts.GraphicsShadowMode >= 1 && on >= 0); // on == 0 is for Sunlight only!
		sunSprite.SetActive(on > 0 && showSaturnForLevel[CurrentLevel]);
		if (_consts == null) return;
		if (_consts.questData == null) return;
		
		exterior_shield.SetActive(on > 0 && showExteriorForLevel[CurrentLevel]
								  && _consts.questData.ShieldActivated);
	}

	public void CyborgConversionToggleForCurrentLevel() {
		if (!LevNumInBounds(CurrentLevel)) return;
	    
		if (CurrentLevel == 6) {
			if (ressurectionActive[CurrentLevel]) {
				ressurectionActive[CurrentLevel] = false;
				ressurectionActive[10] = false;
				ressurectionActive[11] = false;
				ressurectionActive[12] = false;
			} else {
				ressurectionActive[CurrentLevel] = true;
				ressurectionActive[10] = true;
				ressurectionActive[11] = true;
				ressurectionActive[12] = true;
			}
		} else {
			ressurectionActive[CurrentLevel] = !ressurectionActive[CurrentLevel]; // Toggle current level.
		}
	}

	public bool RessurectPlayer() {
		if (!ressurectionActive[CurrentLevel]) return false;

		if (CurrentLevel == 10 ||CurrentLevel == 11 ||CurrentLevel == 12) {
			LoadLevel(6,ressurectionLocation[CurrentLevel].position);
			ressurectionBayDoor[6].ForceClose();
		} else {
			if (CurrentLevel <= 7 && CurrentLevel >= 0) {
				ressurectionBayDoor[CurrentLevel].ForceClose();
			}

			if (CurrentLevel >= 0 || CurrentLevel < 13) {
				Transform plyr = _playerReference.playerCapsule.transform;
				Vector3 spot = ressurectionLocation[CurrentLevel].position;
				plyr.position = transform.TransformPoint(spot);
			}
		}

		// Activate death screen and readouts for
		// "BRAIN ACTIVITY SATISFACTORY..."            ya debatable right
		// etc. etc.
		_playerReference.playerDeathRessurectEffect.SetActive(true);
		_music.PlayTrack(CurrentLevel,TrackType.Revive,MusicType.Override);
		_playerMovement.ressurectingFinished = _pauseScript.relativeTime + 3f;
		return true;
	}

	// Make sure that unneeded objects are unloaded
	public void UnloadLevelData(int levnum) {
		if (!LevNumIsNonCyber(levnum)) return; // In a test or editor space.
		if (!_levelDataLoaded) return; // Already cleared.

		UnloadLevelLights(levnum);
		UnloadLevelGeometry(levnum);
 		UnloadLevelDynamicObjects(levnum,true);
		_levelDataLoaded = false;
		SaveLoad.numLightsWithShadows = 0;
	}

	// Make sure relevant data and objects are loaded in and present for the level.
	public async UniTask LoadLevelData(int levnum) {
		if (!LevNumInBounds(CurrentLevel)) { // In a test or editor space.
			_levelDataLoaded = true;
			return;
		}
		if (_levelDataLoaded) return; // Already loaded.

// 		Debug.Log("Loading level data for " + levnum.ToString());
		LoadLevelGeometry(levnum);
		await LoadStaticObjects(levnum);
		await LoadLevelDynamicObjects(levnum);
		await _music.LoadLevelMusic(levnum);
		_resourcesLoader.ReleaseAllAssets();
		_levelDataLoaded = true;
		UnityEngine.Debug.Log("Number of lights for level " + levnum.ToString() + " with shadows: " + SaveLoad.numLightsWithShadows.ToString());
	}

	public void ChangeGameScene(int levnum, Vector3? targetPosition = null, bool changeSceneForced = false)
	{
		if (!LevNumInBounds(levnum))
		{
			Debug.LogWarning("levnum out of bounds"); 
			return;
		}

		if (CurrentLevel == levnum && !changeSceneForced)
		{
			_consts.sprint(_consts.stringTable[9]);
			return;
		}

		if (!Const.StartingNewGame)
		{
			LoadLevelAfterSceneChanges = true;
			UnloadLevelDynamicObjects(CurrentLevel, true);
			SaveStaticObjects();
		}

		TargetPosition = targetPosition ?? Vector3.zero;
		CurrentLevel = levnum;
		ObjectContainmentSystem.ClearLists();
		ScenesLoader.LoadLevel(levnum);
	}
	
	public void LoadLevel(int levnum, Vector3 targetPosition, bool loadLevelForced = false)
	{
		_consts.QuitAfterSavingDone = false;
		_lightDistanceCuller.Clear();
		LoadLevelAfterSceneChanges = false;
		if (!LevNumInBounds(levnum)) { Debug.LogWarning("levnum out of bounds"); return; }

		// NOTE: Check this first since the button for the current level has a null destination.  This is fine and expected.
		if (CurrentLevel == levnum && !loadLevelForced)
		{
			_consts.sprint(_consts.stringTable[9]);
			_lightDistanceCuller.Rebuild();
			return;
		} //Already there

		_mfdManager.TurnOffElevatorPad();
// 		Debug.Log("Cleared GUI Over Button state from clicking on elevator button in MFD side pane");
		_guiState.ClearOverButton();
		if (targetPosition.x == 0 && targetPosition.y == 0 && targetPosition.z == 0) {
			switch(levnum) {
				case 0:  targetPosition = _elevatorDestinations[25]; break;
				case 1:  targetPosition =  _elevatorDestinations[0]; break;
				case 2:  targetPosition =  _elevatorDestinations[1]; break;
				case 3:  targetPosition =  _elevatorDestinations[3]; break;
				case 4:  targetPosition =  _elevatorDestinations[6]; break;
				case 5:  targetPosition =  _elevatorDestinations[7]; break;
				case 6:  targetPosition =  _elevatorDestinations[9]; break;
				case 7:  targetPosition = _elevatorDestinations[17]; break;
				case 8:  targetPosition = _elevatorDestinations[19]; break;
				case 9:  targetPosition = _elevatorDestinations[21]; break;
				case 10: targetPosition = _elevatorDestinations[22]; break;
				case 11: targetPosition = _elevatorDestinations[23]; break;
				case 12: targetPosition = _elevatorDestinations[24]; break;
			}
		}

		if (_questLogNotesManager != null) _questLogNotesManager.NotifyLevelChange(levnum);
 
		// Return to level from cyberspace.
		_playerReference.playerCapsule.transform.position = targetPosition;
		CurrentLevel = levnum; // Set current level to be the new level
		DisableAllNonOccupiedLevelsExcept(CurrentLevel);
		DynamicCulling.camPositions = new Dictionary<GameObject, Vector3>();
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		if (CurrentLevel == 2 && AutoSplitterData.missionSplitID == 0) {
			AutoSplitterData.missionSplitID++; // 1 - Medical split - we are now on level 2
			Debug.Log("AutoSplitterData missionSplitID incremented: " + AutoSplitterData.missionSplitID.ToString());
		}
		
		PostLoadLevelSetupSystems();
	}
	
	public UniTask LoadLevelFromSave(int levnum) {
		if (!LevNumInBounds(levnum)) return UniTask.CompletedTask;

// 		Debug.Log("LevelManager LoadLevelFromSave()");
	//	LoadLevelData(levnum); // Let this function check and load data if it isn't yet.
		CurrentLevel = levnum; // Set current level to be the new level
		DisableAllNonOccupiedLevelsExcept(CurrentLevel); // Unload last level.
		return PostLoadLevelSetupSystems();
	}

	private async UniTask PostLoadLevelSetupSystems() {
		_music.inCombat = false;
		_music.SFXMain.Stop();
		_music.SFXOverlay.Stop();
		_music.levelEntry = true;
		_playerHealth.radiationArea = false;
		_playerMovement.ladderState = 0;
		await LoadLevelData(CurrentLevel);
		_automap.SetAutomapExploredReference(CurrentLevel);
		_automap.automapBaseImage.overrideSprite = _automap.automapsBaseImages[CurrentLevel];
		_consts.ClearActiveAutomapOverlays(); // After other levels turned off.
		_consts.ResetPauseLists();
		SetSkyVisible(1);
		_config.SetLanguage(); // Update all translatable text.
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
	}

	public void DisableAllNonOccupiedLevelsExcept(int occupiedLevel) {
		return;
		/*
		for (int i=0;i<levels.Length;i++) {
			if (i == occupiedLevel) continue;

			UnloadLevelData(i);
			if (levels[i] != null) levels[i].SetActive(false);
		}*/
	}

	public GameObject GetCurrentDynamicContainer() { // Does not return null
        return levelScript.dynamicObjectsContainer;
	}

	public GameObject GetCurrentGeometryContainer() { // Does not return null
#if UNITY_ANDROID
		return _dummyGameObject;
#else		
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].geometryContainer;
		}

        return levelScripts[currentLevel].geometryContainer;
#endif
	}

	public GameObject GetCurrentLightsStaticImmutableContainer() {
		return levelScript.lightsStaticImmutable;
	}
	
	public GameObject GetCurrentStaticImmutableContainer() { // Does not return null
#if UNITY_ANDROID
		return _dummyGameObject;
#else		
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].staticObjectsImmutable;
		}

		return levelScripts[currentLevel].staticObjectsImmutable;
#endif
	}

	public GameObject GetCurrentStaticSaveableContainer() { // Does not return null
#if UNITY_ANDROID
		return _dummyGameObject;
#else		
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].staticObjectsSaveable;
		}

		return levelScripts[currentLevel].staticObjectsSaveable;
#endif
	}

	public GameObject GetCurrentDoorsContainer() { // Does not return null
		return levelScript.doorsStaticSaveable;
	}

	public GameObject GetCurrentLightsContainer() { // Does not return null
		return levelScript.lightsStaticImmutable;
	}
	
	public GameObject GetRequestedLightsStaticImmutableContainer(int index) {
		return levelScript.lightsStaticImmutable;
	}

	public GameObject GetRequestedLevelDynamicContainer(int index) {
        return levelScript.dynamicObjectsContainer;
	}

	public GameObject GetRequestedLevelNPCContainer(int index) {
        return npcContainer;
	}

	public int GetInstantiateParent(GameObject go, bool isNPC,
									PrefabIdentifier prefID) {

		Transform parTr = go.transform.parent;
		if (parTr == null) return -1;

		GameObject par = parTr.gameObject;
		// func_wall exception.
		if (prefID.constIndex == 517) {
			Transform parpar = par.transform.parent;
			if (parpar != null) par = par.transform.parent.gameObject;
		}

		if (par == null) return -1;

		if (isNPC && par == npcContainer)
		{
			return CurrentLevel;
		}

		if (levelScript && par == levelScript.dynamicObjectsContainer)
		{
			return CurrentLevel;
		}
		
		return -1;
	}

	public void SetInstantiateParent(int lev, GameObject go, bool isNPC) {
		if (!LevNumInBounds(lev)) return;

		Transform par = null;
		if (isNPC) {
			GameObject parNPC = GetRequestedLevelNPCContainer(lev);
			if (parNPC != null) par = parNPC.transform;
		} else {
			GameObject parDyn = GetRequestedLevelDynamicContainer(lev);
			if (parDyn != null) par = parDyn.transform;
		}

		if (par == null) return;
		if (go.transform.parent == par) return;

		go.transform.SetParent(par);
	}

	public int GetCurrentLevelSecurity() {
		if (_consts.difficultyMission < 1) return 0;
		if (!LevNumInBounds(CurrentLevel)) return 0;
		if (superoverride) return 0; // tee hee we are SHODAN, no security blocks in place
		return levelSecurity[CurrentLevel];
	}

	// Typical level
	// 4 CPU nodes
	// 20 cameras
	// 100% = 4x + 20y
	// Assuming that a good camera percentage is 2-3%, CPU % would be about 10-15 each
	public void ReduceCurrentLevelSecurity(SecurityType stype) {
		if (!LevNumIsNonCyber(CurrentLevel)) return;

		float camScore = 4;
		float nodeSmallScore = 10;
		float nodeLargeScore = 27;
		float secscoreTotal = (levelCameraCount[CurrentLevel] * camScore) + (levelSmallNodeCount[CurrentLevel] * nodeSmallScore) + (levelLargeNodeCount[CurrentLevel] * nodeLargeScore);
		//secscoreTotal = 106 for medical level
		float secDrop = camScore; // default to camScore
		switch (stype) {
			case SecurityType.None: return;
			case SecurityType.Camera: secDrop = ((camScore/secscoreTotal) * 100); levelCameraDestroyedCount[CurrentLevel]++; break; // 1 camera divided by the total, so 2/ say (40+60) = 2/100 = 0.02, or 2% using the example numbers above
			case SecurityType.NodeSmall: secDrop = ((nodeSmallScore/secscoreTotal) * 100); levelSmallNodeDestroyedCount[CurrentLevel]++; break;
			case SecurityType.NodeLarge: secDrop = ((nodeLargeScore/secscoreTotal) * 100); levelLargeNodeDestroyedCount[CurrentLevel]++; break;
		}
		levelSecurity [CurrentLevel] -= (int)secDrop;
		if (levelSecurity [CurrentLevel] < 0) levelSecurity [CurrentLevel] = 0;
		if ((levelLargeNodeDestroyedCount[CurrentLevel] == levelLargeNodeCount[CurrentLevel]) && (levelSmallNodeDestroyedCount[CurrentLevel] == levelSmallNodeCount[CurrentLevel]) && (levelCameraDestroyedCount[CurrentLevel] == levelCameraCount[CurrentLevel])) {
			levelSecurity[CurrentLevel] = 0;
		}
		_consts.sprint(_consts.stringTable[306] + levelSecurity[CurrentLevel].ToString() + _consts.stringTable[307]);

		// Notify quest log if all nodes were destroyed
		if (levelLargeNodeDestroyedCount[CurrentLevel] == levelLargeNodeCount[CurrentLevel]) {
			if (_questLogNotesManager != null) _questLogNotesManager.NodesDestroyed(CurrentLevel);
		}
	}

	public bool PosWithinLeafBounds(Vector3 pos, BoxCollider b) {
		if (b == null) { Debug.Log("BUG: null BoxCollider passed to PosWithinLeafBounds!"); return false; }

		float xMax = b.bounds.max.x;
		float xMin = b.bounds.min.x;
		float yMax = b.bounds.max.z; // Yes Unity you stupid engine...z is y people!  
		float yMin = b.bounds.min.z; // NO I'm not making a 2D game....it's y and I'm sticking to it
		if ((pos.x < xMax && pos.x > xMin) && (pos.z < yMax && pos.z > yMin)) {
			return true;
		}

		return false;
	}
	
	public void UnloadLevelGeometry(int curlevel) {
		return;
		if (curlevel > MaxLevelsCount || curlevel < 0)
		{
			return;
		}
		
		List<GameObject> deleteMes = new List<GameObject>();
		Transform parent = geometryContainer.transform;
		int children = parent.childCount;
		for (int i=0;i<children;i++) deleteMes.Add(parent.GetChild(i).gameObject);
		for (int i=0;i<deleteMes.Count;i++) {
			if (deleteMes[i] != null) Destroy(deleteMes[i]);
		}
	}
	
	public void LoadLevelGeometry(int curlevel) {
		/*
		return;
		if (curlevel > MaxLevelsCount || curlevel < 0)
		{
			return;
		}
		
		string gName = "CitadelScene_geometry_level"+curlevel.ToString()+".txt";
		StreamReader sf = Utils.ReadStreamingAsset(gName);
		if (sf == null) {
			UnityEngine.Debug.Log("Geometry input file path invalid");
			return;
		}

		string readline;
		List<string> readFileList = new List<string>();
		int lineNum = 0;
		char splitter = Convert.ToChar(SaveLoad.splitChar);
		Transform parent,child;
		int count = 0;
		Light lit;
		List<Light> chunkLights = new List<Light>();
		GameObject go;
		using (sf) {
			do {
				readline = sf.ReadLine();
				if (readline == null) break;

				string[] entries = readline.Split(splitter);
				
				go = SaveLoad.LoadPrefab(_consts,_consoleEmulator,this,ref entries,lineNum,curlevel);
				if (go != null) parent = go.transform;
				else parent = null;
				
				if (parent != null) {
					// Move all lights off of the prefab and into the cullable lights container.
					child = null;
					count = parent.childCount;
					for (int i=0;i<count;i++) {
						child = parent.GetChild(i);
						lit = child.GetComponent<Light>();
						if (lit != null) chunkLights.Add(lit);
					}
				}
				lineNum++;
			} while (!sf.EndOfStream);
			
			for (int i=0;i<chunkLights.Count;i++) {
				lit = chunkLights[i];
				lit.gameObject.name = "ChunkLight_" + lit.gameObject.name;
				lit.transform.parent = lightContainer.transform;
// 				UnityEngine.Debug.Log("Moved light off of " + lit.gameObject.name);
			}
			
			sf.Close();
		}*/
	}

	public void UnloadLevelLights(int curlevel) {
		/*
		return;
		if (curlevel > MaxLevelsCount || curlevel < 0)
		{
			return;
		}
		
		Component[] compArray = 
		  lightContainer.GetComponentsInChildren(typeof(Light),true);

		GameObject go = null;
		int litCount = compArray.Length;
		for (int i=0;i<litCount;i++) {
			go = compArray[i].gameObject;
			if (go.GetComponent<LightAnimation>() != null) continue;
			if (go.GetComponent<TargetIO>() != null) continue;

			int childCount = go.transform.childCount;
			for (int j = 0; j < childCount; j++) {
				Transform child = go.transform.GetChild(j);
				MeshRenderer mr = child.GetComponent<MeshRenderer>();
				if (mr != null && mr.material != null) {
					Material mat = mr.material;
					mr.material = null; // Clear reference
					Destroy(mat); // Destroy the material instance
				}
			}

			Destroy(go);
		}
		compArray = null;*/
	}

	public void UnloadLevelDynamicObjects(int curlevel, bool saveExisting) {
		_lightDistanceCuller.Clear();
		Transform tr = GetRequestedLevelDynamicContainer(curlevel).transform;
		if (saveExisting) {
			List<GameObject> allDynamicObjects = new List<GameObject>();
			Component[] compArray = tr.gameObject.GetComponentsInChildren(typeof(SaveObject),true);
			for (int i=0;i<compArray.Length;i++) {
				allDynamicObjects.Add(compArray[i].gameObject);
			}

			DynamicObjectsSavestrings[curlevel].Clear(); // Empty list.
			for (int i=0;i<allDynamicObjects.Count;i++) {
				DynamicObjectsSavestrings[curlevel].Add(SaveObject.Save(this,allDynamicObjects[i]));
			}
		}
		
		// Iterate over all gameobjects at first level within.
		for (int i=(tr.childCount-1);i>=0;i--) {
			Destroy(tr.GetChild(i).gameObject); // Go going, gone!
		}
	}

	public void UnloadLevelNPCs(int curlevel) {
		GameObject go = npcContainer;
		Component[] compArray = go.GetComponentsInChildren(typeof(SaveObject),
														   true);

		for (int i=0;i<compArray.Length;i++) {
			AIController aic = compArray[i].gameObject.GetComponent<AIController>();
			if (aic == null) {
				UnityEngine.Debug.Log("AIController missing on "
									  + "child " + compArray[i].gameObject.name
									  + " of NPC container " + go.name);
			} else {
				if (aic != null) {
					if (aic.healthManager != null) {
						Image over = aic.healthManager.linkedOverlay;
						if (over != null) {
							Utils.DisableImage(over);
							Utils.Deactivate(over.gameObject);
						}
					}
				}
			}

			Destroy(compArray[i].gameObject);
		}
		compArray = null;
	}

	public async UniTask LoadLevelDynamicObjects(int curlevel) {
		if (levelScript==null || curlevel < 0) return;

		string[] entries;
// 		MeshRenderer mr;
		GameObject dynGO;
		char splitter = Convert.ToChar(SaveLoad.splitChar);
		for (int i=0;i<DynamicObjectsSavestrings[curlevel].Count;i++) {
			entries = DynamicObjectsSavestrings[curlevel][i].Split(splitter);
			if (entries.Length <= 1) continue;
			
			dynGO = await SaveLoad.LoadPrefab(_consts,_consoleEmulator,this, entries,0,curlevel);
			if (dynGO == null) continue;

			int constIndex = Utils.GetIntFromString(entries[0],"constIndex");
			
// 			if (changeDynamicMaterial) {
// 				mr = dynGO.GetComponent<MeshRenderer>();
// 				if (mr == null) continue;
// 				
// 				mr.sharedMaterial = dynamicObjectsMaterial;
// 				MeshFilter mf = dynGO.GetComponent<MeshFilter>();
// 				switch(constIndex) {
// 					case 307: mf.sharedMesh.uv = GetUVMappedToSubspace(mf.sharedMesh,dynamicObjectsUvs[0]); break;
// 					case 309: mf.sharedMesh.uv = GetUVMappedToSubspace(mf.sharedMesh,dynamicObjectsUvs[1]); break;
// 				}
// 			}
		}

		DynamicObjectsSavestrings[curlevel].Clear();
	}
	
// 	private Vector2[] GetUVMappedToSubspace(Mesh mesh, Rect uvSpace) {
// 		UnityEngine.Debug.Log("uvSpace: " + uvSpace.ToString());
// 		Vector2[] uvsIn = mesh.uv;
// 		Vector2[] newUVs = new Vector2[uvsIn.Length];			
// 		for (int u=0;u<uvsIn.Length;u++) {
// 			newUVs[u].x = (uvsIn[u].x * uvSpace.width) + uvSpace.xMin;
// 			newUVs[u].y = (uvsIn[u].y * uvSpace.height) + uvSpace.yMin;
// 		}
// 		
// 		return newUVs;
// 	}

	public void CheatLoadLevel(int ind) {
		if (ind == 10) {
			LoadLevel(10,_playerMovement.cheatG1Spawn.position);
		} else if (ind == 11) {
			LoadLevel(11,_playerMovement.cheatG2Spawn.position);
		} else if (ind == 12) {
			LoadLevel(12,_playerMovement.cheatG4Spawn.position);
		} else {
			LoadLevel(ind,ressurectionLocation[ind].position);
		}
	}

	public string Save() {
		int i=0;
		s1.Clear();
		s1.Append(Utils.UintToString(LevelManager.CurrentLevel,"currentLevel"));
		s1.Append(Utils.splitChar);
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelSecurity[i],"levelSecurity["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelCameraDestroyedCount[i],"levelCameraDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelSmallNodeDestroyedCount[i],"levelSmallNodeDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelLargeNodeDestroyedCount[i],"levelLargeNodeDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<13;i++) { s1.Append(Utils.BoolToString(ressurectionActive[i],"ressurectionActive["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		s1.Append(Utils.BoolToString(ressurectionActive[13],"ressurectionActive[13]"));
		return s1.ToString();
	}

	public async Task<int> Load(string[] entries, int index) {
		int i = 0;
		int levelNum = Utils.GetIntFromString(entries[index],"currentLevel"); index++;
		await LoadLevelFromSave(levelNum);
		for (i=0;i<14;i++) {levelSecurity[i] = Utils.GetIntFromString(entries[index],"levelSecurity[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelCameraDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelCameraDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelSmallNodeDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelSmallNodeDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelLargeNodeDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelLargeNodeDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { ressurectionActive[i] = Utils.GetBoolFromString(entries[index],"ressurectionActive[" + i.ToString() + "]"); index++; }
		return index;
	}

	private async UniTask LoadStaticObjects(int levNum)
	{
		var staticObjectsStrings = StaticObjectsSaveStrings[levNum];
		
		if (staticObjectsStrings.Count == 0)
		{
			return;
		}

		UnloadLevelNPCs(CurrentLevel);
		
		var splitter = Convert.ToChar(SaveLoad.splitChar);
		GameObject contnr = GetRequestedLevelNPCContainer(levNum);
		var saveableObjects = Utils.FindAllSaveObjectsGOs();
		bool[] alreadyCheckedThisInstantiableGameObjectInScene = new bool[saveableObjects.Count];
		
		foreach (var saveString in staticObjectsStrings)
		{
			var entries = saveString.Split(splitter);
			int constIndex = Utils.GetIntFromString(entries[0],"constIndex");
			var savID = Utils.GetIntFromString(entries[2],"SaveID");
			bool isNpc = ConsoleEmulator.ConstIndexIsNPC(constIndex);
			
			if (isNpc)
			{
				var instGO = await _consoleEmulator.SpawnDynamicObject(constIndex,levNum,false,contnr,savID);
				PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(instGO,true);
				await SaveObject.Load(_consts,this,instGO, entries,0,prefID); // Load NPC.
			}
			else
			{
				for (var i = 0; i < (saveableObjects.Count); i++)
				{
					var currentGameObjectInScene = saveableObjects[i];

					if (alreadyCheckedThisInstantiableGameObjectInScene[i] || currentGameObjectInScene == null)
					{
						continue;
					}
	
					var currentSaveObjectInScene = SaveLoad.GetPrefabSaveObject(currentGameObjectInScene);
					if (!currentSaveObjectInScene.instantiated)
					{
						alreadyCheckedThisInstantiableGameObjectInScene[i] = true; // Huge time saver right here!
					}

					if (currentSaveObjectInScene.SaveID == savID && currentSaveObjectInScene.SaveID != 0)
					{
						PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(currentGameObjectInScene, true);
						await SaveObject.Load(_consts,this,currentGameObjectInScene, entries, i, prefID);
						alreadyCheckedThisInstantiableGameObjectInScene[i] = true; // Huge time saver right here!
						break;
					}
				}
			}
		}
		
		npcsm.RepopulateChildList();

		if (_inventory.hasHardware[1]) {
			// Go through all HealthManagers in the game and initialize the
			// linked overlays now for Automap.  Done after instantiation.
			List<GameObject> hmGOs = new List<GameObject>();
			List<GameObject> allParents = SceneManager.GetActiveScene().GetRootGameObjects().ToList();				
			// Find all HealthManager components.
			bool includeInactive = true;
			for (var i=0;i<allParents.Count;i++) {
				Component[] compArray =
					allParents[i].GetComponentsInChildren(
						typeof(HealthManager),includeInactive);

				// Add all gameObject with a HealthManager components.
				for (var k=0;k<compArray.Length;k++) hmGOs.Add(compArray[k].gameObject);
			}

			for (var i=0;i<hmGOs.Count;i++) {
				if (hmGOs[i] == null) continue;

				HealthManager hm = hmGOs[i].GetComponent<HealthManager>();
				if (hm == null) continue;

				if ((hm.isNPC || hm.isSecCamera)) {
					hm.Awake(); // Set up slots.
					hm.Start(); // Setup overlay.
				}
			}
		}
		
		staticObjectsStrings.Clear();
	}
	
	private void SaveStaticObjects()
	{
		var currentLevelData = levelScript;
		var saveStringsStorage = StaticObjectsSaveStrings[CurrentLevel];
		saveStringsStorage.Clear();

		SaveObjects(currentLevelData.staticObjectsSaveable);
		SaveObjects(currentLevelData.NPCsSaveableInstantiated);
		SaveObjects(currentLevelData.doorsStaticSaveable);
		SaveObjects(currentLevelData.lightsStaticSaveable);
		
		void SaveObjects(GameObject parent)
		{
			foreach (var saveObject in parent.GetComponentsInChildren<SaveObject>(true))
			{
				saveStringsStorage.Add(SaveObject.Save(this,saveObject.gameObject));
			}
		}
	}
	
	void OnDestroy() {
		ressurectionLocation = null;
		ressurectionBayDoor = null;
		sky = null;
		sun = null;
		sunSprite = null;
		saturn = null;
		exterior = null;
		exterior_shield = null;
		skyMR = null;
		npcsm = null;
		levelScript = null;
		npcContainer = null;
		rtxEmissive = null;
		sphereMesh = null;
		pipe_maint2_3_coolant = null;
	}
	
	public sealed class SaveableObjectStringsStorage : IReadOnlyList<List<string>>
	{
		private readonly List<string>[] _objectsSaveStrings = new List<string>[MaxLevelsCount];

		public List<string> this[int index] => _objectsSaveStrings[index];

		public int Count =>_objectsSaveStrings.Length;

		public SaveableObjectStringsStorage()
		{
			ResetSaveStrings();
		}

		public void ResetSaveStrings() 
		{
			for (var i = 0; i < _objectsSaveStrings.Length; ++i)
			{
				if (_objectsSaveStrings[i] != null)
				{
					_objectsSaveStrings[i].Clear();
				}
				else
				{
					_objectsSaveStrings[i] = new List<string>();
				}
			}
		}

		public IEnumerator<List<string>> GetEnumerator()
		{
			IReadOnlyList<List<string>> ienumarable = _objectsSaveStrings;
			return ienumarable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
