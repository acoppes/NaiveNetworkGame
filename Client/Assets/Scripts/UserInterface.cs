using System;
using NaiveNetworkGame.Common;
using UnityEngine;

namespace Scenes
{
    public class UserInterface : MonoBehaviour
    {
        public PlayerButton buildUnitButton;
        public PlayerButton buildFarmButton;
        
        public FixedNumberAnimator goldLabel;
        public PlayerStatsUI playerStats;

        [SerializeField]
        private CanvasGroup canvasGroup;
        
        [NonSerialized]
        public bool visible;

        private void LateUpdate()
        {
            canvasGroup.interactable = visible;
            canvasGroup.alpha = visible ? 1.0f : 0.0f;
        }
    }
}