using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NewPlanning : MonoBehaviour
{
    NewNodePlanning CurrentStartNode;
    NewNodePlanning CurrentTargetNode;

    NewWorld mWorld;

    /***************************************************************************/

    void Start()
    {
        mWorld = GetComponent<NewWorld>();

        Debug.Log("Planning...");
        FindPlan(mWorld.mWorldState, NewWorld.WorldState.WORLD_STATE_EXAM_DONE);
    }

    /***************************************************************************/

    public List<NewNodePlanning> FindPlan(NewWorld.WorldState startWorldState, NewWorld.WorldState targetWorldState)
    {
        CurrentStartNode = new NewNodePlanning(startWorldState, null, 0);

        CurrentStartNode.mEnergy = mWorld.mEnergy;
        CurrentStartNode.mCortisol = mWorld.mCortisol;
        CurrentStartNode.mKnowledge = mWorld.mKnowledge;

        CurrentTargetNode = new NewNodePlanning(targetWorldState, null, 20);

        List<NewNodePlanning> openSet = new List<NewNodePlanning>();
        HashSet<NewNodePlanning> closedSet = new HashSet<NewNodePlanning>();
        openSet.Add(CurrentStartNode);
        mWorld.openSet = openSet;

        NewNodePlanning node = CurrentStartNode;
        while (openSet.Count > 0)
        {
            // Select best node from open list
            node = openSet[0];

            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].fCost < node.fCost || ( openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost ) ) {
                    node = openSet[i];
                }
            }

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            mWorld.openSet = openSet;
            mWorld.closedSet = closedSet;

            // Check destination
            if ((node.mWorldState & targetWorldState) == targetWorldState)
            {
                Debug.Log("PLAN FOUND!");
                RetracePlan(CurrentStartNode, node);

                Debug.LogFormat("Statistics: Total nodes {0}, Open nodes {1}, Closed nodes {2}", openSet.Count + closedSet.Count, openSet.Count, closedSet.Count);

                for (int i = 0; i < mWorld.plan.Count; ++i)
                {
                    Debug.LogFormat("{0}. {1} (Accumulated Cost: {2}, Energy: {3}, Cortisol: {4}, Knowledge: {5})",
                        i + 1, mWorld.plan[i].mAction.mName, mWorld.plan[i].gCost, mWorld.plan[i].mEnergy, mWorld.plan[i].mCortisol, mWorld.plan[i].mKnowledge);
                }

                if(node.mAction.mActionType == NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM)
                {
                    if(node.mKnowledge >= 50)
                    {
                        Debug.Log("EXAM PASSED");
                    }
                    else
                    {
                        Debug.Log("EXAM FAILED");
                    }
                }

                return mWorld.plan;
            }

            // Open neighbours
            foreach (NewNodePlanning neighbour in mWorld.GetNeighbours(node))
            {
                if ( /*!neighbour.mWalkable ||*/ closedSet.Any(n => n.mWorldState == neighbour.mWorldState && n.mActionCount == neighbour.mActionCount)) {
                    continue;
                }

                float newCostToNeighbour = node.gCost + GetDistance( node, neighbour );

                if (newCostToNeighbour < neighbour.gCost || !openSet.Any(n => n.mWorldState == neighbour.mWorldState ) ) {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = Heuristic( neighbour, CurrentTargetNode );
                    neighbour.mParent = node;

                    if (!openSet.Any(n => n.mWorldState == neighbour.mWorldState))
                    {
                        openSet.Add(neighbour);
                        mWorld.openSet = openSet;
                    }
                    else
                    {
                        // Find neighbour and replace
                        openSet[openSet.FindIndex(x => x.mWorldState == neighbour.mWorldState)] = neighbour;
                    }
                }
            }
        }

        return mWorld.plan;
    }

    /***************************************************************************/

    void RetracePlan(NewNodePlanning startNode, NewNodePlanning endNode)
    {
        List<NewNodePlanning> plan = new List<NewNodePlanning>();

        NewNodePlanning currentNode = endNode;
        while (currentNode != startNode)
        {
            plan.Add(currentNode);
            currentNode = currentNode.mParent;
        }
        plan.Reverse();

        mWorld.plan = plan;
    }

    /***************************************************************************/

    float GetDistance(NewNodePlanning nodeA, NewNodePlanning nodeB)
    {
        // Distance function
        //return nodeB.mAction.mCost;
        return nodeB.mAction.GetDynamicCost(nodeA);
    }

    /***************************************************************************/

    float Heuristic(NewNodePlanning nodeA, NewNodePlanning nodeB)
    {
        // Heuristic function
        //return -NewWorld.PopulationCount((int)(nodeA.mWorldState | nodeB.mWorldState)) - NewWorld.PopulationCount((int)(nodeA.mWorldState & nodeB.mWorldState));

        float stepsToTarget = Mathf.Max(0, 15 - nodeA.mActionCount);

        // Penalize for having high cortisol or low energy and knowledge
        float statePenalty = (nodeA.mCortisol * 1.5f) - (nodeA.mEnergy * 1f) - (nodeA.mKnowledge * 1.5f);

        return stepsToTarget + statePenalty;
    }

    /***************************************************************************/

}