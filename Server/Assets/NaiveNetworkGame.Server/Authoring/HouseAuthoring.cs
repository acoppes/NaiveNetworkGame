using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class HouseAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public byte unitSlots;
        public byte goldPerSedond;
        
        public void Convert(Entity e, EntityManager em, GameObjectConversionSystem conversionSystem)
        {
            em.AddComponentData(e, new House
            {
                maxUnits = unitSlots
            });
            em.AddComponentData(e, new ResourceCollector
            {
                goldPerSecond = goldPerSedond
            });
        }
    }
}