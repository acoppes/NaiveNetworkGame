using System;
using NaiveNetworkGame.Common;
using UnityEngine;

namespace NaiveNetworkGame.Client
{
    public class ModelProviderSingleton : SingletonBehaviour<ModelProviderSingleton>
    {
        public GameObject[] prefabs;
        
        [NonSerialized]
        public Transform root;
        
        public void SetRoot(Transform root)
        {
            this.root = root;
        }
    }
}