using System;
using NaiveNetworkGame.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes
{
    public class UserInterface : MonoBehaviour
    {
        public PlayerButton buildUnitButton;
        [FormerlySerializedAs("buildFarmButton")] 
        public PlayerButton buildHouseButton;
        
        public PlayerButton buildBarracksButton;
        public PlayerButton attackButton;
        public PlayerButton defendButton;

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