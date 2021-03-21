export namespace Player
{
    export const TYPE: string = "player";
    
    export namespace Fields
    {
        export const LEVEL: string = "level";

        export const NICK_NAME: string = "nick_name";
    }

    export namespace Tables
    {
        export namespace ObjectiveTable
        {
            export const TABLE_NAME: string = "objective_table";

            export const COL_ID: number = 0;

            export const COL_STATUS: number = 1;
        }
    }
}
