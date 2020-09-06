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
    }
}