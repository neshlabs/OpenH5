namespace Game.Resources.Entity
{
    public class Player
    {
        public const string TYPE = "player";
        
        public class Fields
        {
            /// <summary>
            /// 等级 Int Save=True Sync=True
            /// </summary>
            public const string LEVEL = "level";
            /// <summary>
            /// 名字 String Save=True Sync=True
            /// </summary>
            public const string NICK_NAME = "nick_name";
        }

        public class Tables
        {
            public class ObjectiveTable
            {
                /// <summary>
                /// 目标表 Save=True Sync=True
                /// </summary>
                public const string TABLE_NAME = "objective_table";

                /// <summary>
                /// 编号 Int
                /// </summary>
                public const int COL_ID = 0;
                /// <summary>
                /// 状态 Int
                /// </summary>
                public const int COL_STATUS = 1;
            }
            public class QuestTable
            {
                /// <summary>
                /// 任务表 Save=True Sync=True
                /// </summary>
                public const string TABLE_NAME = "quest_table";

                /// <summary>
                /// 编号 Int
                /// </summary>
                public const int COL_ID = 0;
                /// <summary>
                /// 状态 Int
                /// </summary>
                public const int COL_STATUS = 1;
                /// <summary>
                /// 接受时间 Time
                /// </summary>
                public const int COL_ACCEPT_TIME = 2;
            }

        }
    }
}