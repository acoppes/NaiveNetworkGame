using System;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public struct UnitDebugSystemStateComponent : ISystemStateSharedComponentData, IEquatable<UnitDebugSystemStateComponent>
    {
        public UnitDebugBehaviour unitDebug;

        public bool Equals(UnitDebugSystemStateComponent other)
        {
            return Equals(unitDebug, other.unitDebug);
        }

        public override bool Equals(object obj)
        {
            return obj is UnitDebugSystemStateComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (unitDebug != null ? unitDebug.GetHashCode() : 0);
        }
    }
    
    public class UnitDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ModelInstanceComponent>()
                .WithNone<UnitDebugSystemStateComponent>()
                .ForEach(delegate(Entity e, ModelInstanceComponent m)
                {
                    var debug = new UnitDebugSystemStateComponent
                    {
                        unitDebug = m.instance.gameObject.AddComponent<UnitDebugBehaviour>()
                    };
                    
                    PostUpdateCommands.AddSharedComponent(e, debug);
                });

            Entities
                .WithAll<Unit, UnitDebugSystemStateComponent>()
                .ForEach(delegate(Entity e, UnitDebugSystemStateComponent debug, ref Unit u)
                {
                    // update debug
                    debug.unitDebug.unitId = u.unitId;
                });
        }
    }
}