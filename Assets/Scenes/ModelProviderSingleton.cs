using Scenes;
using UnityEngine;

namespace Mockups
{
    public class ModelProviderSingleton : SingletonBehaviour<ModelProviderSingleton>
    {
        public GameObject[] prefabs;
    }
}