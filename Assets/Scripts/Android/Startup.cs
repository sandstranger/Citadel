using System;
using System.Collections.Generic;
using System.IO;
using Citadel.SceneManagement;
using UnityEngine;
#if UNITY_ANDROID
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

namespace Citadel.Game
{
    internal sealed class Startup : MonoBehaviour
    {
#if UNITY_ANDROID
        private async void Start()
        {
            try
            {
                if (await CopyAddressableTextAssetsAsync())
                {
                    await ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static async UniTask<bool> CopyAddressableTextAssetsAsync()
        {
            var initializationHandle = Addressables.InitializeAsync();
            bool initialized;
            try
            {
                await initializationHandle.Task.AsUniTask();
                initialized = initializationHandle.Status == AsyncOperationStatus.Succeeded;
            }
            finally
            {
                if (initializationHandle.IsValid())
                {
                    Addressables.Release(initializationHandle);
                }
            }

            if (!initialized)
            {
                Debug.LogError("Failed to initialize Addressables. The game scene will not be loaded.");
                return false;
            }

            var textLocations = new Dictionary<string, IResourceLocation>(StringComparer.OrdinalIgnoreCase);

            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                    if (!locator.Locate(key, typeof(TextAsset), out var locations))
                    {
                        continue;
                    }

                    foreach (var location in locations)
                    {
                        if (location == null)
                        {
                            continue;
                        }

                        var fileName = Path.GetFileName(location.PrimaryKey);
                        if (string.IsNullOrWhiteSpace(fileName))
                        {
                            continue;
                        }

                        var extension = Path.GetExtension(fileName);
                        if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) &&
                            !extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        textLocations.TryAdd(fileName, location);
                    }
                }
            }

            if (textLocations.Count == 0)
            {
                Debug.LogError("No addressable .txt or .csv TextAssets were found. The game scene will not be loaded.");
                return false;
            }

            var dataPath = Application.persistentDataPath;
            Directory.CreateDirectory(dataPath);

            var copiedFiles = 0;
            foreach (var entry in textLocations)
            {
                var targetPath = Path.Combine(dataPath, entry.Key);

                // Preserve save files, config, and previously copied data.
                if (File.Exists(targetPath))
                {
                    continue;
                }

                var assetHandle = Addressables.LoadAssetAsync<TextAsset>(entry.Value);
                try
                {
                    await assetHandle.Task;

                    if (assetHandle.Status != AsyncOperationStatus.Succeeded || assetHandle.Result == null)
                    {
                        Debug.LogError($"Failed to load addressable text asset '{entry.Key}'. The game scene will not be loaded.");
                        return false;
                    }

                    await File.WriteAllTextAsync(targetPath, assetHandle.Result.text);
                    copiedFiles++;
                }
                finally
                {
                    if (assetHandle.IsValid())
                    {
                        Addressables.Release(assetHandle);
                    }
                }
            }

            Debug.Log($"Addressable text assets ready. Found {textLocations.Count}, copied {copiedFiles}, path: {dataPath}");
            return true;
        }
#else
        private void Start() => ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex);
#endif
    }
}
