import { NEvents } from "./NEvents";
import NList, { Nuid, Field, Table, Entity } from "./NList";

export interface INode
{
    RCEntity(entity_type: string, entity_event: EntityEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;
    RCField(entity_type: string, field_name: string, field_event: FieldEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;
    RCTable(entity_type: string, table_name: string, table_event: TableEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;

    /*GetFieldBool(nuid: Nuid, field_name: string) : boolean;
    GetFieldInt(nuid: Nuid, field_name: string) : number;
    GetFieldLong(nuid: Nuid, field_name: string) : number;
    GetFieldFloat(nuid: Nuid, field_name: string) : number;
    GetFieldString(nuid: Nuid, field_name: string) : string;
    GetFieldTime(nuid: Nuid, field_name: string) : Date;
    GetFieldNuid(nuid: Nuid, field_name: string) : Nuid;
    GetFieldList(nuid: Nuid, field_name: string) : NList;

    FindRowBool(nuid: Nuid, table_name: string, col: number, value: boolean) : number;

    GetRowColBool(nuid: Nuid, table_name: string, row: number, col: number) : boolean;*/
}

export enum FieldEvent
{
    Create    = 1,
    Change    = 2,
};

export enum TableEvent
{
    Create    = 1,
    AddRow    = 2,
    DelRow    = 3,
    SetRow    = 4,
    SetCol    = 5,
    Clear     = 6,
};

export enum EntityEvent
{
    OnCreate  = 1,
    OnDestroy = 2,
    OnLoad    = 3,
    OnEntry   = 4,
    OnLeave   = 5,
}

export class ClientNode implements INode 
{
    public static readonly NULL_BOOL: boolean = false;
    public static readonly INVALID_ROW: number = -1;
    private _Entitas: Map<string, Entity>;
    private _FieldEvents: NEvents<[string, string, FieldEvent]>;
    private _TableEvents: NEvents<[string, string, TableEvent]>;
    private _EntityEvents: NEvents<[string, EntityEvent]>;

    constructor()
    {
        this._Entitas = new Map<string, Entity>();
        this._FieldEvents = new NEvents<[string, string, FieldEvent]>();
        this._TableEvents = new NEvents<[string, string, TableEvent]>();
        this._EntityEvents = new NEvents<[string, EntityEvent]>();
    }

    RCEntity(entity_type: string, entity_event: EntityEvent, callback: (node: INode, nuid: Nuid, args: NList) => void, priority = 1): void 
    {
        this._EntityEvents.Register([entity_type, entity_event], callback, priority);
    }

    CallbackEntity(entity_type: string, entity_event: EntityEvent, nuid: Nuid, args: NList)
    {
        this._EntityEvents.Callback([entity_type, entity_event], this, nuid, args);
    }

    RCField(entity_type: string, field_name: string, field_event: FieldEvent, callback: (node: INode, nuid: Nuid, args: NList) => void, priority = 1): void 
    {
        this._FieldEvents.Register([entity_type, field_name, field_event], callback, priority);
    }

    RCTable(entity_type: string, table_name: string, table_event: TableEvent, callback: (node: INode, nuid: Nuid, args: NList) => void, priority = 1): void 
    {
        this._TableEvents.Register([entity_type, table_name, table_event], callback, priority);
    }

    LoadEntity(entity: Entity)
    {
        var id: string = String(entity.Id.Unique) + ":" + String(entity.Id.Origin);
        console.log("LoadEntity" + id);
        if (this._Entitas.has(id))
        {
            this._Entitas.delete(id);
        }

        this._Entitas.set(id, entity);
    }

    GetField(nuid: Nuid, field_name: string): any 
    {
        let id: string = nuid.toString();
        let found: Entity = this._Entitas.get(id);

        if (!this._Entitas.has(id)) return undefined;
        if (found == null) return false;

        let field: Field = found._Fields.get(field_name);
        if (field == null) return false;

        return field.Var.Value;
    }

    /*GetFieldBool(nuid: Nuid, field_name: string): boolean 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return false;

        let field = found._Fields.get(field_name);
        if (field == null) return false;

        return field.Var.Value;
    }

    GetFieldInt(nuid: Nuid, field_name: string): number 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldLong(nuid: Nuid, field_name: string): number 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldFloat(nuid: Nuid, field_name: string): number 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldString(nuid: Nuid, field_name: string): string 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldNuid(nuid: Nuid, field_name: string): Nuid 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldTime(nuid: Nuid, field_name: string): Date 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }

    GetFieldList(nuid: Nuid, field_name: string): NList 
    {
        let found: Entity = this._Entitas.get(nuid);
        if (found == null) return undefined;

        let field = found._Fields.get(field_name);
        if (field == null) return undefined;

        return field.Var.Value;
    }*/

    /*FindRowBool(nuid: Nuid, table_name: string, col: number, value: boolean): number 
    {
        if (!this._Entitas.has(nuid)) return -1;

        let found: Entity = this._Entitas.get(nuid);
        let table = found._Tables.get(table_name);
        if (table == null) return undefined;

        table._RowValues.forEach((row_value, row) => {
            let v = row_value.GetBool(col);
            if (v == value)
            {
                return row;
            }
        });

        return ClientNode.INVALID_ROW;
    }

    GetRowColBool(nuid: Nuid, table_name: string, row: number, col: number): boolean 
    {
        if (!this._Entitas.has(nuid)) return ClientNode.NULL_BOOL;

        let found: Entity = this._Entitas.get(nuid);
        let table = found._Tables.get(table_name);
        if (table == null) return ClientNode.NULL_BOOL;

        if (!table._RowValues.has(row)) return ClientNode.NULL_BOOL;

        let row_value = table._RowValues.get(row);
        return row_value.GetBool(col);
    }*/
}
