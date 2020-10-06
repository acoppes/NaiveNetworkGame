using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    public struct BuildingSlot : IBufferElementData
    {
        public float3 position;
        public bool available;
    }
}