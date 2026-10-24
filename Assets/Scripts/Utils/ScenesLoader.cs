using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Citadel.SceneManagement
{
    /// <summary>
    /// Instance-scoped scene loading service. Subscribe to lifecycle events on the injected instance.
    /// </summary>
    internal sealed class ScenesLoader : IDisposable
    {
        public int MainGameSceneIndex => 1;

        public event Action<string> OnStartLoadScene;
        public event Action<string> OnSceneLoaded;
        public event Action<string> OnActiveSceneChanged;

        public string LoadedSceneName => SceneManager.GetActiveScene().name;

        public static ScenesLoader Default { get; } = new();
        
        private readonly string[] _levelScenesNames =
        {
            "0-ReactorLevelScene",
            "1-MedicalLevelScene",
            "2-ScienceLevelScene",
            "3-MaintenanceLevelScene",
            "4-StorageLevelScene",
            "5-FlightDeckLevelScene",
            "6-ExecutiveLevelScene",
            "7-EngineeringLevelScene",
            "8-SecurityLevelScene",
            "9-BridgeLevelScene",
            "10-AlphaGroveLevelScene",
            "11-BetaGroveLevelScene",
            "12-DeltaGroveLevelScene",
            "13-CyberspaceLevelScene"
        };

        public ScenesLoader()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        public async UniTask LoadLevelAsync(int level)
        {
            if (level < 0 || level >= _levelScenesNames.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown Citadel level index.");
            }

            var sceneName = _levelScenesNames[level];
            OnStartLoadScene?.Invoke(sceneName);
            await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _) =>
            OnSceneLoaded?.Invoke(scene.name);

        private void HandleActiveSceneChanged(Scene _, Scene scene) =>
            OnActiveSceneChanged?.Invoke(scene.name);
    }
}
