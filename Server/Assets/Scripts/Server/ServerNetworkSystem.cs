using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Server
{
    // public static class ServerNetworkSettings {
    //     public static float 
    // }
    
    public static class ServerNetworkStatistics
    {
        public static int outputBytesTotal;
        public static int outputBytesLastFrame;
        public static int currentConnections;
    }
    
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
        public ushort port;
    }

    public struct ServerRunningComponent : IComponentData
    {
        
    }

    public struct PlayerConnectionId : IComponentData
    {
        public NetworkConnection connection;
        public int player;
        public bool synchronized;
        public bool initialized;
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
                    PostUpdateCommands.RemoveComponent<ServerStartComponent>(e);

                    networkManager.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(),
                        // m_Driver = NetworkDriver.Create(new SimulatorUtility.Parameters
                        // {
                        //     MaxPacketSize = NetworkParameterConstants.MTU, MaxPacketCount = 30, PacketDelayMs = 100, PacketDropPercentage = 10
                        // }),
                        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
                    };
                    
                    var m_Pipeline = networkManager.networkManager.m_Driver.CreatePipeline(
                        typeof(SimulatorPipelineStage));

                    var endpoint = NetworkEndPoint.AnyIpv4.WithPort(s.port);
                    
                    // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);
                    // endpoint.Port = 9000;
                    
                    Debug.Log($"Starting Server at port: {s.port}");
                    
                    if (networkManager.networkManager.m_Driver.Bind(endpoint) != 0)
                        Debug.Log($"Failed to bind to port {s.port}");
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
                        
                        // create a new player connected command internally

                        var playerEntity = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(playerEntity, new PlayerConnectionId
                        {
                            player = currentConnectionPlayer++,
                            connection = c,
                        });
                        
                        Debug.Log($"Accepted connection from: {networkManager.m_Driver.RemoteEndPoint(c).Address}");
                    }
                    
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        Assert.IsTrue(networkManager.m_Connections[i].IsCreated);

                        NetworkEvent.Type cmd;
                        while ((cmd = m_Driver
                            .PopEventForConnection(networkManager.m_Connections[i], out var stream)) != NetworkEvent.Type.Empty)
                        {
                            if (cmd == NetworkEvent.Type.Data)
                            {
                                // process different data packets and create commands to be processed in server...      

                                var packet = stream.ReadByte();

                                if (packet == 99)
                                {
                                    var pendingPlayerAction = new PendingPlayerAction
                                    {
                                        player = stream.ReadByte(),
                                        command = stream.ReadByte(),
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

    public class ServerCreatePlayerController : ComponentSystem
    {
        private uint lastUnitId;

        protected override void OnCreate()
        {
            base.OnCreate();
            lastUnitId = 0;
        }

        protected override void OnUpdate()
        {
            var playerControllerPrefabEntity = 
                Entities.WithAll<PlayerControllerSharedComponent>().ToEntityQuery().GetSingletonEntity();

            var playerControllerPrefab = 
                EntityManager.GetSharedComponentData<PlayerControllerSharedComponent>(playerControllerPrefabEntity);
            
            Entities.ForEach(delegate(ref PlayerConnectionId p)
            {
                if (p.initialized)
                    return;
                
                var playerControllerEntity = PostUpdateCommands.Instantiate(playerControllerPrefab.prefab);
                PostUpdateCommands.SetComponent(playerControllerEntity, new Unit
                {
                    id = lastUnitId++,
                    player = (uint) p.player
                });
                PostUpdateCommands.AddComponent(playerControllerEntity, new NetworkGameState
                {
                    syncVersion = -1
                });
                
                p.initialized = true;
            });
        }
    }

    public class ServerSendGameStateSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            ServerNetworkStatistics.outputBytesLastFrame = 0;
            ServerNetworkStatistics.currentConnections = 0;
            
            Entities
                .WithAll<ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;
                    var m_Driver = networkManager.m_Driver;
                    
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        var connection = networkManager.m_Connections[i];

                        if (!connection.IsCreated)
                        {
                            // should we destroy player controller?
                            // how to send that with gamestate, maybe mark as destroyed...
                            continue;
                        }

                        ServerNetworkStatistics.currentConnections++;
                        
                        Entities.ForEach(delegate(ref PlayerConnectionId p)
                        {
                            if (p.synchronized)
                                return;
                            
                            // Send player id to player given a connection
                            if (p.connection == connection)
                            {
                                var writer = m_Driver.BeginSend(connection);
                                writer.WriteByte(0);
                                writer.WriteByte((byte) p.player);
                                m_Driver.EndSend(writer);

                                ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;

                                p.synchronized = true;
                            }
                        });
                    }
                });
            
            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;

                    var m_Driver = networkManager.m_Driver;
                    
                    // DataStreamReader stream;
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        var connection = networkManager.m_Connections[i];
                        
                        if (!connection.IsCreated)
                        {
                            // should we destroy player controller?
                            // how to send that with gamestate, maybe mark as destroyed...
                            continue;
                        }

                        Entities.WithAll<NetworkGameState>().ForEach(delegate(ref NetworkGameState n)
                        {
                            // if (n.version == n.syncVersion)
                            //     return;
                            
                            var writer = m_Driver.BeginSend(connection);
                            writer.WriteByte(50);
                            writer.WriteInt(n.frame);
                            writer.WriteFloat(n.delta);
                            writer.WriteUInt((uint) n.unitId);
                            writer.WriteByte((byte) n.playerId);
                            writer.WriteFloat(n.translation.x);
                            writer.WriteFloat(n.translation.y);
                            writer.WriteFloat(n.lookingDirection.x);
                            writer.WriteFloat(n.lookingDirection.y);
                            writer.WriteByte((byte) n.state);
                            m_Driver.EndSend(writer);
                            
                            ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                            ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                            
                            // Debug.Log($"State length: {writer.Length}");
                            // Debug.Log($"State length (bits): {writer.LengthInBits}");
                            // Debug.Log($"State length (bytes): {writer.LengthInBits / 8}");

                            n.syncVersion = n.version;
                        });
                    }
                });
        }
    }
}