using Nesh.Core;
using Nesh.Core.Data;
using Nesh.Core.Utils;
using Nesh.Engine.Utils;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        public async Task<long> AddRow(Nuid id, string table_name, NList value)
        {
            if (NList.IsEmpty(value)) return Global.INVALID_ROW;

            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return Global.INVALID_ROW;
                }

                NList result;
                if (!table.TryAddRow(value, out result))
                {
                    return Global.INVALID_ROW;
                }

                long row = result.Get<long>(0);
                NList row_value = result.Get<NList>(1);
                BatchCahceList.Add(NList.New().Add((int)CacheOption.SetRow).Add(id).Add(table_name).Add(row).Add(row_value));

                await CallbackTable(id, table_name, TableEvent.AddRow, result);

                return row;
            }
            else
            {
                if (id.Origin == Identity)
                {
                    long row = IdUtils.RGen.CreateId();
                    if (await SetCacheRow(id, table_name, row, value))
                    {
                        NList result = NList.New();
                        result.Add(row);
                        result.Append(value);

                        await CallbackTable(id, table_name, TableEvent.AddRow, result);

                        return row;
                    }
                    else
                    {
                        return Global.INVALID_ROW;
                    }
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.AddRow(id, table_name, value);
                }
            }
        }

        public async Task<long> FindRow<T>(Nuid id, string table_name, int col, T value)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return Global.INVALID_ROW;
                }

                return table.FindRow(col, value);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await FindCacheRow<T>(id, table_name, col, value);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.FindRow(id, table_name, col, value);
                }
            }
        }

        public async Task SetRowValue(Nuid id, string table_name, long row, NList value)
        {
            if (NList.IsEmpty(value)) return;

            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return;
                }

                NList result;
                if (!table.TrySetRow(row, value, out result))
                {
                    return;
                }

                BatchCahceList.Add(NList.New().Add((int)CacheOption.SetRow).Add(id).Add(table_name).Add(row).Add(value));
                await CallbackTable(id, table_name, TableEvent.SetRow, result);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    if (await SetCacheRow(id, table_name, row, value))
                    {
                        NList result = NList.New();
                        result.Add(row);
                        result.Append(value);
                        await CallbackTable(id, table_name, TableEvent.SetRow, result);
                    }
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.SetRowValue(id, table_name, row, value);
                }
            }
        }

        public async Task SetRowCol<T>(Nuid id, string table_name, long row, int col, T value)
        {
            if (value == null) return;

            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return;
                }

                NList result;
                if (!table.TrySetRowCol(row, col, value, out result))
                {
                    return;
                }

                NList row_value = table.GetRow(row);
                BatchCahceList.Add(NList.New().Add((int)CacheOption.SetRow).Add(id).Add(table_name).Add(row).Add(row_value));
                await CallbackTable(id, table_name, TableEvent.SetCol, result);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    NList row_value = NList.New();
                    NList old_value = await GetCacheRowValue(id, table_name, row);
                    T old_row_value = old_value.Get<T>(col);
                    row_value.Append(old_value);
                    row_value.Set(col, value);

                    if (await SetCacheRow(id, table_name, row, row_value))
                    {
                        NList result = NList.New();
                        result.Add(row);
                        result.Add(col);
                        result.Add(old_row_value);
                        result.Add(value);

                        await CallbackTable(id, table_name, TableEvent.SetCol, result);
                    }
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.SetRowCol(id, table_name, row, col, value);
                }
            }
        }

        public async Task ClearTable(Nuid id, string table_name)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return;
                }

                if (table.IsEmpty) return;

                table.Clear();

                BatchCahceList.Add(NList.New().Add((int)CacheOption.ClearTable).Add(id).Add(table_name));
                await CallbackTable(id, table_name, TableEvent.Clear, NList.Empty);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    if (await ClearCacheTable(id, table_name))
                    {
                        await CallbackTable(id, table_name, TableEvent.Clear, NList.Empty);
                    } 
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.ClearTable(id, table_name);
                }
            }
        }

        public async Task DelRow(Nuid id, string table_name, long row)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return;
                }

                NList result;
                if (!table.TryDelRow(row, out result))
                {
                    return;
                }

                BatchCahceList.Add(NList.New().Add((int)CacheOption.DelRow).Add(id).Add(table_name).Add(row));
                await CallbackTable(id, table_name, TableEvent.DelRow, NList.Empty);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    NList row_value = await GetCacheRowValue(id, table_name, row);
                    if (await DelCacheRow(id, table_name, row))
                    {
                        NList result = NList.New();
                        result.Add(row);
                        result.Append(row_value);
                        await CallbackTable(id, table_name, TableEvent.DelRow, result);
                    }
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    await node.DelRow(id, table_name, row);
                }
            }
        }

        public async Task<INList> GetRowValue(Nuid id, string table_name, long row)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return NList.Empty;
                }

                return table.GetRow(row);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await GetCacheRowValue(id, table_name, row);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.GetRowValue(id, table_name, row);
                }
            }
        }

        public async Task<T> GetRowCol<T>(Nuid id, string table_name, long row, int col)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return default(T);
                }

                return table.GetRowCol<T>(row, col);
            }
            else
            {
                if (id.Origin == Identity)
                {
                    NList row_value = await GetCacheRowValue(id, table_name, row);
                    return row_value.Get<T>(col);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.GetRowCol<T>(id, table_name, row, col);
                }
            }
        }

        public async Task<INList> GetRows(Nuid id, string table_name)
        {
            Entity entity = EntityManager.Get(id);

            if (entity != null)
            {
                Table table = entity.GetTable(table_name);
                if (table == null)
                {
                    return NList.Empty;
                }

                return table.GetRows();
            }
            else
            {
                if (id.Origin == Identity)
                {
                    return await GetCacheRows(id, table_name);
                }
                else
                {
                    INode node = GrainFactory.GetGrain<INode>(id.Origin);
                    return await node.GetRows(id, table_name);
                }
            }
        }
    }
}
