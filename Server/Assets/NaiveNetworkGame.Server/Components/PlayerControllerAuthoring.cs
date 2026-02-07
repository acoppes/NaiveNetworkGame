using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public struct PlayerController : IComponentData
    {
        public byte player;
        public byte maxUnits;
        public byte currentUnits;

        public ushort maxGold;
        public ushort gold;

        public byte skinType;

        public Entity defendArea;
        public Entity attackArea;

        public byte availableBuildingSlots;

        public byte freeBarracksCount;

        public float defensiveRange;
    }

    public struct PlayerBehaviour : IComponentData
    {
        public const byte aggressive = 1;
        public const byte defensive = 0;
        
        public byte mode;
        // mode data?
    }
    
    public class PlayerControllerAuthoring : MonoBehaviour
    {
        public byte player;
        public byte maxUnits;
        public byte currentUnits;

        public ushort maxGold;
        public ushort gold;

        public byte skinType;

        public GameObject defendArea;
        public GameObject attackArea;

        public byte availableBuildingSlots;

        public byte freeBarracksCount;

        public float defensiveRange;

        private class PlayerControllerBaker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerController()
                {
                    player = authoring.player,
                    maxUnits = authoring.maxUnits,
                    currentUnits = authoring.currentUnits,

                    maxGold = authoring.maxGold,
                    gold = authoring.gold,

                    skinType = authoring.skinType,

                    defendArea = GetEntity(authoring.defendArea, TransformUsageFlags.Dynamic),
                    attackArea = GetEntity(authoring.attackArea, TransformUsageFlags.Dynamic),

                    availableBuildingSlots = authoring.availableBuildingSlots,

                    freeBarracksCount = authoring.freeBarracksCount,

                    defensiveRange = authoring.defensiveRange,
                });
            
                // conversionSystem.GetPrimaryEntity(behaviourData.wanderArea)
            }
        }
    }
}