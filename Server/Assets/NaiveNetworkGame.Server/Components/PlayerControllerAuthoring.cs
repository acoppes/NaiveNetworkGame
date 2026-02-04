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
    
    public class PlayerControllerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
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
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new PlayerController()
            {
                player = player,
                maxUnits = maxUnits,
                currentUnits = currentUnits,

                maxGold = maxGold,
                gold = gold,

                skinType = skinType,

                defendArea = conversionSystem.GetPrimaryEntity(defendArea),
                attackArea = conversionSystem.GetPrimaryEntity(attackArea),

                availableBuildingSlots = availableBuildingSlots,

                freeBarracksCount = freeBarracksCount,

                defensiveRange = defensiveRange,
            });
            
            // conversionSystem.GetPrimaryEntity(behaviourData.wanderArea)
        }
    }
}