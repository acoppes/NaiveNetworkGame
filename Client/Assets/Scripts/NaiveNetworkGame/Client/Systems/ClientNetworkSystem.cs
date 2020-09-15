using Client;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

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

                    client.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(
                            new NetworkDataStreamParameter { size = 0 }
                            ),
                        m_Connections = new NativeList<NetworkConnection>(1, Allocator.Persistent)
                    };

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

                    var networkManager = client.networkManager;

                   // networkManager.m_Driver.
                    
                    var connection = networkManager.m_Driver.Connect(endpoint);

                    networkManager.m_Connections
                        .Add(connection);
                    
                    Debug.Log($"{networkManager.m_Driver.LocalEndPoint().Address}");
                    // Debug.Log($"{networkManager.m_Driver.RemoteEndPoint(connection).Address}:{networkManager.m_Driver.RemoteEndPoint(connection).Port}");

                    client.connectionInitialized = true;
                    PostUpdateCommands.SetSharedComponent(clientEntity, client);
                });


            if (client.networkManager == null || !client.connectionInitialized)
                return;

            DataStreamReader stream;
            NetworkEvent.Type cmd;

            for (var i = 0; i < client.networkManager.m_Connections.Length; i++)
            {
                var m_Connection = client.networkManager.m_Connections[i];
                var m_Driver = client.networkManager.m_Driver;
                
                if (!m_Connection.IsCreated)
                {
                    Debug.Log("Something went wrong during connect");
                    return;
                }
                
                m_Driver.ScheduleUpdate().Complete();

                while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Connect)
                    {
                        Debug.Log("We are now connected to the server");
                        var writer = m_Driver.BeginSend(m_Connection);
                        writer.WriteByte(PacketType.ClientKeepAlive);
                        m_Driver.EndSend(writer);

                        ConnectionState.currentState = ConnectionState.State.Connected;

                    }
                    else if (cmd == NetworkEvent.Type.Data)
                    {
                        var type = stream.ReadByte();

                        if (type == PacketType.ServerSendPlayerId)
                        {
                            // this is my player id!!
                            var networkPlayerId = stream.ReadByte();
                            
                            // TODO: local player controller entity should already be created and
                            // here we just add the component to that entity...
                            
                            // We are assuming this message is not going to be received again...
                            // var localPlayer = PostUpdateCommands.CreateEntity();
                            var localPlayer = GetSingletonEntity<PlayerController>();
                            PostUpdateCommands.AddComponent(localPlayer, new NetworkPlayerId
                            {
                                player = networkPlayerId,
                                connection = m_Connection
                            });
                            // PostUpdateCommands.AddComponent(localPlayer, new PlayerController());
                        }
                        
                        if (type == PacketType.ServerGameState)
                        {
                            // network game state... 
                            // read unit info...
                            
                            var e = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(e, new NetworkGameState().Read(ref stream));
                        }
                        
                        if (type == PacketType.ServerPlayerState)
                        {
                            // network game state... 
                            // read unit info...
                            
                            var e = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(e, new NetworkPlayerState().Read(ref stream));
                        }
                        
                        if (type == PacketType.ServerDeniedConnectionMaxPlayers)
                        {
                            // TODO: show it in the UI
                            Debug.Log("Server reached max players");
                        }
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client got disconnected from server");
                        client.networkManager.m_Connections[i] = default;
                        
                        // PostUpdateCommands.RemoveComponent<ClientRunningComponent>(clientEntity);
                        client.connectionInitialized = false;
                        
                        PostUpdateCommands.SetSharedComponent(clientEntity, client);

                        ConnectionState.currentState = ConnectionState.State.Disconnected;
                    }
                }
            }
            
            Entities
                .WithAll<NetworkPlayerId>()
                .ForEach(delegate(ref NetworkPlayerId networkPlayer)
            {
                var m_Driver = client.networkManager.m_Driver;
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
                        var writer = m_Driver.BeginSend(m_Connection);
                        writer.WriteByte(PacketType.ClientKeepAlive);
                        m_Driver.EndSend(writer);    
                    }
                }
            });
            
            Entities
                .WithAll<DisconnectClientCommand>()
                .ForEach(delegate(Entity e)
                {
                    PostUpdateCommands.DestroyEntity(e);
                    
                    for (var i = 0; i < client.networkManager.m_Connections.Length; i++)
                    {
                        var connection = client.networkManager.m_Connections[i];
                        if (connection.IsCreated)
                        {
                            // Send player manual disconnection
                            var writer = client.networkManager.m_Driver.BeginSend(connection);
                            writer.WriteByte(PacketType.ClientDisconnect);
                            client.networkManager.m_Driver.EndSend(writer);    
                            
                            // client.networkManager.m_Driver.Disconnect(connection);
                            // client.networkManager.m_Connections[i] = default;
                        }
                    }

                    ConnectionState.currentState = ConnectionState.State.Disconnected;
                    
                    client.connectionInitialized = false;
                    
                    PostUpdateCommands.SetSharedComponent(clientEntity, client);
                    
                    // DestroyManager(client.networkManager);
                    // PostUpdateCommands.DestroyEntity(clientEntity);
                });
        }

        private void DestroyManager(NetworkManager manager)
        {
            if (manager == null) 
                return;
                    
            manager.m_Connections.Dispose();
            manager.m_Driver.Dispose();
        }

        protected override void OnDestroy()
        {
            Entities.ForEach(delegate(ClientSingleton c)
            {
                DestroyManager(c.networkManager);
            });
            base.OnDestroy();
        }
    }
}