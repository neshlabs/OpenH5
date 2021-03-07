using Newtonsoft.Json;
using Orleans.Concurrency;
using ProtoBuf;
using System;

namespace Nesh.Core.Data
{
    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public sealed class Field
    {
        [ProtoMember(Global.PROTO_FIELD_NAME), JsonProperty]
        public string Name { get; private set; }

        [ProtoMember(Global.PROTO_FIELD_VALUE), JsonProperty]
        private Var Var { get; set; }

        public static Field Create<T>(string name, T value)
        {
            Var var = Var.Create(value);
            return new Field(name, var);
        }

        public Field() { }

        private Field(string name, Var value)
        {
            Name = name;
            Var = value;
        }

        public T Get<T>()
        {
            Var<T> var = Var as Var<T>;
            return var == null ? default(T) : var.Value;
        }

        public bool TrySet<T>(T value, out NList result)
        {
            result = null;
            if (!(Var is Var<T>)) return false;

            Var<T> var = Var as Var<T>;
            T old_value = var.Value;
            T new_value = value;

            if (old_value.Equals(new_value))
            {
                return false;
            }

            Var.Value = value;

            result = NList.New();
            result.Add(old_value);
            result.Add(new_value);
            return true;
        }
    }
}
