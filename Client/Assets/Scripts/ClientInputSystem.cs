using Client;
using Unity.Entities;
using UnityEngine;

namespace Scenes
{
    public class ClientInputSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<PlayerInputState>();
        }

        protected override void OnUpdate()
        {
            var playerInputStateEntity = GetSingletonEntity<PlayerInputState>();
            var playerInputState = EntityManager.GetComponentData<PlayerInputState>(playerInputStateEntity);
         
            playerInputState.selectUnitButtonPressed = Input.GetMouseButtonUp(0);
            playerInputState.actionButtonPressed = Input.GetMouseButtonUp(1);

            SetSingleton(playerInputState);
        }
    }
}