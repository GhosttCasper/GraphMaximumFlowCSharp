using System;
using System.Collections.Generic;
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
                graph.GenericPushRelabel();
            }
            catch (Exception e)
            {
                Console.WriteLine("Fatal: " + e.Message);
            }
        }
    }
}
