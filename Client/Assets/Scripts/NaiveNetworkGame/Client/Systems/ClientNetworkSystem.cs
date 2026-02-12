using Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace NaiveNetworkGame.Client.Systems
{
    public static class ConnectionSettings
    {
        public static float latencyKeepAliveFrequency = 1;
    }

    public struct ServerConnectionParameters
    {
        public string ip;
        public ushort port;
    }

    public struct StartClientCommand : IComponentData
    {
        
    }
    
    public struct DisconnectClientCommand : IComponentData
    {
        
    }
    
    public partial struct ClientNetworkSystem : ISystem
    {
        private float lastLatencyUpdate;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClientSingleton>();
            
            // now server network system is in charge of creating server singleton...
            var clientEntity = state.EntityManager.CreateEntity();
#if UNITY_EDITOR
            state.EntityManager.SetName(clientEntity, "ClientSingleton");
#endif
            state.EntityManager.AddSharedComponentManaged(clientEntity, new ClientSingleton());
        }

        public void OnUpdate(ref SystemState state)
        {
            var clientEntity = SystemAPI.GetSingletonEntity<ClientSingleton>();
            
            var networkSettings = new NetworkSettings(Allocator.Persistent);
            //   new NetworkDataStreamParameter { size = 0 },
            networkSettings.WithFragmentationStageParameters(payloadCapacity:16 * 1024);
            networkSettings.WithReliableStageParameters(windowSize: 32);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (_, entity) in 
                SystemAPI.Query<RefRO<StartClientCommand>>()
                    .WithEntityAccess())
            {
                Debug.Log("Starting Client");
                
                // state.EntityManager.RemoveComponent<StartClientCommand>(entity);

                var driver = NetworkDriver.Create(networkSettings);
                
                var framentationPipeline = 
                    driver.CreatePipeline(typeof(FragmentationPipelineStage));
                var reliabilityPipeline = 
                    driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                
                var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(9000);

                var parametersObject = ServerConnectionParametersObject.Instance;
                var parameters = new ServerConnectionParameters();
                
                if (parametersObject)
                {
                    parameters = parametersObject.parameters;
                    GameObject.Destroy(parametersObject.gameObject);
                }
                
#if UNITY_EDITOR
                var editorUseRemoteServer =
                    UnityEditor.EditorPrefs.GetBool("Gemserk.NaiveNetworkGame.UseRemoteServerByDefault", false);

                if (string.IsNullOrEmpty(parameters.ip) && editorUseRemoteServer)
                {
                    if (editorUseRemoteServer)
                    {
                        parameters.ip = "209.151.153.172";
                        parameters.port = 9000;
                    }
                }
#endif
                
                if (!string.IsNullOrEmpty(parameters.ip))
                {
                    endpoint = NetworkEndpoint.Parse(parameters.ip, parameters.port);
                }

                // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);

                Debug.Log($"Connecting to {endpoint.Address}, {endpoint.IsValid}");
                
                // endpoint.Address = "167.57.86.221";
                // endpoint.Port = 9000;

               // networkManager.m_Driver.
                
                // var connection = client.m_Driver.Connect(endpoint);
                //
                // networkManager.m_Connections
                //     .Add(connection);
                //
                // Debug.Log($"{networkManager.m_Driver.LocalEndPoint().Address}");
                // // Debug.Log($"{networkManager.m_Driver.RemoteEndPoint(connection).Address}:{networkManager.m_Driver.RemoteEndPoint(connection).Port}");
                //
                // // client.connectionInitialized = true;
                //
                
                // client.endpoint = endpoint;

                // var clientConnectionEntity = ecb.CreateEntity();
                ecb.SetSharedComponentManaged(clientEntity, new ClientNetworkComponentData
                {
                    m_Driver = driver,
                    endpoint = endpoint,
                    framentationPipeline = framentationPipeline,
                    reliabilityPipeline = reliabilityPipeline
                });
                
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            if (!state.EntityManager.HasComponent<ClientNetworkComponentData>(clientEntity))
                return;
            
            var client = state.EntityManager.GetSharedComponentManaged<ClientNetworkComponentData>(clientEntity);

            // if (client.networkManager == null || !client.connectionInitialized)
            //     return;

            if (!client.m_Driver.IsCreated)
                return;
            
            foreach (var (connectPlayer, entity) in 
                SystemAPI.Query<RefRO<ConnectPlayerToServer>>()
                    .WithNone<NetworkPlayerId>()
                    .WithAll<LocalPlayerControllerComponentData>()
                    .WithEntityAccess())
            {
                var connection = client.m_Driver.Connect(client.endpoint);

                Debug.Log("Connecting local player to server");
                // Debug.Log($"Connecting to server: {client.m_Driver.LocalEndPoint().Address}");
                
                state.EntityManager.AddComponentData(entity, new NetworkPlayerId
                {
                    connection = connection
                });
                state.EntityManager.RemoveComponent<ConnectPlayerToServer>(entity);
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;
            
            var m_Driver = client.m_Driver;
            m_Driver.ScheduleUpdate().Complete();

            foreach (var (networkPlayer, playerController, entity) in 
                SystemAPI.Query<RefRW<NetworkPlayerId>, RefRW<LocalPlayerControllerComponentData>>()
                    .WithNone<ConnectPlayerToServer>()
                    .WithEntityAccess())
            {
                var m_Connection = networkPlayer.ValueRO.connection;
                
                networkPlayer.ValueRW.state = NetworkConnection.State.Disconnected;
                
                if (!m_Connection.IsCreated)
                {
                    // this client was disconnected or failed to connect the first time...
                    continue;
                }

                while ((cmd = m_Driver.PopEventForConnection(m_Connection, out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Connect)
                    {
                        Debug.Log("We are now connected to the server");

                        networkPlayer.ValueRW.state = NetworkConnection.State.Connected;

                        ConnectionState.currentState = ConnectionState.State.WaitingForPlayers;
                    }
                    else if (cmd == NetworkEvent.Type.Data)
                    {
                        var type = stream.ReadByte();

                        if (type == PacketType.ServerSimulationStarted)
                        {
                            ConnectionState.currentState = ConnectionState.State.SimulationRunning;
                        }

                        if (type == PacketType.ServerSendPlayerId)
                        {
                            // this is the player ID, read it and store it in local player controller
                            // from player local controller create the action pool buffer with proper entities and costs...

                            var playerId = stream.ReadByte();

                            playerController.ValueRW.player = playerId;

                            var playerActionsCount = stream.ReadByte();

                            var playerActions = state.EntityManager.GetBuffer<PlayerAction>(entity);

                            for (var i = 0; i < playerActionsCount; i++)
                            {
                                playerActions.Add(new PlayerAction
                                {
                                    type = stream.ReadByte(),
                                    cost = stream.ReadByte()
                                });
                            }
                        }

                        if (type == PacketType.ServerGameState)
                        {
                            // network game state... 
                            // read unit info...

                            var count = stream.ReadUShort();

                            for (var j = 0; j < count; j++)
                            {
                                var networkStateEntity = state.EntityManager.CreateEntity();
                                state.EntityManager.AddComponent<ClientOnly>(networkStateEntity);
                                state.EntityManager.AddComponentData(networkStateEntity, new NetworkGameState().Read(ref stream));
                            }
                        }

                        if (type == PacketType.ServerPlayerState)
                        {
                            // network game state... 
                            // read unit info...

                            var networkStateEntity = state.EntityManager.CreateEntity();
                            state.EntityManager.AddComponent<ClientOnly>(networkStateEntity);
                            state.EntityManager.AddComponentData(networkStateEntity, 
                                new NetworkPlayerState().Read(ref stream));
                        }
                        
                        if (type == PacketType.ServerEmptyGameState)
                        {
                            // empty game state, no units in town..

                            var networkStateEntity = state.EntityManager.CreateEntity();
                            state.EntityManager.AddComponent<ClientOnly>(networkStateEntity);
                            state.EntityManager.AddComponentData(networkStateEntity, 
                                new NetworkGameState
                                {
                                    unitId = 0, playerId = 0
                                });
                        }

                        if (type == PacketType.ServerTranslationSync)
                        {
                            var count = stream.ReadUShort();

                            for (var j = 0; j < count; j++)
                            {
                                var networkStateEntity = state.EntityManager.CreateEntity();
                                state.EntityManager.AddComponent<ClientOnly>(networkStateEntity);
                                var translationSync = new NetworkTranslationSync().Read(ref stream);
                                state.EntityManager.AddComponentData(networkStateEntity, translationSync);
                            }

                            // read all packets~!!!

                            // find unit and sync translation??
                        }

                        if (type == PacketType.ServerDeniedConnectionMaxPlayers)
                        {
                            // TODO: show it in the UI
                            Debug.Log("Server reached max players");
                        }

                        if (type == PacketType.ClientKeepAlive)
                        {
                            // keep alive returned, check current latency...
                            var packetIndex = stream.ReadByte();
                            
                            // if not the packet we are waiting, avoid it...
                            if (packetIndex == ConnectionState.latencyWaitPacket)
                            {
                                ConnectionState.latency = (float) ((SystemAPI.Time.ElapsedTime - ConnectionState.latencyPacketLastTime) * 0.5f);
                            }
                        }
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        networkPlayer.ValueRW.connection = default;
                        
                        Debug.Log("Client got disconnected from server");

                        state.EntityManager.SetSharedComponent(clientEntity, client);

                        ConnectionState.currentState = ConnectionState.State.Disconnected;
                    }
                }
            }
            
            foreach (var networkPlayer in 
                SystemAPI.Query<RefRO<NetworkPlayerId>>())
            {
                var m_Connection = networkPlayer.ValueRO.connection;
                
                // TODO: check the connection wasn't destroyed...
                var pendingActionSent = false;

                // avoid destroying server entities with queued client player action...
                foreach (var (pendingAction, entity) in 
                    SystemAPI.Query<RefRO<PendingPlayerAction>>()
                        .WithNone<ServerOnly>()
                        .WithEntityAccess())
                {
                    state.EntityManager.DestroyEntity(entity);
                    // state.EntityManager.RemoveComponent<PendingPlayerAction>(entity);
                            
                    // var writer = m_Driver.BeginSend(m_Connection);
                    m_Driver.BeginSend(m_Connection, out var writer);
                    pendingAction.ValueRO.Write(ref writer);
                    m_Driver.EndSend(writer);

                    pendingActionSent = true;
                }

                // if (!pendingActionSent)
                // {
                //     // send keep alive packet! 
                //     if (m_Driver.IsCreated && m_Connection.IsCreated)
                //     {
                //         if (m_Driver.GetConnectionState(m_Connection) == NetworkConnection.State.Connected)
                //         {
                //             var writer = m_Driver.BeginSend(m_Connection);
                //             writer.WriteByte(PacketType.ClientKeepAlive);
                //             m_Driver.EndSend(writer);
                //         }
                //     }
                // }
            }
            
            // TODO: keep alive frequency...

            lastLatencyUpdate -= SystemAPI.Time.DeltaTime;
            var currentTime = SystemAPI.Time.ElapsedTime;
            
            if (lastLatencyUpdate < 0)
            {
                lastLatencyUpdate = ConnectionSettings.latencyKeepAliveFrequency;
                
                foreach (var networkPlayer in 
                    SystemAPI.Query<RefRO<NetworkPlayerId>>())
                {
                    var m_Connection = networkPlayer.ValueRO.connection;
                    if (!m_Connection.IsCreated)
                        continue;

                    if (m_Driver.GetConnectionState(m_Connection) != NetworkConnection.State.Connected)
                        continue;

                    ConnectionState.latencyPacketLastTime = currentTime;

                    ConnectionState.latencyWaitPacket = ConnectionState.latencyPacket;
                    ConnectionState.latencyPacket++;
                    
                    // var writer = m_Driver.BeginSend(m_Connection);
                    m_Driver.BeginSend(m_Connection, out var writer);
                    writer.WriteByte(PacketType.ClientKeepAlive);
                    writer.WriteByte(ConnectionState.latencyWaitPacket);
                    // writer.WriteFloat(currentTime);
                    m_Driver.EndSend(writer);
                }
            }
            
            // if we want to disconnect from server...
            foreach (var (_, entity) in 
                SystemAPI.Query<RefRO<DisconnectClientCommand>>()
                    .WithEntityAccess())
            {
                state.EntityManager.DestroyEntity(entity);
                
                foreach (var networkPlayer in 
                    SystemAPI.Query<RefRO<NetworkPlayerId>>())
                {
                    var connection = networkPlayer.ValueRO.connection;
                    if (connection.IsCreated && m_Driver.GetConnectionState(connection) == NetworkConnection.State.Connected)
                    {
                        // Send player manual disconnection
                        // var writer = m_Driver.BeginSend(connection);
                        m_Driver.BeginSend(connection, out var writer);
                        writer.WriteByte(PacketType.ClientDisconnect);
                        m_Driver.EndSend(writer);
                    }
                }

                ConnectionState.currentState = ConnectionState.State.Disconnected;
                state.EntityManager.SetSharedComponent(clientEntity, client);
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            var clientEntity = SystemAPI.GetSingletonEntity<ClientSingleton>();
            
            foreach (var client in 
                SystemAPI.Query<ClientNetworkComponentData>())
            {
                if (client.m_Driver.IsCreated)
                    client.m_Driver.Dispose();
            }
            
            state.EntityManager.DestroyEntity(clientEntity);
        }
    }
}
