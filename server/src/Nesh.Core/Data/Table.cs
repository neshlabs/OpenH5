using Nesh.Core.Utils;
using Newtonsoft.Json;
using Orleans.Concurrency;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Nesh.Core.Data
{
    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public sealed class Table
    {
        [ProtoMember(Global.PROTO_TABLE_NAME), JsonProperty]
        public string Name { get; private set; }

        [ProtoMember(Global.PROTO_TABLE_ROW_VALUES), JsonProperty]
        private Dictionary<long, NList> _RowValues;

        public Table()
        {
            _RowValues = new Dictionary<long, NList>();
        }

        public Table(string name) : this()
        {
            Name = name;
        }

        [JsonIgnore]
        public bool IsEmpty { get { return _RowValues.Count == 0; } }

        public void Clear()
        {
            _RowValues.Clear();
        }

        public bool TryAddRow(NList row_value, out NList result)
        {
            long row = IdUtils.RGen.CreateId();
            return TrySetRow(row, row_value, out result);
        }

        public bool TrySetRow(long row, NList row_value, out NList result)
        {
            result = null;

            if (row_value == null) return false;

            if (row == Global.INVALID_ROW)
            {
                return false;
            }

            result = NList.New();
            result.Add(row);
            result.Append(row_value);

            if (!_RowValues.ContainsKey(row))
            {
                _RowValues.Add(row, row_value);
            }
            else
            {
                _RowValues[row] = row_value;
            }

            return true;
        }

        public bool TryDelRow(long row, out NList result)
        {
            result = null;

            NList row_value = NList.Empty;
            if (!_RowValues.TryGetValue(row, out row_value))
            {
                return false;
            }

            _RowValues.Remove(row);

            result = NList.New();
            result.Add(row).Append(row_value);
            return true;
        }

        public bool TrySetRowCol<T>(long row, int col, T value, out NList result)
        {
            result = null;
            if (value == null) return false;

            NList row_value;
            if (!_RowValues.TryGetValue(row, out row_value))
            {
                return false;
            }

            T var = row_value.Get<T>(col);
            if (var == null)
            {
                return false;
            }

            T new_value = value;

            if (var.Equals(new_value))
            {
                return false;
            }

            row_value.Set(col, new_value);

            result = NList.New();
            result.Add(row);
            result.Add(col);
            result.Add(var);
            result.Add(new_value);

            return true;
        }

        public T GetRowCol<T>(long row, int col)
        {
            NList row_value;
            if (!_RowValues.TryGetValue(row, out row_value))
            {
                return default(T);
            }

            return row_value.Get<T>(col);
        }

        public long FindRow<T>(int col, T value)
        {
            if (value == null) return Global.INVALID_ROW;

            long row = Global.INVALID_ROW;
            foreach (KeyValuePair<long, NList> pair in _RowValues)
            {
                T cur_value = pair.Value.Get<T>(col);
                if (cur_value == null)
                {
                    continue;
                }

                if (cur_value.Equals(value))
                {
                    row = pair.Key;
                    break;
                }
            }

            return row;
        }

        public NList GetRow(long row)
        {
            NList row_value;
            if (!_RowValues.TryGetValue(row, out row_value))
            {
                return null;
            }

            return row_value;
        }

        public NList GetRows()
        {
            NList list = NList.New();
            foreach (long row in _RowValues.Keys)
            {
                list.Add(row);
            }

            return list;
        }
    }
}
