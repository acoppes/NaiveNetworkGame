using System;
using System.Collections.Generic;
using NaiveNetworkGame.Common;
using UnityEngine;

namespace NaiveNetworkGame.Client
{
    [Serializable]
    public class SkinModels
    {
        public List<GameObject> modelPrefabs;
    }
    
    public class ModelProviderSingleton : SingletonBehaviour<ModelProviderSingleton>
    {
        public List<SkinModels> skinModels = new List<SkinModels>();
        
        [NonSerialized]
        public Transform root;
        
        public void SetRoot(Transform root)
        {
            this.root = root;
        }
    }
}