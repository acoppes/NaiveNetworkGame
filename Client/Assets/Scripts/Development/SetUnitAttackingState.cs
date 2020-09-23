using System.Collections;
using UnityEngine;

namespace Development
{
    public class SetUnitAttackingState : MonoBehaviour
    {
        public int state;
        
        private void Start()
        {
            StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            var animator = GetComponentInChildren<Animator>();
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 1.0f));
                animator.SetInteger("state", state);
            }
        }

        // private void Update()
        // {
        //     animator.SetInteger("state", state);
        // }
    }
}
