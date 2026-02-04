using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    public struct LocalPlayerControllerComponentData : IComponentData
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

    public class LocalPlayerController : MonoBehaviour, IConvertGameObjectToEntity
    {
        public byte player;
        public byte skinType;
        public ushort gold;
        public byte maxUnits;
        public byte currentUnits;
        public byte buildingSlots;
        public byte freeBarracksCount;
        public byte behaviourMode;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new LocalPlayerControllerComponentData
            {
                player = player,
                skinType = skinType,
                gold = gold,
                maxUnits = maxUnits,
                currentUnits = currentUnits,
                buildingSlots = buildingSlots,
                freeBarracksCount = freeBarracksCount,
                behaviourMode = behaviourMode
            });
        }
    }
}