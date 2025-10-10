using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Citadel.Game
{
    internal interface IResourcesLoader : IDisposable
    {
        UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken = default) where T : UnityEngine.Object;

        T LoadAsset<T>(string assetName) where T : UnityEngine.Object;
        
        UniTask<GameObject> InstantiateAsync (string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform = null, CancellationToken cancellationToken = default);
        
        GameObject Instantiate(string assetName, Vector3 position, Quaternion rotation, Transform parentTransform = null);
        
        void ReleaseAsset(string assetName);

        void ReleaseAllAssets();
    }

    internal static class IResourcesLoaderExtensions
    {
        public static async UniTaskVoid LoadAssetAsync<T>(this IResourcesLoader resourcesLoader, string assetName,
            Action<T> onAssetLoaded) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }

            Assert.IsNotNull(onAssetLoaded);
            
            T loadedAsset = await resourcesLoader.LoadAssetAsync<T>(assetName);
            onAssetLoaded(loadedAsset);
        } 

        public static async UniTaskVoid InstantiateAsync(this IResourcesLoader resourcesLoader, string assetName, 
            Vector3 position, Quaternion rotation, Transform parentTransform, Action<GameObject> onPrefabInstantiated)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }

            Assert.IsNotNull(onPrefabInstantiated);
            
            GameObject instantiatedPrefab = await resourcesLoader.InstantiateAsync(assetName,position,rotation,parentTransform);
            onPrefabInstantiated(instantiatedPrefab);
        }
        
        public static void InstantiateAsync(this IResourcesLoader resourcesLoader, string assetName,
            Vector3 position, Quaternion rotation, Action<GameObject> onPrefabInstantiated)
        {
            resourcesLoader.InstantiateAsync(assetName, position, rotation,null, onPrefabInstantiated);
        }
    }
}