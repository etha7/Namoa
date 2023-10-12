using System;
using System.Collections.Generic;

class Program
{
    public static void Main()
    {
        // Damage per tile
        int[,] grid = {
            {0,0},
            {0,0},
            {1,0},
            {0,1}
        };
        Graph graph = new(grid);
        Node source = graph.nodeGrid[3, 0];
        Node target = graph.nodeGrid[0, 0];

        graph.ShortestPathToAllNodes(source, 3);
        Console.WriteLine(source.openPathCandidates.Min);
        Console.WriteLine(target.openPathCandidates.Min);

        Console.WriteLine("\nPrinting shortest Path from source to target");
        List<Path> shortestPath = Graph.BacktrackShortestPath(source, target, new(), 3);
        foreach (Path p in shortestPath)
        {
            Console.WriteLine(p);
        }
    }
}