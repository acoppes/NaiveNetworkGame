using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Server.Components
{
    public class HouseAuthoring : MonoBehaviour
    {
        public byte unitSlots;
        public byte goldPerSedond;

        private class HouseBaker : Baker<HouseAuthoring>
        {
            public override void Bake(HouseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new House
                {
                    maxUnits = authoring.unitSlots
                });
                AddComponent(entity, new ResourceCollector
                {
                    goldPerSecond = authoring.goldPerSedond
                });
            }
        }
    }
}