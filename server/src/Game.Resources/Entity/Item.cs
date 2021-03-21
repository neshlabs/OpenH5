namespace Game.Resources.Entity
{
    public class Item
    {
        public const string TYPE = "item";
        
        public class Fields
        {
            /// <summary>
            /// 道具编号 Int Save=True Sync=True
            /// </summary>
            public const string ENTRY = "entry";
            /// <summary>
            /// 堆叠数量 Int Save=True Sync=True
            /// </summary>
            public const string COUNT = "count";
        }

        public class Tables
        {
            public class StarTable
            {
                /// <summary>
                /// 强化星级表 Save=True Sync=True
                /// </summary>
                public const string TABLE_NAME = "star_table";

                /// <summary>
                /// 强化编号 Int
                /// </summary>
                public const int COL_ID = 0;
                /// <summary>
                /// 强化等级 Int
                /// </summary>
                public const int COL_LEVEL = 1;
            }

        }
    }
}