using Unity.Entities;
using UnityEngine;


namespace NaiveNetworkGame.Client
{
    public struct PlayerPendingAction : IComponentData
    {
        public bool pending;
        public byte actionType;
        public byte unitType;
    }
    
    public class PlayerPendingActionAuthoring : MonoBehaviour
    {
        private class PlayerPendingActionBaker : Baker<PlayerPendingActionAuthoring>
        {
            public override void Bake(PlayerPendingActionAuthoring authoring)
            {
                var entity= GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerPendingAction());
            }
        }
    }
}