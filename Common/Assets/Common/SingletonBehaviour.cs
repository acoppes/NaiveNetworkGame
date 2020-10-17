using UnityEngine;

namespace NaiveNetworkGame.Common
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static string InstanceName => $"~{typeof(T).Name}";
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var instanceGameObject = GameObject.Find(InstanceName);
                    if (instanceGameObject != null)
                    {
                        _instance = instanceGameObject.GetComponentInChildren<T>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
            // if there is only one Component in the system?
            if (gameObject.GetComponents<MonoBehaviour>().Length == 1)
                gameObject.name = InstanceName;
        }
#endif
    }
}