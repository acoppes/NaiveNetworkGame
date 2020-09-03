using NaiveNetworkGame.Common;
using Scenes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace Client
{
    public static class ServerConnectionParameters
    {
        public static string ip;
        public static ushort port;
    }

    public struct ClientStartComponent : IComponentData
    {
        
    }

    public struct ClientRunningComponent : IComponentData
    {
        
    }
    
    public class ClientNetworkSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<ServerOnly>()
                .WithAll<ClientStartComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent managerSharedComponent, ref ClientStartComponent s)
                {
                    Debug.Log("Starting Client");
                    
                    PostUpdateCommands.RemoveComponent<ClientStartComponent>(e);

                    managerSharedComponent.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(),
                        m_Connections = new NativeList<NetworkConnection>(1, Allocator.Persistent)
                    };

                    var endpoint = NetworkEndPoint.LoopbackIpv4.WithPort(9000);

                    if (!string.IsNullOrEmpty(ServerConnectionParameters.ip))
                    {
                        endpoint = NetworkEndPoint.Parse(ServerConnectionParameters.ip, ServerConnectionParameters.port, NetworkFamily.Ipv4);
                    }

                    // var endpoint = NetworkEndPoint.Parse("167.57.35.238", 9000, NetworkFamily.Ipv4);
                    
                    Debug.Log($"Connecting to {endpoint.Address}, {endpoint.IsValid}");
                    
                    // endpoint.Address = "167.57.86.221";
                    // endpoint.Port = 9000;

                    var networkManager = managerSharedComponent.networkManager;

                   // networkManager.m_Driver.
                    
                    var connection = networkManager.m_Driver.Connect(endpoint);

                    networkManager.m_Connections
                        .Add(connection);
                    
                    Debug.Log($"{networkManager.m_Driver.LocalEndPoint().Address}");
                    // Debug.Log($"{networkManager.m_Driver.RemoteEndPoint(connection).Address}:{networkManager.m_Driver.RemoteEndPoint(connection).Port}");
                    
                    PostUpdateCommands.SetSharedComponent(e, managerSharedComponent);
                    PostUpdateCommands.AddComponent(e, new ClientRunningComponent());
                });
            
            Entities
                .WithNone<ServerOnly>()
                .WithAll<ClientOnly, ClientRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(NetworkManagerSharedComponent networkManager)
                {
                    DataStreamReader stream;
                    NetworkEvent.Type cmd;

                    for (var i = 0; i < networkManager.networkManager.m_Connections.Length; i++)
                    {
                        var m_Connection = networkManager.networkManager.m_Connections[i];
                        var m_Driver = networkManager.networkManager.m_Driver;
                        
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
                                writer.WriteByte(0);
                                m_Driver.EndSend(writer);

                            }
                            else if (cmd == NetworkEvent.Type.Data)
                            {
                                var type = stream.ReadByte();

                                if (type == 0)
                                {
                                    // this is my player id!!
                                    var networkPlayerId = stream.ReadByte();
                                    
                                    // We are assuming this message is not going to be received again...
                                    var networkPlayer = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent(networkPlayer, new NetworkPlayerId
                                    {
                                        player = networkPlayerId,
                                        connection = m_Connection
                                    });
                                }
                                
                                if (type == 50)
                                {
                                    // network game state... 
                                    
                                    var frame = stream.ReadInt();
                                    var time = stream.ReadFloat();
                                    var unitId = stream.ReadUInt();
                                    var playerId = stream.ReadByte();
                                    var unitType = stream.ReadByte();
                                    var x = stream.ReadFloat();
                                    var y = stream.ReadFloat();
                                    var lookingDirectionX = stream.ReadFloat();
                                    var lookingDirectionY = stream.ReadFloat();
                                    var state = stream.ReadByte();

                                    // read unit info...
                                    var e = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent(e, new NetworkGameState
                                    {
                                        // connectionId = (uint) i,
                                        frame = frame,
                                        delta = time,
                                        unitId = (int) unitId,
                                        playerId = playerId,
                                        unitType = unitType,
                                        translation = new float2(x, y),
                                        lookingDirection = new float2(lookingDirectionX, lookingDirectionY),
                                        state = state,
                                    });
                                }
                            }
                            else if (cmd == NetworkEvent.Type.Disconnect)
                            {
                                Debug.Log("Client got disconnected from server");
                                networkManager.networkManager.m_Connections[i] = default;
                            }
                        }
                        
                        
                    }
                });

            var query = Entities.WithAll<NetworkManagerSharedComponent>().ToEntityQuery();
            if (query.CalculateEntityCount() == 0)
                return;
            
            var networkManagerEntity = Entities.WithAll<NetworkManagerSharedComponent>().ToEntityQuery()
                .GetSingletonEntity();
            var networkManager = EntityManager.GetSharedComponentData<NetworkManagerSharedComponent>(networkManagerEntity);
            
            Entities.WithAll<NetworkPlayerId>().ForEach(delegate(ref NetworkPlayerId networkPlayer)
            {
                var m_Driver = networkManager.networkManager.m_Driver;
                var m_Connection = networkPlayer.connection;
                
                // TODO: check the connection wasn't destroyed...
                var pendingActionSent = false;

                Entities.WithNone<ServerOnly>()
                    .WithAll<ClientOnly, PendingPlayerAction>()
                    .ForEach(delegate(Entity e, ref PendingPlayerAction p)
                    {
                        PostUpdateCommands.DestroyEntity(e);
                        // PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                                
                        var writer = m_Driver.BeginSend(m_Connection);
                                
                        // just a number to identify the packet for now...
                        writer.WriteByte(PacketType.PlayerAction);
                        writer.WriteByte((byte) p.player);
                        writer.WriteUInt(p.unit);
                        writer.WriteByte((byte) p.command);
                        writer.WriteFloat(p.target.x);
                        writer.WriteFloat(p.target.y);
                                
                        m_Driver.EndSend(writer);

                        pendingActionSent = true;
                    });

                if (!pendingActionSent)
                {
                    // send keep alive packet! 
                    if (m_Driver.IsCreated && m_Connection.IsCreated)
                    {
                        var writer = m_Driver.BeginSend(m_Connection);
                        writer.WriteByte(1);
                        m_Driver.EndSend(writer);    
                    }
                }
            });
        }

        protected override void OnDestroy()
        {
            Entities
                .WithNone<ServerOnly>()
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
}