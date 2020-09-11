using NaiveNetworkGame.Common;
using Unity.Entities;

namespace Server
{
    public class ServerSendGameStateSystem : ComponentSystem
    {
        private float sendGameStateTime;
        
        protected override void OnUpdate()
        {
            ServerNetworkStatistics.outputBytesLastFrame = 0;
            ServerNetworkStatistics.currentConnections = 0;

            sendGameStateTime += Time.DeltaTime;
            
            Entities
                .WithAll<ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;
                    var m_Driver = networkManager.m_Driver;
                    
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        var connection = networkManager.m_Connections[i];

                        if (!connection.IsCreated)
                        {
                            // should we destroy player controller?
                            // how to send that with gamestate, maybe mark as destroyed...
                            continue;
                        }

                        ServerNetworkStatistics.currentConnections++;
                        
                        Entities.ForEach(delegate(ref PlayerConnectionId p)
                        {
                            if (p.synchronized)
                                return;
                            
                            // Send player id to player given a connection
                            if (p.connection == connection)
                            {
                                var writer = m_Driver.BeginSend(connection);
                                writer.WriteByte(PacketType.ServerSendPlayerId);
                                writer.WriteByte(p.player);
                                m_Driver.EndSend(writer);

                                ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;

                                p.synchronized = true;
                            }
                        });
                    }
                });

            if (sendGameStateTime < ServerNetworkStaticData.sendGameStateFrequency)
                return;

            sendGameStateTime -= ServerNetworkStaticData.sendGameStateFrequency;

            Entities
                .WithNone<ClientOnly>()
                .WithAll<ServerOnly, ServerRunningComponent, NetworkManagerSharedComponent>()
                .ForEach(delegate(Entity e, NetworkManagerSharedComponent serverManagerComponent)
                {
                    var networkManager = serverManagerComponent.networkManager;

                    var m_Driver = networkManager.m_Driver;
                    
                    // DataStreamReader stream;
                    for (var i = 0; i < networkManager.m_Connections.Length; i++)
                    {
                        var connection = networkManager.m_Connections[i];
                        
                        if (!connection.IsCreated)
                        {
                            // should we destroy player controller?
                            // how to send that with gamestate, maybe mark as destroyed...
                            continue;
                        }

                        if (ServerNetworkStaticData.synchronizeStaticObjects)
                        {
                            Entities
                                .WithAll<NetworkGameState, StaticObject>()
                                .ForEach(delegate(ref NetworkGameState n)
                                {
                                    var writer = m_Driver.BeginSend(connection);
                                    n.Write(ref writer);
                                    m_Driver.EndSend(writer);

                                    ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                    ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                                });
                        }
                        
                        Entities
                            .WithAll<NetworkPlayerState>()
                            .ForEach(delegate(ref NetworkPlayerState n, ref PlayerConnectionId p)
                            {
                                // only send player state to each player...
                                if (p.connection == connection)
                                {
                                    var writer = m_Driver.BeginSend(connection);
                                    n.Write(ref writer);
                                    m_Driver.EndSend(writer);

                                    ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                    ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;
                                }
                            });

                        Entities
                            .WithNone<StaticObject>()
                            .WithAll<NetworkGameState>()
                            .ForEach(delegate(ref NetworkGameState n)
                            {
                                // if (n.version == n.syncVersion)
                                //     return;

                                var writer = m_Driver.BeginSend(connection);
                                n.Write(ref writer);
                                m_Driver.EndSend(writer);
                            
                                ServerNetworkStatistics.outputBytesTotal += writer.LengthInBits / 8;
                                ServerNetworkStatistics.outputBytesLastFrame += writer.LengthInBits / 8;

                                // n.syncVersion = n.version;
                            });
                    }
                });
            
            ServerNetworkStaticData.synchronizeStaticObjects = false;
        }
    }
}