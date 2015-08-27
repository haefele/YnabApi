using System;

namespace YnabApi.Helpers
{
    public static class EntityId
    {
        public static string CreateNew()
        {
            return Guid.NewGuid().ToString("D").ToUpper();
        }
    }
}