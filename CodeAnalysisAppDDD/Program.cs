using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
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
                List<Project> projects = soln.Projects.ToList<Project>();
                Project proj = projects[1];
                Console.WriteLine("Start analyzing project: " + proj.Name + ", " + proj.Language);
                Compilation compilation = proj.GetCompilationAsync().Result;

                // Collect IAggregateRoot roots
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

                // Collect references
                Console.WriteLine("Collect references...");
                foreach (var tree in compilation.SyntaxTrees)
                {
                    SemanticModel model = compilation.GetSemanticModel(tree);
                    //  var classes = tree.GetRoot().DescendantNodesAndSelf().Where(x => x.IsKind(SyntaxKind.ClassKeyword));
                    var classes = tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();
                    foreach (var c in classes)
                    {
                        var classSymbol = model.GetDeclaredSymbol(c);
                        var referencesToClass = SymbolFinder.FindReferencesAsync(classSymbol, proj.Solution).Result;
                        // get span from locations:
                        foreach (var referenceToClass in referencesToClass)
                        {
                            var locations = referenceToClass.Locations;
                            foreach (var loc in locations)
                            {
                                var location = loc.Location;
                                var span = location.SourceSpan;
                                // find node:
                                var sourceTree = location.SourceTree;
                                var sourceNodes = sourceTree.GetRoot().DescendantNodes().Where(x => x.Span.IntersectsWith(span));
                                foreach (var node in sourceNodes)
                                {
                                    var classDeclaration = node as ClassDeclarationSyntax;
                                    if (classDeclaration != null)
                                    {
                                        var referenceTo = c.Identifier.Text;
                                        var referenceFrom = classDeclaration.Identifier.Text;
                                        Console.WriteLine(referenceFrom + " -> " + referenceTo);
                                    }
                                }
                            }
                        }
                        //var nodes = sf.GetRoot().DescendantNodes().Where(x => x.Span.IntersectsWith(span));
                        // from node recursevly get Ancestors
                        // fo each ancestor define if it is ClassDeclarationSyntax type;
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
