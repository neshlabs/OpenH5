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
    [ProtoInclude(Global.PROTO_TYPE_BOOL,   typeof(Var<bool>))]
    [ProtoInclude(Global.PROTO_TYPE_INT,    typeof(Var<int>))]
    [ProtoInclude(Global.PROTO_TYPE_FLOAT,  typeof(Var<float>))]
    [ProtoInclude(Global.PROTO_TYPE_LONG,   typeof(Var<long>))]
    [ProtoInclude(Global.PROTO_TYPE_STRING, typeof(Var<string>))]
    [ProtoInclude(Global.PROTO_TYPE_TIME,   typeof(Var<DateTime>))]
    [ProtoInclude(Global.PROTO_TYPE_NUID,   typeof(Var<Nuid>))]
    [ProtoInclude(Global.PROTO_TYPE_ENTITY, typeof(Var<Entity>))]
    [ProtoInclude(Global.PROTO_TYPE_LIST,   typeof(Var<NList>))]
    public abstract class Var
    {
        public static Var<T> Create<T>(T value)
        {
            return new Var<T>(value);
        }

        [JsonIgnore]
        public object Value
        {
            get { return ValueImpl; }
            set { ValueImpl = value; }
        }

        protected abstract object ValueImpl { get; set; }
    }

    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public sealed class Var<T> : Var
    {
        public Var() { }
        public Var(T value) { Value = value; }

        [ProtoMember(Global.PROTO_VALUE), JsonProperty]
        public new T Value { get; set; }

        protected override object ValueImpl
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }

    [ProtoContract, JsonObject]
    [ProtoInclude(Global.PROTO_ILIST, typeof(NList))]
    public interface INList
    {
        [JsonIgnore]
        int Count { get; }
        T Get<T>(int index);
    }

    
    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public sealed class NList : INList
    {
        [ProtoMember(Global.PROTO_LIST, OverwriteList = true), JsonProperty]
        private List<Var> _List { get; set; }

        [JsonIgnore]
        public int Count { get { return _List.Count; } }

        [JsonIgnore]
        public static readonly NList Empty = new NList();

        public static NList New()
        {
            return new NList();
        }

        public NList()
        {
            _List = new List<Var>();
        }

        public NList(List<NList> list)
        {
            _List = new List<Var>();

            foreach (NList child in list)
            {
                _List.Add(Var.Create(child));
            }
        }

        public NList(NList list)
        {
            _List = new List<Var>();
            _List.AddRange(list._List);
        }

        private NList(IEnumerable<Var> collection)
        {
            _List = new List<Var>(collection);
        }

        private Var GetItem(int index)
        {
            if (index < 0 || index >= Count)
            {
                return null;
            }

            return _List[index];
        }

        public void Clear() { _List.Clear(); }

        public NList Append(NList value)
        {
            if (value.Count > 0)
            {
                _List.AddRange(value._List);
            }
            return this;
        }

        public T Get<T>(int index)
        {
            if (index >= _List.Count || index < 0) return default(T);
            Var<T> item = GetItem(index) as Var<T>;
            return item == null ? default(T) : item.Value;
        }

        public void Set<T>(int index, T value)
        {
            Var<T> item = GetItem(index) as Var<T>;
            if (item != null) item.Value = value;
        }

        public NList Add<T>(T value)
        {
            _List.Add(Var.Create(value));
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{List");
            for (int i = 0; i < Count; i++)
            {
                sb.AppendFormat(" ({0}:{1}) ", i, _List[i].Value);
            }
            sb.Append('}');
            return sb.ToString();
        }

        public NList GetRange(int index, int count)
        {
            if (index >= _List.Count)
            {
                return Empty;
            }

            return new NList(_List.GetRange(index, count));
        }

        public static bool IsEmpty(NList list)
        {
            return list == null || list == Empty;
        }
    }
}
