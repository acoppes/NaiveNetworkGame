using System;
using NaiveNetworkGame.Common;
using UnityEngine;

namespace Scenes
{
    public class PlayerStatsUI : MonoBehaviour
    {
        [SerializeField]
        private FixedNumbersLabel maxLabel;

        [SerializeField]
        private FixedNumbersLabel currentLabel;

        [SerializeField]
        private GameObject[] icons;
        
        [NonSerialized]
        public int maxUnits;
        
        [NonSerialized]
        public int currentUnits;
        
        [NonSerialized]
        public byte unitType;

        private void LateUpdate()
        {
            currentLabel.SetNumber(currentUnits);
            maxLabel.SetNumber(maxUnits);

            for (var i = 0; i < icons.Length; i++)
            {
                icons[i].SetActive(i == unitType);
            }
        }
    }
}