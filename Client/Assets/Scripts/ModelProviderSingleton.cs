using Scenes;
using UnityEngine;

namespace Mockups
{
    public class ModelProviderSingleton : SingletonBehaviour<ModelProviderSingleton>
    {
        public GameObject[] prefabs;
        public Transform root;
        
        public void SetRoot(Transform root)
        {
            this.root = root;
        }
    }
}