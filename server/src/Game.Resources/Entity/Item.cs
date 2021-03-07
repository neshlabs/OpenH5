namespace Game.Resources.Entity
{
    public class Item
    {
        public const string TYPE = "item";
        
        public class Fields
        {
            /// <summary>
            /// 编号 Int Save=True Sync=True
            /// </summary>
            public const string ENTRY = "entry";
            /// <summary>
            /// 数量 Int Save=True Sync=True
            /// </summary>
            public const string COUNT = "count";
        }

        public class Tables
        {
            public class StarTable
            {
                /// <summary>
                /// 强化表 Save=True Sync=True
                /// </summary>
                public const string TABLE_NAME = "star_table";

                /// <summary>
                /// 编号 Int
                /// </summary>
                public const int COL_ID = 0;
                /// <summary>
                /// 星 Int
                /// </summary>
                public const int COL_STAR = 1;
            }

        }
    }
}