using UnityEngine;

namespace Citadel.SceneManagement
{
    public abstract class SingletonHelper <T> : MonoBehaviour where T : MonoBehaviour
    {
        private static SingletonHelper<T> _instance;

        public bool InitializeInstance()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (Const.StartingNewGame)
            {
                DestroyImmediate(_instance.gameObject);
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                DestroyImmediate(this.gameObject);
                return false;
            }

            return true;
        }
    }
}