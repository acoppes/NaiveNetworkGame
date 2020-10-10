using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct WanderArea : IComponentData
    {
        public float range;
    }
}