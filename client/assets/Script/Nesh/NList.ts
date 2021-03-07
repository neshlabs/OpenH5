import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember, jsonMapMember, AnyT } from 'typedjson';

enum VarType 
{
    None      = 'None',
    Bool      = 'Bool',
    Int       = 'Int',
    Long      = 'Long',
    Float     = 'Float',
    String    = 'String',
    Time      = 'Time',
    Nuid      = 'Nuid',
    Entity    = 'Entity',
    List      = 'List',
}

@jsonObject
class Var
{
    @jsonMember(AnyT)
    public Value: any;

    @jsonMember(String)
    public Type: VarType;
}

@jsonObject
export default class NList
{
    @jsonArrayMember(Var)
    public _List: Array<Var> = [];

    public static New() : NList
    {
        let lst = new NList();
        return lst;
    }

    public static AsJSON(lst: NList) : string
    {
        const json = TypedJSON.stringify(lst, NList);
        return json;
    }

    public static AsList(json: string) : NList
    {
        return TypedJSON.parse(json, NList);
    }

    public Append(lst: NList) : NList
    {
        this._List.concat(lst._List);
        return this;
    }

    public AddBool(value: boolean) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Bool;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddInt(value: number) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Int;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddLong(value: number) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Long;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddFloat(value: number) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Float;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddString(value: string) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.String;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddTime(value: Date) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Time;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddNuid(value: Nuid) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.Nuid;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public AddList(value: NList) : NList
    {
        let nvar = new Var();
        nvar.Type = VarType.List;
        nvar.Value = value;

        this._List.push(nvar);

        return this;
    }

    public GetBool(index: number) : boolean
    {
        if (index < 0 || index >= this._List.length)
        {
            return false;
        }

        return this._List[index].Value;
    }

    public GetInt(index: number) : number
    {
        if (index < 0 || index >= this._List.length)
        {
            return 0;
        }

        return this._List[index].Value;
    }

    public GetLong(index: number) : number
    {
        if (index < 0 || index >= this._List.length)
        {
            return 0;
        }

        return this._List[index].Value;
    }

    public GetFloat(index: number) : number
    {
        if (index < 0 || index >= this._List.length)
        {
            return 0;
        }

        return this._List[index].Value;
    }

    public GetString(index: number) : string
    {
        if (index < 0 || index >= this._List.length)
        {
            return "";
        }

        return this._List[index].Value;
    }

    public GetNuid(index: number) : Nuid
    {
        if (index < 0 || index >= this._List.length)
        {
            return undefined;
        }

        return this._List[index].Value;
    }

    public GetTime(index: number) : Date
    {
        if (index < 0 || index >= this._List.length)
        {
            return undefined;
        }

        return this._List[index].Value;
    }

    public GetList(index: number) : NList
    {
        if (index < 0 || index >= this._List.length)
        {
            return undefined;
        }

        return this._List[index].Value;
    }
}

@jsonObject
export class Nuid
{
    @jsonMember(Number)
    public Unique: number;

    @jsonMember(Number)
    public Origin: number;

    equals(other: Nuid): boolean {
        return (other.Origin === this.Origin) && (other.Unique === this.Unique);
    }

    toString(): string
    {
        return this.Unique + ":" + this.Origin;
    }
}

@jsonObject
export class Field
{
    @jsonMember(Var)
    public Var: Var;

    @jsonMember(String)
    public Name: string;
}

@jsonObject
export class Table
{
    @jsonMember(String)
    public Name: string;

    @jsonMapMember(Number, NList)
    public _RowValues: Map<number, NList>;
}

@jsonObject
export class Entity
{
    @jsonMember(String)
    public Type: string;

    @jsonMember(Nuid)
    public Id: Nuid;

    @jsonMapMember(String, Field)
    public _Fields: Map<string, Field>;

    @jsonMapMember(String, Table)
    public _Tables: Map<string, Table>;
}