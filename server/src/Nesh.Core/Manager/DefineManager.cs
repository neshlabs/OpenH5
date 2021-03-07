using Nesh.Core.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nesh.Core.Manager
{
    public class entity_def
    {
        [JsonIgnore]
        public string name { get; set; }

        [JsonIgnore]
        public int priority { get; set; }

        [JsonIgnore]
        public entity_def parent { get; set; }

        [JsonIgnore]
        public List<string> ancestors { get; set; }

        [JsonIgnore]
        public List<field_def> all_fields { get; set; }

        [JsonIgnore]
        public List<table_def> all_tables { get; set; }

        public List<field_def> fields { get; set; }
        public List<table_def> tables { get; set; }
        public List<string> includes { get; set; }

        public entity_def()
        {
            ancestors = new List<string>();
            fields = new List<field_def>();
            tables = new List<table_def>();
            includes = new List<string>();
            field_dic = new Dictionary<string, field_def>();
            table_dic = new Dictionary<string, table_def>();
            all_fields = new List<field_def>();
            all_tables = new List<table_def>();
        }

        private Dictionary<string, field_def> field_dic;
        private Dictionary<string, table_def> table_dic;
        public void OnFinish()
        {
            foreach (field_def field in all_fields)
            {
                if (!field_dic.ContainsKey(field.name))
                {
                    field_dic.Add(field.name, field);
                }
            }

            foreach (table_def table in all_tables)
            {
                table.columns.Sort((x, y) =>
                {
                    return x.index - y.index;
                });

                if (!table_dic.ContainsKey(table.name))
                {
                    table_dic.Add(table.name, table);
                }
            }
        }

        public field_def GetField(string field_name)
        {
            field_def found;
            field_dic.TryGetValue(field_name, out found);
            return found;
        }

        public table_def GetTable(string table_name)
        {
            table_def found;
            table_dic.TryGetValue(table_name, out found);
            return found;
        }
    }

    public class field_def
    {
        public string name { get; set; }
        public VarType type { get; set; }
        public bool save { get; set; }
        public bool sync { get; set; }
        public string desc { get; set; }
    }

    public class table_def
    {
        public string name { get; set; }
        public bool save { get; set; }
        public bool sync { get; set; }
        public string desc { get; set; }
        public int cols { get; set; }
        public List<column_def> columns { get; set; }

        public class column_def
        {
            public int index { get; set; }
            public string name { get; set; }
            public VarType type { get; set; }
            public string desc { get; set; }
        }

        public table_def()
        {
            columns = new List<column_def>();
        }
    }

    public class entity_element
    {
        public string type { get; set; }

        public string path { get; set; }

        public int priority { get; set; }

        public List<entity_element> children { get; set; }

        public entity_element()
        {
            children = new List<entity_element>();
        }
    }

    public class DefineManager
    {
        public List<entity_element> root;

        private static Dictionary<string, entity_def> _EntityDic = null;
        private static string _ResourcePath = "";
        private const string ENTITIES_PATH = "entity/entities.json";

        public static void Load(string res_path)
        {
            _EntityDic = new Dictionary<string, entity_def>();
            _ResourcePath = res_path;

            string sb = Path.Combine(_ResourcePath, ENTITIES_PATH);

            StreamReader sr = new StreamReader(sb.ToString(), Encoding.UTF8);
            string content = sr.ReadToEnd();
            sr.Close();

            DefineManager defines = JsonConvert.DeserializeObject<DefineManager>(content);

            foreach (entity_element root_node in defines.root)
            {
                LoadNode(root_node, null);
            }

            foreach (entity_def entity_def in _EntityDic.Values)
            {
                List<string> ancestors = new List<string>();
                FindParent(entity_def, ref ancestors);
                entity_def.ancestors = ancestors;

                entity_def.OnFinish();
            }
        }

        private static void FindParent(entity_def entity_def, ref List<string> ancestor)
        {
            if (entity_def.parent != null)
            {
                ancestor.Add(entity_def.parent.name);

                FindParent(entity_def.parent, ref ancestor);
            }
        }

        private static void LoadNode(entity_element root_node, entity_def parent)
        {
            string sb = Path.Combine(_ResourcePath, root_node.path);

            StreamReader sr = new StreamReader(sb.ToString(), Encoding.UTF8);
            string content = sr.ReadToEnd();
            sr.Close();

            entity_def entity_def = JsonConvert.DeserializeObject<entity_def>(content);
            entity_def.priority = root_node.priority;
            entity_def.parent = parent;
            entity_def.name = root_node.type;
            entity_def.all_fields.AddRange(entity_def.fields);
            entity_def.all_tables.AddRange(entity_def.tables);

            if (parent != null)
            {
                entity_def.all_fields.AddRange(parent.fields);
                entity_def.all_tables.AddRange(parent.tables);
            }

            LoadIncludes(ref entity_def, entity_def.includes);

            foreach (entity_element sub_node in root_node.children)
            {
                LoadNode(sub_node, entity_def);
            }

            _EntityDic.Add(root_node.type, entity_def);
        }

        private static void LoadIncludes(ref entity_def entity_def, List<string> includes)
        {
            foreach (string include_path in entity_def.includes)
            {
                string sb = Path.Combine(_ResourcePath, include_path);
                StreamReader sr = new StreamReader(sb, Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();

                entity_def include_entity_def = JsonConvert.DeserializeObject<entity_def>(content);
                entity_def.fields.AddRange(include_entity_def.fields);
                entity_def.tables.AddRange(include_entity_def.tables);
                entity_def.all_fields.AddRange(include_entity_def.fields);
                entity_def.all_tables.AddRange(include_entity_def.tables);

                LoadIncludes(ref entity_def, include_entity_def.includes);
            }
        }

        public static entity_def GetEntity(string entity_type)
        {
            entity_def found;
            _EntityDic.TryGetValue(entity_type, out found);

            return found;
        }

        public static List<entity_def> GetEntities()
        {
            return _EntityDic.Values.ToList();
        }
    }
}
