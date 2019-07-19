using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum FileFormat : byte { DIMACS, TXT };

namespace GraphMaximumFlowCSharp
{
    public class Graph
    {
        private VerticesList data;
        public int NumberVertices { get; private set; }
        public int NumberEdges { get; private set; }

        public Graph(string graphFile, FileFormat fileFormat)
        {
            switch (fileFormat)
            {
                //case FileFormat.DIMACS: // Discrete Mathematics and Theoretical Computer Science
                //    LoadDimacsFormatGraph(graphFile);
                //    break;
                case FileFormat.TXT:
                    LoadTXTFormatGraph(graphFile);
                    break;
                default:
                    throw new Exception("Format " + fileFormat + " not supported");
            }
        }

        private void LoadTXTFormatGraph(string graphFile)
        {
            FileStream ifs = new FileStream(graphFile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;

            line = sr.ReadLine();
            line = line.Trim();
            while (line.StartsWith("c"))
            {
                line = sr.ReadLine();
                line = line.Trim();
            }

            tokens = line.Split(' ');
            int numVertices = int.Parse(tokens[1]); // Convert.ToInt32
            int numEdges = int.Parse(tokens[2]);
            if (numVertices < 0 || numEdges < 0)
                throw new Exception("Number nodes or edges is a negative");

            data = new VerticesList(numVertices);

            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                line = line.Trim();
                tokens = line.Split(' ');
                int incidentFromIndex = int.Parse(tokens[0]);
                int incidentToIndex = int.Parse(tokens[1]);
                int capacity = int.Parse(tokens[2]);

                if (incidentFromIndex < 1 || incidentFromIndex > numVertices || incidentToIndex < 1 || incidentToIndex > numVertices)
                    throw new Exception("The vertex parameter is not in the range 0...this.numVertices");
                if (capacity < 0)
                    throw new Exception("The capacity parameter is a negative");

                data.SetValue(incidentFromIndex, incidentToIndex, capacity);
            }

            sr.Close();
            ifs.Close();

            NumberVertices = numVertices;
            NumberEdges = numEdges;
        }

        /// <summary>
        /// Проверка файла данных графа до создания экземпляра объекта графа
        /// </summary>
        public static void ValidateGraphFile(string graphFile, FileFormat fileFormat)
        {
            switch (fileFormat)
            {
                //case FileFormat.DIMACS: // Discrete Mathematics and Theoretical Computer Science
                //    ValidateDimacsGraphFile(graphFile);
                //    break;
                case FileFormat.TXT:
                    ValidateTXTGraphFile(graphFile);
                    break;
                default:
                    throw new Exception("Format " + fileFormat + " not supported");
            }
        }

        public static void ValidateTXTGraphFile(string graphFile)
        {
            FileStream ifs = new FileStream(graphFile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;

            line = sr.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                try
                {
                    if (line.StartsWith("p"))
                    {
                        tokens = line.Split(' ');
                        int numNodes = int.Parse(tokens[1]);
                        int numEdges = int.Parse(tokens[2]);
                    }

                    if (line.StartsWith("c") == false && line.StartsWith("p") == false)
                    {
                        tokens = line.Split(' ');
                        int incidentFromIndex = int.Parse(tokens[0]);
                        int incidentToIndex = int.Parse(tokens[1]);
                        int capacity = int.Parse(tokens[2]);
                    }
                }
                catch
                {
                    throw new Exception("Error parsing line = " + line);
                }

                line = sr.ReadLine();
            }

            sr.Close();
            ifs.Close();
        }

        public void FordFulkerson()
        {
            data.InitializeFlowToZero();
            Vertex s = data.GetSource();
            Vertex t = data.GetSink();
            int maxFlow = 0;

            while (FindAugmentingPath())
            {
            }
            foreach (var incidentEdge in s.AdjacencyList)
            {
                maxFlow += incidentEdge.Flow;
            }

            Console.WriteLine(maxFlow);
        }

        private List<IncidentEdge> DFSVisit(Vertex curVertex, Vertex sink, List<IncidentEdge> path, ref int minCapacity, ref bool isFind)
        {
            curVertex.Discovered = true;

            if (curVertex == sink)
            {
                isFind = true;
                return path;
            }

            foreach (var edge in curVertex.AdjacencyList)
            {
                if (edge.IncidentTo.Discovered == false && edge.Capacity - edge.Flow > 0)
                {
                    path.Add(edge);
                    if (edge.Capacity - edge.Flow < minCapacity)
                        minCapacity = edge.Capacity - edge.Flow;
                    edge.IncidentTo.Parent = curVertex;
                    DFSVisit(edge.IncidentTo, sink, path, ref minCapacity, ref isFind);
                }
            }

            return path;
        }

        // поиск пути, по которому возможно пустить поток алгоритмом обхода графа в ширину
        // функция ищет путь из истока в сток, по которому еще можно пустить поток,
        // считая вместимость ребера (i,j) равной c[i][j] - f[i][j]

        public bool FindAugmentingPath() // source - исток, target - сток
        {
            data.InitializeVertices();
            Vertex s = data.GetSource();
            Vertex t = data.GetSink();
            List<IncidentEdge> path = new List<IncidentEdge>();

            int minCapacity = int.MaxValue;
            bool isFind = false;

            if (s.Discovered == false)
                path = DFSVisit(s, t, path, ref minCapacity, ref isFind);

            if (isFind)
            {
                foreach (var incidentEdge in path)
                {
                    incidentEdge.Flow = incidentEdge.Flow + minCapacity;
                }
            }


            return isFind;
        }


        /*
 * Ford-Fulkerson-Method (G, s, t)
   1 Инициализация потока / нулевым значением
   2 while существует увеличивающий путь р в остаточной сети Gу
   3 увеличиваем поток / вдоль пути р
   4 return /

   Ford-Fulkerson(<3, s, t)
   1 for каждого ребра (u, v) е G.E
   2 (u,v).f = 0
   3 while существует путь p из s в t в остаточной сети G/
   4 cf{p) = min {cf(u, v) : (u, v) содержится ър}
   5 for каждого ребра (и, v) в р
   6 if (и, v) е Е
   7 (u,v).f = (u,v).f + cf(p)
   8 else (v, u).f = (v, u).f - cf(p)
 */

        private class VerticesList
        {
            private List<Vertex> data;
            public int Number;

            public VerticesList(int n)
            {
                data = new List<Vertex>(n);
                for (int i = 1; i <= n; i++)
                    data.Add(new Vertex(i));
                Number = n;
            }

            public Vertex GetSource()
            {
                if (data.Count > 0)
                    return data[0];
                return null;
            }

            public Vertex GetSink()
            {
                if (data.Count > 0)
                    return data.Last();
                return null;
            }

            public Vertex GetVertex(int index)
            {
                if (data.Count > 0 && index > 0)
                    return data[index - 1];
                return null;
            }

            //public bool GetValue(int row, int col)
            //{
            //    return data[row][col];
            //}

            public void SetValue(int incidentFromIndex, int incidentToIndex, int capacity)
            {
                Vertex curVertexFrom = data[incidentFromIndex - 1];
                Vertex curVertexTo = data[incidentToIndex - 1];
                IncidentEdge curEdge = new IncidentEdge(curVertexTo, capacity);
                curVertexFrom.AdjacencyList.Add(curEdge);
            }

            public void InitializeFlowToZero()
            {
                foreach (var vertex in data)
                {
                    foreach (var edge in vertex.AdjacencyList)
                    {
                        edge.Flow = 0;
                    }
                }
            }

            public void InitializeVertices()
            {
                foreach (var vertex in data)
                {
                    vertex.Parent = null;
                    vertex.Discovered = false;
                }
            }

        }
    }
}
