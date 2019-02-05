using System;
using System.Collections.Generic;

namespace CodeAnalysisAppDDD
{
    class Digraph
    {
        private int V;
        private int E;
        private Dictionary<string, HashSet<string>> adj;
        public Digraph()
        {
            this.V = 0;
            this.E = 0;
            adj = new Dictionary<string, HashSet<string>>();
        }

        public int getV() { return V;  }
        public int getE() { return E; }
        public void addEdge(string from, string to)
        {
            if (!adj.ContainsKey(from))
            {
                Console.WriteLine("No such vertex in graph");
                return;
            }
            adj[from].Add(to);
            E++;
        }

        public void addVertex(string v)
        {
            if (!adj.ContainsKey(v))
            {
                adj[v] = new HashSet<string>();
            }
        }

        public HashSet<string> getAdjForV(string v)
        {
            if (!adj.ContainsKey(v))
            {
                return null;
            }
            return adj[v];
        }

        public Digraph reverse()
        {
            Digraph reverseD = new Digraph();

            foreach (string key in adj.Keys) 
            {
                reverseD.addVertex(key);
            }

            foreach (KeyValuePair<string, HashSet<string>> pair in adj)
            {
               foreach (string to in  pair.Value)
                {
                    reverseD.addEdge(to, pair.Key);
                }
            }
            return reverseD;
        }

        public void show()
        {
            foreach (KeyValuePair<string, HashSet<string>> pair in adj)
            {
                if (pair.Value.Count == 0)
                {
                    Console.WriteLine(pair.Key + ";");
                } else
                {
                    Console.Write(pair.Key + " -> ");
                    foreach (string w in pair.Value)
                    {
                        Console.Write(w + "; ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
