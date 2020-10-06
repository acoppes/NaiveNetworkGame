using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct BuildingSlot : IBufferElementData
    {
        public float3 position;
    }
}