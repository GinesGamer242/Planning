using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NewWorld : MonoBehaviour
{
    public List<NewNodePlanning> openSet;
    public HashSet<NewNodePlanning> closedSet;

    public List<NewNodePlanning> plan = new List<NewNodePlanning>();

    public WorldState mWorldState;

    public List<NewAction> mActionList;

    public int mEnergy = 90;
    public int mCortisol = 10;
    public int mKnowledge = 0;

    /***************************************************************************/

    public enum WorldState
    {
        WORLD_STATE_NONE = 0,
        WORLD_STATE_HAS_ENERGY = 1,
        WORLD_STATE_CAN_SLEEP = 2,
        WORLD_STATE_HAS_STUDIED = 4,
        WORLD_STATE_HAS_SLEPT = 8,
        WORLD_STATE_HAS_DRANK = 16,
        WORLD_STATE_HAS_PLAYED = 32,
        WORLD_STATE_HAS_DONE_EXERCISE = 64,
        WORLD_STATE_CAN_GO_TO_EXAM = 128,
        WORLD_STATE_EXAM_DONE = 256
    }

    void Awake()
    {
        mActionList = new List<NewAction>();

        mWorldState = WorldState.WORLD_STATE_HAS_ENERGY;

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_STUDY,
            WorldState.WORLD_STATE_HAS_ENERGY,
            WorldState.WORLD_STATE_HAS_STUDIED,
            5.0f, "Study")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_GO_TO_BAR,
            WorldState.WORLD_STATE_HAS_ENERGY,
            WorldState.WORLD_STATE_HAS_DRANK,
            2.5f, "Go to bar")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES,
            WorldState.WORLD_STATE_HAS_ENERGY,
            WorldState.WORLD_STATE_HAS_PLAYED,
            2.0f, "Play videogames")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_DO_EXERCISE,
            WorldState.WORLD_STATE_HAS_ENERGY,
            WorldState.WORLD_STATE_HAS_DONE_EXERCISE,
            1.5f, "Do exercise")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_SLEEP,
            WorldState.WORLD_STATE_CAN_SLEEP,
            WorldState.WORLD_STATE_HAS_SLEPT,
            5.0f, "Sleep")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM,
            WorldState.WORLD_STATE_CAN_GO_TO_EXAM,
            WorldState.WORLD_STATE_EXAM_DONE,
            1.0f, "Go to exam")
        );
    }

    public WorldState UpdateStates(int energy, int cortisol, int knowledge)
    {
        WorldState state = WorldState.WORLD_STATE_NONE;

        if (energy >= 50) {
            state |= WorldState.WORLD_STATE_HAS_ENERGY;
        }
        else if (energy >= 30) {
            state |= WorldState.WORLD_STATE_CAN_SLEEP | WorldState.WORLD_STATE_HAS_ENERGY;
        }
        else {
            state |= WorldState.WORLD_STATE_CAN_SLEEP;
        }
        
        return state;
    }

    public List<NewNodePlanning> GetNeighbours(NewNodePlanning node)
    {
        List<NewNodePlanning> neighbours = new List<NewNodePlanning>();

        foreach (NewAction action in mActionList)
        {
            // If preconditions are met we can apply effects and the new state is valid
            if ((node.mWorldState & action.mPreconditions) == action.mPreconditions)
            {
                int newEnergy = node.mEnergy;
                int newCortisol = node.mCortisol;
                int newKnowledge = node.mKnowledge;

                switch (action.mActionType)
                {
                    case NewAction.ActionType.ACTION_TYPE_STUDY:
                        newEnergy -= 15;
                        newCortisol += 20;
                        newKnowledge += 35;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_SLEEP:
                        newEnergy += 60;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES:
                        newEnergy -= 15;
                        newCortisol -= 25;
                        newKnowledge -= 5;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_DO_EXERCISE:
                        newEnergy -= 20;
                        newCortisol -= 30;
                        newKnowledge -= 5;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_GO_TO_BAR:
                        newEnergy -= 10;
                        newCortisol -= 35;
                        newKnowledge -= 15;
                        break;
                }

                // For avoiding negative or overflow values
                newEnergy = Mathf.Clamp(newEnergy, 0, 100);
                newCortisol = Mathf.Clamp(newCortisol, 0, 100);
                newKnowledge = Mathf.Clamp(newKnowledge, 0, 100);

                WorldState derivedState = UpdateStates(newEnergy, newCortisol, newKnowledge);

                WorldState finalState = derivedState | action.mEffects;

                // If we are in iteration 14, the exam is the next one
                if (node.mActionCount + 1 == 14)
                {
                    finalState |= WorldState.WORLD_STATE_CAN_GO_TO_EXAM;
                }

                // New node with the new values
                NewNodePlanning newNode = new NewNodePlanning(finalState, action, node.mActionCount + 1);
                newNode.mEnergy = newEnergy;
                newNode.mCortisol = newCortisol;
                newNode.mKnowledge = newKnowledge;

                neighbours.Add(newNode);
            }
        }
        return neighbours;
    }

    public static int PopulationCount(int n)
    {
        return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    }
}
