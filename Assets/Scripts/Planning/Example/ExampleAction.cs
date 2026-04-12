using UnityEngine;
using System.Collections;

public class ExampleAction
{
    public ActionType mActionType;
    public ExampleWorld.WorldState mPreconditions;
    public ExampleWorld.WorldState mEffects;
    public ExampleWorld.WorldState mRemoveEffects;
    public float mCost;
    public string mName;

    /***************************************************************************/

    public enum ActionType
    {
        ACTION_TYPE_NONE = -1,
        ACTION_TYPE_STAB,
        ACTION_TYPE_SHOOT,
        ACTION_TYPE_LOAD_GUN,
        ACTION_TYPE_PICK_UP_GUN,
        ACTION_TYPE_PICK_UP_KNIFE,
        ACTION_TYPE_GO_TO_ENEMY,
        ACTION_TYPE_GO_TO_GUN,
        ACTION_TYPE_GO_TO_KNIFE,
        ACTION_TYPE_GET_LINE_OF_SIGHT_TO_ENEMY,
        ACTION_TYPES
    }

    /***************************************************************************/

    public ExampleAction(ActionType actionType, ExampleWorld.WorldState preconditions, ExampleWorld.WorldState effects, ExampleWorld.WorldState removeEffects, float cost, string name)
    {
        mActionType = actionType;
        mPreconditions = preconditions;
        mEffects = effects;
        mRemoveEffects = removeEffects;
        mCost = cost;
        mName = name;
    }

    /***************************************************************************/

}
