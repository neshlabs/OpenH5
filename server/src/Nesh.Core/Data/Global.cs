using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using System;
using System.IO;

namespace Nesh.Core.Data
{
    public static class Global
    {
        #region null const value
        public const bool   NULL_BOOL              = false;
        public const int    NULL_INT               = 0;
        public const long   NULL_LONG              = 0L;
        public const float  NULL_FLOAT             = 0.0f;
        public const string NULL_STRING            = "";

        public const long   INVALID_ROW            = -1;
        public const int    INVALID_COL            = -1;
        #endregion

        #region nuid
        public const int    PROTO_NUID_UNIQUE      = 1;
        public const int    PROTO_NUID_ORIGIN      = PROTO_NUID_UNIQUE + 1;
        public const int    NUID_MEMBERS_SIZE      = 2;
        public const char   NUID_MEMBERS_FLAG      = ':';
        #endregion

        #region proto for nlist
        public const int    PROTO_ILIST            = 0;
        public const int    PROTO_LIST             = 1;
        public const int    PROTO_VALUE            = PROTO_LIST          + 1;
        public const int    PROTO_TYPE_BOOL        = PROTO_VALUE         + 1;
        public const int    PROTO_TYPE_INT         = PROTO_TYPE_BOOL     + 1;
        public const int    PROTO_TYPE_FLOAT       = PROTO_TYPE_INT      + 1;
        public const int    PROTO_TYPE_LONG        = PROTO_TYPE_FLOAT    + 1;
        public const int    PROTO_TYPE_STRING      = PROTO_TYPE_LONG     + 1;
        public const int    PROTO_TYPE_TIME        = PROTO_TYPE_STRING   + 1;
        public const int    PROTO_TYPE_NUID        = PROTO_TYPE_TIME     + 1;
        public const int    PROTO_TYPE_ENTITY      = PROTO_TYPE_NUID     + 1;
        public const int    PROTO_TYPE_LIST        = PROTO_TYPE_ENTITY   + 1;
        public const int    PROTO_TYPE_END         = PROTO_TYPE_LIST     + 1;
        #endregion

        #region entity,field,table
        public const int    PROTO_ENTITY           = 1;
        public const int    PROTO_ENTITY_TYPE      = PROTO_ENTITY        + 1;
        public const int    PROTO_ENTITY_ID        = PROTO_ENTITY_TYPE   + 1;
        public const int    PROTO_ENTITY_FIELDS    = PROTO_ENTITY_ID     + 1;
        public const int    PROTO_ENTITY_TABLES    = PROTO_ENTITY_FIELDS + 1;

        public const int    PROTO_FIELD_NAME       = PROTO_ENTITY_TABLES + 1;
        public const int    PROTO_FIELD_VALUE      = PROTO_FIELD_NAME    + 1;

        public const int    PROTO_TABLE_NAME       = PROTO_FIELD_VALUE   + 1;
        public const int    PROTO_TABLE_ROW_VALUES = PROTO_TABLE_NAME    + 1;
        #endregion

        public const string MARK_UNIQUE = "unique";
        public const string MARK_ORIGIN = "origin";
        public const string MARK_ROW    = "row";

        public static bool IsSubClassOf(Type type, Type baseType)
        {
            var b = type.BaseType;
            while (b != null)
            {
                if (b.Equals(baseType))
                {
                    return true;
                }
                b = b.BaseType;
            }
            return false;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum VarType : int
    {
        None      = 0,
        Bool      = 1,
        Int       = 2,
        Long      = 3,
        Float     = 4,
        String    = 5,
        Time      = 6,
        Nuid      = 7,
        Entity    = 8,
        List      = 9,
    };

    public enum SyncType : int
    {
        Entity    = 1,
        Field     = 2,
        Table     = 3,
        Custom    = 4,
    }

    public enum FieldEvent : int
    {
        Create    = 1,
        Change    = 2,
    };

    public enum TableEvent : int
    {
        Create    = 1,
        AddRow    = 2,
        DelRow    = 3,
        SetRow    = 4,
        SetCol    = 5,
        Clear     = 6,
    };

    public enum EntityEvent : int
    {
        OnCreate  = 1,
        OnDestroy = 2,
        OnLoad    = 3,
        OnEntry   = 4,
        OnLeave   = 5,
    }

    public enum LogLevel : int
    {
        Info      = 1,
        Warn      = 2,
        Error     = 3,
    }
}
