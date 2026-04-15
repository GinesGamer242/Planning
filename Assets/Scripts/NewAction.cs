using UnityEngine;
using System.Collections;

public class NewAction
{
    public ActionType mActionType;
    public NewWorld.WorldState mPreconditions;
    public NewWorld.WorldState mEffects;
    public float mCost;
    public string mName;

    /***************************************************************************/

    public enum ActionType
    {
        ACTION_TYPE_NONE = -1,
        ACTION_TYPE_STUDY,
        ACTION_TYPE_GO_TO_BAR,
        ACTION_TYPE_PLAY_VIDEOGAMES,
        ACTION_TYPE_DO_EXERCISE,
        ACTION_TYPE_SLEEP,
        ACTION_TYPE_GO_TO_EXAM
    }

    /***************************************************************************/

    public NewAction(ActionType actionType, NewWorld.WorldState preconditions, NewWorld.WorldState effects, float cost, string name)
    {
        mActionType = actionType;
        mPreconditions = preconditions;
        mEffects = effects;
        mCost = cost;
        mName = name;
    }

    /***************************************************************************/

    // En NewAction.cs
    public float GetDynamicCost(NewNodePlanning currentNode)
    {
        float baseCost = mCost;

        switch (mActionType)
        {
            case ActionType.ACTION_TYPE_STUDY:
                if (currentNode.mCortisol > 70) {
                    baseCost *= 2f;
                }
                if (currentNode.mEnergy < 30) {
                    baseCost *= 1.5f;
                }

                if (currentNode.mKnowledge > 75) {
                    baseCost *= 1.5f;
                }
                break;

            case ActionType.ACTION_TYPE_SLEEP:
                if(currentNode.mCortisol > 70) {
                    baseCost *= 2.0f;
                }

                if (currentNode.mEnergy < 20) {
                    baseCost *= 0.5f;
                }
                break;

            case ActionType.ACTION_TYPE_GO_TO_BAR:
            case ActionType.ACTION_TYPE_PLAY_VIDEOGAMES:
            case ActionType.ACTION_TYPE_DO_EXERCISE:
                if (currentNode.mCortisol < 10) {
                    baseCost *= 0.5f;
                }
                break;
        }

        return Mathf.Max(0.1f, baseCost);
    }
}
