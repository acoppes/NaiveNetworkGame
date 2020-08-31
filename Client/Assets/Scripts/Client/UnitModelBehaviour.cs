using System;
using UnityEngine;

namespace Client
{
    public class UnitModelBehaviour : MonoBehaviour
    {
        public GameObject playerIndicator;
        
        [NonSerialized]
        public bool isActivePlayer;

        // Update is called once per frame
        private void LateUpdate()
        {
            playerIndicator?.SetActive(isActivePlayer);
        }
    }
}
