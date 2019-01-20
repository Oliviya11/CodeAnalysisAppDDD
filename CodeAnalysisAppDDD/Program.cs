using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Linq;
using System.Reflection;

namespace CodeAnalysisAppDDD
{
    class Program
    {
        const string TEST_SOLUTION = @"D:\ProjectsOAAD\ConsoleApp1\ConsoleApp1.sln";
        const string AGGREGATE_INTERFACE = "IAggregateRoot";
        
        static void Main(string[] args)
        {
            try
            {
                MSBuildWorkspace ws = MSBuildWorkspace.Create();
                Solution soln = ws.OpenSolutionAsync(TEST_SOLUTION).Result;
                Project proj = soln.Projects.Single();
                Console.WriteLine("Start analyzing project: " + proj.Name + ", " + proj.Language);
                Compilation compilation = proj.GetCompilationAsync().Result;

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var classes = tree.GetRoot().DescendantNodesAndSelf().Where(x => x.IsKind(SyntaxKind.ClassDeclaration));
                    if (classes != null)
                    {
                        foreach (var c in classes)
                        {
                            var classDec = (ClassDeclarationSyntax)c;
                            var bases = classDec.BaseList;
                            if (bases != null)
                            {
                                foreach (var b in bases.Types)
                                {
                                    var nodeType = compilation.GetSemanticModel(tree).GetTypeInfo(b.Type);
                                    string name = (nodeType.Type == null) ? "" : nodeType.Type.Name;
                                    if (AGGREGATE_INTERFACE.Equals(name))
                                    {
                                        Console.WriteLine(classDec.Identifier.Text);
                                    }
                                }
                            }
                        }
                    }
                }
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
