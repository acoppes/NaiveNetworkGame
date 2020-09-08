using System;
using UnityEngine;

namespace Client
{
    public class UnitModelBehaviour : MonoBehaviour
    {
        public GameObject playerIndicator;

        public Animator selectorAnimator;
        
        [NonSerialized]
        public bool isActivePlayer;

        [NonSerialized]
        public bool isSelected;

        [NonSerialized]
        public bool isDurationVisible;

        [NonSerialized]
        public float durationAlpha;

        private int selectedKeyHash;

        public ActionDurationBehaviour actionDuration;

        public float interpolationSpeed = 1.0f;

        private void Awake()
        {
            selectedKeyHash = Animator.StringToHash("selected");
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            playerIndicator?.SetActive(isActivePlayer);

            if (selectorAnimator != null)
            {
                selectorAnimator.SetBool(selectedKeyHash, isSelected);
            }

            if (actionDuration != null)
            {
                actionDuration.visible = isDurationVisible;
                actionDuration.alpha = Mathf.Lerp(actionDuration.alpha, durationAlpha, Time.deltaTime * interpolationSpeed);
            }
        }
    }
}
