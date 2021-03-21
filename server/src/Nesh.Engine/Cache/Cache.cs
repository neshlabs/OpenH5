using Microsoft.Extensions.Logging;
using Nesh.Core.Data;
using Nesh.Core.Manager;
using Nesh.Core.Utils;
using Nesh.Engine.Utils;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    enum CacheOption : int
    {
        SetEntity  = 1,
        DelEntity  = 2,
        SetField   = 3,
        ClearTable = 4,
        DelRow     = 5,
        SetRow     = 6,
    }

    public partial class Node
    {
        private IRedisDatabase GetCache(Nuid id)
        {
            int db = (int)(id.Origin % CacheUtils.EntityDBs);
            return _CacheClient.GetDb(db);
        }

        private async Task<bool> CacheExist(Nuid id)
        {
            IRedisDatabase db = GetCache(id);
            string key = CacheUtils.BuildEntities(id);

            return await db.HashExistsAsync(key, id.Unique.ToString());
        }

        private async Task<string> GetCacheType(Nuid id)
        {
            IRedisDatabase db = GetCache(id);
            string key = CacheUtils.BuildEntities(id);

            return await db.HashGetAsync<string>(key, id.Unique.ToString());
        }

        private async Task BatchCache(object arg)
        {
            if (BatchCahceList.Count <= 0) return;

            try
            {
                int db = (int)(Identity % CacheUtils.EntityDBs);
                IRedisDatabase redis = _CacheClient.GetDb(db);

                ITransaction trans = redis.Database.CreateTransaction();

                foreach (NList batch in BatchCahceList)
                {
                    CacheOption option = (CacheOption)batch.Get<int>(0);
                    Nuid entity_id = batch.Get<Nuid>(1);

                    switch (option)
                    {
                        case CacheOption.SetEntity:
                            {
                                string entity_type = batch.Get<string>(2);
                                string key = CacheUtils.BuildEntities(entity_id);
                                Task task = trans.HashSetAsync(key, entity_id.Unique.ToString(), entity_type);
                            }
                            break;
                        case CacheOption.DelEntity:
                            {
                                string entity_type = batch.Get<string>(2);
                                EntityPrefab entity_prefab = Prefabs.GetEntity(entity_type);
                                if (entity_prefab == null)
                                {
                                    continue;
                                }

                                foreach (TablePrefab table_prefab in entity_prefab.tables.Values)
                                {
                                    string table_key = CacheUtils.BuildTable(entity_id, table_prefab.name);
                                    Task table_task = trans.KeyDeleteAsync(table_key);
                                }

                                string field_key = CacheUtils.BuildFields(entity_id);
                                Task field_task = trans.KeyDeleteAsync(field_key);

                                string entity_key = CacheUtils.BuildEntities(entity_id);
                                Task entity_task = trans.HashDeleteAsync(entity_key, entity_id.Unique.ToString());
                            }
                            break;
                        case CacheOption.SetField:
                            {
                                string field_name = batch.Get<string>(2);
                                byte[] field_value = batch.Get<byte[]>(3);

                                string key = CacheUtils.BuildFields(entity_id);
                                Task task = trans.HashSetAsync(key, field_name, field_value);
                            }
                            break;
                        case CacheOption.SetRow:
                            {
                                string table_name = batch.Get<string>(2);
                                long row = batch.Get<long>(3);
                                NList row_value = batch.Get<NList>(4);

                                string key = CacheUtils.BuildTable(entity_id, table_name);
                                Task task = trans.HashSetAsync(key, row, ProtoUtils.Serialize(row_value));
                            }
                            break;
                        case CacheOption.DelRow:
                            {
                                string table_name = batch.Get<string>(2);
                                long row = batch.Get<long>(3);

                                string key = CacheUtils.BuildTable(entity_id, table_name);
                                Task task = trans.HashDeleteAsync(key, row);
                            }
                            break;
                        case CacheOption.ClearTable:
                            {
                                string table_name = batch.Get<string>(2);

                                string key = CacheUtils.BuildTable(entity_id, table_name);
                                Task task = trans.KeyDeleteAsync(key);
                            }
                            break;
                        default:
                            break;
                    }
                }

                bool result = await trans.ExecuteAsync();
                if (result)
                {
                    BatchCahceList.Clear();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, string.Format("{0} BatchCache ExecuteAsync Failed", Identity));
            }
        }
    }
}
