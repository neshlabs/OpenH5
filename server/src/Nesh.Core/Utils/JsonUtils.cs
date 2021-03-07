using JsonSubTypes;
using Nesh.Core.Data;
using Newtonsoft.Json;
using System;

namespace Nesh.Core.Utils
{
    public static class JsonUtils
    {
        static JsonUtils()
        {
            Settings = new JsonSerializerSettings();
            Settings.Converters
                .Add(JsonSubtypesConverterBuilder
                .Of(typeof(Var), "Type") // type property is only defined here
                .RegisterSubtype(typeof(Var<bool>), VarType.Bool)
                .RegisterSubtype(typeof(Var<int>), VarType.Int)
                .RegisterSubtype(typeof(Var<float>), VarType.Float)
                .RegisterSubtype(typeof(Var<long>), VarType.Long)
                .RegisterSubtype(typeof(Var<string>), VarType.String)
                .RegisterSubtype(typeof(Var<DateTime>), VarType.Time)
                .RegisterSubtype(typeof(Var<Nuid>), VarType.Nuid)
                .RegisterSubtype(typeof(Var<Entity>), VarType.Entity)
                .RegisterSubtype(typeof(Var<NList>), VarType.List)
                .SerializeDiscriminatorProperty() // ask to serialize the type property
                .Build());
        }

        private static JsonSerializerSettings Settings;

        public static string ToJson(object value)
        {
            try
            {
                return JsonConvert.SerializeObject(value, Settings);
            }
            catch (Exception ex)
            {
                throw new Exception("JsonUtils ToJson Error", ex);
            }
        }

        public static T ToObject<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, Settings);
            }
            catch (Exception ex)
            {
                throw new Exception("JsonUtils ToObject Error", ex);
            }
        }
    }
}
