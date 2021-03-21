using System;

namespace GenCodes
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GenCodes!");

            GenPrefab_JSON.GenCodes();

            GenPrefab_CS.GenCodes();

            GenPrefab_TS.GenCodes();

            //GenProto.GenNList();

            Console.WriteLine("End GenCodes!");

            Console.ReadLine();
        }
    }
}
