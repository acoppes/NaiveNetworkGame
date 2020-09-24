using Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace NaiveNetworkGame.Client.Systems
{

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
    
    public class ClientNetworkSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireSingletonForUpdate<ClientSingleton>();
            
            // now server network system is in charge of creating server singleton...
            var clientEntity = EntityManager.CreateEntity();
#if UNITY_EDITOR
            EntityManager.SetName(clientEntity, "ClientSingleton");
#endif
            EntityManager.AddSharedComponentData(clientEntity, new ClientSingleton());
        }

        protected override void OnUpdate()
        {
            var clientEntity = GetSingletonEntity<ClientSingleton>();
            var client =
                EntityManager.GetSharedComponentData<ClientSingleton>(clientEntity);

            Entities
                .WithAll<StartClientCommand>()
                .ForEach(delegate(Entity e, ref StartClientCommand s)
                {
                    Debug.Log("Starting Client");

                    PostUpdateCommands.DestroyEntity(e);
                    
                    // PostUpdateCommands.RemoveComponent<StartClientCommand>(e);

                    client.m_Driver = NetworkDriver.Create(
                        new NetworkDataStreamParameter {size = 0},
                        new FragmentationUtility.Parameters
                        {
                            PayloadCapacity = 16 * 1024
                        }
                    );
                    
                    client.framentationPipeline = 
                        client.m_Driver.CreatePipeline(typeof(FragmentationPipelineStage));

                    var endpoint = NetworkEndPoint.LoopbackIpv4.WithPort(9000);

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
                        endpoint = NetworkEndPoint.Parse(parameters.ip, parameters.port);
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
                .WithAll<PlayerController>()
                .ForEach(delegate(Entity e, ref LocalPlayer l)
                {
                    var connection = client.m_Driver.Connect(client.endpoint);

                    Debug.Log("Connecting local player to server");
                    // Debug.Log($"Connecting to server: {client.m_Driver.LocalEndPoint().Address}");
                    
                    PostUpdateCommands.AddComponent(e, new NetworkPlayerId
                    {
                        connection = connection
                    });
                    PostUpdateCommands.RemoveComponent<LocalPlayer>(e);
                });

            DataStreamReader stream;
            NetworkEvent.Type cmd;
            
            var m_Driver = client.m_Driver;
            m_Driver.ScheduleUpdate().Complete();

            Entities
                .WithNone<LocalPlayer>()
                .WithAll<NetworkPlayerId, PlayerController>()
                .ForEach(delegate(Entity e, ref NetworkPlayerId networkPlayer, ref PlayerController p)
                {
                    var m_Connection = networkPlayer.connection;

                    if (!m_Connection.IsCreated)
                    {
                        // this client was disconnected or failed to connect the first time...
                        return;
                    }

                    while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
                    {
                        if (cmd == NetworkEvent.Type.Connect)
                        {
                            Debug.Log("Local player connected to server, sending keep alive");
                            
                            var writer = m_Driver.BeginSend(m_Connection);
                            writer.WriteByte(PacketType.ClientKeepAlive);
                            m_Driver.EndSend(writer);

                            ConnectionState.currentState = ConnectionState.State.Connected;

                        }
                        else if (cmd == NetworkEvent.Type.Data)
                        {
                            ConnectionState.totalReceivedBytes += stream.Length;

                            var type = stream.ReadByte();

                            if (type == PacketType.ServerSendPlayerId)
                            {
                                // this is my player id!!
                                var networkPlayerId = stream.ReadByte();
                                p.player = networkPlayerId;
                                
                                // now we can create units....
                            }

                            if (type == PacketType.ServerGameState)
                            {
                                // get total data
                                var count = stream.ReadUShort();

                                for (var j = 0; j < count; j++)
                                {
                                    var networkStateEntity = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent(networkStateEntity, 
                                        new NetworkGameState().Read(ref stream));
                                }
                            }

                            if (type == PacketType.ServerPlayerState)
                            {
                                // network game state... 
                                // read unit info...

                                var networkStateEntity = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent(networkStateEntity, 
                                    new NetworkPlayerState().Read(ref stream));
                            }

                            if (type == PacketType.ServerTranslationSync)
                            {
                                var count = stream.ReadUShort();

                                for (var j = 0; j < count; j++)
                                {
                                    var networkStateEntity = PostUpdateCommands.CreateEntity();
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

                Entities
                    .WithAll<ClientPlayerAction>()
                    .ForEach(delegate(Entity e, ref ClientPlayerAction p)
                    {
                        PostUpdateCommands.DestroyEntity(e);
                        // PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                                
                        var writer = m_Driver.BeginSend(m_Connection);

                        p.Write(ref writer);
                        m_Driver.EndSend(writer);

                        pendingActionSent = true;
                    });

                if (!pendingActionSent)
                {
                    // send keep alive packet! 
                    if (m_Driver.IsCreated && m_Connection.IsCreated)
                    {
                        if (m_Driver.GetConnectionState(m_Connection) == NetworkConnection.State.Connected)
                        {
                            var writer = m_Driver.BeginSend(m_Connection);
                            writer.WriteByte(PacketType.ClientKeepAlive);
                            m_Driver.EndSend(writer);
                        }
                    }
                }
            });
            
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
                            var writer = m_Driver.BeginSend(connection);
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
                c.m_Driver.Dispose();
            });
            
            base.OnDestroy();
        }
    }
}