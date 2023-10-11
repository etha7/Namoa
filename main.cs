using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.VisualBasic;

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

        graph.ShortestPathToAllNodes(source, 10);
        Console.WriteLine(source.openPathCandidates.Min);
        Console.WriteLine(target.openPathCandidates.Min);

        Console.WriteLine("\nPrinting shortest Path from A to B");
        List<Path> shortestPath = Graph.BacktrackShortestPath(source, target, new(), 10);
        foreach (Path p in shortestPath)
        {
            Console.WriteLine(p);
        }
    }
}