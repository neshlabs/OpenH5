import { TypedJSON } from "typedjson";
import { NEvents } from "./NEvents";
import NList, { Nuid, Field, Table, Entity } from "./NList";

export interface INode
{
    RCEntity(entity_type: string, entity_event: EntityEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;
    RCField(entity_type: string, field_name: string, field_event: FieldEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;
    RCTable(entity_type: string, table_name: string, table_event: TableEvent, callback:(node: INode, nuid: Nuid, args: NList) => void) : void;

    GetField(nuid: Nuid, field_name: string) : any;

    FindRow(nuid: Nuid, table_name: string, col: number, value: any) : number;
    GetRowCol(nuid: Nuid, table_name: string, row: number, col: number) : any;
    GetRowValue(nuid: Nuid, table_name: string, row: number) : NList;
    GetRows(nuid: Nuid, table_name: string) : NList;
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
    private _Entitas: Map<Nuid, Entity>;
    private _FieldEvents: NEvents<[string, string, FieldEvent]>;
    private _TableEvents: NEvents<[string, string, TableEvent]>;
    private _EntityEvents: NEvents<[string, EntityEvent]>;

    constructor()
    {
        this._Entitas = new Map<Nuid, Entity>();
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
        //var id: string = String(entity.Id.Unique) + ":" + String(entity.Id.Origin);
        let id:Nuid = entity.Id;
        console.log("LoadEntity" + id.toString());
        if (this.HasEntity(id))
        {
            this.DelEntity(id);
        }

        this._Entitas.set(id, entity);
    }

    DelEntity(nuid: Nuid)
    {
        this._Entitas.forEach((value, key)=>
        {
            if (nuid.equals(key))
            {
                this._Entitas.delete(key);
                return;
            }
        });
    }

    HasEntity(nuid: Nuid): boolean
    {
        let found = false;
        this._Entitas.forEach((value, key)=>
        {
            if (nuid.equals(key))
            {
                found = true;
                return;
            }
        });

        return found;
    }

    GetEntity(nuid: Nuid): Entity
    {
        let entity = null;
        this._Entitas.forEach((value, key)=>
        {
            if (nuid.equals(key))
            {
                entity = value;
                return;
            }
        });

        return entity;
    }

    SetField(nuid: Nuid, field_name: string, filed_value: any)
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return;

        let field: Field = found._Fields[field_name];
        if (field == null) return;

        field.Var.Value = filed_value;
    }

    GetField(nuid: Nuid, field_name: string): any 
    {
        //let id: string = nuid.toString();
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return undefined;

        let field: Field = found._Fields[field_name];

        return field.Var.Value;
    }

    SetRowCol(nuid: Nuid, table_name: string, row: number, value: any)
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return;

        let table:Table = found._Tables[table_name];
        if (table == null) return;

        let row_value:NList = table._RowValues[row];

        if (row_value == null) return;

        row_value._List[row].Value = value;
    }

    SetRowValue(nuid: Nuid, table_name: string, row: number, row_value: NList)
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return;

        let table:Table = found._Tables[table_name];
        if (table == null) return;

        table._RowValues[row] = row_value;
    }

    GetRowValue(nuid: Nuid, table_name: string, row: number) :NList
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return undefined;

        let table:Table = found._Tables[table_name];

        let row_value:NList = table._RowValues[row];

        if (row_value == null) return undefined;

        return row_value;
    }

    GetRows(nuid: Nuid, table_name: string) :NList
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return undefined;

        let table:Table = found._Tables[table_name];
        if (table == null) return undefined;

        let rows: NList = NList.New();
        for(const temp_row in table._RowValues)
        {
            let row: number = Number(temp_row);
            rows.AddInt(row);
        }

        return rows;
    }

    GetRowCol(nuid: Nuid, table_name: string, row: number, col: number) :any
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return undefined;

        let table:Table = found._Tables[table_name];

        let row_value:NList = table._RowValues[row];

        if (row_value == null) return undefined;

        return row_value._List[col].Value;
    }

    FindRow(nuid: Nuid, table_name: string, col: number, value: any): number 
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return ClientNode.INVALID_ROW;

        let table:Table = found._Tables[table_name];

        if (table == null) return undefined;

        let found_row: string;
        for(const temp_row in table._RowValues)
        {
            let row_value:NList = table._RowValues[temp_row];
            if (row_value._List[col].Value == value)
            {
                found_row = temp_row;
                break;
            }
        }

        let row: number = Number(found_row);
        return row;
    }

    DelRow(nuid: Nuid, table_name: string, row: number)
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return;

        let table:Table = found._Tables[table_name];

        table._RowValues.delete(row);
    }

    ClearTable(nuid: Nuid, table_name: string)
    {
        let id:Nuid = nuid;
        let found: Entity = this.GetEntity(id);

        if (found == null) return;

        let table:Table = found._Tables[table_name];

        table._RowValues.clear();
    }
}
