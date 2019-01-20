using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Linq;
using System.Reflection;

namespace CodeAnalysisAppDDD
{
    class Program
    {
        const string TEST_SOLUTION = @"D:\ProjectsOAAD\ConsoleApp1\ConsoleApp1.sln";
        
        static void Main(string[] args)
        {
            try
            {
                MSBuildWorkspace ws = MSBuildWorkspace.Create();
                Solution soln = ws.OpenSolutionAsync(TEST_SOLUTION).Result;
                Project proj = soln.Projects.Single();
                Console.WriteLine("Start analyzing project: " + proj.Name + ", " + proj.Language);
                Compilation compilation = proj.GetCompilationAsync().Result;
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception inner in ex.LoaderExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
            
            Console.ReadLine();
        }
    }
}
