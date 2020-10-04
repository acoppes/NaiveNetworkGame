using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct ResourceCollector : IComponentData
    {
        public ushort goldPerSecond;
        public ushort collectedGold;
        
        public float currentCollectionTime;
    }
}