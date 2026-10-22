using System;
using System.Collections.Generic;
using System.IO;
using Citadel.SceneManagement;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Citadel.Game
{
    internal sealed class Startup : MonoBehaviour
    {
        private async void Start()
        {
#if UNITY_ANDROID
            try
            {
                if (!await CopyAddressableTextAssetsAsync())
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return;
            }
#endif
            ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex);
        }

#if UNITY_ANDROID
        private static async System.Threading.Tasks.Task<bool> CopyAddressableTextAssetsAsync()
        {
            var initializationHandle = Addressables.InitializeAsync();
            await initializationHandle.Task;

            if (initializationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to initialize Addressables. The game scene will not be loaded.");
                return false;
            }

            var textLocations = new Dictionary<string, UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>(
                StringComparer.OrdinalIgnoreCase);

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
#endif
    }
}
