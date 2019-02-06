using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeAnalysisAppDDD.Collector;

namespace CodeAnalysisAppDDD
{
    class Analyzer
    {
        public Result result;
        public Analyzer(Result result)
        {
            Console.WriteLine("Analyze result:");
            Console.WriteLine();

            this.result = result;

            if (checkIfDigraphConnected())
            {
                Console.WriteLine("1. Relations digraph is connected");
            } else
            {
                Console.WriteLine("1. Relations digraph isn't connected");
            }

            if (checkAggregateSets())
            {
                Console.WriteLine("2. Sets of agregate don't intersect and are reachable only from its root");
            } else
            {
                Console.WriteLine("2. Sets of agregate intersect or are reachable not only from its root");
            }
        }

        bool checkIfDigraphConnected()
        {
            Dictionary<string, bool> reachableV = new Dictionary<string, bool>();
            checkDigraph(result.relationsD, reachableV);
            checkDigraph(result.reversedRelationD, reachableV);
            foreach (bool val in reachableV.Values)
            {
                if (val == false)
                {
                    return false;
                }
            }

            return true;
        }

        void checkDigraph(Digraph digraph, Dictionary<string, bool> reachableV)
        {
            HashSet<string> vertices = digraph.getVertices();
            foreach (string v in vertices)
            {
                if (!reachableV.ContainsKey(v))
                {
                    reachableV.Add(v, false);
                }

                DirectedDFS directedDFS = new DirectedDFS(result.relationsD, v);
                foreach (string w in vertices)
                {
                    if (directedDFS.isMarkedV(w))
                    {
                        reachableV[w] = true;
                    }
                }
            }
        }

        bool checkAggregateSets()
        {
            HashSet<string> vertices = result.relationsD.getVertices();
            foreach (KeyValuePair<string, HashSet<string>> pair in result.aggregates)
            {
                HashSet<string> onlyRefs = new HashSet<string>(vertices);
                HashSet<string> except = new HashSet<string>();
                except.Add(pair.Key);
                onlyRefs.ExceptWith(except);
                DirectedDFS directedDFS = new DirectedDFS(result.relationsD, onlyRefs);
                HashSet<string> marked = directedDFS.getMarked();
                foreach (string w in pair.Value)
                {
                    if (marked.Contains(w))
                    {
                        return false;
                    }
                }

            }

            return true;
        }
    }
}
