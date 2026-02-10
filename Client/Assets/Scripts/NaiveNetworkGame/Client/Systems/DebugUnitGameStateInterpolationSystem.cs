using System;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
{
#if UNITY_EDITOR
    public struct DebugUnitGameStateInterpolationComponent : ISystemStateSharedComponentData, IEquatable<DebugUnitGameStateInterpolationComponent>
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
    public partial class DebugUnitGameStateInterpolationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<TranslationInterpolation>()
                .WithNone<DebugUnitGameStateInterpolationComponent>()
                .ForEach(delegate(Entity e)
                {
                    // create the debug object here...

                    var gameObject = new GameObject("~Debug-" + EntityManager.GetName(e));
                    var debugObject = gameObject.AddComponent<DebugInterpolationMonoBehaviour>();
                    
                    PostUpdateCommands.AddSharedComponent(e, new DebugUnitGameStateInterpolationComponent
                    {
                        debugObject = debugObject
                    });
                });
            
            Entities
                .WithAll<TranslationInterpolation, DebugUnitGameStateInterpolationComponent>()
                .ForEach(delegate(Entity e,  DebugUnitGameStateInterpolationComponent debug, ref TranslationInterpolation interpolation)
                {
                    debug.debugObject.p0 = new Vector3(interpolation.previousTranslation.x, 
                        interpolation.previousTranslation.y, 0);
                    debug.debugObject.p1 = new Vector3(interpolation.currentTranslation.x, 
                        interpolation.currentTranslation.y, 0);
                });

            Entities
                .WithNone<TranslationInterpolation>()
                .WithAll<DebugUnitGameStateInterpolationComponent>()
                .ForEach(delegate(Entity e, DebugUnitGameStateInterpolationComponent debug)
                {
                    GameObject.Destroy(debug.debugObject.gameObject);
                    PostUpdateCommands.RemoveComponent<DebugUnitGameStateInterpolationComponent>(e);
                });
        }
    }
    
#endif

}