using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMaximumFlowCSharp
{
    public class Edge
    {
        public int Weight;
        public Vertex First; // IncidentFrom выходит (начало)
        public Vertex Second; // IncidentTo входит (конец)
        public bool InTree;

        public Edge(Vertex incidentFrom, Vertex incidentTo, int weight)
        {
            First = incidentFrom;
            Second = incidentTo;
            Weight = weight;
            InTree = false;
        }

    }

    public class IncidentEdge
    {
        //private Vertex IncidentTo; // входит (конец)
        public Vertex IncidentTo { get; set; } // входит (конец)
        public int Capacity { get; set; }
        public int Flow { get; set; }

        public IncidentEdge(Vertex incidentTo, int capacity)
        {
            IncidentTo = incidentTo;
            Capacity = capacity;
        }
    }
}
