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
        private UMatrix residualNetwork;
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
            residualNetwork = new UMatrix(numVertices);

            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                line = line.Trim();
                tokens = line.Split(' ');
                int incidentFromIndex = int.Parse(tokens[0]);
                int incidentToIndex = int.Parse(tokens[1]);
                ushort capacity = ushort.Parse(tokens[2]);

                if (incidentFromIndex < 1 || incidentFromIndex > numVertices || incidentToIndex < 1 ||
                    incidentToIndex > numVertices)
                    throw new Exception("The vertex parameter is not in the range 0...this.numVertices");

                data.SetValue(incidentFromIndex, incidentToIndex, capacity);
                residualNetwork.SetValue(incidentFromIndex - 1, incidentToIndex - 1, capacity);
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
                        ushort capacity = ushort.Parse(tokens[2]);
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

        public bool AreAdjacent(int vertexA, int vertexB)
        {
            return residualNetwork.GetValue(vertexA, vertexB) > 0;
        }

        /// <summary>
        /// Алгоритм (метод) Форда-Фалкерсона. Поиск увеличивающего пути через обход в глубину. Сложность 0{Е|f*|).
        /// </summary>
        public int FordFulkerson()
        {
            data.InitializeFlowToZero(); // Инициализация потока / нулевым значением

            Vertex s = data.GetSource();
            List<Edge> path = new List<Edge>();

            while ((path = FindAugmentingPathDFS(s)) != null) // while существует увеличивающий путь р из s в t в остаточной сети Gf
            {
                int minResidualCapacity = GetMinResidualCapacity(path); // cf{p) = min {cf(u, v) : (u, v) содержится в р}
                UpdateFlowAndResidualNetwork(path, minResidualCapacity); // увеличиваем поток / вдоль пути р

                Console.WriteLine(residualNetwork.ToString());
                Console.WriteLine(data.ToString());
            }

            int maxFlow = 0;
            foreach (var incidentEdge in s.AdjacencyList)
            {
                maxFlow += incidentEdge.Flow;
            }

            Console.WriteLine(maxFlow);
            return maxFlow;
        }

        private List<Edge> FindAugmentingPathDFS(Vertex source)
        {
            data.InitializeVertices();

            List<Edge> path = new List<Edge>();
            bool isFind = false;

            path = DFSVisit(source, path, ref isFind);
            if (isFind)
                return path;

            return null;
        }

        private List<Edge> DFSVisit(Vertex curVertex, List<Edge> path, ref bool isFind)
        {
            int i = curVertex.Index - 1;

            if (curVertex == data.GetSink())
            {
                isFind = true;
                return path;
            }

            for (int j = 0; j < NumberVertices; j++)
            {
                if (AreAdjacent(i, j))
                {
                    curVertex.Discovered = true;
                    Vertex neighbor = data.GetVertex(j);
                    if (neighbor.Discovered == false)
                    {
                        var edge = data.GetEdge(i, j) ?? data.GetEdge(j, i);
                        path.Add(edge);
                        DFSVisit(neighbor, path, ref isFind);
                        if (isFind)
                            break;
                        path.Remove(path.Last());
                    }
                }

            }

            return path;
        }

        private int GetMinResidualCapacity(List<Edge> path)
        {
            int minResidualCapacity = int.MaxValue;
            int previousTo = -1;
            foreach (var edge in path)
            {
                int from = edge.IncidentFrom.Index - 1;
                int to = edge.IncidentTo.Index - 1;
                int curResidualCapacity = -1;

                if (previousTo == to)  // if (u, v) не принадлежит Е
                {
                    curResidualCapacity = residualNetwork.GetValue(to, from);
                    previousTo = from;
                }
                else // if (u, v) е Е
                {
                    curResidualCapacity = residualNetwork.GetValue(from, to);
                    previousTo = to;
                }
                if (curResidualCapacity < minResidualCapacity)
                    minResidualCapacity = curResidualCapacity;
            }

            return minResidualCapacity;
        }

        private void UpdateFlowAndResidualNetwork(List<Edge> path, int minResidualCapacity)
        {
            int previousTo = -1;
            foreach (var edge in path) // for каждого ребра (u, v) в р
            {
                int from = edge.IncidentFrom.Index - 1;
                int to = edge.IncidentTo.Index - 1;

                if (previousTo == to) // if (u, v) не принадлежит Е
                {
                    edge.Flow = edge.Flow - minResidualCapacity; // (v, u).f = (v, u).f - cf(p)
                    residualNetwork.SetValue(to, from, (ushort)edge.Flow);
                    previousTo = from;
                }
                else // if (u, v) е Е
                {
                    edge.Flow = edge.Flow + minResidualCapacity; // (u, v).f = (u, v).f + cf(p)
                    residualNetwork.SetValue(from, to, (ushort)(edge.Capacity - edge.Flow));
                    residualNetwork.SetValue(to, from, (ushort)edge.Flow);
                    previousTo = to;
                }
            }
        }

        /// <summary>
        /// Алгоритм Эдмондса-Карпа. Поиск увеличивающего пути р через поиск в ширину. Сложность 0(V*E^2).
        /// </summary>
        public int EdmondsКагр()
        {
            data.InitializeFlowToZero(); // Инициализация потока / нулевым значением

            Vertex s = data.GetSource();
            List<Edge> path = new List<Edge>();

            while ((path = FindAugmentingPathBFS(s)) != null) // while существует увеличивающий путь р из s в t в остаточной сети Gf
            {
                int minResidualCapacity = GetMinResidualCapacity(path); // cf{p) = min {cf(u, v) : (u, v) содержится в р}
                UpdateFlowAndResidualNetwork(path, minResidualCapacity); // увеличиваем поток / вдоль пути р

                Console.WriteLine(residualNetwork.ToString());
                Console.WriteLine(data.ToString());
            }

            int maxFlow = 0;
            foreach (var incidentEdge in s.AdjacencyList)
            {
                maxFlow += incidentEdge.Flow;
            }

            Console.WriteLine(maxFlow);
            return maxFlow;
        }

        private List<Edge> FindAugmentingPathBFS(Vertex source)
        {
            data.InitializeVertices();

            List<Edge> path = new List<Edge>();
            bool isFind = false;
            Vertex sink = data.GetSink();

            source.Discovered = true;
            Queue<Vertex> queue = new Queue<Vertex>();
            queue.Enqueue(source);
            while (queue.Count != 0)
            {
                Vertex curVertex = queue.Dequeue();
                if (curVertex == sink)
                {
                    isFind = true;
                    break;
                }
                int i = curVertex.Index - 1;
                for (int j = 0; j < NumberVertices; j++)
                {
                    if (AreAdjacent(i, j))
                    {
                        Vertex neighbor = data.GetVertex(j);
                        if (neighbor.Discovered == false)
                        {
                            neighbor.Discovered = true;
                            neighbor.Parent = curVertex;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            Vertex curVertexInPath = sink;
            while (curVertexInPath != source)
            {
                int from = curVertexInPath.Parent.Index - 1;
                int to = curVertexInPath.Index - 1;
                var edge = data.GetEdge(from, to) ?? data.GetEdge(to, from);
                path.Add(edge);
                curVertexInPath = curVertexInPath.Parent;
            }

            path.Reverse();


            if (isFind)
                return path;

            return null;
        }

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
                    return data.First();
                return null;
            }

            public Vertex GetSink()
            {
                if (data.Count > 0)
                    return data.Last();
                return null;
            }

            public Vertex GetVertex(int arrayIndex)
            {
                if (data.Count > 0)
                    return data[arrayIndex];
                return null;
            }

            public Edge GetEdge(int fromArrayIndex, int toArrayIndex)
            {
                Vertex vertexFrom = data[fromArrayIndex];
                foreach (var edge in vertexFrom.AdjacencyList)
                {
                    if (edge.IncidentTo.Index == toArrayIndex + 1)
                        return edge;
                }

                return null;
            }

            public void SetValue(int incidentFromIndex, int incidentToIndex, int capacity)
            {
                Vertex vertexFrom = data[incidentFromIndex - 1];
                Vertex vertexTo = data[incidentToIndex - 1];
                Edge curEdge = new Edge(vertexFrom, vertexTo, capacity);
                vertexFrom.AdjacencyList.Add(curEdge);
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
                    vertex.Discovered = false;
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Number; ++i)
                {
                    sb.Append(data[i].Index + ": ");
                    foreach (var edge in data[i].AdjacencyList)
                    {
                        sb.Append(edge.IncidentTo.Index + " ");
                        sb.Append(edge.Flow + "/");
                        sb.Append(edge.Capacity + " ");
                    }
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

        }

        private class UMatrix
        {
            private ushort[,] matrix;
            public readonly int Dim;

            public UMatrix(int n)
            {
                this.matrix = new ushort[n, n];
                this.Dim = n;
            }

            public int GetValue(int row, int col)
            {
                return matrix[row, col];
            }

            public void SetValue(int row, int col, ushort value)
            {
                matrix[row, col] = value;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Dim; ++i)
                {
                    for (int j = 0; j < Dim; ++j)
                    {
                        sb.Append(matrix[i, j]);
                        sb.Append(" ");
                    }

                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }
        }
    }
}