using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    public struct Health : IComponentData
    {
        public float total;
        public float current;
        
        // TODO: sync as a byte percentage
    }
}