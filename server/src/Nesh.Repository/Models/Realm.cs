using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Nesh.Repository.Models
{
    public class Realm
    {
        [BsonId]
        public int id { get; set; }

        public string name { get; set; }

        public DateTime create_time { get; set; }
    }
}
