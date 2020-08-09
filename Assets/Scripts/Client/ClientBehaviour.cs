using System;
using Server;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace Client
{
    public class ClientNetworkManager
    {
        public int networkPlayerId = -1;
        public NetworkDriver m_Driver;
        public NetworkConnection m_Connection;
    }

    public struct ClientNetworkComponent : ISharedComponentData, IEquatable<ClientNetworkComponent>
    {
        public ClientNetworkManager networkManager;

        public bool Equals(ClientNetworkComponent other)
        {
            return Equals(networkManager, other.networkManager);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientNetworkComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (networkManager != null ? networkManager.GetHashCode() : 0);
        }
    }
    
    public class ClientBehaviour : MonoBehaviour
    {
        [NonSerialized]
        public ClientNetworkManager clientNetworkManager = new ClientNetworkManager();
        
        public bool m_Done;

        public GameObject clientObjectPrefab;

        private GameObject[] clientObjects = new GameObject[20];
    
        private bool clientObjectCreated;
        private uint clientId;

        void Start ()
        {
            clientNetworkManager.m_Driver = NetworkDriver.Create();
            clientNetworkManager.m_Connection = default(NetworkConnection);

            var endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            clientNetworkManager.m_Connection = clientNetworkManager.m_Driver.Connect(endpoint);
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var managerEntity = entityManager
                .CreateEntity(ComponentType.ReadOnly<ClientOnly>(), 
                    ComponentType.ReadWrite<ClientNetworkComponent>());
            
            entityManager.SetSharedComponentData(managerEntity, new ClientNetworkComponent
            {
                networkManager = clientNetworkManager
            });
        }

        public void OnDestroy()
        {
            clientNetworkManager.m_Driver.Dispose();
        }

        void Update()
        {
            var m_Driver = clientNetworkManager.m_Driver;
            var m_Connection = clientNetworkManager.m_Connection;
            
            m_Driver.ScheduleUpdate().Complete();

            if (!m_Connection.IsCreated)
            {
                if (!m_Done)
                    Debug.Log("Something went wrong during connect");
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            // var connecting = false;

            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server");

                    var packet = new GamePacket
                    {
                        type = 1,
                        direction = new float2(0, 0)
                    };
                
                    // uint value = 1;

                    var writer = m_Driver.BeginSend(m_Connection);

                    // new GamePacket
                    // {
                    //     type = GamePacket.CONNECT_COMMAND
                    // }.Write(writer);
                
                    packet.Write(ref writer);
                
                    // writer.WriteUInt(packet.type);
                    // writer.WriteFloat(packet.direction.x);
                    // writer.WriteFloat(packet.direction.y);

                    m_Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var packet = new GamePacket().Read(stream);
                
                    if (packet.type == GamePacket.SERVER_CONNECTION_COMPLETED)
                    {
                        clientId = packet.networkPlayerId;
                        clientObjectCreated = true;
                    
                        Debug.Log("Got clientid from server");

                        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                        var query = entityManager.CreateEntityQuery(
                            ComponentType.ReadOnly<ClientOnly>(),
                            ComponentType.ReadWrite<ClientNetworkComponent>()
                        );
                        
                        var clientManagerEntities = query.ToEntityArray(Allocator.TempJob);
                        // query.to<ClientNetworkComponent>(Allocator.Temp);
                        foreach (var clientManagerEntity in clientManagerEntities)
                        {
                            var networkComponent = entityManager.GetSharedComponentData<ClientNetworkComponent>(clientManagerEntity);
                            if (networkComponent.networkManager == clientNetworkManager)
                            {
                                entityManager.AddComponentData(clientManagerEntity, new NetworkPlayerId
                                {
                                    player = packet.networkPlayerId
                                });
                            }
                        }

                        clientManagerEntities.Dispose();

                    }
                    else if (packet.type == GamePacket.SERVER_GAMESTATE_UPDATE)
                    {
                        if (clientObjects[packet.networkPlayerId] == null)
                        {
                            clientObjects[packet.networkPlayerId] = Instantiate(clientObjectPrefab, transform);
                            clientObjects[packet.networkPlayerId].SetActive(true);
                        }

                        var clientObject = clientObjects[packet.networkPlayerId];
                        clientObject.transform.position = new Vector3(packet.mainObjectPosition.x, 
                            packet.mainObjectPosition.y, 0);
                    }
                
                    // m_Done = true;
                    // m_Connection.Disconnect(m_Driver);
                    // m_Connection = default(NetworkConnection);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    m_Connection = default(NetworkConnection);
                }
            }

            if (m_Connection.IsCreated && clientObjectCreated)
            {
                var move = false;
                var moveVector = float2.zero;
            
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    move = true;
                    moveVector.x = -1;
                }
            
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    move = true;
                    moveVector.x = 1;
                }

                if (move)
                {
                    var writer = m_Driver.BeginSend(m_Connection);
           
                    new GamePacket
                    {
                        type = GamePacket.CLIENT_MOVE_COMMAND,
                        networkPlayerId = clientId,
                        direction = moveVector
                    }.Write(ref writer);
                
                    m_Driver.EndSend(writer);
                }
            }

            // if (m_Connection.IsCreated)
            // {
            //     var writer = m_Driver.BeginSend(m_Connection);
            //
            //     new GamePacket
            //     {
            //         type = GamePacket.CLIENT_KEEP_ALIVE
            //     }.Write(ref writer);
            //
            //     m_Driver.EndSend(writer);
            // }
        }
    }
}