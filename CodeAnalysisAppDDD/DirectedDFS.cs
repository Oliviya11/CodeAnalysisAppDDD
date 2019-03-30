using System.Collections.Generic;

namespace CodeAnalysisAppDDD
{
    class DirectedDFS
    {
        private HashSet<string> marked;
        public DirectedDFS(Digraph G, string s)
        {
            marked = new HashSet<string>();
            dfs(G, s);
        }

        public DirectedDFS(Digraph G, IEnumerable<string> sources)
        {
            marked = new HashSet<string>();
            foreach (string s in sources)
            {
                if (!marked.Contains(s))
                {
                    dfs(G, s);
                }
            }
        }

        private void dfs(Digraph G, string v)
        {
            marked.Add(v);
            foreach (string w in G.getAdjForV(v))
            {
                if (!marked.Contains(w))
                {
                    dfs(G, w);
                }
            }
        }

        public bool isMarkedV(string v)
        {
            return marked.Contains(v);
        }

        public HashSet<string> getMarked()
        {
            return new HashSet<string>(marked);
        }
    }
}
