using Nesh.Core.Manager;
using System;
using System.Text;

namespace GenCodes
{
    static class GenDef_TS
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
            }
        }
    }
}
