using System.Collections;
using Client;
using NaiveNetworkGame.Client;
using NaiveNetworkGame.Client.Components;
using NaiveNetworkGame.Common;
using Unity.Entities;
using UnityEngine;

namespace Scenes.Tests
{
    [DisableAutoCreation]
    public partial struct TestUnitsAttackComponentSystem : ISystem
    {
        private bool done;

        public void OnUpdate(ref SystemState state)
        {
            foreach (var p in SystemAPI.Query<RefRO<LocalPlayerControllerComponentData>>()
                         .WithAll<NetworkPlayerId>())
            {
                var command = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(command, new PendingPlayerAction
                {
                    player = p.ValueRO.player,
                    actionType = 2,
                    unitType = 1
                });

                done = true;
            }

            // Entities.WithAll<LocalPlayerControllerComponentData, NetworkPlayerId>()
            //     .ForEach(delegate(ref LocalPlayerControllerComponentData p)
            //     {
            //         var command = PostUpdateCommands.CreateEntity();
            //         PostUpdateCommands.AddComponent(command, new PendingPlayerAction
            //         {
            //             player = p.player,
            //             actionType = 2,
            //             unitType = 1
            //         });
            //
            //         done = true;
            //     });

            // PostUpdateCommands.AddComponent(PostUpdateCommands.CreateEntity(), new ClientPlayerAction
            // {
            //     player = 1,
            //     unit = 0,
            //     actionType = 2,
            //     unitType = 1
            // });
            //
            // // wait some time
            //
            // PostUpdateCommands.AddComponent(PostUpdateCommands.CreateEntity(), new ClientPlayerAction
            // {
            //     player = 1,
            //     unit = 0,
            //     actionType = 2,
            //     unitType = 0
            // });

        }
    }
        
    public class TestUnitsAttackController : MonoBehaviour
    {
        public int unitsCount = 1;

        public GameObject unitPrefab;
        
        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(StartTest());
        }

        private IEnumerator StartTest()
        {
            // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var world = World.DefaultGameObjectInjectionWorld;

            var testUnitsAttack = world.GetOrCreateSystem<TestUnitsAttackComponentSystem>();
            
            // var simulationSystemGroup = world.GetOrCreateSystem<SimulationSystemGroup>();
            // simulationSystemGroup.AddSystemToUpdateList(testUnitsAttack);
            
            // testUnitsAttack.Update(world);
            
            // simulationSystemGroup.SortSystemUpdateList();
            
            // ScriptBehaviourUpdateOrder.AppendWorldToPlayerLoop(world, simulationSystemGroup);
            // ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
            
            // world.CreateManager<MyCustomSystem>();
            // world.CreateManager<MyOtherCustomSystem>();
            // ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
            //
            //
            // yield return new WaitUntil(() => ConnectionState.currentState == ConnectionState.State.Connected);
            //
            // yield return new WaitForSeconds(0.1f);
            //
            // var playerInputStateQuery = entityManager.CreateEntityQuery(
            //     ComponentType.ReadWrite<PlayerPendingAction>());
            //
            // while (unitsCount > 0)
            // {
            //     var entities = playerInputStateQuery.ToEntityArray(Allocator.TempJob);
            //     var inputStates = playerInputStateQuery.ToComponentDataArray<PlayerPendingAction>(Allocator.TempJob);
            //
            //     for (int i = 0; i < inputStates.Length; i++)
            //     {
            //         var inputState = inputStates[i];
            //         
            //         inputState.pending = true;
            //         inputState.actionType = 2;
            //         inputState.unitType = 0;
            //
            //         entityManager.SetComponentData(entities[i], inputState);
            //     }
            //
            //     entities.Dispose();
            //     inputStates.Dispose();
            //
            //     unitsCount--;
            //     
            //     yield return null;
            // }

            yield return null;
        }

    }
}
