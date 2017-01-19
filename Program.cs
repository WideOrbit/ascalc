using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ascalc
{
    class Program
    {
        static int Main(string[] args)
        {
            long nodes, lostNodes, objectsPerNode, replicationFactor, nowritesPercent;

            if (args.Length != 5 ||
                !long.TryParse(args[0], out nodes) ||
                !long.TryParse(args[1], out lostNodes) ||
                !long.TryParse(args[2], out replicationFactor) ||
                !long.TryParse(args[3], out nowritesPercent) ||
                !long.TryParse(args[4], out objectsPerNode))
            {
                Console.WriteLine(
@"Tool to determine if an Aerospike cluster can survive lost nodes.

Version 1.0

Usage: ascalc <nodes> <lost nodes> <replication factor> <nowrite percent> <objects per node>

nodes:               Total cluster nodes.
lost nodes:          Lost cluster nodes.
replication factor:  Usually 2.
nowrite percent:     Usually 100.
objects per node:    Objects are automatically rebalanced between nodes, current object count from any node will usually be ok.");
                return 1;
            }

            Calc(nodes, lostNodes, replicationFactor, nowritesPercent, objectsPerNode);

            return 0;
        }

        static bool Calc(long nodes, long lostNodes, long replicationFactor, long nowritesPercent, long objectsPerNode)
        {
            const long maxObjectsPerNode = 1024L * 1024 * 1024 * 4 - 1;

            if (lostNodes >= nodes)
            {
                Console.WriteLine("No, lost nodes cannot be >= nodes.");
                return false;
            }

            if (lostNodes >= replicationFactor)
            {
                Console.WriteLine("Warning: You will lose distinct objects because lost nodes >= replication factor.");
            }


            long uniqueObjects = objectsPerNode * nodes / replicationFactor;
            long maxObjectsPerNodeIfFail = maxObjectsPerNode * nowritesPercent / 100;
            long objectsPerNodeIfFail = uniqueObjects / (nodes - lostNodes) * replicationFactor;

            if (objectsPerNodeIfFail > maxObjectsPerNodeIfFail)
            {
                Console.WriteLine($"No, cluster cannot survive {lostNodes} failed nodes.");
                Console.WriteLine($"Max objects per node if fail: {maxObjectsPerNodeIfFail}");
                Console.WriteLine($"    Objects per node if fail: {objectsPerNodeIfFail}");
                return false;
            }

            Console.WriteLine($"Yes, cluster can survive {lostNodes} failed nodes.");
            Console.WriteLine($"Max objects per node if fail: {maxObjectsPerNodeIfFail}");
            Console.WriteLine($"    Objects per node if fail: {objectsPerNodeIfFail}");
            return true;
        }
    }
}
