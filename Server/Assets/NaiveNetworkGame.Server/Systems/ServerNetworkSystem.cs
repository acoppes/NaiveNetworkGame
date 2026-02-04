using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace NaiveNetworkGame.Server.Systems
{
    public static class ServerNetworkStaticData
    {
        public static float sendTranslationStateFrequency = 0.1f;
        public static float sendGameStateFrequency = 0.1f;
    }
    
    public static class ServerNetworkStatistics
    {
        public static int outputBytesTotal;
        public static int outputBytesLastFrame;
        public static int currentConnections;
    }

    public struct StartServerCommand : IComponentData
    {
        public ushort port;
        public byte playersNeededToStartSimulation;
    }
    
    public struct StopServerCommand : IComponentData
    {
        public bool restart;
    }

    public class ServerNetworkSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireSingletonForUpdate<ServerSingleton>();
            
            // now server network system is in charge of creating server singleton...
            var serverEntity = EntityManager.CreateEntity();
            #if UNITY_EDITOR
            EntityManager.SetName(serverEntity, "ServerSingleton");
            #endif
            EntityManager.AddSharedComponentData(serverEntity, new ServerSingleton());
        }

        protected override void OnUpdate()
        {
            var serverEntity = GetSingletonEntity<ServerSingleton>();
            var server =
                EntityManager.GetSharedComponentData<ServerSingleton>(serverEntity);
            
            Entities
                .ForEach(delegate(Entity e, ref StopServerCommand stop)
                {
                    PostUpdateCommands.DestroyEntity(e);

                    if (!server.started)
                        return;

                    if (server.networkManager != null)
                    {
                        for (var i = 0; i < server.networkManager.m_Connections.Length; i++)
                        {
                            var c = server.networkManager.m_Connections[i];
                            if (c.IsCreated && server.networkManager.m_Driver.GetConnectionState(c) ==
                                NetworkConnection.State.Connected)
                            {
                                server.networkManager.m_Driver.Disconnect(c);
                            }
                        }
                        
                        server.networkManager.m_Connections.Dispose();
                        server.networkManager.m_Driver.Dispose();
                    }

                    server.networkManager = null;
                    server.started = false;
                    
                    PostUpdateCommands.SetSharedComponent(serverEntity, server);

                    PostUpdateCommands.DestroyEntity(Entities.WithNone<ServerSingleton>().ToEntityQuery());
                    PostUpdateCommands.DestroyEntity(GetSingletonEntity<ServerSimulation>());
                    PostUpdateCommands.DestroyEntity(Entities.WithAll<Prefab>().ToEntityQuery());
                    
                    if (stop.restart)
                    {
                        SceneManager.LoadScene("ServerScene");
                        
                        var restart = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(restart, new StartServerCommand
                        {
                            port = server.port,
                            playersNeededToStartSimulation = server.playersNeededToStartSimulation
                        });
                    }
                });
            
              
            Entities
                .ForEach(delegate(Entity e, ref StartServerCommand start)
                {
                    PostUpdateCommands.DestroyEntity(e);

                    if (server.started)
                        return;
                    
                    // m_ServerDriver = NetworkDriver.Create(new ReliableUtility.Parameters { WindowSize = 32 });
                    // m_Pipeline = m_ServerDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

                    server.started = true;
                    server.port = start.port;
                    server.playersNeededToStartSimulation = start.playersNeededToStartSimulation;
                    
                    server.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(
                            new NetworkDataStreamParameter { size = 0 },
                            new FragmentationUtility.Parameters
                            {
                                PayloadCapacity = 16 * 1024
                            }, 
                            new ReliableUtility.Parameters
                            {
                                WindowSize = 32
                            }),
                        
                        // m_Driver = NetworkDriver.Create(new SimulatorUtility.Parameters
                        // {
                        //     MaxPacketSize = NetworkParameterConstants.MTU, MaxPacketCount = 30, PacketDelayMs = 100, PacketDropPercentage = 10
                        // }),
                        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
                    };

                    // var m_Pipeline = networkManager.networkManager.m_Driver.CreatePipeline(
                    //     typeof(SimulatorPipelineStage));

                    server.framentationPipeline = 
                        server.networkManager.m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));
                    server.reliabilityPipeline = 
                        server.networkManager.m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

                    var endpoint = NetworkEndPoint.AnyIpv4.WithPort(start.port);
                    
                    // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);
                    // endpoint.Port = 9000;
                    
                    Debug.Log($"Starting Server at port: {start.port}");
                    
                    if (server.networkManager.m_Driver.Bind(endpoint) != 0)
                        Debug.Log($"Failed to bind to port {start.port}");
                    else
                        server.networkManager.m_Driver.Listen();

                    PostUpdateCommands.SetSharedComponent(serverEntity, server);
                });

            // create server

            var networkManager = server.networkManager;

            if (networkManager == null)
                return;

            var m_Driver = networkManager.m_Driver;

            if (!m_Driver.IsCreated)
                return;
            
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
            
            Entities
                .WithNone<PlayerConnectionId>()
                .WithAll<PlayerController>()
                .ForEach(delegate(Entity e, ref PlayerController p)
                {   
                    // for each player without connection, we try to accept a connection

                    NetworkConnection c;
                    if ((c = m_Driver.Accept()) != default)
                    {
                        // if there is a connection available, then assign it
                        networkManager.m_Connections.Add(c);
                        Debug.Log($"Accepted connection from: {networkManager.m_Driver.RemoteEndPoint(c).Address}");
                        
                        PostUpdateCommands.AddComponent(e, new PlayerConnectionId
                        {
                            // player = p.player,
                            connection = c,
                        });
                        PostUpdateCommands.AddComponent(e, new NetworkPlayerState());                        
                    }

                });
            
            // TODO: if needed players reached => create server simulation singleton in order to start the simulation..
            
            // If there are still connections, accept them and send denied because max players...
            NetworkConnection c;
            while ((c = m_Driver.Accept()) != default)
            {
                var result = m_Driver.BeginSend(c, out var writer);
                // var writer = m_Driver.BeginSend(c);
                writer.WriteByte(PacketType.ServerDeniedConnectionMaxPlayers);
                m_Driver.EndSend(writer);
                    
                Debug.Log($"Denied connection from: {networkManager.m_Driver.RemoteEndPoint(c).Address}");
                    
                m_Driver.Disconnect(c);
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
                            // TODO: Ignore player actions while simulation not started 
                            
                            var action = new PendingPlayerAction().Read(ref stream);
                            
                            Entities
                                .WithNone<PendingPlayerAction>()
                                .ForEach(delegate(Entity playerEntity, ref PlayerController p)
                            {
                                if (p.player == action.player)
                                {
                                    PostUpdateCommands.AddComponent(playerEntity, action);
                                }
                            });

                            // var pendingActionEntity = PostUpdateCommands.CreateEntity();
                            // // PostUpdateCommands.AddComponent<ServerOnly>(pendingActionEntity);
                            // PostUpdateCommands.AddComponent(pendingActionEntity, pendingPlayerAction);
                        }

                        if (packet == PacketType.ClientDisconnect)
                        {
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
                                        PostUpdateCommands.RemoveComponent<PlayerConnectionSynchronized>(e);
                                        
                                        if (HasSingleton<ServerSimulation>())
                                        {
                                            // If client disconnected manually and simulation already started, then restart server...
                                            PostUpdateCommands.AddComponent(PostUpdateCommands.CreateEntity(), new StopServerCommand
                                            {
                                                restart = true
                                            });
                                        }
                                    }
                                });
                        
                            Debug.Log("Client disconnected from server");
                            // networkManager.m_Connections[i] = default;
                        }

                        if (packet == PacketType.ClientKeepAlive)
                        {
                            var packetIndex = stream.ReadByte();
                            
                            // var writer = m_Driver.BeginSend(networkManager.m_Connections[i]);
                            m_Driver.BeginSend(networkManager.m_Connections[i], out var writer);
                            
                            writer.WriteByte(PacketType.ClientKeepAlive);
                            writer.WriteByte(packetIndex);
                            m_Driver.EndSend(writer);
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
                                PostUpdateCommands.RemoveComponent<PlayerConnectionSynchronized>(e);
                                
                                if (HasSingleton<ServerSimulation>())
                                {
                                    // If client disconnected by timeout, and simulation already started, then restart server...
                                    PostUpdateCommands.AddComponent(PostUpdateCommands.CreateEntity(), new StopServerCommand
                                    {
                                        restart = true
                                    });
                                }
                            }
                        });
                        
                        Debug.Log("Client disconnected from server");
                        networkManager.m_Connections[i] = default;
                    }
                }
            }

            if (!HasSingleton<ServerSimulation>())
            {
                var players = Entities.WithAll<PlayerConnectionId>().ToEntityQuery().CalculateEntityCount();
                if (server.playersNeededToStartSimulation == players)
                {
                    // start simulation if reached needed players
                    EntityManager.CreateEntity(typeof(ServerSimulation));
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