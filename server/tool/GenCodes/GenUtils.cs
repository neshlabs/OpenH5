namespace GenCodes
{
    class GenUtils
    {
        public static string PROJECT_NAME { get { return "Game"; } }
        public static string PROJECT_NAMESPACE { get { return "namespace " + PROJECT_NAME; } }
        public static string PROJECT_FOLDER { get { return "src"; } }

        public const string PATH_SPLIT_CHAR = "/";
        public const string FILTER_SPLIT_CHAR = "\\";

        public static string RESOURCE { get { return PROJECT_NAME + ".Resources"; } }

        public static string RESOURCE_ENTITY_FOLDER      { get { return "/" + PROJECT_FOLDER + "/" + RESOURCE + "/" + "Entity" + "/"; } }
        public static string RESOURCE_MONGO_MODEL_FOLDER { get { return "/" + PROJECT_FOLDER + "/" + RESOURCE + "/" + "Model" + "/"; } }

        public static string RESOURCE_NAMESPACE             { get { return PROJECT_NAMESPACE + ".Resources"; } }
        public static string RESOURCE_ENTITY_NAMESPACE      { get { return RESOURCE_NAMESPACE + ".Entity"; } }
        public static string RESOURCE_MONGO_MODEL_NAMESPACE { get { return RESOURCE_NAMESPACE + ".Model"; } }

        public const string str_tab = "    ";
        public const string str_tab2 = str_tab + str_tab;
        public const string str_tab3 = str_tab2 + str_tab;
        public const string str_tab4 = str_tab3 + str_tab;
        public const string str_tab5 = str_tab4 + str_tab;
        public const string str_tab6 = str_tab5 + str_tab;
        public const string str_tab7 = str_tab6 + str_tab;

        public static string FormatName(string name)
        {
            string[] object_temps = name.Split('_');
            string object_name = "";
            for (int i = 0; i < object_temps.Length; i++)
            {
                string temp = object_temps[i];
                temp = temp.Substring(0, 1).ToUpper() + temp.Substring(1, temp.Length - 1);

                object_name += temp;
            }

            return object_name;
        }
    }
}
