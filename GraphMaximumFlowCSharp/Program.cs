using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 16. Найти максимальный поток методом проталкивания предпотока и найти минимальный разрез в сети.
 *
 * Минимальным разрезом (minimum cut) сети является разрез, пропускная способность которого среди всех разрезов сети минимальна.
 */

namespace GraphMaximumFlowCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string graphFile = "..\\..\\input.txt";
                Graph.ValidateGraphFile(graphFile, FileFormat.TXT);

                Graph graph = new Graph(graphFile, FileFormat.TXT);

                //graph.FordFulkerson();
                //graph.EdmondsКагр();
                //graph.GenericPushRelabel();
                int maxFlow = graph.RelabelToFront();
                string minCut = graph.FindMinimumCut();
                WriteFile(graph, "output.txt", maxFlow, minCut);

                string outputGraphFile = "..\\..\\output.txt";
                graph.SaveTxtFormatGraph(outputGraphFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal: " + e.Message);
            }
        }

        private static void WriteFile(Graph graph, string fileName, int maxFlow, string minCut)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine(graph.ToString());
                writer.WriteLine(maxFlow.ToString());
                writer.WriteLine(minCut);
            }
        }
    }
}
