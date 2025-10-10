using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement;
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
        
        public async Task<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) 
            where T : Object
        {
            try
            {
                using (cancellationToken.Register(() => ReleaseAsset(assetName)));
                return await LoadAssetAsync<T>(assetName).WithCancellationAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        public async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            if (_loadedAssets.TryGetValue(assetName, out var assetInfo))
            {
                if (assetInfo.Handle.IsDone)
                {
                    return assetInfo.GetResult<T>();
                }

                await assetInfo.Handle.Task;
                return assetInfo.GetResult<T>();
            }
            
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            _loadedAssets[assetName] = new AssetInfo(assetName, handle);
            await handle.Task;
            return handle.Result;
        }
        
        public async Task<GameObject> InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform, CancellationToken cancellationToken)
        {
            try
            {
                var prefab = await LoadAssetAsync<GameObject>(assetName, cancellationToken);

                if (prefab is not null)
                {
                    AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(prefab, position, rotation, parentTransform);
                    using (cancellationToken.Register(() => handle.Release()));
                    var prefabInstance = await handle.Task.WithCancellationAsync(cancellationToken);
                    InjectExistingPrefab(prefabInstance);
                    return prefabInstance;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        public async Task<GameObject> InstantiateAsync(string assetName, Vector3 position, Quaternion rotation,
            Transform parentTransform)
        {
            var prefab = await LoadAssetAsync<GameObject>(assetName);
            var prefabInstance = await Addressables.InstantiateAsync(prefab, position, 
                rotation, parentTransform).Task;
            InjectExistingPrefab(prefabInstance);
            return prefabInstance;
        }

        public void ReleaseAsset(string assetName)
        {
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

            public AsyncOperationHandle<T> GetHandle<T>() where T : UnityEngine.Object => Handle.Convert<T>();
            
            public T GetResult<T>() where T : UnityEngine.Object => Handle.IsDone ? Handle.Convert<T>().Result : null;
            
            public bool Equals(AssetInfo other) => AssetKey == other.AssetKey;
            
            public override bool Equals(object obj) => obj is AssetInfo other && Equals(other);

            public override int GetHashCode() => AssetKey.GetHashCode();
        }
    }
}