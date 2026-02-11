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
    
    public partial class ClientNetworkSystem : SystemBase
    {
        private float lastLatencyUpdate;
        
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<ClientSingleton>();
            
//             var serverEntity = EntityManager.CreateEntity();
// #if UNITY_EDITOR
//             EntityManager.SetName(serverEntity, "ServerSingleton");
// #endif
//             EntityManager.AddComponentData(serverEntity, new ServerSingleton());
//             EntityManager.AddSharedComponentManaged(serverEntity, new ServerData());
            
            // now server network system is in charge of creating server singleton...
            var clientEntity = EntityManager.CreateEntity();
#if UNITY_EDITOR
            EntityManager.SetName(clientEntity, "ClientSingleton");
#endif
            EntityManager.AddSharedComponent(clientEntity, new ClientSingleton());
        }

        protected override void OnUpdate()
        {
            var clientEntity = SystemAPI.GetSingletonEntity<ClientSingleton>();
            var client =
                EntityManager.GetSharedComponent<ClientSingleton>(clientEntity);
            
            var networkSettings = new NetworkSettings(Allocator.Persistent);
            //   new NetworkDataStreamParameter { size = 0 },
            networkSettings.WithFragmentationStageParameters(payloadCapacity:16 * 1024);
            networkSettings.WithReliableStageParameters(windowSize: 32);
            
            Entities
                .WithAll<StartClientCommand>()
                .ForEach(delegate(Entity e, ref StartClientCommand s)
                {
                    Debug.Log("Starting Client");

                    PostUpdateCommands.DestroyEntity(e);
                    
                    // PostUpdateCommands.RemoveComponent<StartClientCommand>(e);

                    client.m_Driver = NetworkDriver.Create(networkSettings);
                    
                    client.framentationPipeline = 
                        client.m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));
                    client.reliabilityPipeline = 
                        client.m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
                    
                    var endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(9000);

                    var parametersObject = ServerConnectionParametersObject.Instance;
                    var parameters = new ServerConnectionParameters();
                    
                    if (parametersObject != null)
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
                    
                    client.endpoint = endpoint;
                    
                    PostUpdateCommands.SetSharedComponent(clientEntity, client);
                });


            // if (client.networkManager == null || !client.connectionInitialized)
            //     return;

            if (!client.m_Driver.IsCreated)
                return;
            
            Entities
                .WithNone<NetworkPlayerId>()
                .WithAll<LocalPlayerControllerComponentData>()
                .ForEach(delegate(Entity e, ref ConnectPlayerToServer l)
                {
                    var connection = client.m_Driver.Connect(client.endpoint);

                    Debug.Log("Connecting local player to server");
                    // Debug.Log($"Connecting to server: {client.m_Driver.LocalEndPoint().Address}");
                    
                    PostUpdateCommands.AddComponent(e, new NetworkPlayerId
                    {
                        connection = connection
                    });
                    PostUpdateCommands.RemoveComponent<ConnectPlayerToServer>(e);
                });

            DataStreamReader stream;
            NetworkEvent.Type cmd;
            
            var m_Driver = client.m_Driver;
            m_Driver.ScheduleUpdate().Complete();

            Entities
                .WithNone<ConnectPlayerToServer>()
                .WithAll<NetworkPlayerId, LocalPlayerControllerComponentData>()
                .ForEach(delegate(Entity e, ref NetworkPlayerId networkPlayer, ref LocalPlayerControllerComponentData p)
                {
                    var m_Connection = networkPlayer.connection;
                    
                    networkPlayer.state = NetworkConnection.State.Disconnected;
                    
                    if (!m_Connection.IsCreated)
                    {
                        // this client was disconnected or failed to connect the first time...
                        return;
                    }
                    
                    networkPlayer.state = m_Driver.GetConnectionState(m_Connection);

                    while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
                    {
                        if (cmd == NetworkEvent.Type.Connect)
                        {
                            Debug.Log("Local player connected to server, sending keep alive");
                            ConnectionState.currentState = ConnectionState.State.WaitingForPlayers;

                        }
                        else if (cmd == NetworkEvent.Type.Data)
                        {
                            ConnectionState.totalReceivedBytes += stream.Length;

                            var type = stream.ReadByte();

                            if (type == PacketType.ServerSimulationStarted)
                            {
                                ConnectionState.currentState = ConnectionState.State.SimulationRunning;
                            }
                            
                            if (type == PacketType.ServerSendPlayerId)
                            {
                                // this is my player id!!
                                p.player = stream.ReadByte();

                                var actions = PostUpdateCommands.AddBuffer<PlayerAction>(e);
                                
                                var actionsCount = stream.ReadByte();
                                for (var i = 0; i < actionsCount; i++)
                                {
                                    var actionType = stream.ReadByte();
                                    var actionCost = stream.ReadByte();
                                    actions.Add(new PlayerAction
                                    {
                                        type = actionType,
                                        cost = actionCost
                                    });
                                }
                            }

                            if (type == PacketType.ServerGameState)
                            {
                                // get total data
                                var count = stream.ReadUShort();

                                for (var j = 0; j < count; j++)
                                {
                                    var networkStateEntity = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent<ClientOnly>(networkStateEntity);
                                    PostUpdateCommands.AddComponent(networkStateEntity, 
                                        new NetworkGameState().Read(ref stream));
                                }
                            }

                            if (type == PacketType.ServerPlayerState)
                            {
                                // network game state... 
                                // read unit info...

                                var networkStateEntity = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent<ClientOnly>(networkStateEntity);
                                PostUpdateCommands.AddComponent(networkStateEntity, 
                                    new NetworkPlayerState().Read(ref stream));
                            }
                            
                            if (type == PacketType.ServerEmptyGameState)
                            {
                                // empty game state, no units in town..

                                var networkStateEntity = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent<ClientOnly>(networkStateEntity);
                                PostUpdateCommands.AddComponent(networkStateEntity, 
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
                                    var networkStateEntity = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent<ClientOnly>(networkStateEntity);
                                    var translationSync = new NetworkTranslationSync().Read(ref stream);
                                    PostUpdateCommands.AddComponent(networkStateEntity, translationSync);
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
                                    ConnectionState.latency = (float) ((Time.ElapsedTime - ConnectionState.latencyPacketLastTime) * 0.5f);
                                }
                            }
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            networkPlayer.connection = default;
                            
                            Debug.Log("Client got disconnected from server");

                            PostUpdateCommands.SetSharedComponent(clientEntity, client);

                            ConnectionState.currentState = ConnectionState.State.Disconnected;
                        }
                    }
                });
            
            Entities
                .WithAll<NetworkPlayerId>()
                .ForEach(delegate(ref NetworkPlayerId networkPlayer)
            {
                var m_Connection = networkPlayer.connection;
                
                // TODO: check the connection wasn't destroyed...
                var pendingActionSent = false;

                // avoid destroying server entities with queued client player action...
                Entities
                    .WithNone<ServerOnly>()
                    .WithAll<PendingPlayerAction>()
                    .ForEach(delegate(Entity e, ref PendingPlayerAction p)
                    {
                        PostUpdateCommands.DestroyEntity(e);
                        // PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                                
                        // var writer = m_Driver.BeginSend(m_Connection);
                        m_Driver.BeginSend(m_Connection, out var writer);
                        p.Write(ref writer);
                        m_Driver.EndSend(writer);

                        pendingActionSent = true;
                    });

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
            });
            
            // TODO: keep alive frequency...

            lastLatencyUpdate -= Time.DeltaTime;
            var currentTime = Time.ElapsedTime;
            
            if (lastLatencyUpdate < 0)
            {
                lastLatencyUpdate = ConnectionSettings.latencyKeepAliveFrequency;
                
                Entities
                    .WithAll<NetworkPlayerId>()
                    .ForEach(delegate(ref NetworkPlayerId networkPlayer)
                    {
                        var m_Connection = networkPlayer.connection;
                        if (!m_Connection.IsCreated)
                            return;

                        if (m_Driver.GetConnectionState(m_Connection) != NetworkConnection.State.Connected)
                            return;

                        ConnectionState.latencyPacketLastTime = currentTime;

                        ConnectionState.latencyWaitPacket = ConnectionState.latencyPacket;
                        ConnectionState.latencyPacket++;
                        
                        // var writer = m_Driver.BeginSend(m_Connection);
                        m_Driver.BeginSend(m_Connection, out var writer);
                        writer.WriteByte(PacketType.ClientKeepAlive);
                        writer.WriteByte(ConnectionState.latencyWaitPacket);
                        // writer.WriteFloat(currentTime);
                        m_Driver.EndSend(writer);
                    });
            }
            
            // if we want to disconnect from server...
            Entities
                .WithAll<DisconnectClientCommand>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.DestroyEntity(e);
                    
                    Entities.ForEach(delegate(ref NetworkPlayerId p)
                    {
                        var connection = p.connection;
                        if (connection.IsCreated && m_Driver.GetConnectionState(connection) == NetworkConnection.State.Connected)
                        {
                            // Send player manual disconnection
                            // var writer = m_Driver.BeginSend(connection);
                            m_Driver.BeginSend(connection, out var writer);
                            writer.WriteByte(PacketType.ClientDisconnect);
                            m_Driver.EndSend(writer);
                        }
                    });

                    ConnectionState.currentState = ConnectionState.State.Disconnected;
                    PostUpdateCommands.SetSharedComponent(clientEntity, client);
                });
        }

        protected override void OnDestroy()
        {
            Entities.ForEach(delegate(ClientSingleton c)
            {
                if (c.m_Driver.IsCreated)
                    c.m_Driver.Dispose();
            });
            
            base.OnDestroy();
        }
    }
}