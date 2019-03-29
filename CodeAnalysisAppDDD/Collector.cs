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
        const string DEFAULT_TEST_SOLUTION = @"D:\ProjectsOAAD\ConsoleApp1\ConsoleApp1.sln";
        const string AGGREGATE_INTERFACE = "IAggregateRoot";
        private string testSolution;
        private int testProjectNumber;

        public struct Result
        {
            public Dictionary<string, HashSet<string>> aggregates;
            public Digraph relationsD;
            public Digraph reversedRelationD;
        }

        Result result = new Result();

        public Collector(string testSolution, int testProjectNumber)
        {
            this.testSolution = testSolution;
            if (string.IsNullOrEmpty(this.testSolution))
            {
                this.testSolution = DEFAULT_TEST_SOLUTION;
            }
            this.testProjectNumber = testProjectNumber;

            startCollecting();
            showResult();
        }

        void startCollecting()
        {
            result.aggregates = new Dictionary<string, HashSet<string>>();
            result.relationsD = new Digraph();
            result.reversedRelationD = new Digraph();

            try
            {
                MSBuildWorkspace ws = MSBuildWorkspace.Create();
                Solution soln = ws.OpenSolutionAsync(testSolution).Result;
                List<Project> projects = soln.Projects.ToList<Project>();
                Project proj = projects[this.testProjectNumber];
                Console.WriteLine();
                Console.WriteLine("Project: " + proj.Name + ", " + proj.Language);
                Console.WriteLine();
                Console.WriteLine("Collecting information...");
                Console.WriteLine();
                Compilation compilation = proj.GetCompilationAsync().Result;

                pushVerticesClassesToDigraph(result.relationsD, compilation.SyntaxTrees);

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
                                collectAggregates(classDec, compilation, tree);
                            }

                            // Collect references
                            SemanticModel model = compilation.GetSemanticModel(tree);
                            collectRelationships(model, c, c.Identifier.Text, proj.Solution, result.relationsD);
                        }
                    }
                }

                result.reversedRelationD = result.relationsD.reverse();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception inner in ex.LoaderExceptions)
                {
                    Console.WriteLine(inner.Message);
                }
            }
        }

        void collectAggregates(ClassDeclarationSyntax classDec, Compilation compilation, SyntaxTree tree)
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
                        result.aggregates.Add(className, new HashSet<string>());
                        var properties = classDec.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                        if (properties != null)
                        {
                            foreach (var prop in properties)
                            {
                                string propName = prop.Type.ToString();
                                if (result.relationsD.containsV(propName))
                                {
                                    result.aggregates[className].Add(propName);
                                }
                            }
                        }

                        var fileds = classDec.DescendantNodes().OfType<FieldDeclarationSyntax>();
                        if (fileds != null)
                        {
                            foreach (var field in fileds)
                            {
                                string fieldName = field.Declaration.Type.ToString();
                                if (result.relationsD.containsV(fieldName))
                                {
                                    result.aggregates[className].Add(fieldName);
                                }
                            }
                        }
                    }
                }
            }
        }

        void collectRelationships(SemanticModel model, SyntaxNode c, string className, Solution solution, Digraph digraph)
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

        void showResult()
        {
            Console.WriteLine("Aggregates (roots and their nodes):");
            Console.WriteLine();

            foreach (KeyValuePair<string, HashSet<string>> pair in result.aggregates)
            {
                if (pair.Value.Count == 0)
                {
                    Console.WriteLine(pair.Key + ";");

                }
                else
                {
                    Console.WriteLine(pair.Key + ":");
                    foreach (string val in pair.Value)
                    {
                        Console.WriteLine(val + ";");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("Relations between classes (digraph):");
            Console.WriteLine();
            result.relationsD.show();
            Console.WriteLine();
        }

        public Result getResult()
        {
            return result;
        }
    }
}
