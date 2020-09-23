using System;
using Client;
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
        public UnitModelBehaviour unitModel;

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
                    var modelRoot = ModelProviderSingleton.Instance.root;

                    var modelInstanceComponent = new ModelInstanceComponent
                    {
                        instance = GameObject.Instantiate(m.prefab, modelRoot)
                    };
                    
                    modelInstanceComponent.unitModel =
                        modelInstanceComponent.instance.GetComponentInChildren<UnitModelBehaviour>();

                    PostUpdateCommands.AddSharedComponent(e, modelInstanceComponent);
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