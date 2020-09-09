using Unity.Entities;

namespace Server
{
    public class CreatePlayerControllerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var createdUnitsEntity = Entities.WithAll<CreatedUnits>().ToEntityQuery().GetSingletonEntity();
            var createdUnits = EntityManager.GetComponentData<CreatedUnits>(createdUnitsEntity);
            
            Entities
                .WithNone<PlayerController>()
                .ForEach(delegate(Entity e, ref PlayerConnectionId p)
            {
                PostUpdateCommands.AddComponent(e, new PlayerController
                {
                    gold = 100
                });
            });
            
            PostUpdateCommands.SetComponent(createdUnitsEntity, createdUnits);
        }
    }
}