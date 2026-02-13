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

    public partial struct ServerNetworkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ServerSingleton>();
            
            // now server network system is in charge of creating server singleton...
            var serverEntity = state.EntityManager.CreateEntity();
            #if UNITY_EDITOR
            state.EntityManager.SetName(serverEntity, "ServerSingleton");
            #endif
            state.EntityManager.AddComponentData(serverEntity, new ServerSingleton());
            state.EntityManager.AddSharedComponentManaged(serverEntity, new ServerData());
        }

        public void OnUpdate(ref SystemState state)
        {
            var serverEntity = SystemAPI.GetSingletonEntity<ServerSingleton>();
            var serverData = state.EntityManager.GetSharedComponentManaged<ServerData>(serverEntity);
            
            foreach (var (stopCommand, entity) in 
                SystemAPI.Query<RefRO<StopServerCommand>>()
                    .WithEntityAccess())
            {
                state.EntityManager.DestroyEntity(entity);

                if (!serverData.started)
                    continue;

                if (serverData.networkManager != null)
                {
                    for (var i = 0; i < serverData.networkManager.m_Connections.Length; i++)
                    {
                        var c = serverData.networkManager.m_Connections[i];
                        if (c.IsCreated && serverData.networkManager.m_Driver.GetConnectionState(c) ==
                            NetworkConnection.State.Connected)
                        {
                            serverData.networkManager.m_Driver.Disconnect(c);
                        }
                    }
                    
                    serverData.networkManager.m_Connections.Dispose();
                    serverData.networkManager.m_Driver.Dispose();
                }

                serverData.networkManager = null;
                serverData.started = false;
                
                state.EntityManager.SetSharedComponentManaged(serverEntity, serverData);

                var nonServerDataQuery = SystemAPI.QueryBuilder().WithNone<ServerData>().Build();
                state.EntityManager.DestroyEntity(nonServerDataQuery.ToEntityArray(Allocator.Temp));
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<ServerSimulation>());
                
                var prefabQuery = SystemAPI.QueryBuilder().WithAll<Prefab>().Build();
                state.EntityManager.DestroyEntity(prefabQuery.ToEntityArray(Allocator.Temp));
                
                if (stopCommand.ValueRO.restart)
                {
                    SceneManager.LoadScene("ServerScene");
                    
                    var restart = state.EntityManager.CreateEntity();
                    state.EntityManager.AddComponentData(restart, new StartServerCommand
                    {
                        port = serverData.port,
                        playersNeededToStartSimulation = serverData.playersNeededToStartSimulation
                    });
                }
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
              
            foreach (var (startCommand, entity) in 
                SystemAPI.Query<RefRO<StartServerCommand>>()
                    .WithEntityAccess())
            {
                ecb.DestroyEntity(entity);

                if (serverData.started)
                    continue;
                
                // m_ServerDriver = NetworkDriver.Create(new ReliableUtility.Parameters { WindowSize = 32 });
                // m_Pipeline = m_ServerDriver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

                serverData.started = true;
                serverData.port = startCommand.ValueRO.port;
                serverData.playersNeededToStartSimulation = startCommand.ValueRO.playersNeededToStartSimulation;

                var networkSettings = new NetworkSettings(Allocator.Persistent);
                //   new NetworkDataStreamParameter { size = 0 },
                networkSettings.WithFragmentationStageParameters(payloadCapacity:16 * 1024);
                networkSettings.WithReliableStageParameters(windowSize: 32);

                serverData.networkManager = new NetworkManager
                {
                    m_Driver = NetworkDriver.Create(networkSettings),
                    // m_Driver = NetworkDriver.Create(new SimulatorUtility.Parameters
                    // {
                    //     MaxPacketSize = NetworkParameterConstants.MTU, MaxPacketCount = 30, PacketDelayMs = 100, PacketDropPercentage = 10
                    // }),
                    m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent)
                };

                // var m_Pipeline = networkManager.networkManager.m_Driver.CreatePipeline(
                //     typeof(SimulatorPipelineStage));

                serverData.fragmentationPipeline = 
                    serverData.networkManager.m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));
                serverData.reliabilityPipeline = 
                    serverData.networkManager.m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

                var endpoint = NetworkEndpoint.AnyIpv4.WithPort(startCommand.ValueRO.port);
                
                // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);
                // endpoint.Port = 9000;
                
                Debug.Log($"Starting Server at port: {startCommand.ValueRO.port}");
                
                if (serverData.networkManager.m_Driver.Bind(endpoint) != 0)
                    Debug.Log($"Failed to bind to port {startCommand.ValueRO.port}");
                else
                    serverData.networkManager.m_Driver.Listen();

                ecb.SetSharedComponentManaged(serverEntity, serverData);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            // create server

            var networkManager = serverData.networkManager;

            if (networkManager == null)
                return;

            ecb = new EntityCommandBuffer(Allocator.Temp);
            
            var m_Driver = networkManager.m_Driver;

            m_Driver.ScheduleUpdate().Complete();
            
            ServerNetworkStatistics.currentConnections = 0;
            
            foreach (var (_, entity) in 
                SystemAPI.Query<PlayerController>()
                    .WithNone<PlayerConnectionId>()
                    .WithAll<ServerOnly>()
                    .WithEntityAccess())
            {
                ServerNetworkStatistics.currentConnections++;

                NetworkConnection c1;
                
                if ((c1 = m_Driver.Accept()) != default)
                {
                    // if there is a connection available, then assign it
                    networkManager.m_Connections.Add(c1);
                    Debug.Log($"Accepted connection from: {networkManager.m_Driver.GetRemoteEndpoint(c1).Address}");
                    
                    ecb.AddComponent(entity, new PlayerConnectionId
                    {
                        // player = p.player,
                        connection = c1,
                    });
                    ecb.AddComponent(entity, new NetworkPlayerState());                        
                }

            }
            
     
            
            // TODO: if needed players reached => create server simulation singleton in order to start the simulation..
            
            // If there are still connections, accept them and send denied because max players...
            NetworkConnection c2;
            while ((c2 = m_Driver.Accept()) != default)
            {
                var result = m_Driver.BeginSend(c2, out var writer);
                // var writer = m_Driver.BeginSend(c);
                writer.WriteByte(PacketType.ServerDeniedConnectionMaxPlayers);
                m_Driver.EndSend(writer);
                    
                Debug.Log($"Denied connection from: {networkManager.m_Driver.GetRemoteEndpoint(c2).Address}");
                    
                m_Driver.Disconnect(c2);
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
                            
                            foreach (var (playerController, playerEntity) in 
                                SystemAPI.Query<RefRO<PlayerController>>()
                                    .WithNone<PendingPlayerAction>()
                                    .WithEntityAccess())
                            {
                                if (playerController.ValueRO.player == action.player)
                                {
                                    ecb.AddComponent(playerEntity, action);
                                }
                            }

                            // var pendingActionEntity = state.EntityManager.CreateEntity();
                            // // state.EntityManager.AddComponent<ServerOnly>(pendingActionEntity);
                            // state.EntityManager.AddComponent(pendingActionEntity, pendingPlayerAction);
                        }

                        if (packet == PacketType.ClientDisconnect)
                        {
                            var connection = networkManager.m_Connections[i];
                            foreach (var (playerConnectionId, entity) in 
                                SystemAPI.Query<RefRO<PlayerConnectionId>>()
                                    .WithAll<PlayerController, NetworkPlayerState>()
                                    .WithEntityAccess())
                            {
                                if (playerConnectionId.ValueRO.connection == connection)
                                {
                                    // p.destroyed = true;
                                    ecb.RemoveComponent<PlayerConnectionId>(entity);
                                    ecb.RemoveComponent<NetworkPlayerState>(entity);
                                    ecb.RemoveComponent<PlayerConnectionSynchronized>(entity);
                                    
                                    if (SystemAPI.HasSingleton<ServerSimulation>())
                                    {
                                        // If client disconnected manually and simulation already started, then restart server...
                                        var stopEntity = state.EntityManager.CreateEntity();
                                        ecb.AddComponent(stopEntity, new StopServerCommand
                                        {
                                            restart = true
                                        });
                                    }
                                }
                            }
                        
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
                        foreach (var (playerConnectionId, entity) in 
                            SystemAPI.Query<RefRO<PlayerConnectionId>>()
                                .WithAll<PlayerController, NetworkPlayerState>()
                                .WithEntityAccess())
                        {
                            if (playerConnectionId.ValueRO.connection == connection)
                            {
                                // p.destroyed = true;
                                ecb.RemoveComponent<PlayerConnectionId>(entity);
                                ecb.RemoveComponent<NetworkPlayerState>(entity);
                                ecb.RemoveComponent<PlayerConnectionSynchronized>(entity);
                                
                                if (SystemAPI.HasSingleton<ServerSimulation>())
                                {
                                    // If client disconnected by timeout, and simulation already started, then restart server...
                                    var stopEntity = ecb.CreateEntity();
                                    ecb.AddComponent(stopEntity, new StopServerCommand
                                    {
                                        restart = true
                                    });
                                }
                            }
                        }
                        
                        Debug.Log("Client disconnected from server");
                        networkManager.m_Connections[i] = default;
                    }
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            if (!SystemAPI.HasSingleton<ServerSimulation>())
            {
                var playerConnectionQuery = SystemAPI.QueryBuilder().WithAll<PlayerConnectionId>().Build();
                var players = playerConnectionQuery.CalculateEntityCount();
                if (serverData.playersNeededToStartSimulation == players)
                {
                    // start simulation if reached needed players
                    state.EntityManager.CreateEntity(typeof(ServerSimulation));
                }
            }
            
            
        }

        public void OnDestroy(ref SystemState state)
        {
            var serverEntity = SystemAPI.GetSingletonEntity<ServerSingleton>();
            var server = state.EntityManager.GetSharedComponentManaged<ServerData>(serverEntity);
            
            var networkManager = server.networkManager;

            if (networkManager == null) 
                return;
                    
            networkManager.m_Connections.Dispose();
            networkManager.m_Driver.Dispose();
        }
    }
}
