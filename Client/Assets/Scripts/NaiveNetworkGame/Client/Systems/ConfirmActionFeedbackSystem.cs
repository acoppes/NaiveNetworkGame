using System.Collections;
using Common;
using Scenes;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    // Shows a confirm action feedback for player move actions (currently disabled)
    public partial class ConfirmActionFeedbackSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<ClientPrefabsSharedComponent>();
        }

        protected override void OnUpdate()
        {
            var singletonEntity = GetSingletonEntity<ClientPrefabsSharedComponent>();
            var clientPrefabs = EntityManager.GetSharedComponentManaged<ClientPrefabsSharedComponent>(singletonEntity);
            
            Entities
                .WithAll<ConfirmActionFeedback>()
                .ForEach(delegate(Entity e, ref ConfirmActionFeedback feedback)
                {
                    PostUpdateCommands.DestroyEntity(e);

                    var confirmActionFeedback = GameObject.Instantiate(clientPrefabs.confirmActionPrefab);
                    confirmActionFeedback.transform.position = new Vector3(feedback.position.x, feedback.position.y, 0);
                    confirmActionFeedback.AddComponent<TempMonobehaviourForCoroutines>()
                        .StartCoroutine(DestroyActionOnComplete(confirmActionFeedback));

                });
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