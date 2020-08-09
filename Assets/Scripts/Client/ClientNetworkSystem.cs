using Server;
using Unity.Collections;
using Unity.Entities;
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
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent networkManager, ref ClientStartComponent s)
                {
                    Debug.Log("Starting Client");
                    
                    PostUpdateCommands.RemoveComponent<ClientStartComponent>(e);

                    networkManager.networkManager = new NetworkManager
                    {
                        m_Driver = NetworkDriver.Create(),
                        m_Connections = new NativeList<NetworkConnection>(1, Allocator.Persistent)
                    };
                    
                    var endpoint = NetworkEndPoint.LoopbackIpv4;
                    endpoint.Port = 9000;
                    
                    networkManager.networkManager.m_Connections
                        .Add(networkManager.networkManager.m_Driver.Connect(endpoint));
                    
                    PostUpdateCommands.SetSharedComponent(e, networkManager);
                    PostUpdateCommands.AddComponent(e, new ClientRunningComponent());
                });
            
            Entities
                .WithNone<ServerOnly>()
                .WithAll<ClientRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent networkManager)
                {
                    DataStreamReader stream;
                    NetworkEvent.Type cmd;

                    // var connecting = false;
                    var m_Connection = networkManager.networkManager.m_Connections[0];
                    var m_Driver = networkManager.networkManager.m_Driver;

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
                            
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            Debug.Log("Client got disconnected from server");
                            networkManager.networkManager.m_Connections[0] = default;
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