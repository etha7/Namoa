using System;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.VisualBasic;

class Program
{
    public static void Main()
    {
        Graph graph = new();
        Graph.Node A = new()
        {
            name = "A"
        };
        Graph.Node B = new()
        {
            name = "B"
        };
        Graph.Edge AtoB = new()
        {
            cost = new()
            {
                damage = 1,
                distance = 1
            },
            source = A,
            target = B
        };
        A.edges.Add(AtoB);

        Graph.Edge BtoA = new()
        {
            cost = new()
            {
                damage = 1,
                distance = 1
            },
            source = B,
            target = A
        };
        B.edges.Add(BtoA);

        graph.AddNode(A);
        graph.AddNode(B);

        graph.ShortestPathToAllNodes(A, 10);
        Console.WriteLine(B.openPathCandidates.Min);
        Console.WriteLine(A.openPathCandidates.Min);
    }
}