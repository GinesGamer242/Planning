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
  public Transform      mTarget;

  NodePathfinding       CurrentStartNode;
  NodePathfinding       CurrentTargetNode;

	Grid                Grid;
    Bresenham           Bresenham;

  int                   Iterations = 0;
  float                 LastStepTime = 0.0f;
  float                 TimeBetweenSteps = 0.01f;

  public HeuristicMode  HeuristicUsed = HeuristicMode.Euclidean;
  public bool           SmoothPath = true;    


  /***************************************************************************/

	void Awake()
    {
		Grid = GetComponent<Grid> ();
        Bresenham = GetComponent<Bresenham> ();

        Iterations = 0;
        LastStepTime = 0.0f;
	}

  /***************************************************************************/

	void Update()
  {
    // Positions changed?
    if( PathInvalid() ){
      // Remove old path
      if( Grid.path != null ){
        Grid.path.Clear();
      }
      // Start calculating path again
      Iterations = 0;
      if( TimeBetweenSteps == 0.0f ){
        Iterations = -1;
      }
      FindPath(mSeeker.position, mTarget.position, Iterations );
    }
    else{
      // Path found?
      if( Iterations >= 0 ){
        // One or more iterations?
        if( TimeBetweenSteps == 0.0f ){
          // One iteration, look until path is found
          Iterations = -1;
          FindPath(mSeeker.position, mTarget.position, Iterations );
        }
        else if( Time.time > LastStepTime + TimeBetweenSteps ){
          // Iterate increasing depth every time step
          LastStepTime = Time.time;
          Iterations++;
          FindPath(mSeeker.position, mTarget.position, Iterations );
        }
      }
    }
	}

  /***************************************************************************/

	bool PathInvalid()
  {
    return CurrentStartNode != Grid.NodeFromWorldPoint(mSeeker.position) || CurrentTargetNode != Grid.NodeFromWorldPoint(mTarget.position) ;
  }

  /***************************************************************************/

	void FindPath( Vector3 startPos, Vector3 targetPos, int iterations )
    {
		CurrentStartNode  = Grid.NodeFromWorldPoint(startPos);
		CurrentTargetNode = Grid.NodeFromWorldPoint(targetPos);

		List<NodePathfinding> openSet = new List<NodePathfinding>();
		HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();
		openSet.Add(CurrentStartNode);
        Grid.openSet = openSet;

        int currentIteration = 0;
        NodePathfinding node = CurrentStartNode;

		while( openSet.Count > 0 && node != CurrentTargetNode && ( iterations == -1 || currentIteration < iterations ) )
        {
            // Select best node from open list
            node = openSet[0];

            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost)
                {
                    node = openSet[i];
                }
            }

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            Grid.openSet    = openSet;
            Grid.closedSet  = closedSet;

            // Check destination
            if (node != CurrentTargetNode)
            {
                // Open neighbours
                float shorthestDistanceToNeighbour = 9999f;

                foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, HeuristicUsed == HeuristicMode.Diagonal || 
                                                                               HeuristicUsed == HeuristicMode.Euclidean ? true : false ))
                {
                    if (!neighbour.mWalkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    if (GetDistance(node, neighbour) < shorthestDistanceToNeighbour ||
                        !openSet.Contains(neighbour))
                    {
                        shorthestDistanceToNeighbour = GetDistance(node, neighbour);

                        float gGostFunction = (node.gCost + 
                                               GetDistance(node, neighbour) * neighbour.mCostMultiplier);

                        neighbour.mParent = node;
                        neighbour.gCost = gGostFunction;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }

                currentIteration++;
            }
            else
            {
                // Path found!
                RetracePath(CurrentStartNode,CurrentTargetNode);
    
                Iterations = -1;

                Debug.Log("Statistics:");
                Debug.LogFormat("Total nodes:  {0}", openSet.Count + closedSet.Count );
                Debug.LogFormat("Open nodes:   {0}", openSet.Count );
                Debug.LogFormat("Closed nodes: {0}", closedSet.Count );
            }
		}
	}

  /***************************************************************************/

	void RetracePath(NodePathfinding startNode, NodePathfinding endNode)
    {
		List<NodePathfinding> path = new List<NodePathfinding>();

        NodePathfinding currentNode = endNode;

        while ( currentNode != startNode )
        {
            currentNode = currentNode.mParent;
            path.Add(currentNode);
        }

        path.Reverse();

        if (SmoothPath)
        {
            Bresenham.SmoothPath(path);
        }

        Grid.path = path;
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
