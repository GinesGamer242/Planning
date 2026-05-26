using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public enum HeuristicMode
{
    Manhattan,
    Diagonal,
    Euclidean
}

[RequireComponent(typeof(Bresenham))]
public class Pathfinding : MonoBehaviour
{

    public Transform      mSeeker;

	Grid                Grid;
    Bresenham           Bresenham;

    public HeuristicMode  HeuristicUsed = HeuristicMode.Euclidean;
    public bool           SmoothPath = true;    


  /***************************************************************************/

	void Awake()
    {
		Grid = GetComponent<Grid> ();
        Bresenham = GetComponent<Bresenham> ();
	}

    /***************************************************************************/

    public List<NodePathfinding> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        NodePathfinding startNode = Grid.NodeFromWorldPoint(startPos);
        NodePathfinding targetNode = Grid.NodeFromWorldPoint(targetPos);

        // Clean up previous pathfinding data
        Grid.openSet = new List<NodePathfinding>();
        Grid.closedSet = new HashSet<NodePathfinding>();
        Grid.path = null;

        if (startNode == targetNode)
            return new List<NodePathfinding>();

        List<NodePathfinding> openSet = new List<NodePathfinding>();
        HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();

        openSet.Add(startNode);
        Grid.openSet = openSet;

        NodePathfinding node = startNode;

        while (openSet.Count > 0)
        {
            node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost ||
                   (openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost))
                {
                    node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            // Update realtime visualization of the grid
            Grid.openSet = openSet;
            Grid.closedSet = closedSet;

            if (node == targetNode)
            {
                List<NodePathfinding> path = BuildPath(startNode, targetNode);
                Grid.path = path;

                int totalNodes = openSet.Count + closedSet.Count;
                int openNodes = openSet.Count;
                int closedNodes = closedSet.Count;
                Debug.Log($"Total nodes {totalNodes}, Open nodes: {openNodes}, Closed nodes: {closedNodes}");

                return path;
            }

            bool diagonal = HeuristicUsed == HeuristicMode.Diagonal ||
                            HeuristicUsed == HeuristicMode.Euclidean;

            foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, diagonal))
            {
                if (!neighbour.mWalkable || closedSet.Contains(neighbour))
                    continue;

                float newCost = node.gCost + GetDistance(node, neighbour) * neighbour.mCostMultiplier;

                if (newCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCost;
                    neighbour.hCost = Heuristic(neighbour, targetNode);
                    neighbour.mParent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        return null;
    }

    /***************************************************************************/

    List<NodePathfinding> BuildPath(NodePathfinding startNode, NodePathfinding endNode)
    {
        List<NodePathfinding> path = new List<NodePathfinding>();
        NodePathfinding current = endNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.mParent;
        }
        path.Reverse();

        if (SmoothPath)
            Bresenham.SmoothPath(path);

        return path;
    }

  /***************************************************************************/

	float GetDistance(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        switch(HeuristicUsed)
        {
            case HeuristicMode.Manhattan:

                return Mathf.Abs(nodeA.mGridX - nodeB.mGridX) +
                       Mathf.Abs(nodeA.mGridY - nodeB.mGridY);

            case HeuristicMode.Diagonal:

                // NOT IMPLEMENTED

            case HeuristicMode.Euclidean:

                return Mathf.Sqrt(Mathf.Pow(nodeA.mGridX - nodeB.mGridX, 2) +
                                  Mathf.Pow(nodeA.mGridY - nodeB.mGridY, 2));
            default:

                return 0f;
        }
    }

  /***************************************************************************/

	float Heuristic(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        return GetDistance(nodeA, nodeB);
    }

  /***************************************************************************/

}
