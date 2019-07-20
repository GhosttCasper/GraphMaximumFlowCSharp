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
        //private int _capacity; // пропускная способность
        public bool Discovered { get; set; }
        public bool Color { get; set; }

        //public Vertex Parent { get; set; }


        //private int _flow;     // поток 0 < f{u, v) < с(и, v).
        //private int distance;
        // private Vertex parent;

        public List<Edge> AdjacencyList;


        // private bool discovered;
        //private bool color;

        public Vertex(int index)
        {
            Index = index;
            AdjacencyList = new List<Edge>();
        }
    }
}
