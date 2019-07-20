using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMaximumFlowCSharp
{
    public class Vertex
    {
        public int Index { get; }
        public bool Discovered { get; set; }

        public List<Edge> AdjacencyList;

        public Vertex(int index)
        {
            Index = index;
            AdjacencyList = new List<Edge>();
        }
    }
}
