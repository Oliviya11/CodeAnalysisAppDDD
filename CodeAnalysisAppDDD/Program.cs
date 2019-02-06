using System;
using System.Collections.Generic;
using static CodeAnalysisAppDDD.Collector;

namespace CodeAnalysisAppDDD
{
    class Program
    {   
        static void Main(string[] args)
        {
            Collector collector = new Collector();
            Result result = collector.getResult();
            Analyzer analyzer = new Analyzer(result);

            Console.ReadLine();
        }
    }
}
