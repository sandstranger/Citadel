using System;
using System.Collections.Generic;
using Citadel.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Citadel.SceneManagement
{
    internal static class ScenesLoader
    {
        public static event Action <Scene> OnStartLoadScene;
        public static event Action<Scene> OnSceneLoaded;

        public const string DynamicLevelsSceneName = "CitadelScene";
        
        private static readonly List<string> _levelScenesNames = new()
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

        public static async void LoadScene(string sceneName)
        {
            var sceneToLoad = SceneManager.GetSceneByName(sceneName);
            OnStartLoadScene?.Invoke(sceneToLoad);
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToTask();
            OnSceneLoaded?.Invoke(sceneToLoad);
        }

        public static void LoadLevel(int level)
        {
            if (level < _levelScenesNames.Count)
            {
                LoadScene(_levelScenesNames[level]);
            }
        }
    }
}
