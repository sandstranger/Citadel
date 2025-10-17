using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Citadel.SceneManagement
{
    internal static class ScenesLoader
    {
        public const int MainGameSceneIndex = 1;
        public static event Action<string> OnStartLoadScene;
        public static event Action<string> OnSceneLoaded;
        public static event Action<string> OnActiveSceneChanged; 

        public static string LoadedSceneName => SceneManager.GetActiveScene().name;
        
        private static readonly string[] _levelScenesNames = {
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

        static ScenesLoader()
        {
            SceneManager.sceneLoaded += (scene, _) => OnSceneLoaded?.Invoke(scene.name);
            SceneManager.activeSceneChanged += (_,scene ) => OnActiveSceneChanged?.Invoke(scene.name);
        }

        public static async UniTaskVoid LoadLevelAsync(int level)
        {
            if (level < _levelScenesNames.Length)
            {
                var sceneName = _levelScenesNames[level];
                OnStartLoadScene?.Invoke(sceneName);
                await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
        }
    }
}
