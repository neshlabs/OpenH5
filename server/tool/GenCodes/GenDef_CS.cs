using Nesh.Core.Manager;
using System;
using System.IO;
using System.Text;

namespace GenCodes
{
    static class GenDef_CS
    {
        public static void GenCodes()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("\\tool\\");
            directory = directory.Remove(index);

            StringBuilder sb = new StringBuilder(directory);
            sb.Append("/build/res/");

            StringBuilder out_folder = new StringBuilder(directory);

            DefineManager.Load(sb.ToString());

            foreach (entity_def entity_def in DefineManager.GetEntities())
            {
                GenEntityCode(entity_def, out_folder.ToString());
            }
        }

        private static void GenEntityCode(entity_def entity_def, string basepath)
        {
            string entity_path = basepath + GenUtils.RESOURCE_ENTITY_FOLDER;
            string entity_type = GenUtils.FormatName(entity_def.name);
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

            string namespce_object = GenEntityNamespace(entity_def);
            Console.WriteLine(namespce_object);

            string Fields = GenEntityFields(entity_def);
            Console.WriteLine(Fields);

            string Tables = GenEntityTables(entity_def);
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

        private static string GetEntityParentFormat(this entity_def entity_def)
        {
            string parent_name = entity_def.ancestors.Count > 0 ? entity_def.ancestors[0] : "";

            if (string.IsNullOrEmpty(parent_name))
            {
                return "";
            }

            return GenUtils.FormatName(parent_name);
        }

        private static string GetEntitySelfFormat(this entity_def entity_def)
        {
            return GenUtils.FormatName(entity_def.name);
        }

        private static string GenEntityNamespace(entity_def entity_def)
        {
            StringBuilder sb = new StringBuilder();
            string parent_name = entity_def.GetEntityParentFormat();
            string self_name = entity_def.GetEntitySelfFormat();

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
                sb.Append("public const string TYPE = \"").Append(entity_def.name).Append("\";");
            }
            else
            {
                sb.Append("public new const string TYPE = \"").Append(entity_def.name).Append("\";");
            }

            sb.AppendLine();
            sb.Append(GenUtils.str_tab2);

            return sb.ToString();
        }

        private static string GenEntityFields(entity_def entity_def)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GenUtils.str_tab2);

            string parent_name = entity_def.GetEntityParentFormat();

            if (!string.IsNullOrEmpty(parent_name) && entity_def.parent != null && entity_def.parent.fields.Count >= 0)
            {
                sb.Append("public new class Fields").Append(" : ").Append(parent_name).Append(".Fields");
            }
            else
            {
                sb.Append("public class Fields");
            }
            sb.AppendLine();
            sb.Append(GenUtils.str_tab2 + "{");

            foreach (field_def field in entity_def.fields)
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

        private static string GenEntityTables(entity_def entity_def)
        {
            StringBuilder sb = new StringBuilder();

            string parent_name = entity_def.GetEntityParentFormat();

            if (!string.IsNullOrEmpty(parent_name) && entity_def.parent != null && entity_def.parent.tables.Count >= 0)
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

            foreach (table_def table in entity_def.tables)
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

                foreach (table_def.column_def column in table.columns)
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
