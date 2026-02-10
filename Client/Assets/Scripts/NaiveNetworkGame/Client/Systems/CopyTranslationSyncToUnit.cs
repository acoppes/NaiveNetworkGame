using NaiveNetworkGame.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace NaiveNetworkGame.Client.Systems
{
    public partial class CopyTranslationSyncToUnit : SystemBase
    {
        protected override void OnUpdate()
        {
            var unitsQuery = Entities.WithAll<Unit, Translation>().ToEntityQuery();
            var units = unitsQuery.ToComponentDataArray<Unit>(Allocator.TempJob);
            var unitEntities = unitsQuery.ToEntityArray(Allocator.TempJob);

            Entities
                .WithNone<Unit>()
                .WithAll<NetworkTranslationSync, ClientOnly>()
                .ForEach(delegate(Entity e, ref NetworkTranslationSync n)
                {
                    for (var i = 0; i < units.Length; i++)
                    {
                        var unit = units[i];
                        if (unit.unitId == n.unitId)
                        {
                            PostUpdateCommands.AddComponent(unitEntities[i], n);
                        }
                    }
                    PostUpdateCommands.DestroyEntity(e);
                });

            units.Dispose();
            unitEntities.Dispose();
        }
    }
}