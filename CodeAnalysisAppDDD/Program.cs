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
            collector.startCollecting();
            Result result = collector.getResult();
            Console.WriteLine("Show names of aggregate roots:");
            
            foreach (KeyValuePair<string, HashSet<string>> pair in result.aggregates)
            {
                if (pair.Value.Count == 0)
                {
                    Console.WriteLine(pair.Key + ";");

                } else
                {
                    Console.WriteLine(pair.Key + ":");
                    foreach (string val in pair.Value)
                    {
                        Console.WriteLine(val + ";");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Relations between classes (digraph):");
            result.relationshipsD.show();

            Console.WriteLine();
            Console.WriteLine("Reversed digraph:");
            result.reversedRelationshipsD.show();
            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
