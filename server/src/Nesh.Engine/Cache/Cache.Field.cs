using Microsoft.Extensions.Logging;
using Nesh.Core.Data;
using Nesh.Engine.Utils;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        private async Task<bool> SetCacheField<T>(Nuid id, string field_name, T field_value)
        {
            if (!await CacheExist(id))
            {
                return false;
            }

            bool result = false;
            try
            {
                IRedisDatabase db = GetCache(id);
                string key = CacheUtils.BuildFields(id);
                result = await db.HashSetAsync(key, field_name, field_value);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "'{0} SetCacheField error for field={1} value={2}", id, field_name, field_value);
            }

            return result;
        }

        private async Task<T> GetCacheField<T>(Nuid id, string field_name)
        {
            if (!await CacheExist(id))
            {
                return default(T);
            }

            IRedisDatabase db = GetCache(id);
            string key = CacheUtils.BuildFields(id);
            return await db.HashGetAsync<T>(key, field_name);
        }
    }
}
