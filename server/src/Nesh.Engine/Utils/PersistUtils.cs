using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nesh.Core.Data;
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
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public long origin { get; set; }

        public List<persist_entity> entities { get; set; }
    }

    public static class PersistUtils
    {
        public class NuidSerializer : StructSerializerBase<Nuid>
        {
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Nuid value)
            {
                var writer = context.Writer;
                writer.WriteStartDocument();

                writer.WriteName(nameof(value.Origin));
                writer.WriteInt64(value.Origin);

                writer.WriteName(nameof(value.Unique));
                writer.WriteInt64(value.Unique);

                writer.WriteEndDocument();
            }

            public override Nuid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var reader = context.Reader;
                reader.ReadStartDocument();


                long origin = Global.NULL_LONG;
                long unique = Global.NULL_LONG;

                while (reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    var name = reader.ReadName();
                    switch (name)
                    {
                        case nameof(Nuid.Origin):
                            origin = reader.ReadInt64();
                            break;
                        case nameof(Nuid.Unique):
                            unique = reader.ReadInt64();
                            break;
                        default:
                            break;
                    }
                }

                var value = Nuid.New(unique, origin);

                reader.ReadEndDocument();
                return value;
            }
        }

        public static string EntityDB { get; set; }

        public static string Connection { get; set; }

        public static void Init()
        {
            //BsonSerializer.RegisterSerializer(new NuidSerializer());
        }
    }
}
