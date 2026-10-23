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
        [Inject] 
        private readonly ScenesLoader _scenesLoader;
        
        private void Start() => _scenesLoader.LoadLevelAsync(_scenesLoader.MainGameSceneIndex);
    }
}
