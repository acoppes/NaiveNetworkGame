using System;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
#if UNITY_EDITOR
    public struct DebugUnitGameStateInterpolationComponent : ICleanupSharedComponentData, IEquatable<DebugUnitGameStateInterpolationComponent>
    {
        public DebugInterpolationMonoBehaviour debugObject;

        public bool Equals(DebugUnitGameStateInterpolationComponent other)
        {
            return Equals(debugObject, other.debugObject);
        }

        public override bool Equals(object obj)
        {
            return obj is DebugUnitGameStateInterpolationComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (debugObject != null ? debugObject.GetHashCode() : 0);
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DebugUnitGameStateInterpolationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, entity) in 
                SystemAPI.Query<TranslationInterpolation>()
                    .WithNone<DebugUnitGameStateInterpolationComponent>()
                    .WithEntityAccess())
            {
                // create the debug object here...

                var gameObject = new GameObject("~Debug-" + state.EntityManager.GetName(entity));
                var debugObject = gameObject.AddComponent<DebugInterpolationMonoBehaviour>();
                
                state.EntityManager.AddSharedComponentManaged(entity, new DebugUnitGameStateInterpolationComponent
                {
                    debugObject = debugObject
                });
            }
            
            foreach (var (interpolation, debug) in 
                SystemAPI.Query<RefRO<TranslationInterpolation>, DebugUnitGameStateInterpolationComponent>())
            {
                debug.debugObject.p0 = new Vector3(interpolation.ValueRO.previousTranslation.x, 
                    interpolation.ValueRO.previousTranslation.y, 0);
                debug.debugObject.p1 = new Vector3(interpolation.ValueRO.currentTranslation.x, 
                    interpolation.ValueRO.currentTranslation.y, 0);
            }

            foreach (var (debug, entity) in 
                SystemAPI.Query<DebugUnitGameStateInterpolationComponent>()
                    .WithNone<TranslationInterpolation>()
                    .WithEntityAccess())
            {
                GameObject.Destroy(debug.debugObject.gameObject);
                state.EntityManager.RemoveComponent<DebugUnitGameStateInterpolationComponent>(entity);
            }
        }
    }
    
#endif

}
