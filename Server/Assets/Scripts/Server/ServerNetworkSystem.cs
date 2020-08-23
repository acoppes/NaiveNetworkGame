using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

namespace Server
{
    public class NetworkManager
    {
        public NetworkDriver m_Driver;
        public NativeList<NetworkConnection> m_Connections;
    }

    public struct NetworkManagerSharedComponent : ISharedComponentData, IEquatable<NetworkManagerSharedComponent>
    {
        public NetworkManager networkManager;

        public bool Equals(NetworkManagerSharedComponent other)
        {
            return Equals(networkManager, other.networkManager);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkManagerSharedComponent other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (networkManager != null ? networkManager.GetHashCode() : 0);
        }
    }

    public struct ServerStartComponent : IComponentData
    {
        
    }

    public struct ServerRunningComponent : IComponentData
    {
        
    }

    public struct PlayerConnectionId : IComponentData
    {
        public NetworkConnection connection;
        public int player;
        public bool synchronized;
    }
    
    public class ServerNetworkSystem : ComponentSystem
    {
        private int currentConnectionPlayer;

        protected override void OnCreate()
        {
            base.OnCreate();
            currentConnectionPlayer = 0;
        }

        protected override void OnUpdate()
        {
            // create server
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerStartComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent networkManager, ref ServerStartComponent s)
                {
                    Debug.Log("Starting Server");
                    
                    PostUpdateCommands.RemoveComponent<ServerStartComponent>(e);

                    networkManager.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(),
                        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
                    };

                    var endpoint = NetworkEndPoint.AnyIpv4;
                    endpoint.Port = 9000;
                    
                    if (networkManager.networkManager.m_Driver.Bind(endpoint) != 0)
                        Debug.Log("Failed to bind to port 9000");
                    else
                        networkManager.networkManager.m_Driver.Listen();

                    PostUpdateCommands.SetSharedComponent(e, networkManager);
                    PostUpdateCommands.AddComponent(e, new ServerRunningComponent());
                });
            
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;

                    var m_Driver = networkManager.m_Driver;
                    
                    m_Driver.ScheduleUpdate().Complete();
                    
                    Assert.IsTrue(m_Driver.IsCreated);
                    Assert.IsTrue(m_Driver.Listening);
                    
                    // CleanUpConnections
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        if (!networkManager.m_Connections[i].IsCreated)
                        {
                            networkManager.m_Connections.RemoveAtSwapBack(i);
                            --i;
                        }
                    }
                    
                    // process pending connections....
                    NetworkConnection c;
                    while ((c = m_Driver.Accept()) != default(NetworkConnection))
                    {
                        networkManager.m_Connections.Add(c);
                        Debug.Log("Accepted a connection");
                        
                        // create a new player connected command internally

                        var playerEntity = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(playerEntity, new PlayerConnectionId
                        {
                            player = currentConnectionPlayer++,
                            connection = c,
                        });
                    }
                    
                    DataStreamReader stream;
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        Assert.IsTrue(networkManager.m_Connections[i].IsCreated);

                        NetworkEvent.Type cmd;
                        while ((cmd = m_Driver
                            .PopEventForConnection(networkManager.m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
                        {
                            if (cmd == NetworkEvent.Type.Data)
                            {
                                // process different data packets and create commands to be processed in server...      

                                var packet = stream.ReadUInt();

                                if (packet == 99)
                                {
                                    var pendingPlayerAction = new PendingPlayerAction
                                    {
                                        player = stream.ReadUInt(),
                                        command = stream.ReadUInt(),
                                        target = {
                                            x = stream.ReadFloat(), 
                                            y = stream.ReadFloat()
                                        }
                                    };


                                    var pendingActionEntity = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent<ServerOnly>(pendingActionEntity);
                                    PostUpdateCommands.AddComponent(pendingActionEntity, pendingPlayerAction);
                                }

                            }
                            else if (cmd == NetworkEvent.Type.Disconnect)
                            {
                                // do something to player stuff? 
                                
                                Debug.Log("Client disconnected from server");
                                networkManager.m_Connections[i] = default(NetworkConnection);
                            }
                        }
                    }
                    
                });
        }

        protected override void OnDestroy()
        {
            Entities
                .WithNone<ClientOnly>()
                .WithAll<NetworkManagerSharedComponent>()
                .ForEach(delegate(NetworkManagerSharedComponent networkManager)
                {
                    var manager = networkManager.networkManager;

                    if (manager == null) 
                        return;
                    
                    manager.m_Connections.Dispose();
                    manager.m_Driver.Dispose();
                });
            
            
            base.OnDestroy();
        }
    }

    public class ServerSendGameStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;
                    var m_Driver = networkManager.m_Driver;
                    
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        var connection = networkManager.m_Connections[i];
                        
                        Assert.IsTrue(connection.IsCreated);

                        Entities.ForEach(delegate(ref PlayerConnectionId p)
                        {
                            if (p.synchronized)
                                return;
                            
                            // Send player id to player given a connection
                            if (p.connection == connection)
                            {
                                var writer = m_Driver.BeginSend(connection);
                                writer.WriteUInt(0);
                                writer.WriteUInt((uint)p.player);
                                m_Driver.EndSend(writer);

                                p.synchronized = true;
                            }
                        });
                    }
                });
            
            var unitsQuery = EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<Unit>(),
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<UnitState>(),
                ComponentType.ReadWrite<LookingDirection>()
                );

            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            var translations = unitsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            //LookingDirection
            var lookingDirections = unitsQuery.ToComponentDataArray<LookingDirection>(Allocator.TempJob);

            var states = unitsQuery.ToComponentDataArray<UnitState>(Allocator.TempJob);
            
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;

                    var m_Driver = networkManager.m_Driver;
                    
                    DataStreamReader stream;
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        Assert.IsTrue(networkManager.m_Connections[i].IsCreated);

                        // send game state....   
                        for (var j = 0; j < units.Length; j++)
                        {
                            // for now, I will send different packets for each unit, for each connection
                            var writer = m_Driver.BeginSend(networkManager.m_Connections[i]);
                            writer.WriteUInt(50);
                            writer.WriteUInt(units[j].id);
                            writer.WriteUInt(units[j].player);
                            writer.WriteFloat(translations[j].Value.x);
                            writer.WriteFloat(translations[j].Value.y);
                            writer.WriteFloat(lookingDirections[j].direction.x);
                            writer.WriteFloat(lookingDirections[j].direction.y);
                            writer.WriteInt(states[j].state);
                            m_Driver.EndSend(writer);
                        }

                    }
                });

            lookingDirections.Dispose();
            units.Dispose();
            translations.Dispose();
            states.Dispose();
        }
    }
}