using Nesh.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GenCodes
{
    class XMLEntity : EntityPrefab
    {
        [JsonIgnore]
        public XMLEntity Parent { get; set; }

        [JsonIgnore]
        public List<FieldPrefab> SelfFields { get; set; }

        [JsonIgnore]
        public List<TablePrefab> SelfTables { get; set; }

        public XMLEntity() : base()
        {
            SelfFields = new List<FieldPrefab>();
            SelfTables = new List<TablePrefab>();
        }
    }

    class GenPrefab_JSON
    {
        private static string _DevPath;
        private static string _RuntimePath;

        private static Dictionary<string, XMLEntity> _XMLEntities = new Dictionary<string, XMLEntity>();

        public static void GenCodes()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("\\tool\\");
            directory = directory.Remove(index);

            StringBuilder sb = new StringBuilder(directory);
            sb.Append("/build/dev/entity");
            _DevPath = sb.ToString();

            StringBuilder out_folder = new StringBuilder(directory);
            out_folder.Append("/build/res/entity");
            _RuntimePath = out_folder.ToString();

            Console.WriteLine("-------LoadEntities-------Start-------");
            LoadEntities();
            Console.WriteLine("-------LoadEntities-------End-------");

            Console.WriteLine("-------JSONEntities-------Start-------");

            string JSON = JsonConvert.SerializeObject(_XMLEntities, Newtonsoft.Json.Formatting.Indented);

            string filePath = Path.Combine(_RuntimePath, "entities.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FileInfo fi = new FileInfo(filePath);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

                sw.Write(JSON);
                sw.Close();
                fs.Close();
            }

            Console.WriteLine(JSON);
            Console.WriteLine("-------JSONEntities-------End-------");
        }

        public static List<XMLEntity> GetEntities()
        {
            return _XMLEntities.Values.ToList();
        }

        private static void LoadEntities()
        {
            string sb = Path.Combine(_DevPath, "entities.xml");
            string content = "";
            StreamReader sr = new StreamReader(sb.ToString(), Encoding.UTF8);
            content = sr.ReadToEnd();
            sr.Close();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(content);
            XmlNode root = xml.FirstChild;

            foreach (XmlNode xml_node in xml.FirstChild.ChildNodes)
            {
                LoadEntity(xml_node);
            }

            foreach (string type in _XMLEntities.Keys)
            {
                XMLEntity entity = _XMLEntities[type];

                foreach (FieldPrefab field in entity.SelfFields)
                {
                    entity.fields.Add(field.name, field);
                }

                foreach (TablePrefab table in entity.SelfTables)
                {
                    entity.tables.Add(table.name, table);
                }

                FindAncestor(ref entity, entity.Parent);
            }
        }

        private static void LoadEntity(XmlNode xml_node, XMLEntity parent= null)
        {
            string type = xml_node.Attributes["type"].Value;
            string path = xml_node.Attributes["path"].Value;

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(path))
            {
                Console.WriteLine("{0} Load Fail type or path config error!!", xml_node.Name);
                return;
            }

            string file_name = Path.Combine(_DevPath, path);
            string content = "";
            StreamReader sr = new StreamReader(file_name, Encoding.UTF8);
            content = sr.ReadToEnd();
            sr.Close();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(content);
            XmlNode root = xml.FirstChild;

            XMLEntity entity = new XMLEntity();
            entity.Parent = parent;
            entity.type = type;
            if (xml_node.Attributes["priority"] != null)
            {
                entity.priority = int.Parse(xml_node.Attributes["priority"].Value);
            }

            LoadFields(ref entity, root["fields"]);
            LoadTables(ref entity, root["tables"]);
            LoadIncludes(ref entity, root["includes"]);

            _XMLEntities.Add(entity.type, entity);

            foreach (XmlNode sub_node in xml_node.ChildNodes)
            {
                LoadEntity(sub_node, entity);
            }
        }

        private static void LoadFields(ref XMLEntity entity, XmlNode fields_node)
        {
            if (fields_node == null) return;

            foreach (XmlNode field_node in fields_node.ChildNodes)
            {
                FieldPrefab field_prefab = new FieldPrefab();
                field_prefab.name = field_node.Attributes["name"].Value;
                field_prefab.type = Enum.Parse<VarType>(field_node.Attributes["type"].Value);
                field_prefab.save = bool.Parse(field_node.Attributes["save"].Value);
                field_prefab.sync = bool.Parse(field_node.Attributes["sync"].Value);
                field_prefab.desc = field_node.Attributes["desc"].Value;

                entity.SelfFields.Add(field_prefab);
            }
        }

        private static void LoadTables(ref XMLEntity entity, XmlNode tables_node)
        {
            if (tables_node == null) return;

            foreach (XmlNode table_node in tables_node.ChildNodes)
            {
                TablePrefab table_prefab = new TablePrefab();
                table_prefab.name = table_node.Attributes["name"].Value;
                table_prefab.save = bool.Parse(table_node.Attributes["save"].Value);
                table_prefab.sync = bool.Parse(table_node.Attributes["sync"].Value);
                table_prefab.desc = table_node.Attributes["desc"].Value;
                table_prefab.cols = int.Parse(table_node.Attributes["cols"].Value);

                foreach (XmlNode column_node in table_node.ChildNodes)
                {
                    TablePrefab.ColumnPrefab column = new TablePrefab.ColumnPrefab();
                    column.index = int.Parse(column_node.Attributes["index"].Value);
                    column.type = Enum.Parse<VarType>(column_node.Attributes["type"].Value);
                    column.name = column_node.Attributes["name"].Value;
                    column.desc = column_node.Attributes["desc"].Value;

                    table_prefab.columns.Add(column.index, column);
                }

                
                if (table_prefab.columns.Count != table_prefab.cols ||
                    table_prefab.columns.Keys.ToList()[0] != 0 ||
                    table_prefab.columns.Keys.ToList()[table_prefab.cols-1] != (table_prefab.cols-1))
                {
                    throw new Exception($"{entity.type} table {table_prefab.name} columns config failed");
                }

                entity.SelfTables.Add(table_prefab);
            }
        }

        private static void LoadIncludes(ref XMLEntity entity, XmlNode includes_node)
        {
            if (includes_node == null) return;

            foreach (XmlNode include_node in includes_node.ChildNodes)
            {
                string include_path = Path.Combine(_DevPath, include_node.Attributes["path"].Value);
                StreamReader sr = new StreamReader(include_path, Encoding.UTF8);
                string content = sr.ReadToEnd();
                sr.Close();

                XmlDocument xml = new XmlDocument();
                xml.Load(content);

                LoadFields(ref entity, include_node["fields"]);
                LoadTables(ref entity, include_node["tables"]);
                LoadIncludes(ref entity, include_node["includes"]);
            }
        }

        private static void FindAncestor(ref XMLEntity entity, XMLEntity ancestor)
        {
            if (ancestor != null)
            {
                entity.ancestors.Add(ancestor.type);

                foreach (FieldPrefab field in ancestor.SelfFields)
                {
                    entity.fields.Add(field.name, field);
                }

                foreach (TablePrefab table in ancestor.SelfTables)
                {
                    entity.tables.Add(table.name, table);
                }

                FindAncestor(ref entity, ancestor.Parent);
            }
        }
    }
}
