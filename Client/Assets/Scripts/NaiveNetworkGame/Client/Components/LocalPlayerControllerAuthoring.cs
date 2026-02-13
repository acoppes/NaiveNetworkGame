using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct LocalPlayerController : IComponentData
    {
        public byte player;
        public byte skinType;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
        public byte buildingSlots;
        public byte freeBarracksCount;
        public byte behaviourMode;
    }

    public class LocalPlayerControllerAuthoring : MonoBehaviour
    {
        public byte player;
        public byte skinType;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
        public byte buildingSlots;
        public byte freeBarracksCount;
        public byte behaviourMode;

        private class LocalPlayerControllerBaker : Baker<LocalPlayerControllerAuthoring>
        {
            public override void Bake(LocalPlayerControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new LocalPlayerController
                {
                    player = authoring.player,
                    skinType = authoring.skinType,
                    gold = authoring.gold,
                    maxUnits = authoring.maxUnits,
                    currentUnits = authoring.currentUnits,
                    buildingSlots = authoring.buildingSlots,
                    freeBarracksCount = authoring.freeBarracksCount,
                    behaviourMode = authoring.behaviourMode
                });
                
                AddBuffer<PlayerActionDefinition>(entity);
            }
        }
    }
}