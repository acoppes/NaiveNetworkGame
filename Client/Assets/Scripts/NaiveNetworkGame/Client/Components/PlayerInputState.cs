using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Client
{
    [GenerateAuthoringComponent]
    public struct PlayerInputState : IComponentData
    {
        public bool spawnActionPressed;
        public bool selectUnitButtonPressed;
        public bool actionButtonPressed;
        public float2 position;
    }
}