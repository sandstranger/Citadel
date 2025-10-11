using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

namespace Citadel.Game
{
    internal sealed class AddressablesResourcesLoader : IResourcesLoader
    {
        private const int DefaultAssetsCapacity = 1024;

        public static readonly IResourcesLoader Default = new AddressablesResourcesLoader();
        
        [Inject]
        private readonly DiContainer _container;
        private readonly Dictionary<string, AssetInfo> _loadedAssets = new(DefaultAssetsCapacity);
        
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) 
            where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    ReleaseAsset(assetName);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (cancellationToken.CanBeCanceled)
                {
                    using (cancellationToken.Register(() => ReleaseAsset(assetName)));
                }
                
                if (_loadedAssets.TryGetValue(assetName, out var assetInfo))
                {
                    if (assetInfo.Handle.IsDone)
                    {
                        return assetInfo.GetResult<T>();
                    }

                    await assetInfo.Handle.WithCancellation(cancellationToken, true);
                    return assetInfo.GetResult<T>();
                }
            
                var handle = Addressables.LoadAssetAsync<T>(assetName);
                _loadedAssets[assetName] = new AssetInfo(assetName, handle);
                await handle.WithCancellation(cancellationToken, true);
                return handle.Result;
            }
            catch (OperationCanceledException e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        public async UniTask<GameObject> InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                
                var prefab = await LoadAssetAsync<GameObject>(assetName, cancellationToken);

                if (prefab is not null)
                {
                    AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefab, position, rotation, parentTransform);
                    var prefabInstance = await handle.WithCancellation(cancellationToken,true);
                    InjectExistingPrefab(prefabInstance);
                    return prefabInstance;
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            
            if (_loadedAssets.TryGetValue(assetName, out var assetInfo))
            {
                if (assetInfo.Handle.IsDone)
                {
                    return assetInfo.GetResult<T>();
                }

                assetInfo.Handle.WaitForCompletion();
                return assetInfo.GetResult<T>();
            }
            
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            _loadedAssets[assetName] = new AssetInfo(assetName, handle);
            handle.WaitForCompletion();
            return handle.Result;
        }
        
        public GameObject Instantiate(string assetName, Vector3 position, Quaternion rotation, Transform parentTransform)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            
            var prefab = LoadAsset<GameObject>(assetName);
            var prefabInstanceTask = Addressables.InstantiateAsync(prefab, position, 
                rotation, parentTransform);
            prefabInstanceTask.WaitForCompletion();
            InjectExistingPrefab(prefabInstanceTask.Result);
            return prefabInstanceTask.Result;
        }

        public void ReleaseAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName));
            }
            
            if (_loadedAssets.TryGetValue(assetName, out var assetInfo))
            {
                Addressables.Release(assetInfo.Handle);
                _loadedAssets.Remove(assetName);
            }
        }

        public void ReleaseAllAssets()
        {
            foreach (var assetInfo in _loadedAssets.Values)
            {
                Addressables.Release(assetInfo.Handle);
            }

            _loadedAssets.Clear();
            Resources.UnloadUnusedAssets();
        }
        
        public void Dispose()
        {
            ReleaseAllAssets();
        }

        private void InjectExistingPrefab(GameObject prefabInstance)
        {
            foreach (var component in prefabInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                _container.Inject(component);
            }
        }
        
        private readonly struct AssetInfo : IEquatable<AssetInfo>
        {
            public readonly string AssetKey;

            public AsyncOperationHandle Handle { get; }

            public AssetInfo(string assetKey, AsyncOperationHandle handle)
            {
                AssetKey = assetKey;
                Handle = handle;
            }

            public T GetResult<T>() where T : Object => Handle.IsDone ? Handle.Convert<T>().Result : null;
            
            public bool Equals(AssetInfo other) => AssetKey == other.AssetKey;
            
            public override bool Equals(object obj) => obj is AssetInfo other && Equals(other);

            public override int GetHashCode() => AssetKey.GetHashCode();
        }
    }
}