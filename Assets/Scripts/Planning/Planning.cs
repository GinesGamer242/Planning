using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Planning : MonoBehaviour
{
  ExampleNodePlanning      CurrentStartNode;
  ExampleNodePlanning      CurrentTargetNode;

  ExampleWorld             mWorld;

  /***************************************************************************/

	void Start()
    {
		mWorld = GetComponent<ExampleWorld>();

        Debug.Log( "Planning..." );
        FindPlan( ExampleWorld.WorldState.WORLD_STATE_NONE, ExampleWorld.WorldState.WORLD_STATE_ENEMY_DEAD );
	}

  /***************************************************************************/

	void Update()
  {
	}

  /***************************************************************************/

	public List<ExampleNodePlanning> FindPlan( ExampleWorld.WorldState startWorldState, ExampleWorld.WorldState targetWorldState )
    {
		CurrentStartNode  = new ExampleNodePlanning( startWorldState, null );
		CurrentTargetNode = new ExampleNodePlanning( targetWorldState, null );

		List<ExampleNodePlanning> openSet      = new List<ExampleNodePlanning>();
		HashSet<ExampleNodePlanning> closedSet = new HashSet<ExampleNodePlanning>();
		openSet.Add( CurrentStartNode );
        mWorld.openSet    = openSet;

        ExampleNodePlanning node = CurrentStartNode;
		while( openSet.Count > 0 && ( ( node.mWorldState & CurrentTargetNode.mWorldState ) != CurrentTargetNode.mWorldState ) ) {
            // Select best node from open list
	        node = openSet[0];

            for (int i = 1; i < openSet.Count; i ++) {
				    if (openSet[i].fCost < node.fCost || ( openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost ) ) {
					    node = openSet[i];
				    }
			    }

            // Manage open/closed list
            openSet.Remove(node);
		    closedSet.Add(node);
            mWorld.openSet    = openSet;
            mWorld.closedSet  = closedSet;



            // Check destination
		    if ( ( ( node.mWorldState & CurrentTargetNode.mWorldState ) != CurrentTargetNode.mWorldState ) ) {

                // Open neighbours
                foreach ( ExampleNodePlanning neighbour in mWorld.GetNeighbours( node ) ) {
                    if ( /*!neighbour.mWalkable ||*/ closedSet.Any( n => n.mWorldState == neighbour.mWorldState ) ) {
					    continue;
				    }

				    float newCostToNeighbour = node.gCost + GetDistance( node, neighbour );
				    if (newCostToNeighbour < neighbour.gCost || !openSet.Any( n => n.mWorldState == neighbour.mWorldState ) ) {
                        neighbour.gCost = newCostToNeighbour;
					    neighbour.hCost = Heuristic( neighbour, CurrentTargetNode );
					    neighbour.mParent = node;

					    if (!openSet.Any( n => n.mWorldState == neighbour.mWorldState ) ){
						    openSet.Add(neighbour);
                            mWorld.openSet    = openSet;
                        }
                        else {
                            // Find neighbour and replace
                            openSet[ openSet.FindIndex( x => x.mWorldState == neighbour.mWorldState ) ] = neighbour;
                        }
				    }
                }
            }
            else {
            // Path found!

            // End node must be copied
            CurrentTargetNode.mParent = node.mParent;
            CurrentTargetNode.mAction = node.mAction;
            CurrentTargetNode.gCost   = node.gCost;
            CurrentTargetNode.hCost   = node.hCost;

            RetracePlan(CurrentStartNode,CurrentTargetNode);

            Debug.Log("Statistics:");
            Debug.LogFormat("Total nodes:  {0}", openSet.Count + closedSet.Count );
            Debug.LogFormat("Open nodes:   {0}", openSet.Count );
            Debug.LogFormat("Closed nodes: {0}", closedSet.Count );
            }
		}

        // Log plan
        Debug.Log("PLAN FOUND!");
        for( int i = 0; i < mWorld.plan.Count; ++i){
          Debug.LogFormat( "{0} Accumulated cost: {1}", mWorld.plan[i].mAction.mName, mWorld.plan[i].gCost );
        }

        return mWorld.plan;
	}

  /***************************************************************************/

	void RetracePlan(ExampleNodePlanning startNode, ExampleNodePlanning endNode)
    {
		List<ExampleNodePlanning> plan = new List<ExampleNodePlanning>();

        ExampleNodePlanning currentNode = endNode;

		while (currentNode != startNode) {
			plan.Add(currentNode);
			currentNode = currentNode.mParent;
		}
		plan.Reverse();

        mWorld.plan = plan;
	}

  /***************************************************************************/

	float GetDistance(ExampleNodePlanning nodeA, ExampleNodePlanning nodeB)
    {
        // Distance function
        return nodeB.mAction.mCost;
    }

  /***************************************************************************/

	float Heuristic(ExampleNodePlanning nodeA, ExampleNodePlanning nodeB)
    {
        // Heuristic function
        return -ExampleWorld.PopulationCount( (int)(nodeA.mWorldState | nodeB.mWorldState) ) - ExampleWorld.PopulationCount( (int)(nodeA.mWorldState & nodeB.mWorldState) );
	}

  /***************************************************************************/

}
