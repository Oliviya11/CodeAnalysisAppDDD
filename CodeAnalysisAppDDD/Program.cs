using System;
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
            foreach (string name in result.aggregateRoots)
            {
                Console.WriteLine(name + ";");
            }
            Console.WriteLine();

            Console.WriteLine("Relations between classes (digraph):");
            result.digraph.show();
            Console.WriteLine();

            Console.WriteLine("Reversed digraph:");
            result.reversedDigraph.show();
            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
