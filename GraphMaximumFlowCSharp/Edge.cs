using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMaximumFlowCSharp
{
    public class Edge //Incident
    {
        public Vertex IncidentFrom { get; } // выходит(начало) 
        public Vertex IncidentTo { get; }   // входит (конец)
        public int Capacity { get; }        // пропускная способность
        public int Flow { get; set; }       // поток 0 < f{u, v) < с(и, v).

        public Edge(Vertex incidentFrom, Vertex incidentTo, int capacity)
        {
            IncidentFrom = incidentFrom;
            IncidentTo = incidentTo;
            Capacity = capacity;
        }
    }
}
