using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
    // Given mouse state, updates player input state.
    public class UpdatePlayerInputSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Deprecated now that we dont interact directly with units ...
            
            // if (Camera.main == null)
            //     return;
            //
            // var mousePosition = Input.mousePosition;
            // var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            //
            // Entities.ForEach(delegate(ref PlayerInputState playerInputState)
            // {
            //     playerInputState.selectUnitButtonPressed = Input.GetMouseButtonUp(0);
            //     playerInputState.actionButtonPressed = Input.GetMouseButtonUp(1);
            //     playerInputState.position = new float2(worldPosition.x, worldPosition.y);
            // });
        }
    }
}