using System;
using Citadel.SceneManagement;
using UnityEngine;
using Zenject;
#if UNITY_ANDROID
using Cysharp.Threading.Tasks;
#endif

namespace Citadel.Game
{
    internal sealed class Startup : MonoBehaviour
    {
        [Inject] private ScenesLoader _scenesLoader;

#if UNITY_ANDROID
        [Inject] private IAddressableTextAssetsPreparer _addressableTextAssetsPreparer;

        private void Start() => InitializeAsync().Forget();

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                if (await _addressableTextAssetsPreparer.CopyGameTextAssetsToDiskAsync())
                {
                    await _scenesLoader.LoadLevelAsync(_scenesLoader.MainGameSceneIndex);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
#else
        private void Start() => _scenesLoader.LoadLevelAsync(_scenesLoader.MainGameSceneIndex);
#endif
    }
}
