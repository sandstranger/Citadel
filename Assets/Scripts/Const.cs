using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; // Stopwatch
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Citadel.Game;
using Citadel.SceneManagement;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

// GLOBAL SCRIPT EXECUTION ORDER (set in Unity Project Settings, here for ref)
// UnityEngine.EventSystems.EventSystems.EventSystem -1000
// Music           -950
// Const           -900
// PauseScript     -899
// Level           -800
// LevelManager    -700
// MainMenuHandler -500
// UnityEngine.InputSystem.PlayerInput -100
//                 --   Default Time   --
// UnityEngine.UI.ToggleGroup 10
// PlayerReferenceManager 100
// HealthManager    400
// MFDManager       600
// AIController     700
// CameraView       750
// DynamicCulling   800
// TargetIO         950
// SEGI            1100
// UnityStandardAssets.ImageEffects.ScreenSpaceAmbientOcclusion 1200
// TextLocalization 1300

public class Const : SingletonHelper<Const>
{
	private const int StringBuilderSize = 500 * 1024;
	public float shadowThreshold = 0.03f;
	//Item constants
	public QuestBits questData;

	//Audiolog constants
	public string[] audiologNames;
	[HideInInspector] public string[] audiologSenders;
	public string[] audiologSubjects;
	public AudioClip[] audioLogs;
	[HideInInspector] public AudioLogType[] audioLogType; // 0 = text only
														  // 1 = normal
														  // 2 = email
														  // 3 = papers
														  // 4 = vmail
	public string[] audioLogSpeech2Text;
	[HideInInspector] public int[] audioLogLevelFound;
	public AudioClip[] sounds;

	//Weapon constants
	public float[] delayBetweenShotsForWeapon;
	public float[] delayBetweenShotsForWeapon2;
	public float[] damagePerHitForWeapon;
	public float[] damagePerHitForWeapon2;
	public float[] damageOverloadForWeapon;
	public float[] energyDrainLowForWeapon;
	public float[] energyDrainHiForWeapon;
	public float[] energyDrainOverloadForWeapon;
	public float[] penetrationForWeapon;
	public float[] penetrationForWeapon2;
	public float[] offenseForWeapon;
	public float[] offenseForWeapon2;
	public int[] magazinePitchCountForWeapon;
	public int[] magazinePitchCountForWeapon2;
	public float[] recoilForWeapon;
	public AttackType[] attackTypeForWeapon;

	//NPC constants
	[HideInInspector] public string[] nameForNPC;
	[HideInInspector] public AttackType[] attackTypeForNPC;
	[HideInInspector] public AttackType[] attackTypeForNPC2;
	[HideInInspector] public AttackType[] attackTypeForNPC3;
	[HideInInspector] public float[] damageForNPC; // Primary attack damage
	[HideInInspector] public float[] damageForNPC2; // Secondary attack damage
	[HideInInspector] public float[] damageForNPC3; // Grenade attack damage
	public float[] rangeForNPC; // Primary attack range
	public float[] rangeForNPC2; // Secondary attack range
	public float[] rangeForNPC3; // Grenade throw range
	[HideInInspector] public float[] healthForNPC;
	[HideInInspector] public float[] healthForCyberNPC;
	[HideInInspector] public PerceptionLevel[] perceptionForNPC;
	[HideInInspector] public float[] disruptabilityForNPC;
	[HideInInspector] public float[] armorvalueForNPC;
	[HideInInspector] public AIMoveType[] moveTypeForNPC;
	[HideInInspector] public float[] defenseForNPC;
	[HideInInspector] public float[] yawSpeedForNPC;
	[HideInInspector] public float[] fovForNPC;
	[HideInInspector] public float[] fovAttackForNPC;
	[HideInInspector] public float[] fovStartMovementForNPC;
	[HideInInspector] public float[] distToSeeBehindForNPC;
	[HideInInspector] public float[] sightRangeForNPC;
	[HideInInspector] public float[] walkSpeedForNPC;
	public float[] runSpeedForNPC;
	[HideInInspector] public float[] attack1SpeedForNPC;
	[HideInInspector] public float[] attack2SpeedForNPC;
	[HideInInspector] public float[] attack3SpeedForNPC;
	[HideInInspector] public float[] attack3ForceForNPC;
	[HideInInspector] public float[] attack3RadiusForNPC;
	[HideInInspector] public float[] timeToPainForNPC;
	[HideInInspector] public float[] timeBetweenPainForNPC;
	[HideInInspector] public float[] timeTillDeadForNPC;
	[HideInInspector] public float[] timeToActualAttack1ForNPC;
	[HideInInspector] public float[] timeToActualAttack2ForNPC;
	[HideInInspector] public float[] timeToActualAttack3ForNPC;
	[HideInInspector] public float[] timeBetweenAttack1ForNPC;
	[HideInInspector] public float[] timeBetweenAttack2ForNPC;
	[HideInInspector] public float[] timeBetweenAttack3ForNPC;
	[HideInInspector] public float[] timeToChangeEnemyForNPC;
	[HideInInspector] public float[] timeIdleSFXMinForNPC;
	[HideInInspector] public float[] timeIdleSFXMaxForNPC;
	[HideInInspector] public float[] timeAttack1WaitMinForNPC;
	[HideInInspector] public float[] timeAttack1WaitMaxForNPC;
	[HideInInspector] public float[] timeAttack1WaitChanceForNPC;
	[HideInInspector] public float[] timeAttack2WaitMinForNPC;
	[HideInInspector] public float[] timeAttack2WaitMaxForNPC;
	[HideInInspector] public float[] timeAttack2WaitChanceForNPC;
	[HideInInspector] public float[] timeAttack3WaitMinForNPC;
	[HideInInspector] public float[] timeAttack3WaitMaxForNPC;
	[HideInInspector] public float[] timeAttack3WaitChanceForNPC;
	[HideInInspector] public PoolType[] attack1ProjectileLaunchedTypeForNPC;
	[HideInInspector] public PoolType[] attack2ProjectileLaunchedTypeForNPC;
	[HideInInspector] public PoolType[] attack3ProjectileLaunchedTypeForNPC;
	[HideInInspector] public float[] projectileSpeedAttack1ForNPC;
	[HideInInspector] public float[] projectileSpeedAttack2ForNPC;
	[HideInInspector] public float[] projectileSpeedAttack3ForNPC;
	[HideInInspector] public bool[] hasLaserOnAttack1ForNPC;
	[HideInInspector] public bool[] hasLaserOnAttack2ForNPC;
	[HideInInspector] public bool[] hasLaserOnAttack3ForNPC;
	[HideInInspector] public bool[] explodeOnAttack3ForNPC;
	[HideInInspector] public bool[] preactivateMeleeCollidersForNPC;
	public float[] huntTimeForNPC;
	[HideInInspector] public float[] flightHeightForNPC;
	[HideInInspector] public bool[] flightHeightIsPercentageForNPC;
	[HideInInspector] public bool[] switchMaterialOnDeathForNPC;
	[HideInInspector] public float[] hearingRangeForNPC;
	[HideInInspector] public float[] timeForTranquilizationForNPC;
	[HideInInspector] public bool[] hopsOnMoveForNPC;
												      // NPC Sounds       0,   1,   2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28
	[HideInInspector] public int[] sfxIdleForNPC =         new   int[]{  -1,  -1,  -1, -1, 58, -1, 59, -1, 59, 52, -1, -1, -1, -1, -1, -1,121, -1, -1, -1,121,118, -1, -1, -1, -1, -1, -1, -1};
	[HideInInspector] public int[] sfxSightSoundForNPC =   new   int[]{  -1,  -1, 111,150, 58,150, 59,152,152, -1,150,150,151,152,150, -1,121, -1,151,150,121,119,151, -1, -1, -1, -1, -1, -1};
	[HideInInspector] public int[] sfxAttack1ForNPC =      new   int[]{  -1,  -1, 108, -1, -1,146, -1,146,252,247, -1, -1, -1, -1, -1,122, -1,108,146, -1, -1,118, -1,125,258,258,258,258,258};
	[HideInInspector] public int[] sfxAttack2ForNPC =      new   int[]{  -1, 256,  -1,148, 50, 50, 50, 50, 50,250, 50, 50,146,259,148, -1,121, -1, -1,147, -1, -1,146, -1,258,258,258,258,258};
	[HideInInspector] public int[] sfxAttack3ForNPC =      new   int[]{  -1,  -1,  -1, -1, -1,244,244,244,245, -1, -1,149, -1, -1, -1, -1, -1, -1, -1,244, -1, -1, -1, -1,258,258,258,258,258};
	[HideInInspector] public int[] sfxDeathForNPC =        new   int[]{  -1,  48, 110,143, 48,145, 48, 51, 47, 47,142,143,144, 47,162,123,120,134,144,144,120,117,144,124, -1, -1, -1, -1, -1};
	[HideInInspector] public float[] deathBurstTimerForNPC=new float[]{0.0f,0.0f, 0.1f,0.0f,0.1f,0.1f,0.2f,0.1f,0.1f,0.1f,0.0f,0.45f,0.75f,0.1f,0.0f,0.0f,0.1f,0.224f,0.9f,0.0f,0.1f,0.1f,0.1f,0.2f,0.1f,0.1f,0.1f,0.1f,0.1f};
	[HideInInspector] public NPCType[] typeForNPC;
	[HideInInspector] public int[] projectile1PrefabForNPC;
	[HideInInspector] public int[] projectile2PrefabForNPC;
	[HideInInspector] public int[] projectile3PrefabForNPC;

	// System constants
	[HideInInspector] public string[] creditsText;
	[HideInInspector] public HealthManager[] healthObjectsRegistration; // List of objects with health, used for fast application of damage in explosions
	
	// Layer masks
	[HideInInspector] public int layerMaskPlayerFrob;
	[HideInInspector] public int layerMaskPlayerTargetIDFrob;
	[HideInInspector] public int layerMaskPlayerAttack;
	[HideInInspector] public int layerMaskNPCSight;
	[HideInInspector] public int layerMaskNPCAttack;
	[HideInInspector] public int layerMaskNPCCollision;
	[HideInInspector] public int layerMaskPlayerFeet;
	[HideInInspector] public int layerMaskExplosion;

	public GameObject cameraExplosion;
	public GameObject bloodSpurtSmall;
	public GameObject sparksSmall;
	public GameObject sparksSmallBlue;
	public GameObject bloodSpurtSmallYellow;
	public GameObject bloodSpurtSmallGreen;
	public GameObject Pool_HopperImpact;
	public GameObject Pool_GrenadeFragExplosions;
    public GameObject Pool_Vaporize;
    public GameObject Pool_MagpulseImpacts;
    public GameObject Pool_StungunImpacts;
    public GameObject Pool_RailgunImpacts;
    public GameObject Pool_PlasmaImpacts;
	public GameObject Pool_ProjEnemShot6Impacts;
	public GameObject Pool_ProjEnemShot2Impacts;
	public GameObject Pool_ProjSeedPodsImpacts;
	public GameObject Pool_TempAudioSources;
	public GameObject Pool_GrenadeEMPExplosions;
	public GameObject Pool_ProjEnemShot4Impacts;
	public GameObject Pool_CrateExplosions;
	public GameObject Pool_GrenadeFragLive;
	public GameObject Pool_ConcussionLive;
	public GameObject Pool_EMPLive;
	public GameObject Pool_GasLive;
	public GameObject Pool_GasExplosions;
	public GameObject Pool_CorpseHit;
	public GameObject Pool_LeafBurst;
	public GameObject Pool_MutationBurst;
	public GameObject Pool_GraytationBurst;
	public GameObject Pool_BarrelExplosions;
	public GameObject Pool_CyberDissolve;
	public GameObject Pool_AutomapBotOverlays;
	public GameObject Pool_AutomapCyborgOverlays;
	public GameObject Pool_AutomapMutantOverlays;
	public GameObject Pool_AutomapCameraOverlays;

	//Global object references
	public GameObject loadingScreen;
	public GameObject mainMenuInit; // Used to force mainMenuOn before Start().
	public StatusBarTextDecay statusBar;

	private static int _difficultyCombat = 2;
	private static int _difficultyMission = 2; 
	private static int _difficultyPuzzle = 2;
	private static int _difficultyCyber = 2;
	private static string _playerName;

	public bool QuitAfterSavingDone { get; set; } = false;

	//Config constants
	public int difficultyCombat
	{
		get => _difficultyCombat;
		set => _difficultyCombat = value;
	}

	public int difficultyMission
	{
		get => _difficultyMission;
		set => _difficultyMission = value;
	}

	public int difficultyPuzzle
	{
		get => _difficultyPuzzle;
		set => _difficultyPuzzle = value;
	}

	public int difficultyCyber
	{
		get => _difficultyCyber;
		set => _difficultyPuzzle = value;
	}
	
	public string playerName
	{
		get => _playerName;
		set => _playerName = value;
	}

	[HideInInspector] public int GraphicsResWidth;
	[HideInInspector] public int GraphicsResHeight;
	[HideInInspector] public bool GraphicsFullscreen;
	[HideInInspector] public bool GraphicsSSAO;
	[HideInInspector] public bool GraphicsBloom;
	[HideInInspector] public bool GraphicsSEGI;
	[HideInInspector] public int GraphicsAAMode;
	[HideInInspector] public int GraphicsShadowMode;
	[HideInInspector] public int GraphicsSSRMode;
	[HideInInspector] public int GraphicsFOV;
	[HideInInspector] public int GraphicsGamma;
	[HideInInspector] public int GraphicsModelDetail;	
	[HideInInspector] public bool GraphicsVSync;
	[HideInInspector] public bool NoShootMode;
	[HideInInspector] public bool DynamicMusic;
	[HideInInspector] public const float HeadBobRate = 0.2f;
	[HideInInspector] public const float HeadBobAmount = 0.08f;
	public int[] InputCodeSettings;	  // The integer index values
	public string[] InputCodes;		  // The readable mapping names used as
									  //   labels on the configuration page
	public string[] InputValues;	  // The list of all valid keys: letters,
									  //   numbers, etc.
	public string[] InputConfigNames; // The readable keys used as text
									  //   representations on the configuration
									  //   page for set values.

    public Font mainFont1; // Used to force Point filter mode.
	public Font mainFont2; // Used to force Point filter mode.
	/*[DTValidator.Optional] */private readonly List<GameObject> _targetRegister = new (); // Doesn't need to be full, available space for maps and mods made by the community to use tons of objects
	private readonly List<string> _targetnameRegister = new();
    public string[] stringTable;
	[HideInInspector] public bool stringTableLoaded = false;
	[HideInInspector] public bool loading = false;
	public readonly HashSet<TextLocalization> TextLocalizationRegister = new();
	public float[] reloadTime;

	public GameObject eventSystem;
	public GameObject prefabFallback;
	public Text loadPercentText;
	public Material[] genericMaterials;
	public GameObject[] ReverbRegister;
	public int nextFreeSaveID = 2000000;
	public bool editMode = false;
	public bool noHUD = false;
	
	// Irrelevant to inspector constants; automatically assigned during initialization or play.
	[HideInInspector] public int AudioSpeakerMode;
	[HideInInspector] public bool AudioReverb;
	[HideInInspector] public int AudioVolumeMaster;
	[HideInInspector] public int AudioVolumeMusic;
	[HideInInspector] public int AudioVolumeMessage;
	[HideInInspector] public int AudioVolumeEffects;
	[HideInInspector] public int AudioLanguage;			// The language index. Used for choosing which text to display on-screen.
	[HideInInspector] public float MouseSensitivity;		// The responsiveness of the mouse. Used for scaling slow mice up and fast mice down.
	[HideInInspector] public bool InputInvertLook;
	[HideInInspector] public bool InputInvertCyberspaceLook;
	[HideInInspector] public bool InputInvertInventoryCycling;
	[HideInInspector] public bool InputQuickItemPickup;
	[HideInInspector] public bool InputQuickReloadWeapons;
	[HideInInspector] public bool Footsteps;
	[HideInInspector] public bool HeadBob;
	[HideInInspector] public int[] npcCount;
	[HideInInspector] public int[] audioLogImagesRefIndicesLH;
	[HideInInspector] public int[] audioLogImagesRefIndicesRH;
	[HideInInspector] public string versionString;
	[HideInInspector] public bool gameFinished = false; // Global constants
	[HideInInspector] public float justSavedTimeStamp;
	[HideInInspector] public float savedReminderTime = 7f; // human short-term memory length
	[HideInInspector] public const float doubleClickTime = 0.500f;
	[HideInInspector] public const float frobDistance = 4.9f;
	[HideInInspector] public const float elevatorPadUseDistance = 2f;
	[HideInInspector] public int creditsLength;
	[HideInInspector] public Transform player1TargettingPos;
	[HideInInspector] public GameObject player1Capsule;
	[HideInInspector] public PlayerMovement player1PlayerMovementScript;
	[HideInInspector] public PlayerHealth player1PlayerHealthScript;
	[HideInInspector] public GameObject player1CapsuleMainCameragGO;
	[HideInInspector] public readonly List<PauseRigidbody> prb = new();
	[HideInInspector] public readonly List<PauseParticleSystem> psys = new();
	[HideInInspector] public readonly List<PauseAnimation> panimsList = new();
	[HideInInspector] public float playerCameraOffsetY = 0.84f; //Vertical camera offset from player 0,0,0 position (mid-body)
	[HideInInspector] public Color ssYellowText = new Color(0.8902f, 0.8745f, 0f); // Yellow, e.g. for current inventory text
	[HideInInspector] public Color ssDarkYellowText = new Color(0.8902f * 0.7f, 0.8745f * 0.7f, 0f); // Dark Yellow, e.g. for changing items transition
	[HideInInspector] public Color ssGreenText = new Color(0.3725f, 0.6549f, 0.1686f); // Green, e.g. for inventory text
	[HideInInspector] public Color ssRedText = new Color(0.9176f, 0.1373f, 0.1686f); // Red, e.g. for inventory text
	[HideInInspector] public Color ssWhiteText = new Color(1f, 1f, 1f); // White, e.g. for warnings text
	[HideInInspector] public Color ssOrangeText = new Color(1f, 0.498f, 0f); // Orange, e.g. for map buttons text
	[HideInInspector] public const float camMaxAmount = 0.2548032f;
	[HideInInspector] public const float mapWorldMaxN = 85.83999f;
	[HideInInspector] public const float mapWorldMaxS = -78.00001f;
	[HideInInspector] public const float mapWorldMaxE = -70.44f;
	[HideInInspector] public const float mapWorldMaxW = 93.4f;
	[HideInInspector] public const float mapTileMinX = 8; // top left corner
	[HideInInspector] public const float mapTileMinY = -1016; // bottom right corner
	public bool decoyActive = false;
	[HideInInspector] public const float berserkTime = 20f; //Patch constants
	[HideInInspector] public const float detoxTime = 60f;
	[HideInInspector] public const float geniusTime = 180f;
	[HideInInspector] public const float mediTime = 35f;
	[HideInInspector] public const float reflexTime = 155f;
	[HideInInspector] public const float sightTime = 40f;
	[HideInInspector] public const float sightSideEffectTime = 17f;
	[HideInInspector] public const float staminupTime = 60f;
	[HideInInspector] public const float reflexTimeScale = 0.25f;
	[HideInInspector] public const float defaultTimeScale = 1.0f;
	[HideInInspector] public const float berserkDamageMultiplier = 4.0f;
	[HideInInspector] public const float nitroMinTime = 1.0f; //Grenade constants
	[HideInInspector] public const float nitroMaxTime = 60.0f;
	[HideInInspector] public const float nitroDefaultTime = 7.0f;
	[HideInInspector] public const float earthShMinTime = 4.0f;
	[HideInInspector] public const float earthShMaxTime = 60.0f;
	[HideInInspector] public const float earthShDefaultTime = 10.0f;
	[HideInInspector] public const float globalShakeDistance = 0.3f;
	[HideInInspector] public const float globalShakeForce = 1f;
	[HideInInspector] public Quaternion quaternionIdentity;
	[HideInInspector] public Vector3 vectorZero;
	[HideInInspector] public Vector3 vectorOne;
	[HideInInspector] public int numberOfRaycastsThisFrame = 0;
	public const int maxRaycastsPerFrame = 20;
	public const float raycastTick = 0.2f;
	public const float aiTickTime = 0.1f;
	
	// Credit stats
	[HideInInspector] public int kills = 0;
	[HideInInspector] public int cyberkills = 0;
	[HideInInspector] public int shotsFired = 0;
	[HideInInspector] public int grenadesThrown = 0;
	[HideInInspector] public float damageDealt = 0f;
	[HideInInspector] public float damageReceived = 0f;
	[HideInInspector] public int savesScummed = 0;
	public Material shadowCaster;
	private int lastTargetRegistrySize = 0;

	public static bool StartingNewGame { get; private set; } = false;
	private static int? _saveFileIndex;
	
	// Private CONSTANTS
	[HideInInspector] public int TARGET_FPS = 240;
	private StringBuilder s1;
	private StringBuilder s2;

	public ConsoleEmulator ConsoleEmulator => _consoleEmulator;
	public DynamicCulling DynamicCulling => _dynamicCulling;
	public MouseLookScript MouseLookScript => _mouseLookScript;
	public MFDManager MfdManager => _mfdManager;
	public GameObject Player => _playerReference.gameObject;
	
	internal IResourcesLoader ResourcesLoader => _resourcesLoader;
	
	[Inject] 
	private readonly PlayerReferenceManager _playerReference;
	[Inject] 
	private readonly LevelManager _levelManager;
	[Inject] private readonly ConsoleEmulator _consoleEmulator;
	[Inject] private readonly Config _config;
	[Inject] private readonly MFDManager _mfdManager;
	[Inject] private readonly Automap _automap;
	[Inject] private readonly Inventory _inventory;
	[Inject] private readonly MainMenuHandler _mainMenuHandler;
	[Inject] private readonly MouseLookScript _mouseLookScript;
	[Inject] private readonly PauseScript _pauseScript;
	[Inject] private readonly PlayerHealth _playerHealth;
	[Inject] private readonly DynamicCulling _dynamicCulling;
	[Inject] private readonly QuestLogNotesManager _questLogNotesManager;
	[Inject] private readonly LightDistanceCuller _lightDistanceCuller;
	[Inject] private readonly IResourcesLoader _resourcesLoader;

	private void Awake()
	{
		ScenesLoader.OnSceneLoaded += OnSceneLoaded;
		ScenesLoader.OnStartLoadScene += OnStartLoadScene;
		
#if UNITY_EDITOR || !UNITY_ANDROID
		TARGET_FPS = 144;
		Application.targetFrameRate = TARGET_FPS;
#endif
	
		player1Capsule = _playerReference.playerCapsule;
		player1CapsuleMainCameragGO = _playerReference.playerCapsuleMainCamera;
		player1TargettingPos = player1CapsuleMainCameragGO.transform;
		player1PlayerMovementScript = player1Capsule.GetComponent<PlayerMovement>();
		LoadTextForLanguage(0); // Initialize with US English (index 0)
		// Force Initialize all TextLocalization so language loaded from config
		// is set properly.
		TextLocalization texloc = null;
		List<GameObject> allParents = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
		int i,k, found;
		for (i=0;i<allParents.Count;i++) {
			found = 0;
			Component[] compArray = allParents[i].GetComponentsInChildren(typeof(TextLocalization),true); // find all TextLocalization components, including inactive (hence the true here at the end)
			for (k=0;k<compArray.Length;k++) {
				texloc = compArray[k].gameObject.GetComponent<TextLocalization>();
				if (texloc != null) {
					texloc.Awake();
					found++;
				}
			}
		}
		
		lastTargetRegistrySize = 0;
		for (i=0;i<allParents.Count;i++) {
			found = 0;
			Component[] compArray = allParents[i].GetComponentsInChildren(typeof(TargetIO),true); // find all TargetIO components, including inactive (hence the true here at the end)
			for (k=0;k<compArray.Length;k++) {
				TargetIO tio = compArray[k].gameObject.GetComponent<TargetIO>();
				if (tio != null) {
					tio.RemoteStart(gameObject,"Awake()"); // Reregister
					found++;
				}
			}
		}
		
		allParents.Clear();
		allParents = null;

		s1 = new StringBuilder(StringBuilderSize);
		s2 = new StringBuilder(StringBuilderSize);
		if (mainMenuInit != null) {
			if (!mainMenuInit.activeSelf) mainMenuInit.SetActive(true);
		}

		justSavedTimeStamp = Time.time - savedReminderTime;
		quaternionIdentity = Quaternion.identity;
		vectorZero = Vector3.zero;
		vectorOne = Vector3.one;
		LoadAudioLogMetaData();
		LoadDamageTablesData();
		LoadEnemyTablesData(); // Doing earlier, needed by AIController Start
		versionString = "v0.99.93"; // Global CITADEL PROJECT VERSION
		UnityEngine.Debug.Log("Citadel " + versionString
							  + ": " + System.Environment.NewLine
							  + "Start of C# Game Code, Welcome back Hacker!");
	}
	
	public bool RaycastBudgetExceeded() {
		return (numberOfRaycastsThisFrame > maxRaycastsPerFrame);
	}

    public void LoadTextForLanguage(int lang) {
// 		UnityEngine.Debug.Log("Loading language: " + lang.ToString());
        string readline; // variable to hold each string read in from the file
        int currentline = 0;
        string tF = "text_english.txt";
        switch (lang) {
            case 0: tF = "text_english.txt"; break;
			case 1: tF = "text_espanol.txt"; break; // UPKEEP: Other languages
			case 2: tF = "text_deutsch.txt"; break; // German
			case 3: tF = "text_francais.txt"; break; // French
			case 4: tF = "text_nihongo.txt"; break; // Japanese
			case 5: tF = "text_russkiy.txt"; break; // Russian
			case 6: tF = "text_italiano.txt"; break; // Italian
			case 7: tF = "text_portugues.txt"; break; // Portugese
        }

        StreamReader dataReader = Utils.ReadStreamingAsset(tF);
		if (stringTable.Length < 1025) stringTable = new string[1025];
        using (dataReader) {
            do {
                // Read the next line
                readline = dataReader.ReadLine();
                if (currentline < stringTable.Length) {
                    stringTable[currentline] = readline;
				} else {
					UnityEngine.Debug.Log("WARNING: Ran out of slots in "
										  + "stringTable at "
										  + currentline.ToString());
					dataReader.Close();
					return;
				}
                currentline++;
            } while (!dataReader.EndOfStream);
            dataReader.Close();
			stringTableLoaded = true;
            return;
        }
    }

	void Start() {
		_config.LoadConfig();
		layerMaskNPCSight = LayerMask.GetMask("Default","Geometry",
											  "Door","InterDebris",
											  "PhysObjects","Player","Player2",
											  "Player3","Player4");
		layerMaskNPCAttack = LayerMask.GetMask("Default","Geometry","NPC",
											   "Door","InterDebris",
											   "PhysObjects","Player","Player2",
											   "Player3","Player4");

		// Not including "Bullets" as this is merely used for spawning, not
		// setting level-wide NPC collisions.
		layerMaskNPCCollision = LayerMask.GetMask("Default","TransparentFX",
												  "IgnoreRaycast","Geometry",
												  "NPC","Door","InterDebris",
												  "Player","Clip","NPCClip",
												  "PhysObjects");

		// Water is a hidden layer that prevents the player frobbing through
		// gratings, X-doors, etc.  Oh and also water...if that were a thing.
		layerMaskPlayerFrob = LayerMask.GetMask("Default","Geometry","Water",
												"Door","InterDebris",
												"PhysObjects","Player2",
												"Player3","Player4",
												"CorpseSearchable");

		// Must have the geometry and default layers to prevent locking onto
		// NPCs through walls.
		layerMaskPlayerTargetIDFrob = LayerMask.GetMask("Default","Geometry",
														"Door",
														"Player2","Player3",
														"Player4","NPC",
														"CorpseSearchable");

		layerMaskPlayerAttack = LayerMask.GetMask("Default","Geometry","NPC",
												  "Bullets","Door",
												  "InterDebris","PhysObjects",
												  "Player2","Player3","Player4",
												  "CorpseSearchable");
			
		layerMaskExplosion = LayerMask.GetMask("Default","Geometry","NPC",
												  "Bullets","Door",
												  "InterDebris","PhysObjects",
												  "Player2","Player3","Player4",
											  	  "Player","CorpseSearchable");
			

		layerMaskPlayerFeet = LayerMask.GetMask("Default","Geometry");

		LoadCreditsData();
		StartCoroutine(InitializeEventSystem());
		questData = new QuestBits (_levelManager,this);
// 		if (mainFont1 != null) { // Ensure text is crisp and readable.
			mainFont1.material.mainTexture.filterMode = FilterMode.Point;
// 		}

// 		if (mainFont2 != null) { // Ensure text is crisp and readable.
			mainFont2.material.mainTexture.filterMode = FilterMode.Point;
// 		}
	}
	
	public void ResetPauseLists() {
		prb.Clear();
		psys.Clear();
		panimsList.Clear();
		PauseRigidbody[] prbTemp = FindObjectsByType<PauseRigidbody>(FindObjectsSortMode.None);
		for (int i=0;i<prbTemp.Length;i++) prb.Add(prbTemp[i]);

		 // P.P.S. PP. That's funny right there.  ...What?  I have kids.
		PauseParticleSystem[] ppses = FindObjectsByType<PauseParticleSystem>(FindObjectsSortMode.None);
		for (int i=0;i<ppses.Length;i++) psys.Add(ppses[i]);

		PauseAnimation[] panims = FindObjectsByType<PauseAnimation>(FindObjectsSortMode.None);
		for (int i=0;i<panims.Length;i++) panimsList.Add(panims[i]);

		ObjectContainmentSystem.FindAllFloorGOs();
		ObjectContainmentSystem.UpdateActiveFlooring();	
	}

	IEnumerator InitializeEventSystem () {
		yield return new WaitForSeconds(1f);
		if (eventSystem != null) eventSystem.SetActive(true);
	}

	public void LoadAudioLogMetaData() {
		// The following to be assigned to the arrays in the Unity Const data structure
		int readIndexOfLog, readLogImageLHIndex, readLogImageRHIndex; // look-up index for assigning the following data on the line in the file to the arrays
		StringBuilder readLogText = new StringBuilder(); // loaded into string audioLogSpeech2Text[]
		string readline; // variable to hold each string read in from the file
		char logSplitChar = ',';
		string tF = null;
		switch(AudioLanguage) {
            case 0: tF = "logs_text_english.txt"; break;
			case 1: tF = "logs_text_espanol.txt"; break; // UPKEEP: Other languages
			case 2: tF = "logs_text_deutsch.txt"; break; // German
			case 3: tF = "logs_text_francais.txt"; break; // French
			case 4: tF = "logs_text_nihongo.txt"; break; // Japanese
			case 5: tF = "logs_text_russkiy.txt"; break; // Russian
			case 6: tF = "logs_text_italiano.txt"; break; // Italian
			case 7: tF = "logs_text_portugues.txt"; break; // Portugese
        }

		StreamReader dataReader = Utils.ReadStreamingAsset(tF);
		using (dataReader) {
			do {
				int i = 0;
				// Read the next line
				readline = dataReader.ReadLine();
				string[] entries = readline.Split(logSplitChar);
				readIndexOfLog = Utils.GetIntFromStringAudLogText(entries[i]); i++;
				readLogImageLHIndex = Utils.GetIntFromStringAudLogText(entries[i]); i++;
				readLogImageRHIndex = Utils.GetIntFromStringAudLogText(entries[i]); i++;
				audioLogImagesRefIndicesLH[readIndexOfLog] = readLogImageLHIndex;
				audioLogImagesRefIndicesRH[readIndexOfLog] = readLogImageRHIndex;
				audiologNames[readIndexOfLog] = entries[i]; i++;
				audiologSenders[readIndexOfLog] = entries[i]; i++;
				audiologSubjects[readIndexOfLog] = entries[i]; i++;
				audioLogType[readIndexOfLog] = Utils.GetAudioLogTypeFromInt(Utils.GetIntFromStringAudLogText(entries[i])); i++;
				audioLogLevelFound[readIndexOfLog] = Utils.GetIntFromStringAudLogText(entries[i]); i++;
				readLogText.Clear();
				readLogText.Append(entries[i]); i++;
				// handle extra commas within the body text and append remaining portions of the line
				if (entries.Length > 8) {
					for (int j=9;j<entries.Length;j++) {
						readLogText.Append(",");
						readLogText.Append(entries[j]);  // combine remaining portions of text after other commas and add comma back
					}
				}
				audioLogSpeech2Text[readIndexOfLog] = readLogText.ToString();
			} while (!dataReader.EndOfStream);
			
			dataReader.Close();
			readLogText = null;
			return;
		}
	}

	private void OnStartLoadScene(string sceneName)
	{
		_targetRegister.Clear();
		_targetnameRegister.Clear();
		lastTargetRegistrySize = 0;
	}
	
	private void OnSceneLoaded(string sceneName)
	{
		QuitAfterSavingDone = false;
		ResetPauseLists();
		
		if (StartingNewGame)
		{
			loadingScreen.SetActive(true);
			_mainMenuHandler.OnNewGameStarted();
			StartingNewGame = false;
			LevelManager.CurrentLevel = LevelManager.NewGameLevelIndex;
			GoIntoGame();
			_lightDistanceCuller.Rebuild();
		}
		else if (_saveFileIndex.HasValue)
		{
			StartCoroutine(LoadRoutine(_saveFileIndex.Value,false));
		}
		else if (LevelManager.LoadLevelAfterSceneChanges)
		{
			_levelManager.LoadLevel(LevelManager.CurrentLevel, LevelManager.TargetPosition, loadLevelForced: true);
		}
	}
	
	private void LoadDamageTablesData () {
		string readline; // variable to hold each string read in from the file
		int currentline = 0;
		int readInt = 0;
		StreamReader dataReader = Utils.ReadStreamingAsset("damage_tables.txt");
		using (dataReader) {
			do {
				int i = 0;
				// Read the next line
				readline = dataReader.ReadLine();
				string[] entries = readline.Split(',');
				delayBetweenShotsForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"delayBetweenShotsForWeapon"); i++;
				delayBetweenShotsForWeapon2[currentline] = Utils.GetFloatFromString(entries[i],"delayBetweenShotsForWeapon2"); i++;
				damagePerHitForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"damagePerHitForWeapon"); i++;
				damagePerHitForWeapon2[currentline] = Utils.GetFloatFromString(entries[i],"damagePerHitForWeapon2"); i++;
				damageOverloadForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"damageOverloadForWeapon"); i++;
				energyDrainLowForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"energyDrainLowForWeapon"); i++;
				energyDrainHiForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"energyDrainHiForWeapon"); i++;
				energyDrainOverloadForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"energyDrainOverloadForWeapon"); i++;
				penetrationForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"penetrationForWeapon"); i++;
				penetrationForWeapon2[currentline] = Utils.GetFloatFromString(entries[i],"penetrationForWeapon2"); i++;
				offenseForWeapon[currentline] = Utils.GetFloatFromString(entries[i],"offenseForWeapon"); i++;
				offenseForWeapon2[currentline] = Utils.GetFloatFromString(entries[i],"offenseForWeapon2"); i++;
				readInt = Utils.GetIntFromString(entries[i],"attackTypeForWeapon"); i++;
				attackTypeForWeapon[currentline] = Utils.GetAttackTypeFromInt(readInt);
				currentline++;
			} while (!dataReader.EndOfStream);
			dataReader.Close();
			return;
		}
	}

	private void LoadCreditsData () {
		creditsText = new string[21];
		string readline; // variable to hold each string read in from the file
		int pagenum = 0;
		creditsLength = 1;
		StringBuilder page = new StringBuilder(StringBuilderSize);
		StreamReader dataReader = Utils.ReadStreamingAsset("credits.txt");
		using (dataReader) {
			do {
				// Read the next line
				readline = dataReader.ReadLine();
				char[] checkCharacter = readline.ToCharArray();
				if (checkCharacter.Length > 0) {
					if (checkCharacter[0] == '#') {
						creditsText[pagenum] = page.ToString();
						pagenum++;
						page.Clear();
						creditsLength++;
						continue;
					}
				}

				if (pagenum >= creditsText.Length) {
					UnityEngine.Debug.Log("BUG: Credits pagenum was too large at "
										  + pagenum.ToString());
					return;
				}

				page.Append(readline);
				page.Append(System.Environment.NewLine);
			} while (!dataReader.EndOfStream);

			dataReader.Close();
			return;
		}
	}
	
	private void LoadEnemyTablesData() {
		int numberOfNPCs = 29;
		nameForNPC = new string[numberOfNPCs];
		attackTypeForNPC = new AttackType[numberOfNPCs];
		attackTypeForNPC2 = new AttackType[numberOfNPCs];
		attackTypeForNPC3 = new AttackType[numberOfNPCs];
		damageForNPC = new float[numberOfNPCs];
		damageForNPC2 = new float[numberOfNPCs];
		damageForNPC3 = new float[numberOfNPCs];
		rangeForNPC = new float[numberOfNPCs];
		rangeForNPC2 = new float[numberOfNPCs];
		rangeForNPC3 = new float[numberOfNPCs];
		healthForNPC = new float[numberOfNPCs];
		healthForCyberNPC = new float[numberOfNPCs];
		perceptionForNPC = new PerceptionLevel[numberOfNPCs];
		disruptabilityForNPC = new float[numberOfNPCs];
		armorvalueForNPC = new float[numberOfNPCs];
		moveTypeForNPC = new AIMoveType[numberOfNPCs];
		defenseForNPC = new float[numberOfNPCs];
		yawSpeedForNPC = new float[numberOfNPCs];
		fovForNPC = new float[numberOfNPCs];
		fovAttackForNPC = new float[numberOfNPCs];
		fovStartMovementForNPC = new float[numberOfNPCs];
		distToSeeBehindForNPC = new float[numberOfNPCs];
		sightRangeForNPC = new float[numberOfNPCs];
		walkSpeedForNPC = new float[numberOfNPCs];
		runSpeedForNPC = new float[numberOfNPCs];
		attack1SpeedForNPC = new float[numberOfNPCs];
		attack2SpeedForNPC = new float[numberOfNPCs];
		attack3SpeedForNPC = new float[numberOfNPCs];
		attack3ForceForNPC = new float[numberOfNPCs];
		attack3RadiusForNPC = new float[numberOfNPCs];
		timeToPainForNPC = new float[numberOfNPCs];
		timeBetweenPainForNPC = new float[numberOfNPCs];
		timeTillDeadForNPC = new float[numberOfNPCs];
		timeToActualAttack1ForNPC = new float[numberOfNPCs];
		timeToActualAttack2ForNPC = new float[numberOfNPCs];
		timeToActualAttack3ForNPC = new float[numberOfNPCs];
		timeBetweenAttack1ForNPC = new float[numberOfNPCs];
		timeBetweenAttack2ForNPC = new float[numberOfNPCs];
		timeBetweenAttack3ForNPC = new float[numberOfNPCs];
		timeToChangeEnemyForNPC = new float[numberOfNPCs];
		timeIdleSFXMinForNPC = new float[numberOfNPCs];
		timeIdleSFXMaxForNPC = new float[numberOfNPCs];
		timeAttack1WaitMinForNPC = new float[numberOfNPCs];
		timeAttack1WaitMaxForNPC = new float[numberOfNPCs];
		timeAttack1WaitChanceForNPC = new float[numberOfNPCs];
		timeAttack2WaitMinForNPC = new float[numberOfNPCs];
		timeAttack2WaitMaxForNPC = new float[numberOfNPCs];
		timeAttack2WaitChanceForNPC = new float[numberOfNPCs];
		timeAttack3WaitMinForNPC = new float[numberOfNPCs];
		timeAttack3WaitMaxForNPC = new float[numberOfNPCs];
		timeAttack3WaitChanceForNPC = new float[numberOfNPCs];
		attack1ProjectileLaunchedTypeForNPC = new PoolType[numberOfNPCs];
		attack2ProjectileLaunchedTypeForNPC = new PoolType[numberOfNPCs];
		attack3ProjectileLaunchedTypeForNPC = new PoolType[numberOfNPCs];
		projectileSpeedAttack1ForNPC = new float[numberOfNPCs];
		projectileSpeedAttack2ForNPC = new float[numberOfNPCs];
		projectileSpeedAttack3ForNPC = new float[numberOfNPCs];
		hasLaserOnAttack1ForNPC = new bool[numberOfNPCs];
		hasLaserOnAttack2ForNPC = new bool[numberOfNPCs];
		hasLaserOnAttack3ForNPC = new bool[numberOfNPCs];
		explodeOnAttack3ForNPC = new bool[numberOfNPCs];
		preactivateMeleeCollidersForNPC = new bool[numberOfNPCs];
		huntTimeForNPC = new float[numberOfNPCs];
		flightHeightForNPC = new float[numberOfNPCs];
		flightHeightIsPercentageForNPC = new bool[numberOfNPCs];
		switchMaterialOnDeathForNPC = new bool[numberOfNPCs];
		hearingRangeForNPC = new float[numberOfNPCs];
		timeForTranquilizationForNPC = new float[numberOfNPCs];
		hopsOnMoveForNPC = new bool[numberOfNPCs];
		typeForNPC = new NPCType[numberOfNPCs];
		projectile1PrefabForNPC = new int[numberOfNPCs];
		projectile2PrefabForNPC = new int[numberOfNPCs];
		projectile3PrefabForNPC = new int[numberOfNPCs];
		string readline; // variable to hold each string read in from the file
		bool skippedFirstLine = false;
		int currentline = 1;
		int readInt = 0;
		int i = 0;
		int refIndex = 0;
		StreamReader dataReader = Utils.ReadStreamingAsset("enemy_tables.csv");
		using (dataReader) {
			do {
				i = 0;
				refIndex = 0;
				readline = dataReader.ReadLine(); // Read the next line
				if (!skippedFirstLine) { skippedFirstLine = true; continue; }

				string[] entries = readline.Split(',');
				char[] commentCheck = entries[i].ToCharArray();
				if (commentCheck[0] == '/' && commentCheck[1] == '/') {
					continue; // Skip lines that start with '//'
				}

				refIndex = Utils.GetIntFromStringAudLogText(entries[i+1]); // Index is stored at 2nd spot
				if (refIndex < 0 || refIndex > 28) continue; // Invalid value, skip

				nameForNPC[refIndex] = entries[i].Trim(); i++;
				i++; // No need to read the index again so we skip over it.
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); attackTypeForNPC[refIndex] = Utils.GetAttackTypeFromInt(readInt); i++; 
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); attackTypeForNPC2[refIndex] = Utils.GetAttackTypeFromInt(readInt); i++; 
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); attackTypeForNPC3[refIndex] = Utils.GetAttackTypeFromInt(readInt); i++; 
				damageForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				damageForNPC2[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				damageForNPC3[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				rangeForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				rangeForNPC2[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				rangeForNPC3[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				healthForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				healthForCyberNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); perceptionForNPC[refIndex] = Utils.GetPerceptionLevelFromInt(readInt); i++;
				disruptabilityForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				armorvalueForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				defenseForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				moveTypeForNPC[refIndex] = Utils.GetMoveTypeFromInt(Utils.GetIntFromStringAudLogText(entries[i].Trim())); i++;
				yawSpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				fovForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				fovAttackForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				fovStartMovementForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				distToSeeBehindForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				sightRangeForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				walkSpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				runSpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				attack1SpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				attack2SpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				attack3SpeedForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				attack3ForceForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				attack3RadiusForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeToPainForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeBetweenPainForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeTillDeadForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeToActualAttack1ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeToActualAttack2ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeToActualAttack3ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeBetweenAttack1ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeBetweenAttack2ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeBetweenAttack3ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeToChangeEnemyForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeIdleSFXMinForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeIdleSFXMaxForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack1WaitMinForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack1WaitMaxForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack1WaitChanceForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack2WaitMinForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack2WaitMaxForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack2WaitChanceForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack3WaitMinForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack3WaitMaxForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeAttack3WaitChanceForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++; //attack1ProjectileLaunchedTypeForNPC[refIndex] = GetPoolType(readInt);
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++; //attack2ProjectileLaunchedTypeForNPC[refIndex] = GetPoolType(readInt);
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++; //attack3ProjectileLaunchedTypeForNPC[refIndex] = GetPoolType(readInt); // Not worrying about projectile type for now, would require indexing all of the pools.
				projectileSpeedAttack1ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				projectileSpeedAttack2ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				projectileSpeedAttack3ForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				hasLaserOnAttack1ForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				hasLaserOnAttack2ForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				hasLaserOnAttack3ForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				explodeOnAttack3ForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				preactivateMeleeCollidersForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				huntTimeForNPC[refIndex] = 5f + Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				flightHeightForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				flightHeightIsPercentageForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				switchMaterialOnDeathForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				hearingRangeForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				timeForTranquilizationForNPC[refIndex] = Utils.GetFloatFromStringDataTables(entries[i].Trim()); i++;
				hopsOnMoveForNPC[refIndex] = Utils.GetBoolFromStringInTables(entries[i].Trim()); i++;
				readInt = Utils.GetIntFromStringAudLogText(entries[i].Trim()); typeForNPC[refIndex] = Utils.GetNPCTypeFromInt(readInt); i++;
				projectile1PrefabForNPC[refIndex] = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++;
				projectile2PrefabForNPC[refIndex] = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++;
				projectile3PrefabForNPC[refIndex] = Utils.GetIntFromStringAudLogText(entries[i].Trim()); i++;

				currentline++;
				if (currentline > 29) break;
			} while (!dataReader.EndOfStream);
			dataReader.Close();
			return;
		}
	}
	
	public string GetTargetID(int npcIndex) {
		if (npcIndex > 29) return "BUG: npcIndex too large for GetTargetID!";

		npcCount[npcIndex]++;
		return nameForNPC[npcIndex] + npcCount[npcIndex].ToString();
	}

	public string GetCyberTargetID(int cyberNPCIndex) {
		switch(cyberNPCIndex) {
			case 0: return stringTable[499];
			case 1: return stringTable[500];
			case 2: return stringTable[501];
			case 3: return stringTable[502];
		}
		return stringTable[503];
	}

	// StatusBar Print
	public void sprint(string input, GameObject player) {
		#if UNITY_EDITOR
			// Don't spam unneeded info here.
		#else
			UnityEngine.Debug.Log(input);
		#endif
		statusBar.SendText(input);
	}
	
	public void sprint(int lingdex) {
		if (lingdex < 0) return;
		if (lingdex > stringTable.Length) return;
		
		sprint(stringTable[lingdex],null);
	}

	public void sprint(string input) { sprint(input,null); }

	public GameObject GetObjectFromPool(PoolType pool) {
		if (pool == PoolType.None) return null; // Do nothing, no pool requested.

		GameObject poolContainer = null;
		string poolName = " ";

		switch (pool) {
		case PoolType.SparksSmall:           return InstantiatePrefab(sparksSmall);
		case PoolType.CameraExplosions:      return InstantiatePrefab(cameraExplosion);
		case PoolType.BloodSpurtSmall:       return InstantiatePrefab(bloodSpurtSmall);
		case PoolType.BloodSpurtSmallYellow: return InstantiatePrefab(bloodSpurtSmallYellow);
		case PoolType.BloodSpurtSmallGreen:  return InstantiatePrefab(bloodSpurtSmallGreen);
		case PoolType.SparksSmallBlue:       return InstantiatePrefab(sparksSmallBlue);
		case PoolType.HopperImpact: 
			poolContainer = Pool_HopperImpact;
			poolName = "HopperImpact ";
			break;
		case PoolType.GrenadeFragExplosions: 
			poolContainer = Pool_GrenadeFragExplosions;
			poolName = "GrenadeFragExplosions ";
			break;
        case PoolType.Vaporize:
            poolContainer = Pool_Vaporize;
            poolName = "Vaporize ";
            break;
        case PoolType.MagpulseImpacts:
            poolContainer = Pool_MagpulseImpacts;
            poolName = "MagpulseImpacts ";
            break;
        case PoolType.StungunImpacts:
            poolContainer = Pool_StungunImpacts;
            poolName = "StungunImpacts ";
            break;
        case PoolType.RailgunImpacts:
            poolContainer = Pool_RailgunImpacts;
            poolName = "RailgunImpacts ";
            break;
        case PoolType.PlasmaImpacts:
            poolContainer = Pool_PlasmaImpacts;
            poolName = "PlasmaImpacts ";
            break;
		case PoolType.ProjEnemShot6Impacts:
            poolContainer = Pool_ProjEnemShot6Impacts;
            poolName = "ProjEnemShot6Impacts ";
            break;
		case PoolType.ProjEnemShot2Impacts:
            poolContainer = Pool_ProjEnemShot2Impacts;
            poolName = "ProjEnemShot2Impacts ";
            break;
		case PoolType.ProjSeedPodsImpacts:
            poolContainer = Pool_ProjSeedPodsImpacts;
            poolName = "ProjSeedPodsImpacts ";
            break;
		case PoolType.TempAudioSources:
            poolContainer = Pool_TempAudioSources;
            poolName = "TempAudioSources ";
            break;
		case PoolType.GrenadeEMPExplosions:
            poolContainer = Pool_GrenadeEMPExplosions;
            poolName = "GrenadeEMPExplosions ";
            break;
		case PoolType.ProjEnemShot4Impacts:
            poolContainer = Pool_ProjEnemShot4Impacts;
            poolName = "ProjEnemShot4Impacts ";
            break;
		case PoolType.CrateExplosions:
            poolContainer = Pool_CrateExplosions;
            poolName = "CrateExplosions ";
            break;
		case PoolType.GrenadeFragLive:
            poolContainer = Pool_GrenadeFragLive;
            poolName = "GrenadeFragLive ";
            break;
		case PoolType.ConcussionLive:
            poolContainer = Pool_ConcussionLive;
            poolName = "ConcussionLive ";
            break;
		case PoolType.EMPLive:
            poolContainer = Pool_EMPLive;
            poolName = "EMPLive ";
            break;
		case PoolType.GasLive:
            poolContainer = Pool_GasLive;
            poolName = "GasLive ";
            break;
		case PoolType.GasExplosions:
            poolContainer = Pool_GasExplosions;
            poolName = "GasExplosions ";
            break;
		case PoolType.CorpseHit:
            poolContainer = Pool_CorpseHit;
            poolName = "CorpseHit ";
            break;
		case PoolType.LeafBurst:
			poolContainer = Pool_LeafBurst;
			poolName = "LeafBurst ";
			break;
		case PoolType.MutationBurst:
			poolContainer = Pool_MutationBurst;
			poolName = "MutationBurst ";
			break;
		case PoolType.GraytationBurst:
			poolContainer = Pool_GraytationBurst;
			poolName = "GraytationBurst ";
			break;
		case PoolType.BarrelExplosions:
			poolContainer = Pool_BarrelExplosions;
			poolName = "BarrelExplosions ";
			break;
		case PoolType.CyberDissolve:
			poolContainer = Pool_CyberDissolve;
			poolName = "CyberDissolve ";
			break;
		case PoolType.AutomapBotOverlays:
			poolContainer = Pool_AutomapBotOverlays;
			poolName = "AutomapBotOverlays ";
			break;
		case PoolType.AutomapCyborgOverlays:
			poolContainer = Pool_AutomapCyborgOverlays;
			poolName = "AutomapCyborgOverlays ";
			break;
		case PoolType.AutomapMutantOverlays:
			poolContainer = Pool_AutomapMutantOverlays;
			poolName = "AutomapMutantOverlays ";
			break;
		case PoolType.AutomapCameraOverlays:
			poolContainer = Pool_AutomapCameraOverlays;
			poolName = "AutomapCameraOverlays ";
			break;
        }

		if (poolContainer == null) { UnityEngine.Debug.Log("Cannot find " + poolName + "pool"); return null; }
		for (int i=0;i<poolContainer.transform.childCount;i++) {
			Transform child = poolContainer.transform.GetChild(i);
			if (child.gameObject.activeSelf == false) return child.gameObject;
			if (i == (poolContainer.transform.childCount - 1)) UnityEngine.Debug.Log("Ran out of items for " + poolName);
		}

		return null;

		GameObject InstantiatePrefab(GameObject prefab)
		{
			return RootInstaller.InstantiatePrefab(prefab, Vector3.zero, quaternionIdentity);
		}
	}

	void ClearAutomapOverlay(GameObject over) {
		Image img = over.GetComponent<Image>();
		Utils.DisableImage(img);
		Utils.Deactivate(over);
	}

	public void ClearActiveAutomapOverlays() {
		for (int i = 0; i < Pool_AutomapCameraOverlays.transform.childCount; i++) {
			Transform child = Pool_AutomapCameraOverlays.transform.GetChild(i);
			ClearAutomapOverlay(child.gameObject);
		}

		for (int i = 0; i < Pool_AutomapMutantOverlays.transform.childCount; i++) {
			Transform child = Pool_AutomapMutantOverlays.transform.GetChild(i);
			ClearAutomapOverlay(child.gameObject);
		}

		for (int i = 0; i < Pool_AutomapCyborgOverlays.transform.childCount; i++) {
			Transform child = Pool_AutomapCyborgOverlays.transform.GetChild(i);
			ClearAutomapOverlay(child.gameObject);
		}

		for (int i = 0; i < Pool_AutomapBotOverlays.transform.childCount; i++) {
			Transform child = Pool_AutomapBotOverlays.transform.GetChild(i);
			ClearAutomapOverlay(child.gameObject);
		}
	}

    public GameObject GetImpactType(HealthManager hm) {
        if (hm == null) return GetObjectFromPool(PoolType.SparksSmall);
        switch (hm.bloodType) {
            case BloodType.None: return GetObjectFromPool(PoolType.SparksSmall);
            case BloodType.Red: return GetObjectFromPool(PoolType.BloodSpurtSmall);
            case BloodType.Yellow: return GetObjectFromPool(PoolType.BloodSpurtSmallYellow);
            case BloodType.Green: return GetObjectFromPool(PoolType.BloodSpurtSmallGreen);
            case BloodType.Robot: return GetObjectFromPool(PoolType.SparksSmallBlue);
			case BloodType.Leaf: return GetObjectFromPool(PoolType.LeafBurst);
			case BloodType.Mutation: return GetObjectFromPool(PoolType.MutationBurst);
			case BloodType.GrayMutation: return GetObjectFromPool(PoolType.GraytationBurst);
        }

        return GetObjectFromPool(PoolType.SparksSmall);
	}

	// Wrapper function to enable Save to be a coroutine so we can display
	// progress.  We don't though, currently we just haul off and get it with
	// top speed, no pausing momentarily to draw any progress bar since it is
	// plenty fast enough.
	public void StartSave(int index, string savename) {
		if (_playerHealth.hm.health < 1.0f) return; // Can't save while dead!
		StartCoroutine(SaveRoutine(index,savename));
	}

	// Save the Game
	// ========================================================================
	public IEnumerator SaveRoutine(int saveFileIndex,string savename) {
		bool quitAfterSaving = QuitAfterSavingDone;
		sprint(stringTable[194]); // Indicate we are saving "Saving..."
		yield return null; // Update to show this sprint.

		Stopwatch saveTimer = new Stopwatch();
		saveTimer.Start();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		List<string> saveData = new List<string>();
		int i,j;

		// All saveable classes
		List<GameObject> saveableGameObjects = new List<GameObject>();
		saveableGameObjects.AddRange(Utils.FindAllSaveObjectsGOs());
		//UnityEngine.Debug.Log("Found "
		//					  + saveableGameObjects.Count.ToString()
		//					  + " total saveables for save.");

		if (string.IsNullOrWhiteSpace(savename)) {
			savename = "Unnamed " + saveFileIndex.ToString();
		}

		saveData.Add(savename);
		
		// Credit Stats and Times
		s1.Clear();
		s1.Append(Utils.FloatToString(_pauseScript.relativeTime,"GameTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(_pauseScript.absoluteTime,"TotalPlayTime"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(kills,"kills"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(kills,"cyberkills"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(shotsFired,"shotsFired"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(grenadesThrown,"grenadesThrown"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(damageDealt,"damageDealt"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(damageReceived,"damageReceived"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.IntToString(savesScummed,"savesScummed"));
		saveData.Add(s1.ToString());

		s1.Clear();
		s1.Append(_levelManager.Save());
		s1.Append(Utils.splitChar);
		s1.Append(questData.Save());
		s1.Append(Utils.splitChar);
		s1.Append(_questLogNotesManager.Save());
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(difficultyCombat,"difficultyCombat"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(difficultyMission,"difficultyMission"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(difficultyPuzzle,"difficultyPuzzle"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.UintToString(difficultyCyber,"difficultyCyber"));
		saveData.Add(s1.ToString());
		
		s1.Clear();

		// Save all the objects data
		for (i=0;i<saveableGameObjects.Count;i++) {
			// Take this object's data and add it to the array.
			saveData.Add(SaveObject.Save(_levelManager,saveableGameObjects[i])); // <<< THIS IS IT <<<
		}
		
		foreach (var dynamicObjectsSaveList in LevelManager.DynamicObjectsSavestrings)
		{
			saveData.AddRange(dynamicObjectsSaveList);
		}
		
		foreach (var staticObjectsSaveList in LevelManager.StaticObjectsSaveStrings)
		{
			saveData.AddRange(staticObjectsSaveList);
		}
		
		// Write to file
		string sName = "sav" + saveFileIndex.ToString() + ".txt";
		string basePath = Utils.GetAppropriateDataPath();
		string sPath;
		sPath = Utils.SafePathCombine(basePath,sName);
		StreamWriter sw = new StreamWriter(sPath,false,Encoding.ASCII);
		if (sw != null) {
			using (sw) {
				for (j=0;j<saveData.Count;j++) {
					if (!string.IsNullOrWhiteSpace(saveData[j])) {
						sw.WriteLine(saveData[j]);
					}
				}
				sw.Close();
			}
		}

		// Make "Done!" appear at the end of the line after "Saving..." is finished, concept from Halo's "Checkpoint...Done!"
		saveTimer.Stop();
		sprint(stringTable[195] + " (" + saveTimer.Elapsed.ToString() + ")");
		if (saveFileIndex < 7) justSavedTimeStamp = Time.time + savedReminderTime; // using normal run time, don't ask again to save for next 7 seconds

		if (quitAfterSaving)
		{
			QuitAfterSavingDone = false;
			_pauseScript.NoSavePauseQuit();
		}
	}

	// Start a New Game
	// ========================================================================
	// Sequence is as follows
	// 1. Button on New Game page sets difficulty, player name, and calls this.
	// 2. Write ng.dat file to show value of false to prevent intro playing.
	// 3. Look for GameNotYetStarted GameObject existence.
	// 4a. If fresh
	//   1. Destroy the GameNotYetStarted indicator GameObject.
	//   2. then simply turn off menu.
	// 4b. If not fresh
	//   1. Create NewGameIndicator GameObject, with transition handler.
	//   2. Flag it as DontDestroyOnLoad so it is preserved across scenes.
	//   3. Unload current scene.
	//   4. SceneTransitionHandler on DontDestroyOnLoad GameObject loads scene.
	// 5. _consts.Start() checks for NewGameIndicator existence.
	// 6a. If present, simply turn off main menu.
	// 6b. Else same as if game was started from scratch.
	// 7. Go into the game.  Player now has normal control.
	public void NewGame() {
		//UnityEngine.Debug.Log("Starting new game!");
		StartingNewGame = true;
		loadingScreen.SetActive(true);
		_levelManager.ChangeGameScene(LevelManager.NewGameLevelIndex, changeSceneForced: true);
	}

	// Going into the game removes the helper GameObjects for these reasons:
	// - GameNotYetStarted, Game is now started, mark it as such.  This happens
	//                      only on game entry at beginning of session (first
	//                      time after launching the game).
	// - NewGameIndicator,  Game is no longer a new game, because it's started.
	// - LoadGameIndicator, Game should have been loaded prior to entry.
	public async UniTask GoIntoGame(Stopwatch loadTimer) {
		await _levelManager.LoadLevelData(LevelManager.CurrentLevel);
		Cursor.visible = true;
		Utils.Deactivate(loadingScreen);
		Utils.Deactivate(_mainMenuHandler.IntroVideo);
		Utils.Deactivate(_mainMenuHandler.IntroVideoContainer);
		_automap.ActivateAutomapUI();
		_automap.DeactivateAutomapUI();
		Utils.Deactivate(_pauseScript.mainMenu);
		_pauseScript.PauseDisable();
		if (_playerHealth != null) {
			if (_playerHealth.hm != null) _playerHealth.hm.ClearOverlays();
		}

		if (NoShootMode) _mouseLookScript.ForceInventoryMode();
		Utils.Activate(player1Capsule);
		Utils.Activate(player1CapsuleMainCameragGO.transform.parent.gameObject);
		Utils.Activate(player1CapsuleMainCameragGO);
		Utils.EnableCamera(_mouseLookScript.playerCamera);
		if (loadTimer == null) {
			sprint(stringTable[197]);
		} else {
			sprint(stringTable[197] + " (" + loadTimer.Elapsed.ToString() + ")"); // Loading...Done!
		}
		
		_dynamicCulling.Cull_Init();
		_dynamicCulling.Cull(false);
	}

	public void GoIntoGame() {
		GoIntoGame(null);
	}

	public void ShowLoading() {
		Utils.DisableCamera(_mouseLookScript.playerCamera); // Hide changes.
		_pauseScript.mainMenu.SetActive(false); // Ensure that main menu is 
												 // off if came from Load page.
 		_pauseScript.PauseEnable(); // Enable pause to make sure that nothing 
									 // goes on during couroutine as it happens
									 // over multiple frames.
		_pauseScript.DisablePauseUI(); // Enable loading texts and unlock cursor.
		sprint(stringTable[196]); // Loading...
		if (_playerHealth != null) _playerHealth.hm.ClearOverlays();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		loadPercentText.text = "(1) --.--";

		// Clear the HUD
		_mfdManager.TabReset(true);
		_mfdManager.TabReset(false);
		_mfdManager.DisableAllCenterTabs();
		loadingScreen.SetActive(true);
		AutoSplitterData.isLoading = true;
	}

	// Load the Game
	// ========================================================================
	// Sequence is as follows
	// 1. Player clicks on a button in load game menu or presses Quick Load.
	// 2. This function Load() is called with index -1 thru 7 and actual=false.
	// 3. Load then creates a DontDestroyOnLoad gameobject
	// 4. Current scene is unloaded.
	// 5. SceneTransitionHandler on DontDestroyOnLoad gameobject loads scene.
	// 6. Const Start() detects DontDestroyOnLoad object, uses Load actual=true
	// 7. Load then does actual load.
	//    a. Iterate over and destroy all dynamic objects in level containers.
	//    b. Find all remaining saveables to load static objects to.
	//    c. Load to static saveable objects.
	//    d. Iterate over dynamic object containers instantiating from save.
	public void Load(int saveFileIndex, bool actual) {
		_saveFileIndex = null;
		ShowLoading();
		StartingNewGame = false;
		_saveFileIndex = saveFileIndex;
	
		var levelIndexFromSave = ReadLevelIndexFromSave(saveFileIndex);

		if (levelIndexFromSave != LevelManager.CurrentLevel)
		{
			_levelManager.ChangeGameScene(levelIndexFromSave);
		}
		else
		{
			StartCoroutine(LoadRoutine(_saveFileIndex.Value,false));
		}
	}

	// LOAD 2. Called from Load menu or Quick Load.
	// LOAD 6. Called from _consts.Start().
	public IEnumerator LoadRoutine(int saveFileIndex, bool actual)
	{
		QuitAfterSavingDone = false;
		Stopwatch loadTimer = new Stopwatch();
		Stopwatch loadUpdateTimer = new Stopwatch(); // For loading % indicator.
		loadTimer.Start();
		loading = true;
		UnityEngine.Debug.Log("Start of Load for index " + saveFileIndex.ToString());
		yield return null; // Update the view to show ShowLoading changes.

		string readline; 					// Initialize temporary variables.
		int numSaveablesFromSavefile = 0;
		int i,j,k;
		GameObject currentGameObjectInScene = null;
		List<GameObject> saveableGameObjectsInScene = new List<GameObject>();
		loadPercentText.text = "Preparing...";
		yield return null; // Update progress text.

		SaveObject.currentObjectInfo = "Start of Load...";

		// Remove and clear out everything and reset any lists.
		ClearActiveAutomapOverlays();
		_targetRegister.Clear();
		_targetnameRegister.Clear();
		for (i=0;i<healthObjectsRegistration.Length;i++) {
			healthObjectsRegistration[i] = null;
		}
		
		LevelManager.ResetSaveStrings();
		LevelManager.StaticObjectsSaveStrings.ResetSaveStrings();
		var readFileList = ReadSave(saveFileIndex);
		
		_levelManager.UnloadLevelDynamicObjects(LevelManager.CurrentLevel,false); // Delete them all!
		_levelManager.UnloadLevelNPCs(LevelManager.CurrentLevel); // Delete them all!
		loadPercentText.text = "Preparing level " + i.ToString();
		yield return new WaitForSeconds(0.1f); // Update progress text.

		loadPercentText.text = "Open Save File         ";
		yield return null; // Update progress text.

		int index = 0; // Caching since it will be iterated over in a loop.
		string[] entries = Array.Empty<string>();
											 // on individual lines.
		List<GameObject> allParents = SceneManager.GetActiveScene().GetRootGameObjects().ToList();
		allParents.Add(Player);
		if (readFileList.Count > 0) {
			loadPercentText.text = "Load Quest Data...     ";
			yield return null; // to update the sprint
			int numSaveFileLines = readFileList.Count;
			numSaveablesFromSavefile = numSaveFileLines - 3;

			// readFileList[0] == saveName;  Not important, we are loading already now
			//index = 0; // Uncomment this if we pull in the saveName from this line for something.

			// Read in global time, pause data, credit stats
			entries = readFileList[1].Split(Utils.splitCharChar);
			
			// The global time from which everything checks it's
			// somethingerotherFinished timer states.
			_pauseScript.relativeTime = Utils.GetFloatFromString(entries[index],"GameTime"); index++;
			_pauseScript.absoluteTime = Utils.GetFloatFromString(entries[index],"TotalPlayTime"); index++;
			kills = Utils.GetIntFromString(entries[index],"kills"); index++;
			cyberkills = Utils.GetIntFromString(entries[index],"cyberkills"); index++;
			shotsFired = Utils.GetIntFromString(entries[index],"shotsFired"); index++;
			grenadesThrown = Utils.GetIntFromString(entries[index],"grenadesThrown"); index++;
			damageDealt = Utils.GetFloatFromString(entries[index],"damageDealt"); index++;
			damageReceived = Utils.GetFloatFromString(entries[index],"damageReceived"); index++;
			savesScummed = 1 + Utils.GetIntFromString(entries[index],"savesScummed"); // 1+, you're doin' it now!
			index = 0; // reset before starting next line

			// Read in global states, difficulties, and quest mission bits.
			entries = readFileList[2].Split(Utils.splitCharChar);
			var loadLevelTask = _levelManager.Load(entries, index);
			yield return loadLevelTask;
			index = loadLevelTask.Result;
			index = questData.Load(ref entries,index);
			index = _questLogNotesManager.Load(ref entries,index);
			difficultyCombat = Utils.GetIntFromString(entries[index],"difficultyCombat"); index++;
			difficultyMission = Utils.GetIntFromString(entries[index],"difficultyMission"); index++;
			difficultyPuzzle = Utils.GetIntFromString(entries[index],"difficultyPuzzle"); index++;
			difficultyCyber = Utils.GetIntFromString(entries[index],"difficultyCyber"); index++;
			loadPercentText.text = "Preprocess Save File...";
			yield return null;

			// First pass to initialize tracking arrays:
			// - saveFile_Line_SaveID, This holds the full list of all unique IDs.
			// - saveableIsInstantiated, True if object is instantiated prefab.
			int[] saveFile_Line_SaveID = new int[numSaveFileLines];
			bool[] saveFile_Line_IsInstantiated = new bool[numSaveFileLines];
			bool[] alreadyLoadedLineFromSaveFile = new bool[numSaveFileLines];
			Utils.BlankBoolArray(ref alreadyLoadedLineFromSaveFile,false); // Fill with false.
			for (i = 3; i < numSaveFileLines; i++) {
				entries = readFileList[i].Split(Utils.splitCharChar);
				if (entries.Length < 1)  continue;

				saveFile_Line_SaveID[i] = Utils.GetIntFromString(entries[2],"SaveID");
				saveFile_Line_IsInstantiated[i] = Utils.GetBoolFromString(entries[3],"instantiated");
			}

			loadPercentText.text = "Preprocess Arrays...   ";
			yield return null;
			index = 3;
			SaveObject currentSaveObjectInScene;

			// LOAD 7b. FIND ALL STATIC SAVEABLES
			// DO THIS AFTER BLANKING TO ENSURE WE HAVE UP-TO-DATE LIST!!
			// Find all gameobjects with SaveObject script attached.
			// This assumes every prefab and static GameObject has only one
			// SaveObject script attached at top parent for that object.
			// Exceptions:
			// - func_wall has its SaveObject on first child
			// - se_corpse_eaten has its SearchableItem on first child
			saveableGameObjectsInScene.Clear();
			saveableGameObjectsInScene.AddRange(Utils.FindAllSaveObjectsGOs());
			//UnityEngine.Debug.Log("Found " 
			//					  + saveableGameObjectsInScene.Count.ToString()
			//					  + " total static saveables remaining in "
			//					  + "scene after blanking out dynamic "
			//					  + "containers and NPC containers.");

			bool[] alreadyCheckedThisSaveableGameObjectInScene = new bool[saveableGameObjectsInScene.Count];
			Utils.BlankBoolArray(ref alreadyCheckedThisSaveableGameObjectInScene,false); // Fill with false.

			bool[] alreadyCheckedThisInstantiableGameObjectInScene = new bool[saveableGameObjectsInScene.Count];
			Utils.BlankBoolArray(ref alreadyCheckedThisInstantiableGameObjectInScene,false); // Fill with false.

			// LOAD 7c. LOAD TO STATIC SAVEABLES
			// Ok, so we have a list of all saveableGameObjectsInScene and a list of
			//   all saveables from the savefile.
			// Main iteration loops through all lines in the savefile.
			// Second iteration loops through all saveableGameObjectsInScene to find a match.
			// The save file will always have more objects in it than in the
			//   level since we removed the instantiables.
			// When we come across an instantiated object in the saveable file,
			//   we need to skip it for later and instantiate them all.
			loadPercentText.text = "Loading Static Objects: 0.0% (    0 / "
								   + numSaveablesFromSavefile.ToString() + ")";
			yield return null;
			loadUpdateTimer.Start(); // For loading update
			float perc = 0f;
			for (i = 3; i < numSaveFileLines; i++)
			{
				if (saveFile_Line_IsInstantiated[i]) continue; // Skip instantiables.
				bool wasLoaded = false;
				alreadyLoadedLineFromSaveFile[i] = true;

				for (j=0;j<(saveableGameObjectsInScene.Count);j++) {
					if (alreadyCheckedThisSaveableGameObjectInScene[j] || saveableGameObjectsInScene[j] == null)
					{
						continue;
					} // skip checking this and doing GetComponent

					currentGameObjectInScene = saveableGameObjectsInScene[j];
					currentSaveObjectInScene = SaveLoad.GetPrefabSaveObject(currentGameObjectInScene);

					if (!currentSaveObjectInScene.instantiated) alreadyCheckedThisInstantiableGameObjectInScene[j] = true; // Huge time saver right here!

					// Static Objects all have unique ID.
// 					if (currentSaveObjectInScene.SaveID == 999999) UnityEngine.Debug.Log("Checking player during load");
					if (currentSaveObjectInScene.SaveID == saveFile_Line_SaveID[i]
						&& currentSaveObjectInScene.SaveID != 0) {
						
// 						if (currentSaveObjectInScene.SaveID == 999999) UnityEngine.Debug.Log("Found player in savefile on line " + i.ToString() + " during load");

						//if (!saveableGameObjectsInScene[j].isStatic // EDITOR ONLY!!!
						if (currentSaveObjectInScene.instantiated
							&& currentSaveObjectInScene.saveType != SaveableType.Light) {
							UnityEngine.Debug.Log("For some reason, attempting "
												  + "to load to dynamic object "
												  + saveableGameObjectsInScene[j].name);
						}
						
						
						entries = readFileList[i].Split(Utils.splitCharChar);
						PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(currentGameObjectInScene,true);
						yield return SaveObject.Load(this,_levelManager,currentGameObjectInScene, entries,i,prefID);
						wasLoaded = true;
						alreadyCheckedThisSaveableGameObjectInScene[j] = true; // Huge time saver right here!
						break;
					}
				}

				const string levelIdName = "levelID";
				if (!wasLoaded && readFileList[i].Contains(levelIdName))
				{
					var line = readFileList[i];
					var entryToParse = line.Split(Utils.splitCharChar).First(entry => entry.Contains(levelIdName));
					LevelManager.StaticObjectsSaveStrings[Utils.GetIntFromString(entryToParse,levelIdName)].Add(line);
				}

				perc = (float)i/(float)numSaveablesFromSavefile*100f;
				loadPercentText.text = "Loading Static Objects: "
									   + perc.ToString("0.0") + "% ("
									   + i.ToString() + " / "
									   + numSaveablesFromSavefile.ToString()
									   + ")";
									   
				if (loadUpdateTimer.ElapsedMilliseconds > 500) {
					loadUpdateTimer.Reset();
					loadUpdateTimer.Start();
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					yield return null;
				}
			}
			loadUpdateTimer.Stop();

			// Check if we missed a static non-instantiable object to load to.
			int numberOfMissedObjects = 0;
			SaveObject sob;
			for (i=0;i<saveableGameObjectsInScene.Count;i++) {
				if (alreadyCheckedThisInstantiableGameObjectInScene[i]) {
					continue;
				}

				sob = SaveLoad.GetPrefabSaveObject(saveableGameObjectsInScene[i]);
				if (sob != null) {
					if (!sob.instantiated) {
						UnityEngine.Debug.Log(saveableGameObjectsInScene[i].name
						+ " not loaded during Static Pass and is static");
					} else {
						UnityEngine.Debug.Log(saveableGameObjectsInScene[i].name
						+ " not loaded during Static Pass and is not static");
					}
				} else {
					UnityEngine.Debug.Log(saveableGameObjectsInScene[i].name
						+ " not loaded during Static Pass and is not static");
				}
				numberOfMissedObjects++;
			}
			if (numberOfMissedObjects > 0) {
				UnityEngine.Debug.Log("numberOfMissedObjects: "
									  + numberOfMissedObjects.ToString());
			}

			// LOAD 7d. INSTANTIATE AND LOAD TO INSTANTIATED SAVEABLES
			// Now time to instantiate anything left that's supposed to be here
			loadUpdateTimer.Start(); // For loading update
			int constdex = -1; // To store the index of Master Index table.
			int levID = 1; // To store the level this was in.
			int savID = -1; // To store the SaveObject.SaveID.
			float percLoaded = 0f;
			GameObject instGO = null;
			GameObject contnr = null;
// 			UnityEngine.Debug.Log("numSaveFileLines: " + numSaveFileLines.ToString());
			for (i = 3 ; i < numSaveFileLines; i++) {
				if (alreadyLoadedLineFromSaveFile[i]) continue;

				entries = readFileList[i].Split(Utils.splitCharChar);
				if (entries.Length > 1) {
					constdex = Utils.GetIntFromString(entries[0],"constIndex");
					levID = Utils.GetIntFromString(entries[19],"levelID");
					if (!ConsoleEmulator.ConstIndexInBounds(constdex)) continue;

					// Already did _levelManager.LoadLevel above, and since its
					// savestrings lists were empty, safe to spawn dynamics now.
					savID = Utils.GetIntFromString(entries[2],"SaveID");
					bool isNpc = ConsoleEmulator.ConstIndexIsNPC(constdex);
					bool isDynamicObject = ConsoleEmulator.ConstIndexIsDynamicObject(constdex);
					bool levelExists = levID == LevelManager.CurrentLevel;
					bool saveObjectToStaticStrings = !isDynamicObject && !levelExists && i < (readFileList.Count - 1);

					Task<GameObject> taskToWait = null;
					
					if (saveObjectToStaticStrings)
					{
						LevelManager.StaticObjectsSaveStrings[levID].Add(readFileList[i]);
					}
					else if (isNpc && levelExists) {
						contnr = _levelManager.GetRequestedLevelNPCContainer(levID);
						taskToWait = _consoleEmulator.SpawnDynamicObject(constdex, levID, false, contnr, savID).AsTask();
						yield return taskToWait;
						instGO = taskToWait.Result;
						PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(instGO,true);
						yield return SaveObject.Load(this,_levelManager,instGO, entries,i,prefID); // Load NPC.
					} else if (ConsoleEmulator.ConstIndexIsDynamicObject(constdex) && !isNpc) {
						// For DynamicObjects, if current level, go ahead and Instantiate new Prefabs, else add string to LevelManager's list for other levels.
						if (levID == LevelManager.CurrentLevel) {
							contnr = _levelManager.GetRequestedLevelDynamicContainer(levID);
							taskToWait = _consoleEmulator.SpawnDynamicObject(constdex, levID, false, contnr, savID).AsTask();
							yield return taskToWait;
							instGO = taskToWait.Result;
							PrefabIdentifier prefID = SaveLoad.GetPrefabIdentifier(instGO,true);
							yield return SaveObject.Load(this,_levelManager,instGO, entries,i,prefID); // Load NPC.
						} else {
							if (levID < LevelManager.DynamicObjectsSavestrings.Count && levID >= 0) { // levID < 14
								if (i < (readFileList.Count - 1) && readFileList.Count > 0 && i >= 0) {
									LevelManager.DynamicObjectsSavestrings[levID].Add(readFileList[i]);
								}
							}
						}
					}

				}

				percLoaded = ((float)i / (float)numSaveablesFromSavefile*100f);
				loadPercentText.text = "Loading Dynamic Objects: "
									   + percLoaded.ToString("0.0")
									   + "% (" + i.ToString() + " / "
									   + numSaveablesFromSavefile.ToString()
									   + ")";
				if (loadUpdateTimer.ElapsedMilliseconds > 50) {
					loadUpdateTimer.Reset();
					loadUpdateTimer.Start();
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					yield return null;
				}
			}
			
			// OK we read in all the dynamic objects above into the savestrings
			// list, now actaully instantiate them.
			yield return _levelManager.LoadLevelDynamicObjects(LevelManager.CurrentLevel);
			loadUpdateTimer.Stop();

			_levelManager.npcsm.RepopulateChildList();
			
			if (_inventory.hasHardware[1]) {
				// Go through all HealthManagers in the game and initialize the
				// linked overlays now for Automap.  Done after instantiation.
				List<GameObject> hmGOs = new List<GameObject>();
				
				// Find all HealthManager components.
				bool includeInactive = true;
				for (i=0;i<allParents.Count;i++) {
					Component[] compArray =
						allParents[i].GetComponentsInChildren(
							typeof(HealthManager),includeInactive);

					// Add all gameObject with a HealthManager components.
					for (k=0;k<compArray.Length;k++) hmGOs.Add(compArray[k].gameObject);
				}

				for (i=0;i<hmGOs.Count;i++) {
					if (hmGOs[i] == null) continue;

					HealthManager hm = hmGOs[i].GetComponent<HealthManager>();
					if (hm == null) continue;

					if ((hm.isNPC || hm.isSecCamera)) {
						hm.Awake(); // Set up slots.
						hm.Start(); // Setup overlay.
					}
				}
			}
		}
		
		loadPercentText.text = "Re-register targets...";
		yield return null;
		for (i=0;i<allParents.Count;i++) {
			Component[] compArray = allParents[i].GetComponentsInChildren(typeof(TargetIO),true); // find all SaveObject components, including inactive (hence the true here at the end)
			for (k=0;k<compArray.Length;k++) {
				TargetIO tio = compArray[k].gameObject.GetComponent<TargetIO>();
				if (tio != null) {
					tio.RemoteStart(this.gameObject,"LoadRoutine()"); // Reregister
				}
			}
		}
		
		allParents.Clear();
		allParents = null; // Done with it.
		ResetPauseLists();
		loadPercentText.text = "Re-init cull systems...";
		yield return null;
		_dynamicCulling.Cull_Init();
		_dynamicCulling.CullCore();
		loadPercentText.text = "Cleaning Up...";
		yield return null;

 		System.GC.Collect(); // Collect it all!
		System.GC.WaitForPendingFinalizers();
		AutoSplitterData.isLoading = false;
		loadTimer.Stop();
		loading = false;
		loadPercentText.text = "";
		yield return GoIntoGame(loadTimer);
		_lightDistanceCuller.Rebuild();
	}

	public void NPCAudioOcclusion() {
		// Raytraced Audio Occlusion ;)
		int hitCount = 0;
		float newVolume = 1.0f;
		RaycastHit[] results = new RaycastHit[6];
		AIController aic = null;
		for (int i=0;i<healthObjectsRegistration.Length;i++) {
			if (healthObjectsRegistration[i] == null) continue;

			aic = healthObjectsRegistration[i].GetComponent<AIController>();
			if (aic == null) continue;
			if (aic.SFX == null) continue;
			if (aic.index < sfxSightSoundForNPC.Length && aic.index >= 0) {
				if (sfxSightSoundForNPC[aic.index] < sounds.Length && sfxSightSoundForNPC[aic.index] >= 0) {
					if (aic.SFX.clip == sounds[sfxSightSoundForNPC[aic.index]]) {
						aic.SFX.volume = aic.normalVolume;
						continue;
					}
				}
			}

			hitCount = Physics.RaycastNonAlloc(
						_mouseLookScript.transform.position,
						aic.transform.position
						  - _mouseLookScript.transform.position,
						results,32f,layerMaskPlayerFrob,
						QueryTriggerInteraction.UseGlobal);

			aic.SFX.volume = aic.normalVolume;
			if (hitCount > 0) {
				if (hitCount > 5) {
					newVolume = aic.normalVolume * 0.65f;
				} else if (hitCount == 5) {
					newVolume = aic.normalVolume * 0.70f;
				} else if (hitCount == 4) {
					newVolume = aic.normalVolume * 0.75f;
				} else if (hitCount == 3) {
					newVolume = aic.normalVolume * 0.85f;
				} else if (hitCount == 2) {
					newVolume = aic.normalVolume * 0.90f;
				} else {
					newVolume = aic.normalVolume * 0.95f;
				}

				aic.SFX.volume = newVolume;
			}
		}
	}

	public void RegisterObjectWithHealth(HealthManager hm) {
		if (hm == null) return;

		for (int i=0;i<healthObjectsRegistration.Length;i++) {
			if (healthObjectsRegistration[i] != null) {
				if (healthObjectsRegistration[i] == hm) {
					return; // already in the list
				}
			}
		}

		int len = healthObjectsRegistration.Length;
		for (int i=0;i<len;i++) {
			if (healthObjectsRegistration[i] == null) {
				healthObjectsRegistration[i] = hm;
				return;
			}

			if (i == (len - 1)) {
				string msg = "WARNING: Could not register object with health. "
							 + " Hit limit of ";

				UnityEngine.Debug.Log(msg + len.ToString());
			}
		}
	}

	private int ReadLevelIndexFromSave(int saveFileIndex)
	{
		var saveData = ReadSave(saveFileIndex);
		return saveData.Count > 0 ? Utils.GetIntFromString(saveData[2].Split(Utils.splitCharChar)[0], "currentLevel") : LevelManager.NewGameLevelIndex;
	}

	private IReadOnlyList<string> ReadSave(int saveFileIndex)
	{
		string lName = "sav" + saveFileIndex.ToString() + ".txt";
		var pathToSave = Path.Combine(Path.Combine(Utils.GetAppropriateDataPath(), lName));
		return File.Exists(pathToSave) ? File.ReadAllLines(pathToSave) : Array.Empty<string>();
	}
	
	private void LockCPUScreenCode() {
		switch (LevelManager.CurrentLevel) {
			case 1:
				if (questData.lev1SecCodeLocked) return;

				questData.lev1SecCodeLocked = true;
				questData.lev1SecCode = UnityEngine.Random.Range(0,10);
			break;
			case 2:
				if (questData.lev2SecCodeLocked) return;

				questData.lev2SecCodeLocked = true;
				questData.lev2SecCode = UnityEngine.Random.Range(0,10);
				break;
			case 3:
				if (questData.lev3SecCodeLocked) return;

				questData.lev3SecCodeLocked = true;
				questData.lev3SecCode = UnityEngine.Random.Range(0,10);
				break;
			case 4:
				if (questData.lev4SecCodeLocked) return;

				questData.lev4SecCodeLocked = true;
				questData.lev4SecCode = UnityEngine.Random.Range(0,10);
				break;
			case 5:
				if (questData.lev5SecCodeLocked) return;

				questData.lev5SecCodeLocked = true;
				questData.lev5SecCode = UnityEngine.Random.Range(0,10);
				break;
			case 6:
				if (questData.lev6SecCodeLocked) return;

				questData.lev6SecCodeLocked = true;
				questData.lev6SecCode = UnityEngine.Random.Range(0,10);
				break;
		}
	}

	// Called by something's Use()
	public void UseTargets(GameObject go, UseData ud, string targetname) {
		if (loading) { UnityEngine.Debug.LogError("Attempted to UseTargets during loading!"); return; }
		
		if (go != null) {
			TargetIO tio = go.GetComponent<TargetIO>();
			if (tio != null) {
				ud.SetBits(tio);
			}
		}

		// First do things that don't actually need any other named object.
		if (ud.lockCodeToScreenMaterialChanger) LockCPUScreenCode();

		// Next check if targetname is valid.  This is fine if not, some
		// triggers we just want to play the trigger's SFX and do nothing else.
		if (string.IsNullOrWhiteSpace(targetname)) return;

		UseData tempUD = new UseData();
		float numtargetsfound = 0;
		// Find each gameobject with matching targetname in the register, then
		// call Use for each.
		bool succeeded = false;
		for (int i=0;i<_targetRegister.Count;i++) {
			if (_targetnameRegister.Count < 1) {
				UnityEngine.Debug.LogWarning("NO TARGETNAMES IN "
										   + "TargetnameRegister!!!");
				return;
			}

			if (_targetnameRegister[i] != targetname) continue;

			if (_targetRegister[i] != null) {
				numtargetsfound++;
				tempUD.CopyBitsFromUseData(ud);

				UnityEngine.Debug.Log("Running targets for " + targetname);

				// Added activeSelf bit to keep from spamming SetActive
				// when running targets through a trigger_multiple
				if (tempUD.GOSetActive && !_targetRegister[i].activeSelf) {
					//UnityEngine.Debug.Log("GOSetActive on " + targetname);
					_targetRegister[i].SetActive(true);
					succeeded = true;
				}

				// Diddo for activeSelf to prevent spamming SetActive.
				if (tempUD.GOSetDeactive && _targetRegister[i].activeSelf) {
					//UnityEngine.Debug.Log("GOSetDeactive on " + targetname);
					_targetRegister[i].SetActive(false);
					succeeded = true;
				}

				if (tempUD.GOToggleActive) {
					// If I abuse this with a trigger_multiple someone should
					// shoot me.
					_targetRegister[i].SetActive(!_targetRegister[i].activeSelf);
					succeeded = true;
				}

				TargetIO tio = _targetRegister[i].GetComponent<TargetIO>();
				tio.Targetted(tempUD);
				succeeded = true;
			}
		}

		if (!succeeded) {
			UnityEngine.Debug.LogWarning("Failed to find a matching targetname"
										 + " for " + targetname);
		}
	}

	// Should ONLY come from a TargetIO
	public void AddToTargetRegister(TargetIO tio, GameObject go) {
		string tn = tio.targetname;
	    for (int i=0;i<_targetRegister.Count; i++) {
	        if (_targetRegister[i] == null) continue;
	        if (_targetRegister[i] != go) continue; // Key check for whole loop.
			
	        // GameObject go is in registry already
            if (_targetnameRegister[i] == tn) {
                return; // Already in register, name and object.
            } else {
                _targetnameRegister[i] = tn; // Fix up partial registry.
                return; // Ok it's good now.
            }
	    }
	    
	    // GameObject isn't in registry, add fresh.
	    _targetRegister.Add(go);
		_targetnameRegister.Add(tn);
		lastTargetRegistrySize = _targetnameRegister.Count;
	}

	public void AddToTextLocalizationRegister(TextLocalization txtloc) {
		if (txtloc == null) return;

		TextLocalizationRegister.Add(txtloc);
	}

	public void ReverbOn() {
		for (int i=0;i<ReverbRegister.Length;i++) {
			if (ReverbRegister[i] != null) {
				AudioReverbZone arz = ReverbRegister[i].GetComponent<AudioReverbZone>();
				if (arz != null) arz.enabled = true;
			}
		}
	}

	public void ReverbOff() {
		for (int i=0;i<ReverbRegister.Length;i++) {
			if (ReverbRegister[i] != null) {
				AudioReverbZone arz = ReverbRegister[i].GetComponent<AudioReverbZone>();
				if (arz != null) arz.enabled = false;
			}
		}
	}

	public void AddToReverbRegister (GameObject go) {
		for (int i=0;i<ReverbRegister.Length;i++) {
			if (ReverbRegister[i] == null) {
				ReverbRegister[i] = go;
				return; // Ok, gameobject added to the register.
			}
		}
	}

	public void Shake(bool effectIsWorldwide, float distance, float force) {
		if (distance == -1) distance = globalShakeDistance;
		if (force == -1) force = globalShakeForce;

		if (effectIsWorldwide) {
			// The whole station is a shakin' and a movin'!
			_mouseLookScript.ScreenShake(force,1f);
		} else {
			// check if player is close enough and shake em' up!
			if (Vector3.Distance(transform.position,player1Capsule.transform.position) < distance) {
				_mouseLookScript.ScreenShake(force,1f);
			}
		}
	}
	
	public float DifficultyIndex() {
	    float stupid = 0f;
	    stupid += (difficultyCombat * difficultyCombat);
        stupid += (difficultyPuzzle * difficultyPuzzle);
        stupid += (difficultyMission * difficultyMission);
        stupid += (difficultyCyber * difficultyCyber);
        return stupid;
	}
	
	public float GetScore(bool isFinal) {
        float stupid = DifficultyIndex();
        float score = 0f;
        float victories = (float)(kills + cyberkills);
        float secs = 0f;
        
        secs = Mathf.Floor(_pauseScript.relativeTime / 3600f);
        if (!isFinal) { // Report score if no deaths.
            score = victories * 10000f;
            score -= Mathf.Min(score * 0.666f,secs * 100f);
            score *= ((stupid + 1f) / 37f);
            if (stupid > 35f) score += 2222222f; // secret kevin bonus
            return Mathf.Floor(score);
        }
        
        // Death is 10 anti-kills, but you always keep at least a third of your
        // kills.
        float deathPenalty = _playerHealth.ressurections * 10f;
        score = victories - Mathf.Min(deathPenalty,victories * 0.666f);
        score *= 10000f;
        score -= Mathf.Min(score * 0.666f,secs * 100f);
        score *= ((stupid + 1f) / 37f); // 9 * 4 + 1 is best difficulty factor
        if (stupid > 35f) score += 2222222f; // secret kevin bonus
        return Mathf.Floor(score);
	}

	public string CreditsStats() {
	    StringBuilder s1 = new StringBuilder();
	    s1.Clear();
	    s1.Append("======================================================================");
        s1.Append("\n");
	    s1.Append("CITADEL");
	    s1.Append("\n");
	    s1.Append("======================================================================");
	    s1.Append("\n");
	    s1.Append("CONGRATULATIONS " + playerName);
	    s1.Append("\n");
	    
	    string hours, minutes, secs;
	    float t = _pauseScript.relativeTime;
		float tb = (Mathf.Floor(t/3600f));
        hours = tb.ToString("0");
        t = t - (tb * 3600f);
        tb = Mathf.Floor(t / 60f); 
        minutes = tb.ToString("00");
        t = t - (tb * 60f);
        secs = t.ToString("00.000");
	    s1.Append("Straight Time: " + hours + "h " + minutes + "m " + secs
	              + "s");
	              
	    s1.Append("\n");
	    
	    t = _pauseScript.absoluteTime;
		tb = (Mathf.Floor(t/3600f));
        hours = tb.ToString("0");
        t = t - (tb * 3600f);
        tb = Mathf.Floor(t / 60f); 
        minutes = tb.ToString("00");
        t = t - (tb * 60f);
        secs = t.ToString("00.000"); 
        s1.Append("Total Time (with reload from deaths): ");
	    s1.Append("\n");
	    
	    s1.Append("Kills: " + kills.ToString());
	    s1.Append("\n");
	    s1.Append("Kills in Cyberspace: " + cyberkills.ToString());
	    s1.Append("\n");
	    
	    s1.Append("Score Subtotal: " + GetScore(false).ToString("0"));
	    s1.Append("\n");
	    s1.Append("Deaths: " + _playerHealth.deaths.ToString());
	    s1.Append("\n");
	    s1.Append("Ressurections: " + _playerHealth.ressurections.ToString());
	    s1.Append("\n");
	    s1.Append("Combat: " + difficultyCombat.ToString()
	              + " | Puzzle: " + difficultyPuzzle.ToString()
	              + " | Mission: " + difficultyMission.ToString()
	              + " | Cyber: " + difficultyCyber.ToString());
	    s1.Append("\n");
	    s1.Append("Difficulty Index: " + DifficultyIndex().ToString());
	    s1.Append("\n");
	    s1.Append("Final Score: " + GetScore(true));
	    s1.Append("\n");
	    s1.Append("\n");
	    s1.Append("Shots Fired: " + shotsFired.ToString());
	    s1.Append("\n");
	    s1.Append("Grenades Thrown: " + grenadesThrown.ToString());
	    s1.Append("\n");
	    s1.Append("Damage Dealt: " + damageDealt.ToString());
	    s1.Append("\n");
	    s1.Append("Damage Received: " + damageReceived.ToString());
	    s1.Append("\n");
	    s1.Append("Saves Scummed: " + savesScummed.ToString());
	    s1.Append("\n");
	    s1.Append("\n");
	    s1.Append("Click to continue...");
		return s1.ToString();
	}
	
	void OnDestroy() {
		ScenesLoader.OnSceneLoaded -= OnSceneLoaded;
		ScenesLoader.OnStartLoadScene -= OnStartLoadScene;
		questData = null;
		audiologNames = null;
		audiologSenders = null;
		audiologSubjects = null;
		audioLogs = null;
		audioLogType = null;
		audioLogSpeech2Text = null;
		audioLogLevelFound = null;
		sounds = null;
		attackTypeForWeapon = null;
		nameForNPC = null;
		attackTypeForNPC = null;
		attackTypeForNPC2 = null;
		attackTypeForNPC3 = null;
		perceptionForNPC = null;
		moveTypeForNPC = null;
		typeForNPC = null;
		creditsText = null;
		healthObjectsRegistration = null;
		loadingScreen = null;
		mainMenuInit = null;
		statusBar = null;
		InputCodeSettings = null;
		InputCodes = null;
		InputValues = null;
		InputConfigNames = null;
		mainFont1 = null;
		mainFont2 = null;
		_targetRegister.Clear();
		_targetnameRegister.Clear();
		stringTable = null;
		reloadTime = null;
		eventSystem = null;
		loadPercentText = null;
		genericMaterials = null;
		ReverbRegister = null;
		npcCount = null;
		audioLogImagesRefIndicesLH = null;
		audioLogImagesRefIndicesRH = null;
		player1TargettingPos = null;
		player1Capsule = null;
		player1PlayerMovementScript = null;
		player1PlayerHealthScript = null;
		player1CapsuleMainCameragGO = null;
		s1 = null;
		s2 = null;
	}
}
