using System;
using Unity.Collections;
using Unity.Entities;

namespace NaiveNetworkGame.Common
{
    public static class EntityQueryExtensionMethods
    {
        public static bool TryGetSingletonEntity(this EntityQuery query, out Entity e)
        {
            e = Entity.Null;
            if (query.CalculateEntityCount() != 1)
                return false;
            e =  query.GetSingletonEntity();
            return true;
        }

        public static Entity TryGetFirstReadOnly<T>(this EntityQuery query, Func<T, bool> matcher) 
            where T : struct, IComponentData
        {
            var entity = Entity.Null;
            
            var array = query.ToComponentDataArray<T>(Allocator.TempJob);
            var entities = query.ToEntityArray(Allocator.TempJob);

            for (var i = 0; i < array.Length; i++)
            {
                var entry = array[i];
                if (matcher(entry))
                {
                    entity = entities[i];
                    break;
                }
            }

            array.Dispose();
            entities.Dispose();
            
            return entity;
        }
    }
}