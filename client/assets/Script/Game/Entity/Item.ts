export namespace Item
{
    const TYPE: string = "item";
    
    export class Fields
    {
        public static readonly ENTRY: string = "entry";

        public static readonly COUNT: string = "count";
    }

    export namespace Tables
    {
        export class StarTable
        {
            public static readonly TABLE_NAME: string = "star_table";

            public static readonly COL_ID: number = 0;

            public static readonly COL_LEVEL: number = 1;
        }
    }
}