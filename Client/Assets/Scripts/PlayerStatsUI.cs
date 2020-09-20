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
        
        public int maxUnits;
        public int currentUnits;

        private void LateUpdate()
        {
            currentLabel.SetNumber(currentUnits);
            maxLabel.SetNumber(maxUnits);
        }
    }
}