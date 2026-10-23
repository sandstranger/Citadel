using System.Collections.Generic;
using Citadel.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Zenject;

namespace Citadel.Game
{
    internal sealed class RootInstaller : MonoInstaller
    {
        public const string MainAudioSourceName = "main_menu_music";

        private static RootInstaller _instance;
        private readonly List<object> _itemsToInject = new();
        
        [SerializeField] 
        private TexturesStorage texturesStorage;
        [SerializeReference] private AudioSource _mainmenuMusic;
        [SerializeField] private PlayerReferenceManager _playerReference;
        [SerializeField] private BiomonitorGraphSystem _biomonitorGraphSystem;
        [SerializeField] private PlayerEnergy _playerEnergy;
        [SerializeField] private Const _const;
        [SerializeField] private MFDManager _mfdManager;
        [SerializeField] private Automap _automap;
        [SerializeField] private GUIState _guiState;
        [SerializeField] private PlayerPatch _playerPatch;
        [SerializeField] private GetInput _getInput;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private MainMenuHandler _mainMenuHandler;
        [SerializeField] private MissionTimer _missionTimer;
        [SerializeField] private MouseCursor _mouseCursor;
        [SerializeField] private MinigameCursor _minigameCursor;
        [SerializeField] private MouseLookScript _mouseLookScript;
        [SerializeField] private Music _music;
        [SerializeField] private PauseScript _pauseScript;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private WeaponFire _weaponFire;
        [SerializeField] private WeaponCurrent _weaponCurrent;
        [SerializeField] private QuestLogNotesManager _questLogNotesManager;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private PostProcessProfile _postProcessProfile;
        [SerializeField] private PostProcessLayer[] _postProcessLayers;

        private readonly List<Config.PostProcessLayerStorage> _postProcessLayerStorages = new();

        private void Awake()
        {
            // Keep exactly one persistent root installer. A newly loaded gameplay
            // scene must replace the menu installer when a new game starts.
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Const.StartingNewGame)
            {
                // The previous scene's Const and PlayerReferenceManager were kept
                // alive separately from the root installer. Replace those copies
                // with the instances serialized in the newly loaded gameplay scene.
                var previousInstaller = _instance;
                var previousConst = previousInstaller._const;
                var previousPlayerReference = previousInstaller._playerReference;

                if (previousConst != null &&
                    previousConst.gameObject != _const.gameObject &&
                    previousConst.gameObject != previousInstaller.gameObject)
                    DestroyImmediate(previousConst.gameObject);
                if (previousPlayerReference != null &&
                    previousPlayerReference.gameObject != _playerReference.gameObject &&
                    previousPlayerReference.gameObject != previousInstaller.gameObject)
                    DestroyImmediate(previousPlayerReference.gameObject);

                DestroyImmediate(previousInstaller.gameObject);
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Other gameplay scenes may contain serialized copies of these
                // persistent managers. Destroy the copies together with their
                // duplicate installer, keeping the active persistent instances.
                var duplicateConst = _const;
                var duplicatePlayerReference = _playerReference;
                var existingConst = _instance._const;
                var existingPlayerReference = _instance._playerReference;

                if (duplicateConst != null &&
                    duplicateConst.gameObject != existingConst.gameObject &&
                    duplicateConst.gameObject != gameObject)
                    DestroyImmediate(duplicateConst.gameObject);
                if (duplicatePlayerReference != null &&
                    duplicatePlayerReference.gameObject != existingPlayerReference.gameObject &&
                    duplicatePlayerReference.gameObject != gameObject)
                    DestroyImmediate(duplicatePlayerReference.gameObject);

                DestroyImmediate(gameObject);
                return;
            }

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            DontDestroyOnLoad(_const.gameObject);
            DontDestroyOnLoad(_playerReference.gameObject);
            _playerPatch.Initialize();

            foreach (var postProcessLayer in _postProcessLayers)
            {
                _postProcessLayerStorages.Add(new Config.PostProcessLayerStorage(postProcessLayer));
            }
        }
        
        private new void Start() => OnSceneChanged(SceneManager.GetActiveScene().name);

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        public override void InstallBindings()
        {
            Container.Bind<AudioSource>().WithId(MainAudioSourceName).FromInstance(_mainmenuMusic).AsSingle();
            Container.Bind<ITexturesStorage>().FromInstance(texturesStorage).AsSingle();
            Container.Bind<IResourcesLoader>().To<AddressablesResourcesLoader>().AsSingle();
            Container.Bind<Utils>().AsSingle();
            Container.Bind<AndroidConfig>().AsSingle();
            Container.Bind<ScenesLoader>().AsSingle();
            Container.Bind<StringBuilderPool>().AsSingle();
            Container.Bind<SaveLoad>().AsSingle();
            Container.Bind<ObjectContainmentSystem>().AsSingle();
            Container.Bind<AutoSplitterData>().AsSingle();
            Container.Bind<IReadOnlyCollection<Config.PostProcessLayerStorage>>()
                .FromInstance(_postProcessLayerStorages).AsSingle();
            Container.Bind<Config>().AsSingle();
            Container.Bind<ConsoleEmulator>().AsSingle();
            Container.Bind<PostProcessProfile>().FromInstance(_postProcessProfile).AsSingle();
            Container.BindInstance(_mainCamera).AsSingle();
            Container.BindInstance(_playerReference).AsSingle();
            Container.BindInstance(_biomonitorGraphSystem).AsSingle();
            Container.BindInstance(_playerEnergy).AsSingle();
            FindSceneComponents(out var dynamicCulling, out var levelManager,
                out var levelEditor, out var lightsCuller);
            Container.Bind<DynamicCulling>().FromInstance(dynamicCulling).AsTransient();
            Container.Bind<LevelManager>().FromInstance(levelManager).AsTransient();
            Container.Bind<LevelEditor>().FromInstance(levelEditor).AsTransient();
            Container.Bind<LightDistanceCuller>().FromInstance(lightsCuller).AsTransient();
            Container.BindInstance(_const).AsSingle();
            Container.BindInstance(_mfdManager).AsSingle();
            Container.BindInstance(_automap).AsSingle();
            Container.BindInstance(_guiState).AsSingle();
            Container.BindInstance(_playerPatch).AsSingle();
            Container.BindInstance(_getInput).AsSingle();
            Container.BindInstance(_inventory).AsSingle();
            Container.BindInstance(_mainMenuHandler).AsSingle();
            Container.BindInstance(_missionTimer).AsSingle();
            Container.BindInstance(_mouseCursor).AsSingle();
            Container.BindInstance(_minigameCursor).AsSingle();
            Container.BindInstance(_mouseLookScript).AsSingle();
            Container.BindInstance(_music).AsSingle();
            Container.BindInstance(_pauseScript).AsSingle();
            Container.BindInstance(_playerHealth).AsSingle();
            Container.BindInstance(_playerMovement).AsSingle();
            Container.BindInstance(_weaponFire).AsSingle();
            Container.BindInstance(_weaponCurrent).AsSingle();
            Container.BindInstance(_questLogNotesManager).AsSingle();

            // These service objects can live in the persistent scene. Reinject them
            // after scene-specific bindings (LevelManager, cullers, etc.) are rebound.
            _itemsToInject.AddRange(new object[]
            {
                _playerReference,
                _biomonitorGraphSystem,
                _playerEnergy,
                _const,
                _mfdManager,
                _automap,
                _guiState,
                _playerPatch,
                _getInput,
                _inventory,
                _mainMenuHandler,
                _missionTimer,
                _mouseCursor,
                _minigameCursor,
                _mouseLookScript,
                _music,
                _pauseScript,
                _playerHealth,
                _playerMovement,
                _weaponFire,
                _weaponCurrent,
                _questLogNotesManager
            });
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene activeScene) =>
            OnSceneChanged(activeScene.name);

        private void OnSceneChanged(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = SceneManager.GetActiveScene();
            }

            FindSceneComponents( out var dynamicCulling, out var levelManager,
                out var levelEditor, out var lightsCuller);

            if (lightsCuller != null)
                Container.Rebind<LightDistanceCuller>().FromInstance(lightsCuller).AsTransient();
            if (dynamicCulling != null)
                Container.Rebind<DynamicCulling>().FromInstance(dynamicCulling).AsTransient();
            if (levelManager != null)
                Container.Rebind<LevelManager>().FromInstance(levelManager).AsTransient();
            if (levelEditor != null)
                Container.Rebind<LevelEditor>().FromInstance(levelEditor).AsTransient();

            foreach (var root in scene.GetRootGameObjects())
            {
                Container.InjectGameObject(root);
            }

            foreach (var item in _itemsToInject)
            {
                if (item != null)
                {
                    Container.Inject(item);
                }
            }
        }

        private static void FindSceneComponents(
            out DynamicCulling dynamicCulling,
            out LevelManager levelManager,
            out LevelEditor levelEditor,
            out LightDistanceCuller lightsCuller)
        {
            var findObjectsRule = FindObjectsInactive.Include;
            dynamicCulling = FindFirstObjectByType<DynamicCulling>(findObjectsRule);
            levelManager = FindFirstObjectByType<LevelManager>(findObjectsRule);
            levelEditor = FindFirstObjectByType<LevelEditor>(findObjectsRule);
            lightsCuller = FindFirstObjectByType<LightDistanceCuller>(findObjectsRule);
        }

    }
}
