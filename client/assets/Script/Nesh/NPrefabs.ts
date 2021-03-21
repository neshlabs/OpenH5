import { VarType } from "./NList";

export class FieldPrefab
{
    public name: string;

    public type: VarType;
}

export class ColumnPrefab
{
    public index: number;

    public name: string;

    public type: VarType;
}

export class TablePrefab
{
    public name: string;

    public cols: number;

    public columns: Map<number, ColumnPrefab>;

}

export class EntityPrefab
{
    public type: string;
    
    public ancestors: Array<string>;

    public fields: Map<string, FieldPrefab>;

    public tables: Map<string, TablePrefab>;
}

export class NPrefabs
{
    public static entities: Map<string, EntityPrefab> = new Map<string, EntityPrefab>();

    public static AsPrefabs(prefabs: string) : any
    {
        let lst = JSON.parse(prefabs);

        for(let entity_key in lst)
        {
            let entity_prefab = new EntityPrefab();
            let entity_json = lst[entity_key];
            entity_prefab.type = entity_json["type"];

            entity_prefab.fields = new Map<string, FieldPrefab>();
            for(let field_key in entity_json["fields"])
            {
                let field = new FieldPrefab();
                field.name = field_key;
                field.type = entity_json["fields"][field_key]["type"];
                
                entity_prefab.fields.set(field_key, field);
            }

            entity_prefab.tables = new Map<string, TablePrefab>();
            for(let table_key in entity_json["tables"])
            {
                let table = new TablePrefab();
                table.name = table_key;
                table.cols = entity_json["tables"][table_key]["cols"];
                table.columns = new Map<number, ColumnPrefab>();

                let columns_json = entity_json["tables"][table_key]["columns"];
                for (let col_key in columns_json)
                {
                    let col = new ColumnPrefab();
                    col.index = columns_json[col_key]["index"];
                    col.name = columns_json[col_key]["name"];
                    col.type = columns_json[col_key]["type"];

                    table.columns.set(Number.parseInt(col_key), col);
                }

                entity_prefab.tables.set(table_key, table);
            }

            entity_prefab.ancestors = new Array<string>();
            for(let ancestor_key in entity_json["ancestors"])
            {
                entity_prefab.ancestors.push(entity_json["ancestors"][ancestor_key]);
            }

            NPrefabs.entities.set(entity_key, entity_prefab);
        }
    }
}