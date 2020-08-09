using System;
using System.Collections.Generic;
using Server;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes
{
    public struct ClientViewSharedComponent : ISharedComponentData, IEquatable<ClientViewSharedComponent>
    {
        // for the view prefab    
        // and the dictionary of entity,instance
        // and the transform...

        public GameObject prefab;
        public Dictionary<uint, GameObject> instances;
        public Transform parent;

        public bool Equals(ClientViewSharedComponent other)
        {
            return Equals(prefab, other.prefab) && Equals(instances, other.instances) && Equals(parent, other.parent);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientViewSharedComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (prefab != null ? prefab.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (instances != null ? instances.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (parent != null ? parent.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    // for when there a unit was updated from the server
    public struct ClientViewUpdate : IComponentData
    {
        public uint unitId;
        public float2 position;
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ClientViewSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // iterate over client view updates...

            Entities.ForEach(delegate(Entity e, ref ClientViewUpdate c)
            {
                PostUpdateCommands.DestroyEntity(e);
                
                var unitId = c.unitId;
                var position = c.position;
                
                Entities.WithAll<ClientViewSharedComponent>().ForEach(
                    delegate(ClientViewSharedComponent clientViewManager)
                    {
                        if (!clientViewManager.instances.ContainsKey(unitId))
                        {
                            var instance = GameObject.Instantiate(clientViewManager.prefab, clientViewManager.parent);
                            instance.SetActive(true);
                            clientViewManager.instances[unitId] = instance;
                        }

                        var unitView = clientViewManager.instances[unitId];
                        unitView.transform.position = new Vector3(position.x, position.y, 0);
                    });
            });
        }
    }
    
    public class EcsTestClientSceneController : MonoBehaviour
    {
        public Camera camera;

        public uint networkPlayerId;
        public int button;

        public GameObject prefab;
        public Transform parent;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var clientViewManager = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(clientViewManager, new ClientViewSharedComponent
            {
                prefab = prefab,
                parent = parent,
                instances = new Dictionary<uint, GameObject>()
            });
        }

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // if (clientBehaviour.clientNetworkManager.networkPlayerId == -1)
            //     return;

            if (Input.GetMouseButtonUp(button))
            {
                var mousePosition = Input.mousePosition;
                var worldPosition = camera.ScreenToWorldPoint(mousePosition);

                var entity = entityManager.CreateEntity(ComponentType.ReadOnly<ClientOnly>());
                entityManager.AddComponentData(entity, new PendingPlayerAction
                {
                    player = networkPlayerId,
                    command = 0,
                    target = new float2(worldPosition.x, worldPosition.y)
                });
            }
        }
    }
}
