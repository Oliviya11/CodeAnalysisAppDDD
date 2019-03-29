using System;
using static CodeAnalysisAppDDD.Collector;

namespace CodeAnalysisAppDDD
{
    class Program
    {   
        static void Main(string[] args)
        {
            Console.WriteLine("Enter solution's path to analyze: ");
            string solutionName = Console.ReadLine();
            Console.WriteLine("Enter project's number in solution to analyze: ");
            int number = Int32.Parse(Console.ReadLine());
            Collector collector = new Collector(solutionName, number);
            Result result = collector.getResult();
            Analyzer analyzer = new Analyzer(result);

            Console.ReadLine();
        }
    }
}
