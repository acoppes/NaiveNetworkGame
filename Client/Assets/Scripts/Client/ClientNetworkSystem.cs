using Scenes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace Client
{
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
                    
                    var endpoint = NetworkEndPoint.LoopbackIpv4;
                    endpoint.Port = 9000;

                    var networkManager = managerSharedComponent.networkManager;
                    
                    networkManager.m_Connections
                        .Add(networkManager.m_Driver.Connect(endpoint));
                    
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
                                writer.WriteUInt(0);
                                m_Driver.EndSend(writer);
                            }
                            else if (cmd == NetworkEvent.Type.Data)
                            {
                                var type = stream.ReadUInt();

                                if (type == 0)
                                {
                                    // this is my player id!!
                                    var networkPlayerId = stream.ReadUInt();
                                    
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
                                    var unitId = stream.ReadUInt();
                                    var playerId = stream.ReadUInt();
                                    var x = stream.ReadFloat();
                                    var y = stream.ReadFloat();
                                    var lookingDirectionX = stream.ReadFloat();
                                    var lookingDirectionY = stream.ReadFloat();
                                    var state = stream.ReadUInt();

                                    // read unit info...
                                    var e = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent(e, new NetworkGameStateUpdate
                                    {
                                        // connectionId = (uint) i,
                                        frame = frame,
                                        unitId = (int) unitId,
                                        playerId = (int) playerId,
                                        translation = new float2(x, y),
                                        lookingDirection = new float2(lookingDirectionX, lookingDirectionY),
                                        state = (int) state,
                                    });
                                }
                            }
                            else if (cmd == NetworkEvent.Type.Disconnect)
                            {
                                Debug.Log("Client got disconnected from server");
                                networkManager.networkManager.m_Connections[i] = default;
                            }
                        }
                        
                        // TODO: check the connection wasn't destroyed...
                        
                        Entities.WithNone<ServerOnly>()
                            .WithAll<ClientOnly, PendingPlayerAction>()
                            .ForEach(delegate(Entity e, ref PendingPlayerAction p)
                            {
                                PostUpdateCommands.DestroyEntity(e);
                                // PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                                
                                var writer = m_Driver.BeginSend(m_Connection);
                                
                                // just a number to identify the packet for now...
                                writer.WriteUInt(99);
                                writer.WriteUInt(p.player);
                                writer.WriteUInt(p.command);
                                writer.WriteFloat(p.target.x);
                                writer.WriteFloat(p.target.y);
                                
                                m_Driver.EndSend(writer);
                            });
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