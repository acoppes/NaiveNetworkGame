using System;
using Unity.Mathematics;
using UnityEngine;

namespace Client
{
    public class UnitModelBehaviour : MonoBehaviour
    {
        public SpriteRenderer model;
        
        public GameObject playerIndicator;

        public Animator selectorAnimator;
        
        [NonSerialized]
        public bool isActivePlayer;

        [NonSerialized]
        public bool isSelected;

        [NonSerialized]
        public bool isDurationVisible;
        
        [NonSerialized]
        public bool isHealthBarVisible;

        [NonSerialized]
        public float healthBarAlpha;

        [NonSerialized]
        public float durationAlpha;

        private int selectedKeyHash;

        public BarBehaviour actionDuration;
        public BarBehaviour healthBar;
        
        public float interpolationSpeed = 1.0f;
        
        [NonSerialized]
        public float2 lookingDirection;

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

            if (healthBar != null)
            {
                healthBar.visible = isHealthBarVisible;
                healthBar.alpha = Mathf.Lerp(healthBar.alpha, healthBarAlpha, Time.deltaTime * interpolationSpeed);
            }
            
            if (model != null)
            {
                model.flipX = lookingDirection.x < 0;
            }

        }
    }
}
