using Nesh.Core.Data;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCodes
{
    static class GenProto
    {
        public static void GenNList()
        {
            string directory = Environment.CurrentDirectory;
            int index = directory.IndexOf("\\tool\\");
            directory = directory.Remove(index);

            StringBuilder sb = new StringBuilder(directory);
            sb.Append("/build/res/proto/nlist.proto");

            StringBuilder out_folder = new StringBuilder(directory);
            string proto = Serializer.GetProto<NList>(ProtoSyntax.Proto3);

            proto = proto.Replace("package Nesh.Core.Data;", "package nesh;");
            proto = proto.Replace("protobuf-net/bcl.proto", "bcl.proto");

            string filePath = sb.ToString();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FileInfo fi = new FileInfo(filePath);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();

            FileStream fs = new FileStream(filePath, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write(proto);
            sw.Close();
            fs.Close();
        
        
        }
    }
}
