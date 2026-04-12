using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NewWorld : MonoBehaviour
{
    public List<ExampleNodePlanning> openSet;
    public HashSet<ExampleNodePlanning> closedSet;

    public List<ExampleNodePlanning> plan;

    public WorldState mWorldState;

    public List<NewAction> mActionList;

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
        WORLD_STATE_CAN_GO_TO_EXAM = 2048, //Llevar una cuenta de cuántas acciones se han ejecutado y cuando se llegue a 20 acciones en total,
                                          //por ejemplo, se activa mediante una función)
        WORLD_STATE_EXAM_DONE = 4096 //En una función comprobar si KNOWLEDGE => 5 para APROBAR
    }

    void Awake()
    {
        mActionList = new List<NewAction>();

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_STUDY,
            WorldState.WORLD_STATE_HIGH_ENERGY,
            WorldState.WORLD_STATE_HAS_STUDIED,
            1.0f, "Study")
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
            NewAction.ActionType.ACTION_TYPE_SLEEP,
            WorldState.WORLD_STATE_LOW_ENERGY,
            WorldState.WORLD_STATE_HAS_SLEPT,
            1.0f, "Sleep")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_MAKE_CHEATSHEET,
            WorldState.WORLD_STATE_LOW_KNOWLEDGE | WorldState.WORLD_STATE_RELAXED,
            WorldState.WORLD_STATE_HAS_CHEATSHEET,
            1.0f, "Make cheatsheet")
        );

        mActionList.Add(
            new NewAction(
            NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM,
            WorldState.WORLD_STATE_CAN_GO_TO_EXAM,
            WorldState.WORLD_STATE_EXAM_DONE,
            0.0f, "Go to exam")
        );
    }

    public List<NewNodePlanning> GetNeighbours(NewNodePlanning node)
    {
        List<NewNodePlanning> neighbours = new List<NewNodePlanning>();

        // Para calcular estados derivados a partir del estado actual (añadir CAN_STAB o CAN_SHOOT)
        WorldState derivedStates = WorldState.WORLD_STATE_NONE;

        // Añadir los estados derivados al estado real
        WorldState effectiveState = node.mWorldState | derivedStates;

        foreach (NewAction action in mActionList)
        {
            // Usar effectiveState para evaluar precondiciones
            if ((effectiveState & action.mPreconditions) == action.mPreconditions)
            {
                WorldState newWorldState = node.mWorldState & ~action.mRemoveEffects;
                NewNodePlanning newNodePlanning = new NewNodePlanning(newWorldState | action.mEffects, action);
                neighbours.Add(newNodePlanning);
            }
        }

        return neighbours;
    }
}
