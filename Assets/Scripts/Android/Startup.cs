using System;
using Citadel.SceneManagement;
using UnityEngine;
#if UNITY_ANDROID
using Cysharp.Threading.Tasks;
using Zenject;
#endif

namespace Citadel.Game
{
    internal sealed class Startup : MonoBehaviour
    {
#if UNITY_ANDROID
        [Inject]
        private IAddressableTextAssetsPreparer _addressableTextAssetsPreparer;

        private void Start() => InitializeAsync().Forget();

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                if (await _addressableTextAssetsPreparer.PrepareAsync())
                {
                    await ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
#else
        private void Start() => ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex);
#endif
    }
}
