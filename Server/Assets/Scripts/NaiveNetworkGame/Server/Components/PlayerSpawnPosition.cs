using Unity.Entities;

namespace NaiveNetworkGame.Server.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerSpawnPosition : IComponentData
    {
        public byte player;
    }
}