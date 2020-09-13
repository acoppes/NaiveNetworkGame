using System;
using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Server
{
    public static class ServerNetworkStaticData
    {
        public static bool synchronizeStaticObjects;
        public static float sendGameStateFrequency = 0.1f;

        public static bool staticObjectsEnabled;

        public static readonly byte totalPlayers = 2;
    }
    
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

    public struct ServerSingleton : ISharedComponentData, IEquatable<ServerSingleton>
    {
        public NetworkManager networkManager;

        public bool Equals(ServerSingleton other)
        {
            return Equals(networkManager, other.networkManager);
        }

        public override bool Equals(object obj)
        {
            return obj is ServerSingleton other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (networkManager != null ? networkManager.GetHashCode() : 0);
        }
    }

    public struct StartServerCommand : IComponentData
    {
        public ushort port;
    }

    public class ServerNetworkSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireSingletonForUpdate<ServerSingleton>();
            
            // now server network system is in charge of creating server singleton...
            var serverEntity = EntityManager.CreateEntity();
            EntityManager.SetName(serverEntity, "ServerSingleton");
            EntityManager.AddSharedComponentData(serverEntity, new ServerSingleton());
        }

        protected override void OnUpdate()
        {
            var serverEntity = GetSingletonEntity<ServerSingleton>();
            var server =
                EntityManager.GetSharedComponentData<ServerSingleton>(serverEntity);
            
            // create server
            Entities
                .ForEach(delegate(Entity e, ref StartServerCommand s)
                {
                    server.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(),
                        // m_Driver = NetworkDriver.Create(new SimulatorUtility.Parameters
                        // {
                        //     MaxPacketSize = NetworkParameterConstants.MTU, MaxPacketCount = 30, PacketDelayMs = 100, PacketDropPercentage = 10
                        // }),
                        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
                    };
                    
                    // var m_Pipeline = networkManager.networkManager.m_Driver.CreatePipeline(
                    //     typeof(SimulatorPipelineStage));

                    var endpoint = NetworkEndPoint.AnyIpv4.WithPort(s.port);
                    
                    // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);
                    // endpoint.Port = 9000;
                    
                    Debug.Log($"Starting Server at port: {s.port}");
                    
                    if (server.networkManager.m_Driver.Bind(endpoint) != 0)
                        Debug.Log($"Failed to bind to port {s.port}");
                    else
                        server.networkManager.m_Driver.Listen();

                    PostUpdateCommands.SetSharedComponent(serverEntity, server);
                    PostUpdateCommands.DestroyEntity(e);
                });
            
            var networkManager = server.networkManager;

            if (networkManager == null)
                return;

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
                // if we are at maximum players
                // send disconnected or something...

                var connectionProcessed = false;
                
                Entities
                        .WithNone<PlayerConnectionId>()
                        .WithAll<PlayerController>()
                        .ForEach(delegate(Entity e, ref PlayerController p)
                        {
                            if (connectionProcessed)
                                return;
                            
                            PostUpdateCommands.AddComponent(e, new PlayerConnectionId
                            {
                                // player = p.player,
                                connection = c,
                            });
                            PostUpdateCommands.AddComponent(e, new NetworkPlayerState());

                            connectionProcessed = true;
                        });

                if (connectionProcessed)
                {
                    // otherwise, assign player and continue...
                    networkManager.m_Connections.Add(c);
                    Debug.Log($"Accepted connection from: {networkManager.m_Driver.RemoteEndPoint(c).Address}");
                }
                else
                {
                    // send disconnect... 
                    
                    Debug.Log($"Denied connection from: {networkManager.m_Driver.RemoteEndPoint(c).Address}");
                }
                    
                // create a new player connected command internally
                
                // find created player controller and assign connection id?

                // var playerEntity = PostUpdateCommands.CreateEntity();
                // PostUpdateCommands.AddComponent(playerEntity, new PlayerConnectionId
                // {
                //     player = currentConnectionPlayer++,
                //     connection = c,
                // });
                
                
                ServerNetworkStaticData.synchronizeStaticObjects = true;
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

                        if (packet == PacketType.ClientPlayerAction)
                        {
                            var pendingPlayerAction = new ClientPlayerAction().Read(ref stream);

                            var pendingActionEntity = PostUpdateCommands.CreateEntity();
                            // PostUpdateCommands.AddComponent<ServerOnly>(pendingActionEntity);
                            PostUpdateCommands.AddComponent(pendingActionEntity, pendingPlayerAction);
                        }

                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        // do something to player stuff? 

                        var connection = networkManager.m_Connections[i];
                        Entities
                            .WithAll<PlayerController, PlayerConnectionId, NetworkPlayerState>()
                            .ForEach(delegate(Entity e, ref PlayerConnectionId p)
                        {
                            if (p.connection == connection)
                            {
                                // p.destroyed = true;
                                PostUpdateCommands.RemoveComponent<PlayerConnectionId>(e);
                                PostUpdateCommands.RemoveComponent<NetworkPlayerState>(e);
                            }
                        });
                        
                        Debug.Log("Client disconnected from server");
                        networkManager.m_Connections[i] = default(NetworkConnection);
                        
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            var serverEntity = GetSingletonEntity<ServerSingleton>();
            var server =
                EntityManager.GetSharedComponentData<ServerSingleton>(serverEntity);
            
            var networkManager = server.networkManager;

            if (networkManager == null) 
                return;
                    
            networkManager.m_Connections.Dispose();
            networkManager.m_Driver.Dispose();

            base.OnDestroy();
        }
    }
}