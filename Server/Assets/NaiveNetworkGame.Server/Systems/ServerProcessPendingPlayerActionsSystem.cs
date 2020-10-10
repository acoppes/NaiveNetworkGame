using NaiveNetworkGame.Common;
using NaiveNetworkGame.Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NaiveNetworkGame.Server.Systems
{
    public class ServerProcessPendingPlayerActionsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // process all player pending actions
            Entities
                .WithAll<ClientPlayerAction, PlayerController, Translation>()
                .ForEach(delegate (Entity e, ref ClientPlayerAction p, ref PlayerController playerController, ref Translation t)
                {
                    // PostUpdateCommands.DestroyEntity(e);
                    PostUpdateCommands.RemoveComponent<ClientPlayerAction>(e);

                    var player = p.player;
                    var unitId = p.unit;
                
                    if (p.actionType == ClientPlayerAction.MoveUnitAction)
                    {
                        var pendingAction = new PendingAction
                        {
                            command = p.actionType,
                            target = p.target
                        };

                        Entities.WithAll<Unit, Movement>().ForEach(delegate(Entity unitEntity, ref Unit unit)
                        {
                            if (unit.player != player)
                                return;

                            if (unit.id != unitId)
                                return;

                            PostUpdateCommands.RemoveComponent<PendingAction>(unitEntity);
                            PostUpdateCommands.RemoveComponent<MovementAction>(unitEntity);
                            PostUpdateCommands.AddComponent(unitEntity, pendingAction);
                        });
                    } else if (p.actionType == ClientPlayerAction.BuildUnit)
                    {
                        // var prefab = Entity.Null;
                        var position = t.Value;

                        var playerActions = GetBufferFromEntity<PlayerAction>()[e];
                        var playerAction = playerActions[p.unitType];

                        // can't execute action if not enough gold...


                        var prefab = playerAction.prefab;
                        
                        var unitComponent = GetComponentDataFromEntity<Unit>()[prefab];

                        // dont create unit if at maximum capacity
                        if (unitComponent.slotCost > 0 && 
                            playerController.currentUnits + unitComponent.slotCost > playerController.maxUnits) 
                            return;

                        if (playerController.gold < playerAction.cost)
                            return;
                        
                        if (p.unitType == 0)
                        {
                            // prefab = playerController.unitPrefab;
                        }
                        else if (p.unitType == 1)
                        {
                            if (playerController.buildingSlots == 0)
                                return;
                        
                            var buildingSlotBuffer = GetBufferFromEntity<BuildingSlot>()[e];
                            
                            for (int i = 0; i < buildingSlotBuffer.Length; i++)
                            {
                                var buildingSlot = buildingSlotBuffer[i];
                                if (buildingSlot.available)
                                {
                                    position = buildingSlot.position;
                                    buildingSlot.available = false;
                                    buildingSlotBuffer[i] = buildingSlot;
                                    break;
                                }
                            }
                            
                            // var updateBuffer = PostUpdateCommands.SetBuffer<BuildingSlot>(e);
                            // for (var i = 0; i < updateBuffer.Length; i++)
                            // {
                            //     updateBuffer[i] = buildingSlotBuffer[i];
                            // }

                            // PostUpdateCommands.buff;
                        }

                        // consume gold
                        playerController.gold -= playerAction.cost;
                        
                        var unitEntity = PostUpdateCommands.Instantiate(prefab);

                        unitComponent.id = NetworkUnitId.current++;
                        unitComponent.player = player;
                        // unitComponent.type = p.unitType;
                        
                        PostUpdateCommands.SetComponent(unitEntity, unitComponent);

                        PostUpdateCommands.SetComponent(unitEntity, new Translation
                        {
                            Value = position
                            // Value = new float3(p.target.x, p.target.y, 0)
                        });
                        PostUpdateCommands.SetComponent(unitEntity, new UnitState
                        {
                            state = UnitState.spawningState
                        });
                        
                        // PostUpdateCommands.SetComponent(unitEntity, new Health
                        // {
                        //     total = 100,
                        //     current = UnityEngine.Random.Range(5, 100)
                        // });
                        
                        // PostUpdateCommands.AddComponent(unitEntity, new SpawningAction
                        // {
                        //     duration = 1.0f
                        // });
                        //   target = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, 1.25f)
                        
                        var wanderArea = playerController.playerWander;

                        PostUpdateCommands.AddComponent(unitEntity, new UnitBehaviour
                        {
                            wanderArea = wanderArea
                        });
                        
                        PostUpdateCommands.AddComponent<NetworkUnit>(unitEntity);
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkGameState());
                        PostUpdateCommands.AddComponent(unitEntity, new NetworkTranslationSync());
                    }
                });
        }
    }
}