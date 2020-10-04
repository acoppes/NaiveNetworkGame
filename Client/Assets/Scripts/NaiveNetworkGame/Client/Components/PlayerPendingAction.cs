using Unity.Entities;


namespace NaiveNetworkGame.Client
{
    [GenerateAuthoringComponent]
    public struct PlayerPendingAction : IComponentData
    {
        public bool pending;
        public byte actionType;
        public byte unitType;
    }
}