using System;
using System.Collections.Generic;
using NaiveNetworkGame.Client;
using NaiveNetworkGame.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlayerButton : MonoBehaviour
    {
        [SerializeField]
        protected Button button;

        // [SerializeField]
        // protected Image background;
        
        [SerializeField]
        protected Animator animator;

        public ClientSceneController controller;

        private int activeHash;

        [SerializeField]
        private List<GameObject> skinIcons;

        [SerializeField]
        private Transform iconsContainer;

        [SerializeField]
        private GameObject[] icons;

        [NonSerialized]
        public byte skinType;

        [SerializeField]
        private FixedNumbersLabel costLabel;
        
        public PlayerActionAsset actionType;

        [NonSerialized]
        public byte cost;

        [NonSerialized]
        public bool enabled;
        
        private void Start()
        {
            activeHash = Animator.StringToHash("active");
            button.onClick.AddListener(OnPressed);
        }

        private void OnPressed()
        {
            controller.OnPlayerAction(actionType);
        }

        private void LateUpdate()
        {
            animator.SetBool(activeHash, enabled);

            for (var i = 0; i < iconsContainer.childCount; i++)
            {
                iconsContainer.GetChild(i).gameObject.SetActive(false);
            }
            
            skinIcons[skinType].SetActive(true);
            
            // iconsPerSkin[skinType].list[actionType.unitType].SetActive(true);

            // for (var i = 0; i < icons.Length; i++)
            // {
            //     icons[i].SetActive(i == skinType);
            // }

            if (costLabel != null)
            {
                costLabel.SetNumber(cost);
            }
        }
    }
}