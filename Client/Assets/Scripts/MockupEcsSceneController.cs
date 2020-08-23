using System;
using Client;
using Scenes;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Mockups
{
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
            // var modelProvider = ModelProviderSingleton.Instance;
            // var clientModelRootQuery = Entities.WithAll<ClientModelRootSharedComponent>().ToEntityQuery();

            Entities
                .WithAll<ModelPrefabComponent>()
                .WithNone<ModelInstanceComponent>()
                .ForEach(delegate(Entity e, ModelPrefabComponent m)
                {
                    // var clientInstanceId = c.id;

                    var modelRoot = ModelProviderSingleton.Instance.root;

                    // Entities.ForEach(delegate(Entity e, ClientModelRootSharedComponent clientModelRoot)
                    // {
                    //     if (clientModelRoot.networkPlayerId == clientInstanceId)
                    //     {
                    //         modelRoot = clientModelRoot.parent;
                    //     }
                    // });

                    PostUpdateCommands.AddSharedComponent(e, new ModelInstanceComponent
                    {
                        instance = GameObject.Instantiate(m.prefab, modelRoot)
                    });
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
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class VisualModelUpdatePositionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, Translation>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref Translation t)
                {
                    m.instance.transform.position = t.Value;
                });
            
            Entities
                .WithAll<ModelPrefabComponent, ModelInstanceComponent, UnitState>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref UnitState state)
                {
                    var animator = m.instance.GetComponent<Animator>();
                    animator.SetInteger("state", state.state);
                });
            
            Entities
                .WithAll<ModelInstanceComponent, LookingDirection>()
                .ForEach(delegate(Entity e,  ModelInstanceComponent m, ref LookingDirection lookingDirection)
                {
                    m.instance.transform.localScale = new Vector3(lookingDirection.direction.x >= 0 ? 1: -1, 1, 1);
                });
        }
    }

    public class MockupEcsSceneController : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var modelProvider = ModelProviderSingleton.Instance;
                
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entity = manager.CreateEntity();
                manager.AddSharedComponentData(entity, new ModelPrefabComponent
                {
                    prefab = modelProvider.prefabs[UnityEngine.Random.Range(0, modelProvider.prefabs.Length)]
                });
                manager.AddComponentData(entity, new Translation
                {
                    Value = position
                });

            }
        }
    }
}