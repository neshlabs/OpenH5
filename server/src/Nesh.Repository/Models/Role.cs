using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Nesh.Repository.Models
{
    public class Role
    {
        [BsonId]
        public long id { get; set; }

        public Guid user_id { get; set; }

        public int realm_id { get; set; }

        public DateTime create_time { get; set; }
    }
}
