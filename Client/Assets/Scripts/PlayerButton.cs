using System;
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

        private void Start()
        {
            activeHash = Animator.StringToHash("active");
            button.onClick.AddListener(OnPressed);
        }

        private void OnPressed()
        {
            controller.ToggleSpawning();
        }

        private void LateUpdate()
        {
            animator.SetBool(activeHash, controller.IsSpawnEnabled());
        }
    }
}