using Nesh.Core.Data;
using System;
using System.IO;
using System.Text;

namespace GenCodes
{
    static class GenPrefab_TS
    {
        public static void GenCodes()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("\\tool\\");
            directory = directory.Remove(index);

            StringBuilder out_folder = new StringBuilder(directory);
            out_folder.Append("/build/res/ts/");

            foreach (XMLEntity entity in GenPrefab_JSON.GetEntities())
            {
                GenEntityCode(entity, out_folder.ToString());
            }
        }

        private static void GenEntityCode(XMLEntity entity, string basepath)
        {
            string entity_path = basepath;
            string entity_type = GenUtils.FormatName(entity.type);
            string filePath = entity_path + entity_type + ".ts";

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
            StringBuilder sb = new StringBuilder();

            string namespce_object = GenEntityNamespace(entity);
            Console.WriteLine(namespce_object);

            string Fields = GenEntityFields(entity);
            Console.WriteLine(Fields);

            string Tables = GenEntityTables(entity);
            Console.WriteLine(Tables);

            sb.AppendLine(namespce_object);
            sb.AppendLine(Fields);
            sb.AppendLine(Tables);

            sb.Append("}");

            sb.AppendLine();
            sw.Write(sb.ToString());
            sw.Close();
            fs.Close();
        }

        private static string GetEntitySelfFormat(this EntityPrefab entity_prefab)
        {
            return GenUtils.FormatName(entity_prefab.type);
        }

        private static string GenEntityNamespace(EntityPrefab entity_prefab)
        {
            StringBuilder sb = new StringBuilder();
            string self_name = entity_prefab.GetEntitySelfFormat();

            sb.Append($"export namespace {self_name}").AppendLine();
            sb.Append("{").AppendLine();

            sb.Append(GenUtils.str_tab).Append($"export const TYPE: string = \"{entity_prefab.type}\";").AppendLine();

            return sb.ToString();
        }

        private static string GenEntityFields(XMLEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GenUtils.str_tab).Append("export namespace Fields").AppendLine();

            sb.Append(GenUtils.str_tab + "{").AppendLine();

            foreach (FieldPrefab field in entity.fields.Values)
            {
                sb.Append(GenUtils.str_tab2).Append("export const ").Append(field.name.ToUpper()).Append($": string = \"{field.name}\";").AppendLine();
                sb.AppendLine();
            }

            sb.Append(GenUtils.str_tab + "}").AppendLine();

            return sb.ToString();
        }

        private static string GenEntityTables(XMLEntity entity)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GenUtils.str_tab).Append("export namespace Tables").AppendLine();
            sb.Append(GenUtils.str_tab).Append("{").AppendLine();

            foreach (TablePrefab table in entity.tables.Values)
            {
                string className = FormatSplit(table.name);
                sb.Append(GenUtils.str_tab2).Append("export namespace ").Append(className.ToString()).AppendLine();
                sb.Append(GenUtils.str_tab2 + "{").AppendLine();

                sb.Append(GenUtils.str_tab3).Append($"export const TABLE_NAME = \"{table.name}\";").AppendLine();

                foreach (TablePrefab.ColumnPrefab column in table.columns.Values)
                {
                    sb.AppendLine();
                    sb.Append(GenUtils.str_tab3).Append($"export const COL_{column.name.ToUpper()}: number = {column.index};").AppendLine();
                }

                sb.AppendLine();
                sb.Append(GenUtils.str_tab2 + "}").AppendLine();
                sb.AppendLine();
            }

            sb.Append(GenUtils.str_tab + "}")/*.AppendLine()*/;

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
