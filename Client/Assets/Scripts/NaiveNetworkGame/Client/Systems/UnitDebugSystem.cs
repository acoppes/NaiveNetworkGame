using System;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Systems
{
    public struct UnitDebugSystemStateComponent : ICleanupSharedComponentData, IEquatable<UnitDebugSystemStateComponent>
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
    
    public partial struct UnitDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (modelInstance, entity) in 
                SystemAPI.Query<ModelInstanceComponent>()
                    .WithNone<UnitDebugSystemStateComponent>()
                    .WithEntityAccess())
            {
                var debug = new UnitDebugSystemStateComponent
                {
                    unitDebug = modelInstance.instance.gameObject.AddComponent<UnitDebugBehaviour>()
                };
                
                ecb.AddSharedComponentManaged(entity, debug);
            }

            foreach (var (debug, unit) in 
                SystemAPI.Query<UnitDebugSystemStateComponent, RefRO<Unit>>())
            {
                // update debug
                debug.unitDebug.unitId = unit.ValueRO.unitId;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
