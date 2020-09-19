using System;
using Mockups;
using Unity.Entities;
using UnityEngine;

namespace NaiveNetworkGame.Client.Systems
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
                    // PostUpdateCommands.AddComponent(e, new Selectable());
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
}