using ProtoBuf;
using System;
using System.IO;

namespace Nesh.Core.Utils
{
    public static class ProtoUtils
    {
        public static byte[] Serialize<T>(T t)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                Serializer.Serialize(ms, t);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\n{1}\n", ex.Message, ex.StackTrace));
            }
            byte[] bytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(bytes, 0, bytes.Length);
            ms.Dispose();
            ms = null;

            return bytes;
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            T t;
            try
            {
                t = Serializer.Deserialize<T>(ms);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\n{1}\n", ex.Message, ex.StackTrace));
            }
            ms.Dispose();
            ms = null;

            return t;
        }
    }
}
