using System;
using System.Collections.Generic;

namespace CsharpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IDictionary<string, string> table = new Dictionary<string, string>();

            table["KEY"] =  "v1";

            table["Key"] = "v2";

            table["KEY"] = "v3";


            Console.WriteLine(table["KEY"]);

            Console.WriteLine(table["Key"]);

            //Console.WriteLine(table["None"]);


        }
    }
}
