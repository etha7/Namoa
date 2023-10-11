using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;
using System.Timers;

class Graph
{

    HashSet<Node> nodes = new();

    // Costs are tuples (int damage, int distance)
    SortedSet<Path> openPaths = new();

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public class Node
    {

        public String name;
        public HashSet<Edge> edges = new();

        // Paths terminating in 'this' which haven't been explored
        public SortedSet<Path> openPathCandidates = new(); //SortedSet is a Red-Black tree, which has faster deletion than PQ

        // Paths terminating in 'this' which have been explored
        public SortedSet<Path> closedPathCandidates = new();

        // Check whether a new path to 'this' is better than any found ones. If so, forget the dominated paths.
        public bool RemoveDominatedPaths(Path possibleDominatingPath)
        {
            return RemoveDominatedPaths(openPathCandidates, possibleDominatingPath) ||
            RemoveDominatedPaths(closedPathCandidates, possibleDominatingPath);
        }

        // Look for possible dominated vectors, from worst to best, and remove them from a set of paths
        // Return true if new path dominates any current paths
        public static bool RemoveDominatedPaths(SortedSet<Path> currentPaths, Path possibleDominatingPath)
        {
            List<Path> pathsToRemove = new();
            int comparisonResult;
            foreach (Path p in currentPaths.Reverse())
            {
                comparisonResult = possibleDominatingPath.Dominates(p);
                if (comparisonResult == -1)
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

            // if new path dominates any paths, including the null path
            return pathsToRemove.Count > 0 || currentPaths.Count == 0;
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

        public int Dominates(Path p)
        {
            if (cost.damage < p.cost.damage && cost.distance < p.cost.distance)
            {
                return -1;
            }
            else if (cost.damage > p.cost.damage && cost.distance > p.cost.distance)
            {
                return 1;
            }
            else
            {
                return 0; // paths don't dominate each other
            }
        }

        // Lexicographic ordering
        public int CompareTo(Path p)
        {
            return cost.CompareTo(p.cost);
        }

        public override string ToString()
        {
            return $"(Damage: {cost.damage}, Distance: {cost.distance})";
        }

    }

    // Gets rid of path data from previous shortest path searches
    void ClearGraphPaths()
    {
        openPaths = new();
        foreach (Node n in nodes)
        {
            n.openPathCandidates = new();
            n.closedPathCandidates = new();
        }
    }

    //NAMOA* algorithm
    public Path ShortestPathToAllNodes(Node source, int maxPathLength)
    {
        ClearGraphPaths();
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
        source.openPathCandidates.Add(initialPath);
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
                // We have action points left to reach new node
                if (pathToTarget.cost.distance < maxPathLength)
                {

                    // Get rid of paths which the new path beats
                    bool pathToTargetDominatesSomePath = currentTarget.RemoveDominatedPaths(pathToTarget);
                    if (pathToTargetDominatesSomePath)
                    {
                        //Add new path to list of open paths we still need to investigate
                        currentTarget.openPathCandidates.Add(pathToTarget);
                        openPaths.Add(pathToTarget);
                    }
                }
            }

        }
        return null;
    }
}