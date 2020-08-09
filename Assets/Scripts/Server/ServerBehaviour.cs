using Unity.Entities;
using UnityEngine;

namespace Server
{
    
    public class ServerBehaviour : MonoBehaviour
    {
        public GameObject go;
        
        private void Start ()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var serverEntity = entityManager.CreateEntity(ComponentType.ReadOnly<ServerOnly>());
            entityManager.AddSharedComponentData(serverEntity, new NetworkManagerSharedComponent());
            entityManager.AddComponentData(serverEntity, new ServerStartComponent());
            
            // create multiple player controllers, all disabled....
            // with each connection, remove disabled component.

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, settings);

            var e1 = entityManager.Instantiate(prefab);
            var e2 = entityManager.Instantiate(prefab);
            
            entityManager.SetComponentData(e1, new Unit
            {
                id = 0,
                player = 0
            });
            
            entityManager.SetComponentData(e2, new Unit
            {
                id = 1,
                player = 1
            });
        }

        void Update ()
        {
            // for (var i = 0; i < m_Connections.Length; i++)
            // {
            //     if (m_Connections[i].IsCreated)
            //     {
            //         for (var j = 0; j < clientObjects.Length; j++)
            //         {
            //             var clientObject = clientObjects[j];
            //             if (clientObject == null)
            //                 continue;
            //         
            //             var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
            //         
            //             new GamePacket
            //             {
            //                 type = GamePacket.SERVER_GAMESTATE_UPDATE,
            //                 networkPlayerId = (uint) j,
            //                 mainObjectPosition = (Vector2) clientObject.transform.position
            //             }.Write(ref writer);
            //         
            //             m_Driver.EndSend(writer);
            //         }
            //
            //     }
            // }
        }
    }
}