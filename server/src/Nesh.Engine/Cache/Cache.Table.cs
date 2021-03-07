using Microsoft.Extensions.Logging;
using Nesh.Core.Data;
using Nesh.Engine.Utils;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        private async Task<NList> GetCacheRows(Nuid id, string table_name)
        {
            if (!await CacheExist(id))
            {
                return NList.Empty;
            }

            IRedisDatabase db = GetCache(id);
            string key = CacheUtils.BuildTable(id, table_name);

            NList rows = NList.New();
            foreach (string hash_key in await db.HashKeysAsync(key))
            {
                int row = int.Parse(hash_key);
                rows.Add(row);
            }

            return rows.Count > 0 ? rows : NList.Empty;
        }

        private async Task<NList> GetCacheRowValue(Nuid id, string table_name, long row)
        {
            if (!await CacheExist(id))
            {
                return NList.Empty;
            }

            IRedisDatabase db = GetCache(id);
            string key = CacheUtils.BuildTable(id, table_name);
            return await db.HashGetAsync<NList>(key, row.ToString());
        }

        private async Task<bool> SetCacheRow(Nuid id, string table_name, long row, NList row_value)
        {
            if (!await CacheExist(id))
            {
                return false;
            }

            bool result = false;
            try
            {
                IRedisDatabase db = GetCache(id);
                string key = CacheUtils.BuildTable(id, table_name);
                result = await db.HashSetAsync(key, row.ToString(), row_value);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "'{0} SetCacheRow error for table={1} row={2} value={3}", id, table_name, row, row_value);
            }

            return result;
        }

        private async Task<bool> DelCacheRow(Nuid id, string table_name, long row)
        {
            if (!await CacheExist(id))
            {
                return false;
            }

            bool result = false;
            try
            {
                IRedisDatabase db = GetCache(id);
                string key = CacheUtils.BuildTable(id, table_name);
                result = await db.HashDeleteAsync(key, row.ToString());
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "'{0} DelCacheRow error for table={1} row={2}", id, table_name, row);
            }

            return result;
        }

        private async Task<bool> ClearCacheTable(Nuid id, string table_name)
        {
            if (!await CacheExist(id))
            {
                return false;
            }

            bool result = false;
            try
            {
                IRedisDatabase db = GetCache(id);
                string key = CacheUtils.BuildTable(id, table_name);
                result = await db.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "'{0} ClearCacheTable error for table={1}", id, table_name);
            }

            return result;
        }

        private async Task<long> FindCacheRow<T>(Nuid id, string table_name, int col, T value)
        {
            if (!await CacheExist(id))
            {
                return Global.INVALID_ROW;
            }

            long row = Global.INVALID_ROW;
            try
            {
                IRedisDatabase db = GetCache(id);
                string key = CacheUtils.BuildTable(id, table_name);
                Dictionary<string, NList> row_values = await db.HashGetAllAsync<NList>(key);
                foreach (KeyValuePair<string, NList> pair in row_values)
                {
                    if (pair.Value.Get<T>(col).Equals(value))
                    {
                        row = long.Parse(pair.Key);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "'{0} FindCacheRow error for table={1}", id, table_name);
            }

            return row;
        }
    }
}
