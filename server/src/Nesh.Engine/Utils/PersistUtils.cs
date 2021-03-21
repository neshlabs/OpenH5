using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Nesh.Engine.Utils
{
    public class persist_entity
    {
        public long unique { get; set; }

        public string type { get; set; }
    }

    public class persist_entity_list
    {
        [BsonId]
        public long origin { get; set; }

        public List<persist_entity> entities { get; set; }
    }

    public static class PersistUtils
    {
        public const string ENTITY_DB = "entity";

        public const string ENTITIES = "entities";
    }
}
