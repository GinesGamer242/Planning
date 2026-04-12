using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExampleWorld : MonoBehaviour
{
    public List<ExampleNodePlanning> openSet;
    public HashSet<ExampleNodePlanning> closedSet;

    public List<ExampleNodePlanning> plan;

    public WorldState mWorldState;

    public List<ExampleAction> mActionList;

    /***************************************************************************/

    public enum WorldState
    {
        WORLD_STATE_NONE = 0,
        WORLD_STATE_ENEMY_DEAD = 1,
        WORLD_STATE_GUN_OWNED = 2,
        WORLD_STATE_GUN_LOADED = 4,
        WORLD_STATE_KNIFE_OWNED = 8,
        WORLD_STATE_CLOSE_TO_ENEMY = 16,
        WORLD_STATE_CLOSE_TO_GUN = 32,
        WORLD_STATE_CLOSE_TO_KNIFE = 64,
        WORLD_STATE_LINE_OF_SIGHT_TO_ENEMY = 128
    }

    /***************************************************************************/

    void Awake()
    {
        mActionList = new List<ExampleAction>();
        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_STAB,
            WorldState.WORLD_STATE_CLOSE_TO_ENEMY | WorldState.WORLD_STATE_KNIFE_OWNED,
            WorldState.WORLD_STATE_ENEMY_DEAD,
            WorldState.WORLD_STATE_NONE,
            5.0f, "Stab")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_SHOOT,
            WorldState.WORLD_STATE_LINE_OF_SIGHT_TO_ENEMY | WorldState.WORLD_STATE_GUN_LOADED | WorldState.WORLD_STATE_GUN_OWNED,
            WorldState.WORLD_STATE_ENEMY_DEAD,
            WorldState.WORLD_STATE_NONE,
            100.0f, "Shoot")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_LOAD_GUN,
            WorldState.WORLD_STATE_GUN_OWNED,
            WorldState.WORLD_STATE_GUN_LOADED,
            WorldState.WORLD_STATE_NONE,
            1.0f, "Load gun")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_PICK_UP_GUN,
            WorldState.WORLD_STATE_CLOSE_TO_GUN,
            WorldState.WORLD_STATE_GUN_OWNED,
            WorldState.WORLD_STATE_NONE,
            1.0f, "Pick up gun")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_PICK_UP_KNIFE,
            WorldState.WORLD_STATE_CLOSE_TO_KNIFE,
            WorldState.WORLD_STATE_KNIFE_OWNED,
            WorldState.WORLD_STATE_NONE,
            1.0f, "Pick up knife")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_GO_TO_ENEMY,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_CLOSE_TO_ENEMY,
            WorldState.WORLD_STATE_CLOSE_TO_GUN | WorldState.WORLD_STATE_CLOSE_TO_KNIFE,
            1.0f, "Go to enemy")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_GO_TO_GUN,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_CLOSE_TO_GUN,
            WorldState.WORLD_STATE_CLOSE_TO_ENEMY | WorldState.WORLD_STATE_CLOSE_TO_KNIFE,
            20.0f, "Go to gun")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_GO_TO_KNIFE,
            WorldState.WORLD_STATE_NONE,
            WorldState.WORLD_STATE_CLOSE_TO_KNIFE,
            WorldState.WORLD_STATE_CLOSE_TO_ENEMY | WorldState.WORLD_STATE_CLOSE_TO_GUN,
            20.0f, "Go to knife")
        );

        mActionList.Add(
            new ExampleAction(
            ExampleAction.ActionType.ACTION_TYPE_GET_LINE_OF_SIGHT_TO_ENEMY,
            WorldState.WORLD_STATE_GUN_LOADED | WorldState.WORLD_STATE_GUN_OWNED,
            WorldState.WORLD_STATE_LINE_OF_SIGHT_TO_ENEMY,
            WorldState.WORLD_STATE_NONE,
            10.0f, "Get line of sight to enemy")
        );


    }

    /***************************************************************************/

    public List<ExampleNodePlanning> GetNeighbours(ExampleNodePlanning node)
    {
        List<ExampleNodePlanning> neighbours = new List<ExampleNodePlanning>();

        foreach (ExampleAction action in mActionList)
        {
            // If preconditions are met we can apply effects and the new state is valid
            if ((node.mWorldState & action.mPreconditions) == action.mPreconditions)
            {

                // Remove effects that are no longer valid
                WorldState newWorldState = node.mWorldState & ~action.mRemoveEffects;

                // Apply action and effects
                //NodePlanning newNodePlanning = new NodePlanning( node.mWorldState | action.mEffects, action );
                ExampleNodePlanning newNodePlanning = new ExampleNodePlanning(newWorldState | action.mEffects, action);
                neighbours.Add(newNodePlanning);
            }
        }

        return neighbours;
    }

    /***************************************************************************/

    public static int PopulationCount(int n)
    {
        return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    }

    /***************************************************************************/

}