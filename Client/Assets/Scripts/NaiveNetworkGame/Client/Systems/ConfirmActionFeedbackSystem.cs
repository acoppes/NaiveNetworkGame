using System.Collections;
using Common;
using Scenes;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    // Shows a confirm action feedback for player move actions (currently disabled)
    public partial struct ConfirmActionFeedbackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClientPrefabsSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var singletonEntity = SystemAPI.GetSingletonEntity<ClientPrefabsSingleton>();
            // var singletonEntity = GetSingletonEntity<ClientPrefabsSharedComponent>();
            var clientPrefabs = state.EntityManager.GetSharedComponentManaged<ClientPrefabsSharedComponent>(singletonEntity);
            
            foreach (var (feedback, e) in SystemAPI.Query<RefRO<ConfirmActionFeedback>>().WithEntityAccess())
            {
                var confirmActionFeedback = GameObject.Instantiate(clientPrefabs.confirmActionPrefab);
                confirmActionFeedback.transform.position = new Vector3(feedback.ValueRO.position.x, feedback.ValueRO.position.y, 0);
                confirmActionFeedback.AddComponent<TempMonobehaviourForCoroutines>()
                    .StartCoroutine(DestroyActionOnComplete(confirmActionFeedback));
                
                state.EntityManager.DestroyEntity(e);
            }
            
            // Entities
            //     .WithAll<ConfirmActionFeedback>()
            //     .ForEach(delegate(Entity e, ref ConfirmActionFeedback feedback)
            //     {
            //         PostUpdateCommands.DestroyEntity(e);
            //
            //         var confirmActionFeedback = GameObject.Instantiate(clientPrefabs.confirmActionPrefab);
            //         confirmActionFeedback.transform.position = new Vector3(feedback.position.x, feedback.position.y, 0);
            //         confirmActionFeedback.AddComponent<TempMonobehaviourForCoroutines>()
            //             .StartCoroutine(DestroyActionOnComplete(confirmActionFeedback));
            //
            //     });
        }
        
        private IEnumerator DestroyActionOnComplete(GameObject actionInstance)
        {
            var animator = actionInstance.GetComponent<Animator>();
            var hiddenState = Animator.StringToHash("Hidden");
            
            animator.SetTrigger("Action");

            yield return null;
            
            yield return new WaitUntil(delegate
            {
                var currentState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
                return currentState == hiddenState;
            });
            
            GameObject.Destroy(actionInstance);
        }
    }
}