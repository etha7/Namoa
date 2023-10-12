using System;
using System.Collections.Generic;
using System.Linq;


class Graph
{
    HashSet<Node> nodes = new();
    public Node[,] nodeGrid;

    // Costs are tuples (int damage, int distance)
    SortedSet<Path> openPaths = new();

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public Graph(int[,] grid)
    {
        int numRows = grid.GetLength(0);
        int numColumns = grid.GetLength(1);
        Node[,] nodes = new Node[numRows, numColumns];


        // Initialize node grid with new nodes
        for (int row = 0; row < numRows; ++row)
        {
            for (int column = 0; column < numColumns; ++column)
            {
                nodes[row, column] = new Node()
                {
                    name = $"[{row}, {column}]"
                };
                this.nodes.Add(nodes[row, column]);
            }
        }
        nodeGrid = nodes;

        //Add edges with appropriate damage, based on input damage grid
        for (int row = 0; row < numRows; ++row)
        {
            for (int column = 0; column < numColumns; ++column)
            {
                Node current = nodeGrid[row, column];
                //Check all neighbors
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        // Neighbor is in bounds of grid
                        if (row + i < numRows && row + i >= 0 && column + j < numColumns && column + j >= 0)
                        {

                            Node neighbor = nodeGrid[row + i, column + j];
                            Cost costToReachNeighbor;
                            Edge edgeFromCurrentToNeighbor;
                            // skip current node
                            if (!(i == 0 && j == 0))
                            {
                                // Diagonal
                                if (Math.Abs(i) == Math.Abs(j))
                                {
                                    costToReachNeighbor = new()
                                    {
                                        damage = grid[row + i, column + j],
                                        distance = 2
                                    };
                                }
                                else //Non Diagonal neighbor
                                {
                                    costToReachNeighbor = new()
                                    {
                                        damage = grid[row + i, column + j],
                                        distance = 1
                                    };
                                }
                                edgeFromCurrentToNeighbor = new()
                                {
                                    cost = costToReachNeighbor,
                                    source = current,
                                    target = neighbor
                                };
                                current.edges.Add(edgeFromCurrentToNeighbor);
                            }
                        }
                    }
                };

            }

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
            totalCost = new()
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
                    totalCost = Cost.Add(currentOpenPath.totalCost, e.cost),
                    costToPrevious = e.cost,
                    head = currentTarget,
                    previous = currentOpenPath.head
                };
                // We have action points left to reach new node
                if (pathToTarget.totalCost.distance < maxPathLength)
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

    // Only works after calling ShortestPathToAllNodes
    public static List<Path> BacktrackShortestPath(Node source, Node target, List<Path> shortestPath, int maxDistance)
    {
        if (source.Equals(target))
        {
            shortestPath.Add(new()
            {
                head = source,
                totalCost = new()
                {
                    damage = 0,
                    distance = 0
                }
            });
            return shortestPath;
        }
        if (shortestPath.Count == 0)
        {
            Path bestPath = target.openPathCandidates.Min;
            shortestPath.Add(bestPath);
            if (bestPath == null || bestPath.previous == null)
            {
                return new();
            }
            return BacktrackShortestPath(source, bestPath.previous, shortestPath, maxDistance - bestPath.costToPrevious.distance);
        }
        else
        {
            foreach (Path p in target.openPathCandidates)
            {
                if (p.totalCost.distance + shortestPath.Last().costToPrevious.distance <= maxDistance)
                {
                    shortestPath.Add(p);
                    return BacktrackShortestPath(source, p.previous, shortestPath, maxDistance - p.costToPrevious.distance);
                }
            }
        }
        return new();
    }
}


public class Node
{

    public string name;
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
    public bool RemoveDominatedPaths(SortedSet<Path> currentPaths, Path possibleDominatingPath)
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
                break; // If the new path is dominated by or indifferent to p, it will also be dominated by or indifferent to all better paths
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
        if (damage != c.damage)
        {
            return damage.CompareTo(c.damage);
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
    public Cost totalCost;
    public Cost costToPrevious;
    public Node head;
    public Node previous;

    public int Dominates(Path p)
    {
        if (totalCost.damage <= p.totalCost.damage && totalCost.distance < p.totalCost.distance)
        {
            return -1;
        }
        else if (totalCost.damage >= p.totalCost.damage && totalCost.distance > p.totalCost.distance)
        {
            return 1;
        }
        else
        {
            return 0; // paths don't dominate each other
        }
    }

    // TODO - use a sorted data structure that actually supports duplicates or
    // Use a nonhashcode based tie breaking solution that has a 0 percent chance of colliding 
    // Lexicographic ordering
    public int CompareTo(Path p)
    {
        int result = totalCost.CompareTo(p.totalCost);
        if (result == 0)
        {
            // Break ties using HashCode so SortedSet doesn't treat two Paths with different head nodes as duplicates.
            int hashResult = this.GetHashCode().CompareTo(p.GetHashCode());
            if (hashResult == 0)
            {
                // If we tie again (1 in trillion chance), take a second 1 in trillion chance
                return this.head.GetHashCode().CompareTo(p.head.GetHashCode());
            }
        }
        return result;
    }

    public override string ToString()
    {
        return $"Head: {head.name}, Previous {previous?.name}, Cost To Previous: (Damage: {costToPrevious?.damage}, Distance: {costToPrevious?.distance}) , Total Cost: (Damage: {totalCost.damage}, Distance: {totalCost.distance})";
    }

}
