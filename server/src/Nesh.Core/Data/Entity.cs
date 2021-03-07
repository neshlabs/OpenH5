using Newtonsoft.Json;
using Orleans.Concurrency;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nesh.Core.Data
{
    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public sealed class Entity
    {
        [ProtoMember(Global.PROTO_ENTITY_FIELDS), JsonProperty]
        private Dictionary<string, Field> _Fields = null;
        [ProtoMember(Global.PROTO_ENTITY_TABLES), JsonProperty]
        private Dictionary<string, Table> _Tables = null;

        [JsonIgnore]
        private Field _Field = null;
        [JsonIgnore]
        private Table _Table = null;

        public Entity()
        {
            _Fields = new Dictionary<string, Field>();
            _Tables = new Dictionary<string, Table>();
            Activated = false;
        }

        public Entity(Entity cobject)
        {
            Id = cobject.Id;
            Type = cobject.Type;
            _Fields = new Dictionary<string, Field>(cobject._Fields);
            _Tables = new Dictionary<string, Table>(cobject._Tables);
        }

        public Entity Copy()
        {
            return new Entity(this);
        }

        [ProtoMember(Global.PROTO_ENTITY_ID), JsonProperty]
        public Nuid Id { get; set; }

        [ProtoMember(Global.PROTO_ENTITY_TYPE), JsonProperty]
        public string Type { get; set; }

        [JsonIgnore]
        public bool Activated { get; set; }

        public void Clear()
        {
            foreach (Table table in _Tables.Values)
            {
                table.Clear();
            }

            _Fields.Clear();
            _Tables.Clear();
        }

        public Field GetField(string field_name)
        {
            if (_Field != null && _Field.Name.Equals(field_name))
            {
                return _Field;
            }

            _Fields.TryGetValue(field_name, out _Field);

            return _Field;
        }

        public Table GetTable(string table_name)
        {
            if (_Table != null && _Table.Name.Equals(table_name))
            {
                return _Table;
            }

            _Tables.TryGetValue(table_name, out _Table);

            return _Table;
        }

        public Field[] GetFields()
        {
            Field[] found = new Field[_Fields.Values.Count];
            _Fields.Values.CopyTo(found, 0);

            return found;
        }

        public Table[] GetTables()
        {
            Table[] found = new Table[_Tables.Values.Count];
            _Tables.Values.CopyTo(found, 0);

            return found;
        }

        public Field CreateField<T>(string field_name, T field_value)
        {
            Field field = null;
            if (_Fields.ContainsKey(field_name))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Create Field [{0}] Fail is exist", field_name);
                throw new Exception(sb.ToString());
            }
            else
            {
                field = Field.Create(field_name, field_value);
                _Fields.Add(field_name, field);
            }

            return field;
        }

        public Table CreateTable(string table_name)
        {
            Table table = null;
            if (_Tables.ContainsKey(table_name))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Create Table [{0}] Fail is exist", table_name);
                throw new Exception(sb.ToString());
            }
            else
            {
                table = new Table(table_name);
                _Tables.Add(table_name, table);
            }

            return table;
        }
    }
}
