using Nesh.Core.Data;
using System;
using System.IO;
using System.Text;

namespace GenCodes
{
    static class GenPrefab_CS
    {
        public static void GenCodes()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("\\tool\\");
            directory = directory.Remove(index);

            StringBuilder out_folder = new StringBuilder(directory);

            foreach (XMLEntity entity in GenPrefab_JSON.GetEntities())
            {
                GenEntityCode(entity, out_folder.ToString());
            }
        }

        private static void GenEntityCode(XMLEntity entity, string basepath)
        {
            string entity_path = basepath + GenUtils.RESOURCE_ENTITY_FOLDER;
            string entity_type = GenUtils.FormatName(entity.type);
            string filePath = entity_path + entity_type + ".cs";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FileInfo fi = new FileInfo(filePath);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();

            FileStream fs = new FileStream(filePath, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            StringBuilder sb = new StringBuilder(GenUtils.RESOURCE_ENTITY_NAMESPACE);
            sb.AppendLine();
            sb.Append("{");
            sb.AppendLine();

            sb.Append(GenUtils.str_tab);

            string namespce_object = GenEntityNamespace(entity);
            Console.WriteLine(namespce_object);

            string Fields = GenEntityFields(entity);
            Console.WriteLine(Fields);

            string Tables = GenEntityTables(entity);
            Console.WriteLine(Tables);

            sb.AppendLine(namespce_object);
            sb.AppendLine(Fields);
            sb.AppendLine(Tables);

            //sb.AppendLine();
            sb.Append(GenUtils.str_tab + "}");

            sb.AppendLine();
            sb.Append("}");
            sw.Write(sb.ToString());
            sw.Close();
            fs.Close();
        }

        private static string GetEntityParentFormat(this EntityPrefab entity_prefab)
        {
            string parent_name = entity_prefab.ancestors.Count > 0 ? entity_prefab.ancestors[0] : "";

            if (string.IsNullOrEmpty(parent_name))
            {
                return "";
            }

            return GenUtils.FormatName(parent_name);
        }

        private static string GetEntitySelfFormat(this EntityPrefab entity_prefab)
        {
            return GenUtils.FormatName(entity_prefab.type);
        }

        private static string GenEntityNamespace(EntityPrefab entity_prefab)
        {
            StringBuilder sb = new StringBuilder();
            string parent_name = entity_prefab.GetEntityParentFormat();
            string self_name = entity_prefab.GetEntitySelfFormat();

            if (string.IsNullOrEmpty(parent_name))
            {
                sb.Append("public class ").Append(self_name);
            }
            else
            {
                sb.Append("public class ").Append(self_name).Append(" : ").Append(parent_name);
            }
            sb.AppendLine();
            sb.Append(GenUtils.str_tab + "{");

            sb.AppendLine();
            sb.Append(GenUtils.str_tab2);

            if (string.IsNullOrEmpty(parent_name))
            {
                sb.Append("public const string TYPE = \"").Append(entity_prefab.type).Append("\";");
            }
            else
            {
                sb.Append("public new const string TYPE = \"").Append(entity_prefab.type).Append("\";");
            }

            sb.AppendLine();
            sb.Append(GenUtils.str_tab2);

            return sb.ToString();
        }

        private static string GenEntityFields(XMLEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GenUtils.str_tab2);

            string parent_name = entity.GetEntityParentFormat();

            if (!string.IsNullOrEmpty(parent_name) && entity.Parent != null && entity.Parent.fields.Count >= 0)
            {
                sb.Append("public new class Fields").Append(" : ").Append(parent_name).Append(".Fields");
            }
            else
            {
                sb.Append("public class Fields");
            }
            sb.AppendLine();
            sb.Append(GenUtils.str_tab2 + "{");

            foreach (FieldPrefab field in entity.SelfFields)
            {
                sb.AppendLine();
                sb.Append(GenUtils.str_tab3).Append("/// <summary>").AppendLine();
                sb.Append(GenUtils.str_tab3).AppendFormat("/// {0} {1} Save={2} Sync={3}", field.desc, field.type, field.save, field.sync).AppendLine();
                sb.Append(GenUtils.str_tab3).Append("/// </summary>").AppendLine();

                sb.Append(GenUtils.str_tab3).Append("public const string ").Append(field.name.ToUpper()).Append(" = \"");
                sb.Append(field.name).Append("\";");

                //sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append(GenUtils.str_tab2 + "}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string GenEntityTables(XMLEntity entity)
        {
            StringBuilder sb = new StringBuilder();

            string parent_name = entity.GetEntityParentFormat();

            if (!string.IsNullOrEmpty(parent_name) && entity.Parent != null && entity.Parent.tables.Count >= 0)
            {
                sb.Append(GenUtils.str_tab2 + "public new class Tables").Append(" : ").Append(parent_name).Append(".Tables");

            }
            else
            {
                sb.Append(GenUtils.str_tab2 + "public class Tables");
            }

            sb.AppendLine();
            sb.Append(GenUtils.str_tab2 + "{");
            sb.AppendLine();

            foreach (TablePrefab table in entity.SelfTables)
            {
                string className = FormatSplit(table.name);
                sb.Append(GenUtils.str_tab3).Append("public class ").Append(className.ToString());
                sb.AppendLine();
                sb.Append(GenUtils.str_tab3 + "{");

                sb.AppendLine();
                sb.Append(GenUtils.str_tab4).Append("/// <summary>").AppendLine();
                sb.Append(GenUtils.str_tab4).AppendFormat("/// {0} Save={1} Sync={2}", table.desc, table.save, table.sync).AppendLine();
                sb.Append(GenUtils.str_tab4).Append("/// </summary>");

                sb.AppendLine().Append(GenUtils.str_tab4);
                sb.Append("public const string TABLE_NAME = \"").Append(table.name).Append("\";");

                sb.AppendLine();

                foreach (TablePrefab.ColumnPrefab column in table.columns.Values)
                {
                    sb.AppendLine();
                    sb.Append(GenUtils.str_tab4).Append("/// <summary>").AppendLine();
                    sb.Append(GenUtils.str_tab4).AppendFormat("/// {0} {1}", column.desc, column.type).AppendLine();
                    sb.Append(GenUtils.str_tab4).Append("/// </summary>");

                    sb.AppendLine().Append(GenUtils.str_tab4);
                    sb.Append("public const int COL_").Append(column.name.ToUpper()).Append(" = ").Append(column.index).Append(";");
                }

                sb.AppendLine();
                sb.Append(GenUtils.str_tab3 + "}");
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.Append(GenUtils.str_tab2 + "}")/*.AppendLine()*/;

            return sb.ToString();
        }

        private static string FormatSplit(string split)
        {
            string[] splits = split.Split('_');
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < splits.Length; i++)
            {
                sb.Append(splits[i].Substring(0, 1).ToUpper() + splits[i].Substring(1, splits[i].Length - 1));
            }
            return sb.ToString();
        }
    }
}
