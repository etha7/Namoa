using System;
using System.Xml;
using Microsoft.VisualBasic;

class Program
{
    public static void Main(string[] args)
    {
        Graph graph = new();
        Graph.Node A = new();
        Graph.Node B = new();
        Graph.Edge AtoB = new()
        {
            cost = (1, 1),
            source = A,
            target = B
        };
        A.edges.Add(AtoB);

        Graph.Edge BtoA = new()
        {
            cost = (1, 1),
            source = B,
            target = A
        };
        B.edges.Add(BtoA);

        graph.AddNode(A);
        graph.AddNode(B);   

    }
}