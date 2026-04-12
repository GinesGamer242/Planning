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
    public int mStress = 10;
    public int mKnowledge = 0;

    /***************************************************************************/

    public enum WorldState
    {
        WORLD_STATE_NONE = 0,
        WORLD_STATE_HIGH_ENERGY = 1,
        WORLD_STATE_LOW_ENERGY = 2,
        WORLD_STATE_HIGH_KNOWLEDGE = 4,
        WORLD_STATE_LOW_KNOWLEDGE = 8,
        WORLD_STATE_STRESSED = 16,
        WORLD_STATE_RELAXED = 32,
        WORLD_STATE_HAS_STUDIED = 64,
        WORLD_STATE_HAS_CHEATSHEET = 128,
        WORLD_STATE_HAS_SLEPT = 256,
        WORLD_STATE_HAS_DRANK = 512,
        WORLD_STATE_HAS_PLAYED = 1024,
        WORLD_STATE_HAS_DONE_EXERCISE = 2048,
        WORLD_STATE_CAN_GO_TO_EXAM = 4096, //Llevar una cuenta de cuántas acciones se han ejecutado y cuando se llegue a 20 acciones en total,
                                          //por ejemplo, se activa mediante una función)
        WORLD_STATE_EXAM_DONE = 8192 //En una función comprobar si KNOWLEDGE => 5 para APROBAR
    }

    void Awake()
    {
        mActionList = new List<NewAction>();

        mWorldState = WorldState.WORLD_STATE_HIGH_ENERGY | WorldState.WORLD_STATE_RELAXED | WorldState.WORLD_STATE_LOW_KNOWLEDGE;

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_STUDY,
            WorldState.WORLD_STATE_HIGH_ENERGY,
            WorldState.WORLD_STATE_HAS_STUDIED,
            5.0f, "Study")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_GO_TO_BAR,
            WorldState.WORLD_STATE_HIGH_ENERGY,
            WorldState.WORLD_STATE_HAS_DRANK,
            1.0f, "Go to bar")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES,
            WorldState.WORLD_STATE_HIGH_ENERGY,
            WorldState.WORLD_STATE_HAS_PLAYED,
            1.0f, "Play videogames")
        );

        mActionList.Add(
            new NewAction(
                NewAction.ActionType.ACTION_TYPE_DO_EXERCISE,
                WorldState.WORLD_STATE_HIGH_ENERGY,
                WorldState.WORLD_STATE_HAS_DONE_EXERCISE,
                1.0f, "Do exercise")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_SLEEP,
            WorldState.WORLD_STATE_LOW_ENERGY,
            WorldState.WORLD_STATE_HAS_SLEPT,
            1.0f, "Sleep")
        );

        //mActionList.Add(
        //    new NewAction(
        //    NewAction.ActionType.ACTION_TYPE_MAKE_CHEATSHEET,
        //    WorldState.WORLD_STATE_LOW_KNOWLEDGE | WorldState.WORLD_STATE_RELAXED,
        //    WorldState.WORLD_STATE_HAS_CHEATSHEET,
        //    10.0f, "Make cheatsheet")
        //);

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM,
            WorldState.WORLD_STATE_CAN_GO_TO_EXAM,
            WorldState.WORLD_STATE_EXAM_DONE,
            1.0f, "Go to exam")
        );
    }

    public WorldState UpdateStates(int energy, int stress, int knowledge)
    {
        WorldState state = WorldState.WORLD_STATE_NONE;

        if (energy >= 50) {
            state |= WorldState.WORLD_STATE_HIGH_ENERGY;
        }
        else {
            state |= WorldState.WORLD_STATE_LOW_ENERGY;
        }

        if (knowledge >= 50) {
            state |= WorldState.WORLD_STATE_HIGH_KNOWLEDGE;
        }
        else {
            state |= WorldState.WORLD_STATE_LOW_KNOWLEDGE;
        }

        if (stress >= 50) {
            state |= WorldState.WORLD_STATE_STRESSED;
        }
        else {
            state |= WorldState.WORLD_STATE_RELAXED;
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
                // 1. Simular el cambio de variables numéricas basándonos en el nodo actual
                int newEnergy = node.mEnergy;
                int newStress = node.mStress;
                int newKnowledge = node.mKnowledge;

                switch (action.mActionType)
                {
                    case NewAction.ActionType.ACTION_TYPE_STUDY:
                        newEnergy -= 20;
                        newKnowledge += 30;
                        newStress += 15;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_SLEEP:
                        newEnergy += 50;
                        newStress -= 20;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES:
                        newStress -= 30;
                        newEnergy -= 5;
                        newKnowledge -= 10;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_DO_EXERCISE:
                        newEnergy -= 40;
                        newStress -= 50;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_GO_TO_BAR:
                        newEnergy -= 10;
                        newStress -= 40;
                        newKnowledge -= 20;
                        break;
                    case NewAction.ActionType.ACTION_TYPE_MAKE_CHEATSHEET:
                        newKnowledge += 15;
                        newStress += 10;
                        break;
                }

                // For avoiding negative or overflow values
                newEnergy = Mathf.Clamp(newEnergy, 0, 100);
                newStress = Mathf.Clamp(newStress, 0, 100);
                newKnowledge = Mathf.Clamp(newKnowledge, 0, 100);

                WorldState derivedState = UpdateStates(newEnergy, newStress, newKnowledge);

                WorldState finalState = derivedState | action.mEffects;

                // If we are in iteration 19, the exam is the next one
                if (node.mActionCount + 1 >= 19)
                {
                    finalState |= WorldState.WORLD_STATE_CAN_GO_TO_EXAM;
                }

                // New node with the new values
                NewNodePlanning newNode = new NewNodePlanning(finalState, action, node.mActionCount + 1);
                newNode.mEnergy = newEnergy;
                newNode.mStress = newStress;
                newNode.mKnowledge = newKnowledge;

                neighbours.Add(newNode);
            }
        }
        return neighbours;
    }

    //public static int PopulationCount(int n)
    //{
    //    return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    //}
}
