using System;
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
        private GameObject[] icons;

        [NonSerialized]
        public byte unitType;

        public PlayerActionAsset actionType;
        
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
            animator.SetBool(activeHash, controller.IsSpawnEnabled());

            for (var i = 0; i < icons.Length; i++)
            {
                icons[i].SetActive(i == unitType);
            }
        }
    }
}