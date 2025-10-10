using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Citadel.Game
{
    internal interface IResourcesLoader : IDisposable
    {
        Task<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) where T : UnityEngine.Object;

        Task<T> LoadAssetAsync<T>(string assetName) where T : UnityEngine.Object;

        Task<GameObject> InstantiateAsync (string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform, CancellationToken cancellationToken);
        
        Task<GameObject> InstantiateAsync (string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform );
        
        void ReleaseAsset(string assetName);

        void ReleaseAllAssets();
    }

    internal static class IResourcesLoaderExtensions
    {
        public static Task<GameObject> InstantiateAsync (this IResourcesLoader resourcesLoader,string assetName, 
            Vector3 position, Quaternion rotation, CancellationToken cancellationToken)
        {
            return resourcesLoader.InstantiateAsync(assetName, position, rotation, null, cancellationToken);
        }

        public static Task<GameObject> InstantiateAsync (this IResourcesLoader resourcesLoader,string assetName, Vector3 position,
            Quaternion rotation)
        {
            return resourcesLoader.InstantiateAsync(assetName, position, rotation, null);
        }

        public static async void LoadAssetAsync<T>(this IResourcesLoader resourcesLoader, string assetName,
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

        public static async void InstantiateAsync(this IResourcesLoader resourcesLoader, string assetName, 
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

        public static T LoadAsset<T>(this IResourcesLoader resourcesLoader, string assetName) where T : UnityEngine.Object
        {
            var loadAssetTask = resourcesLoader.LoadAssetAsync<T>(assetName);
            return loadAssetTask.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static GameObject Instantiate(this IResourcesLoader resourcesLoader, string assetName, Vector3 position,
            Quaternion rotation, Transform parentTransform)
        {
            var instantiatePrefabTask = resourcesLoader.InstantiateAsync(assetName,position,rotation,parentTransform);
            return instantiatePrefabTask.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static GameObject Instantiate(this IResourcesLoader resourcesLoader, string assetName,
            Vector3 position, Quaternion rotation)
        {
            return resourcesLoader.Instantiate(assetName, position, rotation, null);
        }
    }
}