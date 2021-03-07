using MongoDB.Bson;
using Nesh.Core.Data;
using Nesh.Core.Manager;
using Nesh.Core.Utils;
using Nesh.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        private async Task PushPersistTables(Entity entity)
        {
            entity_def entity_def = DefineManager.GetEntity(entity.Type);
            if (entity_def == null)
            {
                return;
            }

            var database = _IMongoClient.GetDatabase(PersistUtils.EntityDB);

            Table[] tables = entity.GetTables();
            foreach (Table table in tables)
            {
                var collection = database.GetCollection<BsonDocument>(table.Name);

                table_def table_def = entity_def.GetTable(table.Name);
                if (table_def == null)
                {
                    continue;
                }

                if (!table_def.save)
                {
                    continue;
                }

                NList rows = table.GetRows();
                for (int i = 0; i < rows.Count; i++)
                {
                    long row = rows.Get<long>(i);
                    NList row_value = table.GetRow(row);

                    Dictionary<string, object> models = new Dictionary<string, object>();
                    models.Add(Global.MARK_ORIGIN, entity.Id.Origin);
                    models.Add(Global.MARK_UNIQUE, entity.Id.Unique);
                    models.Add(Global.MARK_ROW, row);

                    for (int col = 0; col < table_def.cols; col++)
                    {
                        table_def.column_def column = table_def.columns[col];
                        switch (column.type)
                        {
                            case VarType.Bool:
                                models.Add(column.name, row_value.Get<bool>(col));
                                break;
                            case VarType.Int:
                                models.Add(column.name, row_value.Get<int>(col));
                                break;
                            case VarType.Float:
                                models.Add(column.name, row_value.Get<float>(col));
                                break;
                            case VarType.Long:
                                models.Add(column.name, row_value.Get<long>(col));
                                break;
                            case VarType.Time:
                                models.Add(column.name, row_value.Get<DateTime>(col));
                                break;
                            case VarType.Nuid:
                                models.Add(column.name, BsonDocument.Parse(JsonUtils.ToJson(row_value.Get<Nuid>(col))));
                                break;
                            case VarType.String:
                                models.Add(column.name, row_value.Get<string>(col));
                                break;
                            case VarType.List:
                                models.Add(column.name, BsonDocument.Parse(JsonUtils.ToJson(row_value.Get<NList>(col))));
                                break;
                        }
                    }

                    await collection.InsertOneAsync(new BsonDocument(models));
                }
            }
        }
    }
}
