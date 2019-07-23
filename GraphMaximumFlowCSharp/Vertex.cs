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
        public Vertex Parent { get; set; }
        public int ExcessFlow { get; set; } // избыточный поток, входящий в вершину, представляет собой величину, на которую входящий поток превышает исходящий.
        public int Height { get; set; } // (distance function), а высота вершины называется меткой расстояния (distance label)
        public List<Vertex> NeighborList { get; } // односвязный список вершин, смежных c u в графе G. (u, v) £ Е или (v,u) G Е.

        public List<Edge> AdjacencyList { get; }

        public Vertex(int index)
        {
            Index = index;
            AdjacencyList = new List<Edge>();
            NeighborList = new List<Vertex>();
        }
    }
}
