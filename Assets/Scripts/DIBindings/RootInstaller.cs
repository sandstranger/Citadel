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
        
        private ScenesLoader _scenesLoader;

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
            DontDestroyOnLoad(gameObject);

            _const.InitializeInstance();
            _playerReference.InitializeInstance();
            _playerPatch.Initialize();

            foreach (var postProcessLayer in _postProcessLayers)
            {
                _postProcessLayerStorages.Add(new Config.PostProcessLayerStorage(postProcessLayer));
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
            Container.Bind<IReadOnlyCollection<Config.PostProcessLayerStorage>>()
                .FromInstance(_postProcessLayerStorages).AsSingle();
            Container.Bind<Config>().AsSingle();
            Container.Bind<ConsoleEmulator>().AsSingle();
            Container.Bind<PostProcessProfile>().FromInstance(_postProcessProfile).AsSingle();
            Container.BindInstance(_mainCamera).AsSingle();
            Container.BindInstance(_playerReference).AsSingle();
            Container.BindInstance(_biomonitorGraphSystem).AsSingle();
            Container.BindInstance(_playerEnergy).AsSingle();
            var activeScene = SceneManager.GetActiveScene();
            Container.Bind<DynamicCulling>().FromInstance(FindComponentInScene<DynamicCulling>(activeScene)).AsTransient();
            Container.Bind<LevelManager>().FromInstance(FindComponentInScene<LevelManager>(activeScene)).AsTransient();
            Container.Bind<LevelEditor>().FromInstance(FindComponentInScene<LevelEditor>(activeScene)).AsTransient();
            Container.Bind<LightDistanceCuller>().FromInstance(FindComponentInScene<LightDistanceCuller>(activeScene))
                .AsTransient();
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

            
            _scenesLoader = Container.Resolve<ScenesLoader>();
            _scenesLoader.OnActiveSceneChanged += OnSceneChanged;
        }

        private void Start()
        {
            OnSceneChanged(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (_scenesLoader != null)
            {
                _scenesLoader.OnActiveSceneChanged -= OnSceneChanged;
            }
        }

        private void OnSceneChanged(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = SceneManager.GetActiveScene();
            }

            var dynamicCulling = FindComponentInScene<DynamicCulling>(scene);
            var levelManager = FindComponentInScene<LevelManager>(scene);
            var levelEditor = FindComponentInScene<LevelEditor>(scene);
            var lightsCuller = FindComponentInScene<LightDistanceCuller>(scene);

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
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                var components = root.GetComponentsInChildren<T>(true);
                if (components.Length > 0)
                {
                    return components[0];
                }
            }

            return null;
        }
    }
}
