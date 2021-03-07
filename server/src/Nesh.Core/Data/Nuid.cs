using Newtonsoft.Json;
using Orleans.Concurrency;
using ProtoBuf;
using System;

namespace Nesh.Core.Data
{
    [Immutable, Serializable]
    [ProtoContract, JsonObject]
    public struct Nuid : IComparable<Nuid>, IEquatable<Nuid>
    {
        [ProtoMember(Global.PROTO_NUID_UNIQUE), JsonProperty]
        public long Unique { get; private set; }

        [ProtoMember(Global.PROTO_NUID_ORIGIN), JsonProperty]
        public long Origin { get; private set; }

        public static readonly Nuid Empty = new Nuid(0, 0);

        private Nuid(long unique, long origin)
        {
            Unique = unique;
            Origin = origin;
        }

        private Nuid(string s)
        {
            string[] guids = s.Split(Global.NUID_MEMBERS_FLAG);
            if (guids == null || guids.Length != Global.NUID_MEMBERS_SIZE)
            {
                Unique = Global.NULL_LONG;
                Origin = Global.NULL_LONG;
            }

            Unique = long.Parse(guids[Global.PROTO_NUID_UNIQUE-1]);
            Origin = long.Parse(guids[Global.PROTO_NUID_ORIGIN-1]);
        }

        public static Nuid Parse(string g)
        {
            try
            {
                return new Nuid(g);
            }
            catch
            {
                return Empty;
            }
        }

        public int CompareTo(Nuid other)
        {
            int result = Origin.CompareTo(other.Origin);
            if (result != 0)
            {
                return result;
            }

            return Unique.CompareTo(other.Unique);
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Origin.ToString(), Unique.ToString());
        }

        public override int GetHashCode()
        {
            return Unique.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((Nuid)obj);
        }

        public static bool operator == (Nuid id1, Nuid id2)
        {
            return (id1.Origin == id2.Origin) && (id1.Unique == id2.Unique);
        }

        public static bool operator != (Nuid id1, Nuid id2)
        {
            return !(id1 == id2);
        }

        public static Nuid New(long unique, long origin)
        {
            Nuid cid = new Nuid(unique, origin);

            return cid;
        }

        [JsonIgnore]
        public bool IsOrigin
        {
            get { return Unique == Origin; }
        }

        public static bool IsEmpty(Nuid id)
        {
            return id == Empty;
        }

        public bool Equals(Nuid other)
        {
            return Origin.Equals(other.Origin) && Unique.Equals(other.Unique);
        }
    }
}
