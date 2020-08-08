using Unity.Entities;
using Unity.Mathematics;

namespace Server
{
    public struct Unit : IComponentData
    {
        public int player;
    }

    public struct Movement : IComponentData
    {
        public float speed;
    }

    public struct MovementAction : IComponentData
    {
        public float2 target;
    }

    public struct PendingAction : IComponentData
    {
        public int command;
        public float2 target;
    }


}