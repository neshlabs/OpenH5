using MongoDB.Bson;
using MongoDB.Driver;
using Nesh.Core.Data;
using Nesh.Core.Manager;
using Nesh.Engine.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nesh.Engine.Node
{
    public partial class Node
    {
        private async Task Persist()
        {
            IReadOnlyList<Entity> entities = EntityManager.GetEntities();
                    
            persist_entity_list persist_entity_list = new persist_entity_list();
            persist_entity_list.entities = new List<persist_entity>();
            persist_entity_list.origin = Identity;

            foreach (Entity entity in entities)
            {
                persist_entity_list.entities.Add(new persist_entity() { unique = entity.Id.Unique, type = entity.Type });
            }

            var database = _IMongoClient.GetDatabase(PersistUtils.EntityDB);
            var collection = database.GetCollection<persist_entity_list>("entities");

            var filter = Builders<persist_entity_list>.Filter.And(Builders<persist_entity_list>.Filter.Eq(n=> n.origin, Identity));
            persist_entity_list found = await collection.FindOneAndReplaceAsync(filter, persist_entity_list);
            if (found == null)
            {
                await collection.InsertOneAsync(persist_entity_list);
            }

            foreach (Entity entity in entities)
            {
                await PushPersistFields(entity);

                await PushPersistTables(entity);
            }
        }
    }
}
