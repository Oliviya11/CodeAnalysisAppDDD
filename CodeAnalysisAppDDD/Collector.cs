using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeAnalysisAppDDD
{
    class Collector
    {
        const string TEST_SOLUTION = @"D:\ProjectsOAAD\ConsoleApp1\ConsoleApp1.sln";
        const string AGGREGATE_INTERFACE = "IAggregateRoot";

        public struct Result
        {
            public HashSet<string> aggregateRoots;
            public Digraph digraph;
            public Digraph reversedDigraph;
        }

        Result result = new Result();

        public void startCollecting()
        {
            result.aggregateRoots = new HashSet<string>();
            result.digraph = new Digraph();
            result.reversedDigraph = new Digraph();

            try
            {
                MSBuildWorkspace ws = MSBuildWorkspace.Create();
                Solution soln = ws.OpenSolutionAsync(TEST_SOLUTION).Result;
                List<Project> projects = soln.Projects.ToList<Project>();
                Project proj = projects[1];
                Console.WriteLine("Start collecting info about project: " + proj.Name + ", " + proj.Language);
                Console.WriteLine();
                Compilation compilation = proj.GetCompilationAsync().Result;

                pushVerticesClassesToDigraph(result.digraph, compilation.SyntaxTrees);

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var classes = tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();
                    if (classes != null)
                    {
                        foreach (var c in classes)
                        {
                            // Collect IAggregateRoot roots
                            var classDec = c as ClassDeclarationSyntax;
                            if (classDec != null)
                            {
                                collectAggregateRoots(classDec, compilation, tree);
                            }

                            // Collect references
                            SemanticModel model = compilation.GetSemanticModel(tree);
                            collectReferences(model, c, c.Identifier.Text, proj.Solution, result.digraph);
                        }
                    }
                }

                result.reversedDigraph = result.digraph.reverse();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception inner in ex.LoaderExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
        }

        void collectAggregateRoots(ClassDeclarationSyntax classDec, Compilation compilation, SyntaxTree tree)
        {
            var bases = classDec.BaseList;
            if (bases != null)
            {
                foreach (var b in bases.Types)
                {
                    var nodeType = compilation.GetSemanticModel(tree).GetTypeInfo(b.Type);
                    string name = (nodeType.Type == null) ? "" : nodeType.Type.Name;
                    if (AGGREGATE_INTERFACE.Equals(name))
                    {
                        string className = classDec.Identifier.Text;
                        result.aggregateRoots.Add(className);
                    }
                }
            }
        }

        void collectReferences(SemanticModel model, SyntaxNode c, string className, Solution solution, Digraph digraph)
        {
            var classSymbol = model.GetDeclaredSymbol(c);
            var referencesToClass = SymbolFinder.FindReferencesAsync(classSymbol, solution).Result;
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
                            var referenceFrom = classDeclaration.Identifier.Text;
                            digraph.addEdge(referenceFrom, className);
                        }
                    }
                }
            }
        }

        void pushVerticesClassesToDigraph(Digraph digraph, IEnumerable<SyntaxTree> trees)
        {
            foreach (var tree in trees)
            {
                var classes = tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();
                if (classes != null)
                {
                    foreach (var c in classes)
                    {
                        digraph.addVertex(c.Identifier.Text);
                    }
                }
            }
        }

        public Result getResult()
        {
            return result;
        }
    }
}
