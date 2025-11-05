using UnityEngine;

namespace Singletons
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }
    
        [SerializeField] private bool _dontDestroyOnLoad = false;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            if (_dontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(this);
            }

            Init();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        protected virtual void Init() { }
    }
}