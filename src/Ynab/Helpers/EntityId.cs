using System;

namespace Ynab.Helpers
{
    public static class EntityId
    {
        public static string CreateNew()
        {
            return Guid.NewGuid().ToString("D").ToUpper();
        }
    }
}