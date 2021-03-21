using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nesh.Core.Data
{
    public class EntityPrefab
    {
        public string type { get; set; }

        public int priority { get; set; }

        public Dictionary<string, FieldPrefab> fields { get; set; }

        public Dictionary<string, TablePrefab> tables { get; set; }

        public List<string> ancestors { get; set; }

        public EntityPrefab()
        {
            fields = new Dictionary<string, FieldPrefab>();
            tables = new Dictionary<string, TablePrefab>();
            ancestors = new List<string>();
        }
    }

    public class FieldPrefab
    {
        public string name { get; set; }

        public VarType type { get; set; }

        public bool save { get; set; }

        public bool sync { get; set; }

        public string desc { get; set; }
    }

    public class TablePrefab
    {
        public string name { get; set; }

        public bool save { get; set; }

        public bool sync { get; set; }

        public string desc { get; set; }

        public int cols { get; set; }

        public SortedDictionary<int, ColumnPrefab> columns { get; set; }

        public class ColumnPrefab
        {
            public int index { get; set; }
            public string name { get; set; }
            public VarType type { get; set; }
            public string desc { get; set; }
        }

        public TablePrefab()
        {
            columns = new SortedDictionary<int, ColumnPrefab>();
        }
    }

    public class Prefabs
    {
        private static Dictionary<string, EntityPrefab> entities = null;
        private static string _ResourcePath = "";
        private const string ENTITIES_PATH = "entity/entities.json";
        public static string JSON = "";

        public static void Load(string res_path)
        {
            _ResourcePath = res_path; 

            string sb = Path.Combine(_ResourcePath, ENTITIES_PATH);

            StreamReader sr = new StreamReader(sb.ToString(), Encoding.UTF8);
            JSON = sr.ReadToEnd();
            sr.Close();

            entities = JsonConvert.DeserializeObject<Dictionary<string, EntityPrefab>>(JSON);
        }

        public static EntityPrefab GetEntity(string entity_type)
        {
            EntityPrefab found;
            entities.TryGetValue(entity_type, out found);

            return found;
        }

        public static List<EntityPrefab> GetEntities()
        {
            return entities.Values.ToList();
        }
    }
}
