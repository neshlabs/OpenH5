export namespace Equip
{
    const TYPE: string = "player";
    
    export class Fields
    {
        public static readonly LEVEL: string = "level";

        public static readonly NICK_NAME: string = "nick_name";
    }

    export namespace Tables
    {
        export class ObjectiveTable
        {
            public static readonly TABLE_NAME: string = "objective_table";

            public static readonly COL_ID: number = 0;

            public static readonly COL_STATUS: number = 1;
        }
    }
}
