using Nesh.Core.Data;
using System.Text;

namespace Nesh.Engine.Utils
{
    public static class CacheUtils
    {
        public static int EntityDBs = 4;

        public const string MARK_FIELDS = "fields";
        public const string MARK_TABLES = "tables";
        public const string MARK_ENTITIES = "entities";

        public static string BuildEntities(Nuid entity_id)
        {
            StringBuilder key = new StringBuilder();
            key.Append(entity_id.Origin);
            key.Append(Global.NUID_MEMBERS_FLAG);
            key.Append(MARK_ENTITIES);

            return key.ToString();
        }

        private static string BuildEntity(Nuid entity_id)
        {
            StringBuilder key = new StringBuilder();
            key.Append(entity_id.Origin);
            key.Append(Global.NUID_MEMBERS_FLAG);
            key.Append(entity_id.Unique);

            return key.ToString();
        }

        public static string BuildFields(Nuid entity_id)
        {
            StringBuilder key = new StringBuilder();
            key.Append(BuildEntity(entity_id));
            key.Append(Global.NUID_MEMBERS_FLAG);
            key.Append(MARK_FIELDS);

            return key.ToString();
        }

        public static string BuildTable(Nuid entity_id, string table_name)
        {
            StringBuilder key = new StringBuilder();
            key.Append(BuildEntity(entity_id));
            key.Append(Global.NUID_MEMBERS_FLAG);
            key.Append(MARK_TABLES);
            key.Append(Global.NUID_MEMBERS_FLAG);
            key.Append(table_name);

            return key.ToString();
        }
    }
}
