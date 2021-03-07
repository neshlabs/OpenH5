﻿namespace Game.Resources.Entity
{
    public class Equip : Item
    {
        public new const string TYPE = "equip";
        
        public new class Fields : Item.Fields
        {
            /// <summary>
            /// 编号 Int Save=True Sync=True
            /// </summary>
            public const string COLOR = "color";
        }

        public new class Tables : Item.Tables
        {

        }
    }
}