using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

namespace NaiveNetworkGame.Client.Systems
{
    public partial struct LocalClientControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalClientController>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var clientController = SystemAPI.GetSingleton<LocalClientController>();
            
            foreach (var (command, entity) in SystemAPI.Query<RefRO<CreateLocalPlayerCommand>>().WithEntityAccess())
            {
                var localPlayerEntity = ecb.Instantiate(clientController.localPlayerPrefab);

                if (command.ValueRO.active)
                {
                    ecb.AddComponent(localPlayerEntity, new ActivePlayerComponent());
                }
                
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}