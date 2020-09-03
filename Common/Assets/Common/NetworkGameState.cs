using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Common
{
    public struct NetworkGameState : IComponentData
    {
        public int frame;
        public float delta;
        public int unitId;
        public int playerId;
        public byte unitType;
        public float2 translation;
        public float2 lookingDirection;
        public int state;
    }
}