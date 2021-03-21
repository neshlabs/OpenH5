using MongoDB.Bson;
using MongoDB.Driver;
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
        private async Task PushPersistFields(Entity entity)
        {
            EntityPrefab entity_prefab = Prefabs.GetEntity(entity.Type);
            if (entity_prefab == null)
            {
                return;
            }

            var database = _IMongoClient.GetDatabase(PersistUtils.ENTITY_DB);

            var collection = database.GetCollection<BsonDocument>(entity.Type);

            Dictionary<string, object> models = new Dictionary<string, object>();
            models.Add(Global.MARK_ORIGIN, entity.Id.Origin);
            models.Add(Global.MARK_UNIQUE, entity.Id.Unique);

            Field[] fields = entity.GetFields();
            foreach (Field field in fields)
            {
                FieldPrefab field_prefab = entity_prefab.fields[field.Name];
                if (field_prefab == null)
                {
                    continue;
                }
                if (!field_prefab.save)
                {
                    continue;
                }

                switch (field_prefab.type)
                {
                    case VarType.Bool:
                        {
                            models.Add(field_prefab.name, field.Get<bool>());
                        }
                        break;
                    case VarType.Int:
                        {
                            models.Add(field_prefab.name, field.Get<int>());
                        }
                        break;
                    case VarType.Float:
                        {
                            models.Add(field_prefab.name, field.Get<float>());
                        }
                        break;
                    case VarType.Long:
                        {
                            models.Add(field_prefab.name, field.Get<long>());
                        }
                        break;
                    case VarType.Nuid:
                        {
                            BsonDocument document = BsonDocument.Parse(JsonUtils.ToJson(field.Get<Nuid>()));
                            models.Add(field_prefab.name, document);
                        }
                        break;
                    case VarType.Time:
                        {
                            models.Add(field_prefab.name, field.Get<DateTime>());
                        }
                        break;
                    case VarType.String:
                        {
                            models.Add(field_prefab.name, field.Get<string>());
                        }
                        break;
                    case VarType.List:
                        {
                            BsonDocument document = BsonDocument.Parse(JsonUtils.ToJson(field.Get<NList>()));
                            models.Add(field_prefab.name, document);
                        }
                        break;
                }
            }

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;

            FilterDefinition<BsonDocument> filter = builder.And(builder.Eq("unique", entity.Id.Unique), builder.Eq("origin", entity.Id.Origin));

            var found = await collection.FindOneAndUpdateAsync(filter, new BsonDocument(models));
            if (found == null)
            {
                await collection.InsertOneAsync(new BsonDocument(models));
            }
        }

        private async Task PullPersistFields(string entity_type)
        {
            var database = _IMongoClient.GetDatabase(PersistUtils.ENTITY_DB);

            var collection = database.GetCollection<BsonDocument>(entity_type);

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;

            FilterDefinition<BsonDocument> filter = builder.And(builder.Eq("unique", Identity), builder.Eq("origin", Identity));
            //获取数据
            var result = (await collection.FindAsync<BsonDocument>(filter)).ToList();

            EntityPrefab entity_prefab = Prefabs.GetEntity(entity_type);

            foreach (BsonDocument doc in result)
            {
                long unique = doc.GetValue("unique").AsInt64;
                long origin = doc.GetValue("origin").AsInt64;
                Entity entity = EntityManager.Create(Nuid.New(unique, origin), entity_type);

                foreach (FieldPrefab field_prefab in entity_prefab.fields.Values)
                {
                    if (!field_prefab.save)
                    {
                        continue;
                    }

                    Field field = entity.GetField(field_prefab.name);
                    if (field == null)
                    {
                        continue;
                    }

                    BsonValue bsonValue = doc.GetValue(field_prefab.name);
                    switch (field_prefab.type)
                    {
                        case VarType.Bool:
                            {
                                bool value = bsonValue.AsBoolean;
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.Int:
                            {
                                int value = bsonValue.AsInt32;
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.Long:
                            {
                                long value = bsonValue.AsInt64;
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.Float:
                            {
                                float value = (float)bsonValue.AsDouble;
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.String:
                            {
                                string value = bsonValue.AsString;
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.Time:
                            {
                                DateTime value = bsonValue.AsBsonDateTime.ToUniversalTime();
                                field.TrySet(value, out NList res);
                            }
                            break;
                        case VarType.Nuid:
                            {
                                string value = bsonValue.AsBsonDocument.ToJson();
                                Nuid nuid = JsonUtils.ToObject<Nuid>(value);
                                field.TrySet(nuid, out NList res);
                            }
                            break;
                        case VarType.List:
                            {
                                string value = bsonValue.AsBsonDocument.ToJson();
                                NList lst = JsonUtils.ToObject<NList>(value);
                                field.TrySet(lst, out NList res);
                            }
                            break;
                    }
                }
            }

            //int count = await collection.CountAsync();
            //BsonDocument b = collection.AsQueryable();
            //var list = (await collection.FindAsync(x => x.GetValue(Global.MARK_UNIQUE) == Identity)).ToList();
        }
    }
}
