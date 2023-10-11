using System;
using System.Collections.Generic;

class Graph
{

    Graph solutionGraph = new();

    HashSet<Node> nodes;

    // Costs are tuples (int damage, int distance)
    SortedSet<Path> openPaths = new();

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public class Node
    {
        public HashSet<Edge> edges;

        // Paths terminating in 'this' which haven't been explored
        public SortedSet<Path> openPathCandidates = new(); //SortedSet is a Red-Black tree, which has faster deletion than PQ

        // Paths terminating in 'this' which have been explored
        public SortedSet<Path> closedPathCandidates = new();

        // Check whether a new path to 'this' is better than any found ones. If so, forget the dominated paths.
        public void RemoveDominatedPaths(Path possibleDominatingPath)
        {
            RemoveDominatedPaths(openPathCandidates, possibleDominatingPath);
            RemoveDominatedPaths(closedPathCandidates, possibleDominatingPath);
        }

        // Look for possible dominated vectors, from worst to best, and remove them from a set of paths
        public static void RemoveDominatedPaths(SortedSet<Path> currentPaths, Path possibleDominatingPath)
        {
            List<Path> pathsToRemove = new();
            bool comparisonResult;
            foreach (Path p in currentPaths.Reverse())
            {
                comparisonResult = possibleDominatingPath.Dominates(p);
                if (comparisonResult)
                {
                    pathsToRemove.Add(p);
                }
                else
                {
                    break; // If the new path is dominated by p, it will also be dominated by all better paths
                    // so we can stop looking
                }
            }
            foreach (Path dominatedPath in pathsToRemove)
            {
                currentPaths.Remove(dominatedPath);
            }
        }
    }

    public class Edge
    {
        public (int, int) cost;
        public Node source;
        public Node target;
    }

    public class Path : IComparable<Path>
    {
        public (int, int) cost;
        public Node head;
        public Node previous;

        public bool Dominates(Path p)
        {
            return cost.Item1 <= p.cost.Item1 && cost.Item2 <= p.cost.Item2;
        }

        // Lexicographic ordering
        public int CompareTo(Path p)
        {
            int result = cost.Item1.CompareTo(cost.Item1);
            return result != 0 ? result : cost.Item2.CompareTo(cost.Item2);
        }
    }


    //NAMOA* algorithm
    public Path shortestPath(Node source, Node target, int distance)
    {

        //Initialize 
        Path initialPath = new Path
        {
            cost = (0, 0),
            head = source
        };
        openPaths.Add(initialPath);


        while (openPaths.Count > 0)
        {
            Path currentOpenPath = openPaths.Min;
            openPaths.Remove(currentOpenPath);
            foreach (Edge e in currentOpenPath.head.edges)
            {
                Node currentTarget = e.target;
                Path pathToTarget = new()
                {
                    cost = (currentOpenPath.cost.Item1 + e.cost.Item1, currentOpenPath.cost.Item2 + e.cost.Item2)
                    head = currentTarget,
                    previous = currentOpenPath.head
                };
                currentTarget.RemoveDominatedPaths(pathToTarget);
            }

        }
        return null;

    }

}