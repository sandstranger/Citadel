using System.Collections.Generic;
using Citadel.SceneManagement;
using UnityEngine;
using Zenject;

namespace Citadel.Game
{
    internal sealed class RootInstaller : MonoInstaller
    {
        [SerializeField] 
        private PlayerReferenceManager _playerReference;
        [SerializeField] 
        private BiomonitorGraphSystem _biomonitorGraphSystem;
        [SerializeField] 
        private PlayerEnergy _playerEnergy;
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

        private readonly Config _config = new();
        private readonly ConsoleEmulator _consoleEmulator = new();
        private readonly List<object> _itemsToInject = new();

        private static RootInstaller _instance;
        
        public override void InstallBindings()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (Const.StartingNewGame)
            {
                DestroyImmediate(_instance.gameObject);
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            ScenesLoader.OnActiveSceneChanged += OnSceneChanged;
            
            _const.InitializeInstance();
            _playerReference.InitializeInstance();
            _playerPatch.Initialize();
            _mouseLookScript.Initialize();

            Container.Bind<Config>().FromInstance(_config).AsSingle();
            Container.BindInstance(_playerReference).AsSingle();
            Container.BindInstance(_biomonitorGraphSystem).AsSingle();
            Container.BindInstance(_playerEnergy).AsSingle();
            Container.Bind<DynamicCulling>().FromInstance(FindFirstObjectByType<DynamicCulling>()).AsTransient();
            Container.Bind<LevelManager>().FromInstance(FindFirstObjectByType<LevelManager>()).AsTransient();
            Container.Bind<LevelEditor>().FromInstance(FindFirstObjectByType<LevelEditor>()).AsTransient();
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
            Container.BindInstance(_consoleEmulator).AsSingle();
            Container.BindInstance(_questLogNotesManager).AsSingle();
            
            Container.Inject(_config);
            Container.Inject(_consoleEmulator);
            
            _itemsToInject.Add(_config);
            _itemsToInject.Add(_playerReference);
            _itemsToInject.Add(_biomonitorGraphSystem);
            _itemsToInject.Add(_playerEnergy);
            _itemsToInject.Add(_const);
            _itemsToInject.Add(_mfdManager);
            _itemsToInject.Add(_automap);
            _itemsToInject.Add(_guiState);
            _itemsToInject.Add(_playerPatch);
            _itemsToInject.Add(_getInput);
            _itemsToInject.Add(_inventory);
            _itemsToInject.Add(_mainMenuHandler);
            _itemsToInject.Add(_missionTimer);
            _itemsToInject.Add(_mouseCursor);
            _itemsToInject.Add(_minigameCursor);
            _itemsToInject.Add(_mouseLookScript);
            _itemsToInject.Add(_music);
            _itemsToInject.Add(_pauseScript);
            _itemsToInject.Add(_playerHealth);
            _itemsToInject.Add(_playerMovement);
            _itemsToInject.Add(_weaponFire);
            _itemsToInject.Add(_weaponCurrent);
            _itemsToInject.Add(_consoleEmulator);
            _itemsToInject.Add(_questLogNotesManager);
        }

        private void OnDestroy()
        {
            ScenesLoader.OnActiveSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(string sceneName)
        {
            if (!LevelManager.UseDynamicLevelsLoading)
            {
                var dynamicCulling = FindFirstObjectByType<DynamicCulling>();
                var levelManager = FindFirstObjectByType<LevelManager>();
                var levelEditor = FindFirstObjectByType<LevelEditor>();
                
                Container.Rebind<DynamicCulling>().FromInstance(dynamicCulling).AsTransient();
                Container.Rebind<LevelManager>().FromInstance(levelManager).AsTransient();
                Container.Rebind<LevelEditor>().FromInstance(levelEditor).AsTransient();
                
                Container.Inject(levelEditor);
                Container.Inject(levelManager);
                Container.Inject(dynamicCulling);
                
                foreach (var itemToInject in _itemsToInject)
                {
                    Container.Inject(itemToInject);
                }
            }
        }
        
        public static GameObject InstantiatePrefab(GameObject original, Vector3 position, Quaternion rotation, Transform parentTransform = null)
        {
            return _instance.Container.InstantiatePrefab(original, position, rotation, parentTransform);
        }
    }
}
