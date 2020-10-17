using Unity.Entities;
using Unity.Mathematics;

namespace NaiveNetworkGame.Server.Components
{
    public static class NetworkUnitId
    {
        public static ushort current = 1;
    }

    public struct ServerSimulation : IComponentData
    {
            
    }
    
    public struct Unit : IComponentData
    {
        public ushort id;
        public byte player;
        public byte type;
        public byte slotCost;
    }

    public struct Skin : IComponentData
    {
        public byte type;
    }
    
    public struct House : IComponentData
    {
        public byte maxUnits;
    }
    
    public struct ResourceCollector : IComponentData
    {
        public ushort goldPerSecond;
        public ushort collectedGold;
        
        public float currentCollectionTime;
    }

    public struct Damage : IComponentData
    {
        public Entity target;
        public float damage;
    }
    
    public struct DynamicObstacle : IComponentData
    {
        public uint index;
        public byte priority;
        public float range;
        public float rangeSq;
        public float3 movement;
    }

    public struct IsAlive : IComponentData
    {
        
    }
    
    public struct DeathAction : IComponentData
    {
        public float time;
        public float duration;
    }
    
    public struct IdleAction : IComponentData
    {
        public float time;
    }
    
    public struct AttackAction : IComponentData
    {
        public bool performed;
        public float time;
    }

    public struct ReloadAction : IComponentData
    {
        public float time;
        public float duration;
    }

    public struct MovementAction : IComponentData
    {
        public float2 target;
        public float2 direction;
    }

    public struct SpawningAction : IComponentData
    {
        public float duration;
        public float time;
    }
    
    public struct WanderArea : IComponentData
    {
        public float range;
    }
}