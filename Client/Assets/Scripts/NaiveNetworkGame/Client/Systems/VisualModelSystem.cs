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
    
    public struct ModelInstanceComponent : ICleanupSharedComponentData, IEquatable<ModelInstanceComponent>
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

    public partial struct VisualModelSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (modelPrefab, entity) in 
                SystemAPI.Query<ModelPrefabComponent>()
                    .WithNone<ModelInstanceComponent>()
                    .WithEntityAccess())
            {
                var modelRoot = ModelProviderSingleton.Instance.root;

                var modelInstanceComponent = new ModelInstanceComponent
                {
                    instance = GameObject.Instantiate(modelPrefab.prefab, modelRoot)
                };
                
                modelInstanceComponent.unitModel =
                    modelInstanceComponent.instance.GetComponentInChildren<UnitModelBehaviour>();

                state.EntityManager.AddSharedComponentManaged(entity, modelInstanceComponent);
            }

            foreach (var (modelInstance, entity) in 
                SystemAPI.Query<ModelInstanceComponent>()
                    .WithNone<ModelPrefabComponent>()
                    .WithEntityAccess())
            {
                GameObject.Destroy(modelInstance.instance);
                state.EntityManager.RemoveComponent<ModelInstanceComponent>(entity);
            }
        }
    }
}
