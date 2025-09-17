using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Citadel.Game;
using Citadel.SceneManagement;
using Zenject;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
	public const int MaxLevelsCount = 14;
	public const int NewGameLevelIndex = 1;

	public GameObject[] levels;
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
	public NPCSubManager[] npcsm;
	public Level[] levelScripts;
	public GameObject[] geometryContainers;
	public GameObject[] lightContainers;
	public GameObject[] npcContainers;
	public GameObject[] elevatorTargetDestinations;
	public Material rtxEmissive;
	public Mesh sphereMesh;
	public SkyRotate skyRotate;
	public Material pipe_maint2_3_coolant;
	
	private bool getValparsed;
	private bool[] levelDataLoaded;
	private int getValreadInt;
	private float getValreadFloat;
	private static readonly StringBuilder s1 = new(200*1024);
	private GameObject _dummyGameObject;

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

	public static bool LoadLevelAfterSceneChanges { get; private set; }
	public static Vector3 TargetPosition { get; private set; } = Vector3.zero;
	public static SaveableObjectStringsStorage StaticObjectsSaveStrings { get; } = new();
	public static SaveableObjectStringsStorage DynamicObjectsSavestrings { get; } = new();
	public static int currentLevel = NewGameLevelIndex;
	public static bool UseDynamicLevelsLoading => ScenesLoader.LoadedSceneName == ScenesLoader.DynamicLevelsSceneName;

	void Awake () {
		if (currentLevel < 0) {
			if (_consts == null) return;
			if (_consts.player1CapsuleMainCameragGO == null) return;

			Camera cam = _consts.player1CapsuleMainCameragGO.GetComponent<Camera>();
			if (cam == null) return;

			cam.useOcclusionCulling = false; // For debug whiteroom
			return;
		}
		if (currentLevel < 0 || currentLevel > 12) return; // 12 because I don't think I support starting in cyberspace, 13, for testing.

		if (sky == null) Debug.Log("BUG: LevelManager missing manually assigned reference for sky.");
		else sky.SetActive(true);

		SetSkyVisible(1);
		if (ressurectionBayDoor.Length != 8) Debug.Log("BUG: LevelManager ressurectionBayDoor array length not equal to 8.");
		Time.timeScale = Const.defaultTimeScale;
		levelDataLoaded = new bool[MaxLevelsCount];
		for (int i=0;i<MaxLevelsCount;i++) levelDataLoaded[i] = false;

		if (Const.StartingNewGame)
		{
			StaticObjectsSaveStrings.ResetSaveStrings();
			LoadDynamicObjectsSavestrings();
		}

		if (!Const.StartingNewGame && UseDynamicLevelsLoading)
		{
			LoadLevelData(currentLevel);
		}
	}

	public bool LevelExists(int levelID)
	{
		return levelScripts[levelID] != null;
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
		skyMR.enabled = (on > 0 && showSkyForLevel[currentLevel]);
		saturn.SetActive(on > 0 && showSaturnForLevel[currentLevel]);
		exterior.SetActive(on > 0 && showExteriorForLevel[currentLevel]);
		if (on == 1) Debug.Log("SkyVisible passed a 1, sky + sunlight");
		if (on == 0) Debug.Log("SkyVisible passed a 0, sunlight only");
		if (on == -1) Debug.Log("SkyVisible passed a -1, nope");
		sun.SetActive(_consts.GraphicsShadowMode >= 1 && on >= 0); // on == 0 is for Sunlight only!
		sunSprite.SetActive(on > 0 && showSaturnForLevel[currentLevel]);
		if (_consts == null) return;
		if (_consts.questData == null) return;
		
		exterior_shield.SetActive(on > 0 && showExteriorForLevel[currentLevel]
								  && _consts.questData.ShieldActivated);
	}

	public void CyborgConversionToggleForCurrentLevel() {
		if (!LevNumInBounds(currentLevel)) return;
	    
		if (currentLevel == 6) {
			if (ressurectionActive[currentLevel]) {
				ressurectionActive[currentLevel] = false;
				ressurectionActive[10] = false;
				ressurectionActive[11] = false;
				ressurectionActive[12] = false;
			} else {
				ressurectionActive[currentLevel] = true;
				ressurectionActive[10] = true;
				ressurectionActive[11] = true;
				ressurectionActive[12] = true;
			}
		} else {
			ressurectionActive[currentLevel] = !ressurectionActive[currentLevel]; // Toggle current level.
		}
	}

	public bool RessurectPlayer() {
		if (!ressurectionActive[currentLevel]) return false;

		if (currentLevel == 10 ||currentLevel == 11 ||currentLevel == 12) {
			LoadLevel(6,ressurectionLocation[currentLevel].position);
			ressurectionBayDoor[6].ForceClose();
		} else {
			if (currentLevel <= 7 && currentLevel >= 0) {
				ressurectionBayDoor[currentLevel].ForceClose();
			}

			if (currentLevel >= 0 || currentLevel < 13) {
				Transform plyr = _playerReference.playerCapsule.transform;
				Vector3 spot = ressurectionLocation[currentLevel].position;
				plyr.position = transform.TransformPoint(spot);
			}
		}

		// Activate death screen and readouts for
		// "BRAIN ACTIVITY SATISFACTORY..."            ya debatable right
		// etc. etc.
		_playerReference.playerDeathRessurectEffect.SetActive(true);
		_music.PlayTrack(currentLevel,TrackType.Revive,MusicType.Override);
		_playerMovement.ressurectingFinished = _pauseScript.relativeTime + 3f;
		return true;
	}

	// Make sure that unneeded objects are unloaded
	public void UnloadLevelData(int levnum) {
		if (!LevNumIsNonCyber(levnum)) return; // In a test or editor space.
		if (!levelDataLoaded[levnum]) return; // Already cleared.

		UnloadLevelLights(levnum);
		UnloadLevelGeometry(levnum);
 		UnloadLevelDynamicObjects(levnum,true);
		levelDataLoaded[levnum] = false;
		SaveLoad.numLightsWithShadows = 0;
	}

	// Make sure relevant data and objects are loaded in and present for the level.
	public void LoadLevelData(int levnum) {
		if (!LevNumInBounds(currentLevel)) { // In a test or editor space.
			levelDataLoaded[levnum] = true;
			return;
		}
		if (levelDataLoaded[levnum]) return; // Already loaded.

// 		Debug.Log("Loading level data for " + levnum.ToString());
		LoadLevelLights(levnum);
		LoadLevelGeometry(levnum);
		LoadStaticObjects(levnum);
		LoadLevelDynamicObjects(levnum);
		_music.LoadLevelMusic(levnum);
		levelDataLoaded[levnum] = true;
		UnityEngine.Debug.Log("Number of lights for level " + levnum.ToString() + " with shadows: " + SaveLoad.numLightsWithShadows.ToString());
	}

	public void ChangeGameScene(int levnum, Vector3? targetPosition = null, bool changeSceneForced = false)
	{
		if (!LevNumInBounds(levnum))
		{
			Debug.LogWarning("levnum out of bounds"); 
			return;
		}

		if (currentLevel == levnum && !changeSceneForced)
		{
			_consts.sprint(_consts.stringTable[9]);
			return;
		}

		var useDynamicLevelsLoading = UseDynamicLevelsLoading;
		
		if (useDynamicLevelsLoading && !Const.StartingNewGame)
		{
			LoadLevel(levnum, targetPosition ?? Vector3.zero, changeSceneForced);
			return;
		}

		if (!Const.StartingNewGame && !useDynamicLevelsLoading)
		{
			LoadLevelAfterSceneChanges = true;
			UnloadLevelDynamicObjects(currentLevel, true);
			SaveStaticObjects();
		}

		TargetPosition = targetPosition ?? Vector3.zero;
		currentLevel = levnum;
		ObjectContainmentSystem.ClearLists();
		ScenesLoader.LoadLevel(levnum);
	}
	
	public void LoadLevel(int levnum, Vector3 targetPosition, bool loadLevelForced = false)
	{
		_lightDistanceCuller.Clear();
		LoadLevelAfterSceneChanges = false;
		if (!LevNumInBounds(levnum)) { Debug.LogWarning("levnum out of bounds"); return; }

		// NOTE: Check this first since the button for the current level has a null destination.  This is fine and expected.
		if (currentLevel == levnum && !loadLevelForced)
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
				case 0:  targetPosition = elevatorTargetDestinations[25].transform.position; break;
				case 1:  targetPosition =  elevatorTargetDestinations[0].transform.position; break;
				case 2:  targetPosition =  elevatorTargetDestinations[1].transform.position; break;
				case 3:  targetPosition =  elevatorTargetDestinations[3].transform.position; break;
				case 4:  targetPosition =  elevatorTargetDestinations[6].transform.position; break;
				case 5:  targetPosition =  elevatorTargetDestinations[7].transform.position; break;
				case 6:  targetPosition =  elevatorTargetDestinations[9].transform.position; break;
				case 7:  targetPosition = elevatorTargetDestinations[17].transform.position; break;
				case 8:  targetPosition = elevatorTargetDestinations[19].transform.position; break;
				case 9:  targetPosition = elevatorTargetDestinations[21].transform.position; break;
				case 10: targetPosition = elevatorTargetDestinations[22].transform.position; break;
				case 11: targetPosition = elevatorTargetDestinations[23].transform.position; break;
				case 12: targetPosition = elevatorTargetDestinations[24].transform.position; break;
			}
		}

		if (_questLogNotesManager != null) _questLogNotesManager.NotifyLevelChange(levnum);
 
		// Return to level from cyberspace.
		_playerReference.playerCapsule.transform.position = targetPosition;
		currentLevel = levnum; // Set current level to be the new level
		DisableAllNonOccupiedLevelsExcept(currentLevel);
		DynamicCulling.camPositions = new Dictionary<GameObject, Vector3>();
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		levels[levnum].SetActive(true); // enable new level
		if (currentLevel == 2 && AutoSplitterData.missionSplitID == 0) {
			AutoSplitterData.missionSplitID++; // 1 - Medical split - we are now on level 2
			Debug.Log("AutoSplitterData missionSplitID incremented: " + AutoSplitterData.missionSplitID.ToString());
		}
		
		PostLoadLevelSetupSystems();
		_lightDistanceCuller.Rebuild();
		if (currentLevel != 13) {
			_dynamicCulling.Cull_Init();
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			StartCoroutine(DelayedCull());
		}
	}
	
	public IEnumerator DelayedCull() {
		yield return new WaitForSeconds(0.5f);
		_dynamicCulling.CullCore(); // For Level 10, visible screen with camera view can't update until cams awake.
	}

	public void LoadLevelFromSave(int levnum) {
		if (!LevNumInBounds(levnum)) return;

// 		Debug.Log("LevelManager LoadLevelFromSave()");
		LoadLevelData(levnum); // Let this function check and load data if it isn't yet.
		currentLevel = levnum; // Set current level to be the new level
		DisableAllNonOccupiedLevelsExcept(currentLevel); // Unload last level.
		levels[currentLevel].SetActive(true); // Load new level
		PostLoadLevelSetupSystems();
	}

	private void PostLoadLevelSetupSystems() {
		_music.inCombat = false;
		_music.SFXMain.Stop();
		_music.SFXOverlay.Stop();
		_music.levelEntry = true;
		_playerHealth.radiationArea = false;
		_playerMovement.ladderState = 0;
		LoadLevelData(currentLevel);
		_automap.SetAutomapExploredReference(currentLevel);
		_automap.automapBaseImage.overrideSprite = _automap.automapsBaseImages[currentLevel];
		_consts.ClearActiveAutomapOverlays(); // After other levels turned off.
		_consts.ResetPauseLists();
		SetSkyVisible(1);
		_config.SetLanguage(); // Update all translatable text.
		_consts.ClearPrefabs();
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		Resources.UnloadUnusedAssets();
	}

	public void DisableAllNonOccupiedLevelsExcept(int occupiedLevel) {
		for (int i=0;i<levels.Length;i++) {
			if (i == occupiedLevel) continue;

			UnloadLevelData(i);
			if (levels[i] != null) levels[i].SetActive(false);
		}
	}

	public GameObject GetCurrentDynamicContainer() { // Does not return null
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].dynamicObjectsContainer;
		}

        return levelScripts[currentLevel].dynamicObjectsContainer;
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
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].lightsStaticImmutable;
		}
		
		return levelScripts[currentLevel].lightsStaticImmutable;
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
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].doorsStaticSaveable;
		}

		return levelScripts[currentLevel].doorsStaticSaveable;
	}

	public GameObject GetCurrentLightsContainer() { // Does not return null
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].lightsStaticImmutable;
		}

		return levelScripts[currentLevel].lightsStaticImmutable;
	}
	
	public GameObject GetRequestedLightsStaticImmutableContainer(int index) {
		if (!LevNumInBounds(index)) {
			return levelScripts[1].lightsStaticImmutable;
		}
		
		return levelScripts[index].lightsStaticImmutable;
	}

	public GameObject GetRequestedLevelDynamicContainer(int index) {
		if (!LevNumInBounds(currentLevel)) {
			return levelScripts[1].dynamicObjectsContainer; // Default to Medical level
		}
		
        return levelScripts[index].dynamicObjectsContainer;
	}

	public GameObject GetRequestedLevelNPCContainer(int index) {
		if (!LevNumInBounds(currentLevel)) {
			return npcContainers[1]; // Default to Medical level
		}

        return npcContainers[index];
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

		for (int i=0; i < 14; i++) {
			if (isNPC && par == npcContainers[i]) return i;
			else if (levelScripts[i]!=null && par == levelScripts[i].dynamicObjectsContainer) return i;
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
		if (!LevNumInBounds(currentLevel)) return 0;
		if (superoverride) return 0; // tee hee we are SHODAN, no security blocks in place
		return levelSecurity[currentLevel];
	}

	// Typical level
	// 4 CPU nodes
	// 20 cameras
	// 100% = 4x + 20y
	// Assuming that a good camera percentage is 2-3%, CPU % would be about 10-15 each
	public void ReduceCurrentLevelSecurity(SecurityType stype) {
		if (!LevNumIsNonCyber(currentLevel)) return;

		float camScore = 4;
		float nodeSmallScore = 10;
		float nodeLargeScore = 27;
		float secscoreTotal = (levelCameraCount[currentLevel] * camScore) + (levelSmallNodeCount[currentLevel] * nodeSmallScore) + (levelLargeNodeCount[currentLevel] * nodeLargeScore);
		//secscoreTotal = 106 for medical level
		float secDrop = camScore; // default to camScore
		switch (stype) {
			case SecurityType.None: return;
			case SecurityType.Camera: secDrop = ((camScore/secscoreTotal) * 100); levelCameraDestroyedCount[currentLevel]++; break; // 1 camera divided by the total, so 2/ say (40+60) = 2/100 = 0.02, or 2% using the example numbers above
			case SecurityType.NodeSmall: secDrop = ((nodeSmallScore/secscoreTotal) * 100); levelSmallNodeDestroyedCount[currentLevel]++; break;
			case SecurityType.NodeLarge: secDrop = ((nodeLargeScore/secscoreTotal) * 100); levelLargeNodeDestroyedCount[currentLevel]++; break;
		}
		levelSecurity [currentLevel] -= (int)secDrop;
		if (levelSecurity [currentLevel] < 0) levelSecurity [currentLevel] = 0;
		if ((levelLargeNodeDestroyedCount[currentLevel] == levelLargeNodeCount[currentLevel]) && (levelSmallNodeDestroyedCount[currentLevel] == levelSmallNodeCount[currentLevel]) && (levelCameraDestroyedCount[currentLevel] == levelCameraCount[currentLevel])) {
			levelSecurity[currentLevel] = 0;
		}
		_consts.sprint(_consts.stringTable[306] + levelSecurity[currentLevel].ToString() + _consts.stringTable[307]);

		// Notify quest log if all nodes were destroyed
		if (levelLargeNodeDestroyedCount[currentLevel] == levelLargeNodeCount[currentLevel]) {
			if (_questLogNotesManager != null) _questLogNotesManager.NodesDestroyed(currentLevel);
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
		if (curlevel > (geometryContainers.Length - 1) || curlevel < 0 || !UseDynamicLevelsLoading)
		{
			return;
		}
		
		List<GameObject> deleteMes = new List<GameObject>();
		Transform parent = geometryContainers[curlevel].transform;
		int children = parent.childCount;
		for (int i=0;i<children;i++) deleteMes.Add(parent.GetChild(i).gameObject);
		for (int i=0;i<deleteMes.Count;i++) {
			if (deleteMes[i] != null) Destroy(deleteMes[i]);
		}
	}
	
	public void LoadLevelGeometry(int curlevel) {

		if (curlevel > (geometryContainers.Length - 1) || curlevel < 0 || !UseDynamicLevelsLoading)
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
				lit.transform.parent = lightContainers[curlevel].transform;
// 				UnityEngine.Debug.Log("Moved light off of " + lit.gameObject.name);
			}
			
			sf.Close();
		}
	}

	public void UnloadLevelLights(int curlevel) {
		if (curlevel > 12 || curlevel > (lightContainers.Length - 1) || curlevel < 0 || !UseDynamicLevelsLoading)
		{
			return;
		}
		
		Component[] compArray = 
		  lightContainers[curlevel].GetComponentsInChildren(typeof(Light),true);

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
		compArray = null;
	}

	public void LoadLevelLights(int curlevel) {

		if (curlevel > 12 || curlevel > (lightContainers.Length - 1) || curlevel < 0 || !UseDynamicLevelsLoading)
		{
			return;
		}

		string lName = "CitadelScene_lights_level"+curlevel.ToString()+".txt";
		StreamReader sf = Utils.ReadStreamingAsset(lName);
		if (sf == null) {
			UnityEngine.Debug.Log("Lights input file path invalid");
			return;
		}

		string readline;
		List<string> readFileList = new List<string>();
		int lineNum = 0;
		char splitter = Convert.ToChar(SaveLoad.splitChar);
		using (sf) {
			do {
				readline = sf.ReadLine();
				if (readline == null) break;
				
				string[] entries = readline.Split(splitter);
				SaveLoad.LoadPrefab(_consts,_consoleEmulator,this,ref entries,lineNum,curlevel);
				lineNum++;
			} while (!sf.EndOfStream);
			sf.Close();
		}
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
		GameObject go = npcContainers[curlevel];
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

	public void LoadLevelDynamicObjects(int curlevel) {
		if (curlevel > (levelScripts.Length - 1)) return;
		if (curlevel < 0) return;

		string[] entries;
// 		MeshRenderer mr;
		GameObject dynGO;
		char splitter = Convert.ToChar(SaveLoad.splitChar);
		for (int i=0;i<DynamicObjectsSavestrings[curlevel].Count;i++) {
			entries = DynamicObjectsSavestrings[curlevel][i].Split(splitter);
			if (entries.Length <= 1) continue;
			
			dynGO = SaveLoad.LoadPrefab(_consts,_consoleEmulator,this,ref entries,0,curlevel);
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
		s1.Append(Utils.UintToString(LevelManager.currentLevel,"currentLevel"));
		s1.Append(Utils.splitChar);
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelSecurity[i],"levelSecurity["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelCameraDestroyedCount[i],"levelCameraDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelSmallNodeDestroyedCount[i],"levelSmallNodeDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<14;i++) { s1.Append(Utils.UintToString(levelLargeNodeDestroyedCount[i],"levelLargeNodeDestroyedCount["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		for (i=0;i<13;i++) { s1.Append(Utils.BoolToString(ressurectionActive[i],"ressurectionActive["+i.ToString()+"]")); s1.Append(Utils.splitChar); }
		s1.Append(Utils.BoolToString(ressurectionActive[13],"ressurectionActive[13]"));
		return s1.ToString();
	}

	public int Load(ref string[] entries, int index) {
		int i = 0;
		int levelNum = Utils.GetIntFromString(entries[index],"currentLevel"); index++;
		LoadLevelFromSave(levelNum);
		for (i=0;i<14;i++) {levelSecurity[i] = Utils.GetIntFromString(entries[index],"levelSecurity[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelCameraDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelCameraDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelSmallNodeDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelSmallNodeDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { levelLargeNodeDestroyedCount[i] = Utils.GetIntFromString(entries[index],"levelLargeNodeDestroyedCount[" + i.ToString() + "]"); index++; }
		for (i=0;i<14;i++) { ressurectionActive[i] = Utils.GetBoolFromString(entries[index],"ressurectionActive[" + i.ToString() + "]"); index++; }
		return index;
	}

	private void LoadStaticObjects(int levNum)
	{
		var staticObjectsStrings = StaticObjectsSaveStrings[levNum];
		
		if (UseDynamicLevelsLoading || staticObjectsStrings.Count == 0)
		{
			return;
		}

		UnloadLevelNPCs(currentLevel);
		
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
				var instGO = _consoleEmulator.SpawnDynamicObject(constIndex,levNum,false,contnr,savID);
				PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(instGO,true);
				SaveObject.Load(_consts,this,instGO,ref entries,0,prefID); // Load NPC.
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
						SaveObject.Load(_consts,this,currentGameObjectInScene, ref entries, i, prefID);
						alreadyCheckedThisInstantiableGameObjectInScene[i] = true; // Huge time saver right here!
						break;
					}
				}
			}
		}
		
		// LOAD 8.  Repopulate registries as needed that were on Awake.
		for (var i = 0; i < npcsm.Length; i++ ) {
			npcsm[i]?.RepopulateChildList();
		}
			
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
		if (UseDynamicLevelsLoading)
		{
			return;
		}
		
		var currentLevelData = levelScripts[currentLevel];
		var saveStringsStorage = StaticObjectsSaveStrings[currentLevel];
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
		levels = null;
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
		levelScripts = null;
		geometryContainers = null;
		lightContainers = null;
		npcContainers = null;
		elevatorTargetDestinations = null;
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
