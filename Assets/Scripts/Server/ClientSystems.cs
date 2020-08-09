using Client;
using Unity.Entities;

namespace Server
{
    public class ClientNetworkOutgoingCommandsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // var managerEntity = EntityManager
            //     .CreateEntityQuery(
            //         new ComponentType(typeof(ClientNetworkComponent)),
            //         ComponentType.ReadOnly<ClientOnly>())
            //     .GetSingletonEntity();
            //
            // var network = EntityManager
            //     .GetSharedComponentData<ClientNetworkComponent>(managerEntity).networkManager;
            //
            // Debug.Log(network.m_Connection.ToString());
            
            Entities
                .WithAll<ClientOnly, ClientNetworkComponent, NetworkPlayerId>()
                .ForEach(delegate(ClientNetworkComponent c, ref NetworkPlayerId networkPlayerId)
                {
                    var player = networkPlayerId.player;
                    
                    Entities
                        .WithAll<ClientOnly, PendingPlayerAction>()
                        .ForEach(delegate (Entity e, ref PendingPlayerAction p)
                        {
                            PostUpdateCommands.DestroyEntity(e);
                            // Send command through the network with a packet...

                            // TODO: send message to the server...
                            // var writer = network.m_Driver.BeginSend(network.m_Connection);
                            // writer.WriteUInt(0);
                            // network.m_Driver.EndSend(writer);

                            var serverCommand = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(serverCommand, p);
                        });
                });
            

        }
    }
}