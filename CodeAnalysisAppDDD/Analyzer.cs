using System;
using System.Collections.Generic;
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
                Console.WriteLine("2. Sets of agregates don't intersect and are reachable only from their roots");
            } else
            {
                Console.WriteLine("2. Sets of agregates intersect or are reachable not only from their roots");
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
            bool checkResult = true;

            foreach (KeyValuePair<string, HashSet<string>> pair in result.aggregates)
            {
                foreach (string child in pair.Value)
                {
                    HashSet<string> roots = result.reversedRelationD.getAdjForV(child);
                    if (roots.Count > 1)
                    {
                        checkResult = false;
                        Console.WriteLine("Problem with class: " + child);
                    }
                }

            }

            return checkResult;
        }
    }
}
