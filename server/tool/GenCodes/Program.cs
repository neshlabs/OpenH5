using System;

namespace GenCodes
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GenCodes!");

            GenDef_CS.GenCodes();

            GenDef_TS.GenCodes();

            GenProto.GenNList();

            Console.WriteLine("End GenCodes!");

            Console.ReadLine();
        }
    }
}
