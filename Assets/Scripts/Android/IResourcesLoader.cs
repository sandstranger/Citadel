using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Citadel.Game
{
    /// <summary>
    /// Addressables-backed asset service. Convenience overloads live on the service
    /// rather than in static extension classes so the API is resolved through DI.
    /// </summary>
    internal interface IResourcesLoader : IDisposable
    {
        UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken)
            where T : UnityEngine.Object;

        UniTask<T> LoadAssetAsync<T>(string assetName)
            where T : UnityEngine.Object;

        UniTaskVoid LoadAssetAsync<T>(string assetName, Action<T> onAssetLoaded)
            where T : UnityEngine.Object;

        UniTask<GameObject> InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform, CancellationToken cancellationToken);

        UniTask<GameObject> InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform = null);

        UniTaskVoid InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform, Action<GameObject> onPrefabInstantiated);

        void InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Action<GameObject> onPrefabInstantiated);

        UniTask<GameObject> InstantiatePrefabAsync(int prefabIndex, Vector3 position, Quaternion rotation,
            Transform parentTransform = null);

        UniTaskVoid InstantiatePrefabAsync(int prefabIndex, Vector3 position, Quaternion rotation,
            Transform parentTransform, Action<GameObject> onPrefabInstantiated);

        void ReleaseAsset(string assetName);

        UniTask ReleaseAllAssetsAsync();
    }
}
