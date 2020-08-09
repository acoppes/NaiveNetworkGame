using Scenes;
using Server;
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
    
    [UpdateAfter(typeof(ServerNetworkSystem))]
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
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent networkManager)
                {
                    DataStreamReader stream;
                    NetworkEvent.Type cmd;

                    // var connecting = false;
                    var m_Connection = networkManager.networkManager.m_Connections[0];
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
                            if (type == 50)
                            {
                                var unitId = stream.ReadUInt();
                                var player = stream.ReadUInt();
                                var x = stream.ReadFloat();
                                var y = stream.ReadFloat();
                                
                                // read unit info...
                                var clientViewUpdate = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent(clientViewUpdate, new ClientViewUpdate
                                {
                                    unitId = unitId,
                                    position = new float2(x, y)
                                });
                            }
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            Debug.Log("Client got disconnected from server");
                            networkManager.networkManager.m_Connections[0] = default;
                        }
                    }
                    
                    // TODO: check the connection wasn't destroyed...
                    
                    Entities.WithNone<ServerOnly>()
                        .WithAll<ClientOnly, PendingPlayerAction>()
                        .ForEach(delegate(Entity e, ref PendingPlayerAction p)
                        {
                            PostUpdateCommands.RemoveComponent<PendingPlayerAction>(e);
                            
                            var writer = m_Driver.BeginSend(m_Connection);
                            
                            // just a number to identify the packet for now...
                            writer.WriteUInt(99);
                            writer.WriteUInt(p.player);
                            writer.WriteUInt(p.command);
                            writer.WriteFloat(p.target.x);
                            writer.WriteFloat(p.target.y);
                            
                            m_Driver.EndSend(writer);
                        });
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