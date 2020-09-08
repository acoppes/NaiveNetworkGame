using System;
using UnityEngine;

namespace Client
{
    public class ActionDurationBehaviour : MonoBehaviour
    {
        public GameObject model;

        public Transform barTransform;
        
        [NonSerialized]
        public bool visible = false;

        [NonSerialized]
        public float alpha = 0;

        private void LateUpdate()
        {
            model.SetActive(visible);
            barTransform.localScale = new Vector3(alpha, 1, 0);
        }
    }
}