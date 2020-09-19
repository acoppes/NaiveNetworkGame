using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    // Given mouse state, updates player input state.
    public class UpdatePlayerInputSystem : ComponentSystem
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
            
            var mousePosition = Input.mousePosition;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            playerInputState.position = new float2(worldPosition.x, worldPosition.y);

            SetSingleton(playerInputState);
        }
    }
}