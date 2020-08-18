using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Mockups
{
    // public struct UnitComponent : IComponentData
    // {
    //     
    // }
    
    public struct ModelPrefabComponent : ISharedComponentData, IEquatable<ModelPrefabComponent>
    {
        public GameObject prefab;

        public bool Equals(ModelPrefabComponent other)
        {
            return Equals(prefab, other.prefab);
        }

        public override bool Equals(object obj)
        {
            return obj is ModelPrefabComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (prefab != null ? prefab.GetHashCode() : 0);
        }
    }
    
    public struct ModelInstanceComponent : ISystemStateSharedComponentData, IEquatable<ModelInstanceComponent>
    {
        public GameObject instance;

        public bool Equals(ModelInstanceComponent other)
        {
            return Equals(instance, other.instance);
        }

        public override bool Equals(object obj)
        {
            return obj is ModelInstanceComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (instance != null ? instance.GetHashCode() : 0);
        }
    }
    
    public class VisualModelSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ModelPrefabComponent>()
                .WithNone<ModelInstanceComponent>()
                .ForEach(delegate(Entity e, ModelPrefabComponent m)
                {
                    PostUpdateCommands.AddSharedComponent(e, new ModelInstanceComponent()
                    {
                        instance = GameObject.Instantiate(m.prefab)
                    });
                });
            
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, Translation>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref Translation t)
                {
                    m.instance.transform.position = t.Value;
                });
            
            Entities
                .WithNone<ModelPrefabComponent>()
                .WithAll<ModelInstanceComponent>()
                .ForEach(delegate(Entity e, ModelInstanceComponent m)
                {
                    GameObject.Destroy(m.instance);
                    PostUpdateCommands.RemoveComponent<ModelInstanceComponent>(e);
                });
        }
    }
    
    public class MockupEcsSceneController : MonoBehaviour
    {
        public GameObject prefab;

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entity = manager.CreateEntity();
                manager.AddSharedComponentData(entity, new ModelPrefabComponent
                {
                    prefab = prefab
                });
                manager.AddComponentData(entity, new Translation
                {
                    Value = position
                });

            }
        }
    }
}
