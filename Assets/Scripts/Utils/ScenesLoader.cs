using System;
using Citadel.Extensions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Citadel.SceneManagement
{
    internal static class ScenesLoader
    {
        public const string DynamicLevelsSceneName = "CitadelScene";
        
        public static event Action<string> OnStartLoadScene;
        public static event Action<string> OnSceneLoaded;

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
        }
        
        public static void LoadScene(string sceneName)
        {
            OnStartLoadScene?.Invoke(sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadLevel(int level)
        {
            if (level < _levelScenesNames.Length)
            {
                LoadScene(_levelScenesNames[level]);
            }
        }
    }
}
