using Citadel.SceneManagement;
using UnityEngine;

namespace Citadel.Game
{ 
    internal sealed class Startup : MonoBehaviour
    {
        private void Start() => ScenesLoader.LoadLevelAsync(ScenesLoader.MainGameSceneIndex); 
    }
}
