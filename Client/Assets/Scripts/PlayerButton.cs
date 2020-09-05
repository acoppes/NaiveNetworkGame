using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class PlayerButton : MonoBehaviour
    {
        [SerializeField]
        protected Button button;

        [SerializeField]
        protected Image background;

        public ClientSceneController controller;

        private void Start()
        {
            button.onClick.AddListener(OnPressed);
        }

        private void OnPressed()
        {
            controller.ToggleSpawning();
        }

        private void LateUpdate()
        {
            if (controller.IsWaitingForSpawing())
            {
                background.color = Color.red;
            }
            else
            {
                background.color = Color.white;
            }
        }
    }
}