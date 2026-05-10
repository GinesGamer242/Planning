using UnityEngine;
using System.Collections.Generic;
using NPBehave;

public class BTPlanExecutor : MonoBehaviour
{
    private Root mBehaviorTree;
    private Blackboard mBlackboard;

    public NewPlanning mPlanner;
    public NewWorld mWorld;

    private List<NewNodePlanning> mPlan;
    private int mCurrentActionIndex = -1;

    private float mTimeStartAction = 0.0f;
    private float mTimeActionDuration = 0.5f;

    void Start()
    {
        if (mPlanner == null) mPlanner = GetComponent<NewPlanning>();
        if (mWorld == null) mWorld = GetComponent<NewWorld>();

        mPlan = new List<NewNodePlanning>();

        mBlackboard = new Blackboard(UnityContext.GetClock());

        mBehaviorTree = new Root(mBlackboard, UnityContext.GetClock(),
            new Selector(
                new BlackboardCondition("isExamDone", Operator.IS_EQUAL, true, Stops.BOTH,
                    new Sequence(
                        new Action(() =>
                        {
                            if (mWorld.mKnowledge >= 50) Debug.Log("EXAM PASSED");
                            else Debug.Log("EXAM FAILED");
                            return true;
                        }),
                        new WaitUntilStopped()
                    )
                ),
                new Sequence(
                    new Action((bool planning) =>
                    {
                        Debug.Log("Planning...");
                        mPlan = mPlanner.FindPlan(mWorld.mWorldState, NewWorld.WorldState.WORLD_STATE_EXAM_DONE);

                        if (mPlan != null && mPlan.Count > 0)
                        {
                            mCurrentActionIndex = -1;
                            Debug.Log("Plan created for the behavior tree.");
                            return Action.Result.SUCCESS;
                        }
                        return Action.Result.FAILED;
                    }),

                    new Repeater(-1,
                        new Sequence(
                            new Action((bool nextAction) =>
                            {
                                mCurrentActionIndex++;
                                mTimeStartAction = Time.time;

                                if (mCurrentActionIndex >= mPlan.Count)
                                    return Action.Result.FAILED;

                                return Action.Result.SUCCESS;
                            }),

                            new Selector(
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM, "Go to exam"),
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_STUDY, "Study"),
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_SLEEP, "Sleep"),
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES, "Play videogames"),
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_DO_EXERCISE, "Do exercise"),
                                CreateBTAction(NewAction.ActionType.ACTION_TYPE_GO_TO_BAR, "Go to bar")
                            )
                        )
                    )
                )
            )
        );

        mBehaviorTree.Start();
    }

    private Action CreateBTAction(NewAction.ActionType type, string label)
    {
        return new Action((bool tick) =>
        {
            // Check if the action is the one expected in the plan
            if (mPlan[mCurrentActionIndex].mAction.mActionType != type)
            {
                return Action.Result.FAILED;
            }

            // Check effect values (there is enough energy to perform the action except for sleeping)
            if (type != NewAction.ActionType.ACTION_TYPE_SLEEP && mWorld.mEnergy <= 0)
            {
                Debug.LogWarning($"{type} failed because the agent doesn't have energy.");
                return Action.Result.FAILED;
            }

            // Check action preconditions
            NewAction actionData = mPlan[mCurrentActionIndex].mAction;
            if ((mWorld.mWorldState & actionData.mPreconditions) != actionData.mPreconditions)
            {
                Debug.LogError($"BT: Precondition failed for {type}. Aborting plan.");
                return Action.Result.FAILED;
            }

            // Simulate action duration
            if (Time.time < mTimeStartAction + mTimeActionDuration)
            {
                return Action.Result.PROGRESS;
            }

            // Apply effects
            ExecuteActionEffects(type, actionData);
            Debug.Log($"{mCurrentActionIndex + 1}. {label} (Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");

            return Action.Result.SUCCESS;

        });
    }

    private void ExecuteActionEffects(NewAction.ActionType type, NewAction actionData)
    {
        switch (type)
        {
            case NewAction.ActionType.ACTION_TYPE_STUDY:
                mWorld.mEnergy -= 15;
                mWorld.mCortisol += 20;
                mWorld.mKnowledge += 35;
                break;

            case NewAction.ActionType.ACTION_TYPE_SLEEP:
                mWorld.mEnergy += 60;
                break;

            case NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES:
                mWorld.mEnergy -= 15;
                mWorld.mCortisol -= 25;
                mWorld.mKnowledge -= 5;
                break;

            case NewAction.ActionType.ACTION_TYPE_DO_EXERCISE:
                mWorld.mEnergy -= 20;
                mWorld.mCortisol -= 30;
                mWorld.mKnowledge -= 5;
                break;

            case NewAction.ActionType.ACTION_TYPE_GO_TO_BAR:
                mWorld.mEnergy -= 10;
                mWorld.mCortisol -= 35;
                mWorld.mKnowledge -= 15;
                break;
        }

        mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy, 0, 100);
        mWorld.mCortisol = Mathf.Clamp(mWorld.mCortisol, 0, 100);
        mWorld.mKnowledge = Mathf.Clamp(mWorld.mKnowledge, 0, 100);

        // Update world state bits based on the new stats
        mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge);

        // Apply action effects to world state
        mWorld.mWorldState |= actionData.mEffects;

        // The exam is always action number 15
        if (mCurrentActionIndex + 1 == 14)
            mWorld.mWorldState |= NewWorld.WorldState.WORLD_STATE_CAN_GO_TO_EXAM;

        if (type == NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM)
        {
            mWorld.mWorldState |= NewWorld.WorldState.WORLD_STATE_EXAM_DONE;
            mBlackboard["isExamDone"] = true;
        }
    }

    private void OnDestroy()
    {
        if (mBehaviorTree != null && mBehaviorTree.CurrentState == Node.State.ACTIVE)
            mBehaviorTree.Stop();
    }
}