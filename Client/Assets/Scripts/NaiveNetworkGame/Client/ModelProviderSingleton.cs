using System;
using System.Collections.Generic;
using NaiveNetworkGame.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace NaiveNetworkGame.Client
{
    [Serializable]
    public class SkinModels
    {
        [FormerlySerializedAs("modelPrefabs")] 
        public List<GameObject> list;
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