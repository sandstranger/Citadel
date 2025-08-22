using System;
using System.Collections.Generic;
using Citadel.Extensions;
using UnityEngine.SceneManagement;

namespace Citadel.SceneManagement
{
    internal static class ScenesLoader
    {
        public static event Action <Scene> OnStartLoadScene;
        public static event Action<Scene> OnSceneLoaded;

        private static readonly Lazy<List<string>> _levelScenesNames = new(() =>
        {
            var scenesNames = new List<string>();
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
            {
                var scene = SceneManager.GetSceneByBuildIndex(i);
                if (scene.name.Contains("LevelScene"))
                {
                    scenesNames.Add(scene.name);
                }
            }
            return scenesNames;
        });

        public static async void LoadScene(string sceneName)
        {
            var sceneToLoad = SceneManager.GetSceneByName(sceneName);
            OnStartLoadScene?.Invoke(sceneToLoad);
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToTask();
            OnSceneLoaded?.Invoke(sceneToLoad);
        }

        public static void LoadLevel(int level)
        {
            if (level < _levelScenesNames.Value.Count)
            {
                LoadScene(_levelScenesNames.Value[level]);
            }
        }
    }
}
