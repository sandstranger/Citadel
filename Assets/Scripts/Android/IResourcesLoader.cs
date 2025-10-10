using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        public static Task<GameObject> InstantiateAsync (this IResourcesLoader resourcesLoader,string assetName, Vector3 position, Quaternion rotation, CancellationToken cancellationToken)
        {
            return resourcesLoader.InstantiateAsync(assetName, position, rotation, null, cancellationToken);
        }

        public static Task<GameObject> InstantiateAsync (this IResourcesLoader resourcesLoader,string assetName, Vector3 position, Quaternion rotation)
        {
            return resourcesLoader.InstantiateAsync(assetName, position, rotation, null);
        }
    }
}