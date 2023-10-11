using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;

class Graph
{

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
        public Cost cost;
        public Node source;
        public Node target;
    }

    // Cost of a Path or an Edge
    public class Cost : IComparable<Cost>
    {
        public int damage;
        public int distance;

        // Lexicographic ordering
        public int CompareTo(Cost c)
        {
            if (damage < c.damage)
            {
                return -1;
            }
            else
            {
                return distance.CompareTo(c.distance);
            }
        }

        public static Cost Add(Cost a, Cost b)
        {
            return new()
            {
                damage = a.damage + b.damage,
                distance = a.distance + b.distance
            };
        }
    }

    public class Path : IComparable<Path>
    {
        public Cost cost;
        public Node head;
        public Node previous;

        public bool Dominates(Path p)
        {
            return cost.damage <= p.cost.damage && cost.distance <= p.cost.distance;
        }

        // Lexicographic ordering
        public int CompareTo(Path p)
        {
            return cost.CompareTo(p.cost);
        }

    }


    //NAMOA* algorithm
    public Path ShortestPathToAllNodes(Node source, int distance)
    {

        //Initialize 
        Path initialPath = new()
        {
            cost = new()
            {
                damage = 0,
                distance = 0
            },
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
                    cost = Cost.Add(currentOpenPath.cost, e.cost),
                    head = currentTarget,
                    previous = currentOpenPath.head
                };
                currentTarget.RemoveDominatedPaths(pathToTarget);
            }

        }
        return null;

    }

}