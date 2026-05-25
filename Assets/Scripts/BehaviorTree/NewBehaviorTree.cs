using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using System.Runtime.CompilerServices;

public class NewBehaviorTree : MonoBehaviour
{
    private Root mBehaviorTree;
    public NewWorld mWorld;
    public NewPlanning mPlanning;
    private Blackboard mBlackboard;

    List<NewNodePlanning> mPlan;
    int mCurrentActionIndex = -1;

    private int mCurrentActionWaypointIndex = -1;

    int mTotalStepsCounter = 0;

    float mTimeStartAction = 0.0f;
    float mTimeActionLast = 1.0f;

    public Unit mUnit;
    private bool mUnitArrived = false;

    // [0] Study, [1] Go to Bar, [2] Play Videogames, [3] Do Exercise, [4] Sleep, [5] Go to Exam
    public Transform[] mActionWaypoints = new Transform[6];

    /****************************************************************************/

    void Start()
    {
        mWorld = GetComponent<NewWorld>();
        mPlanning = GetComponent<NewPlanning>();

        mBlackboard = new Blackboard(UnityContext.GetClock());

        mBehaviorTree = new Root(mBlackboard,
            new Selector(
                new BlackboardCondition("examDone", Operator.IS_EQUAL, true,
                    Stops.IMMEDIATE_RESTART, new WaitUntilStopped()),

                new Sequence(
                    new Action((bool planning) =>
                    {
                        Debug.Log("Planning...");

                        mPlan = mPlanning.FindPlan(mWorld.mWorldState, NewWorld.WorldState.WORLD_STATE_EXAM_DONE);

                        if (mPlan == null || mPlan.Count == 0)
                        {
                            Debug.LogError("No plan found");
                            return Action.Result.FAILED;
                        }

                        mCurrentActionIndex = 0;
                        Debug.Log($"Planned in {mPlan.Count} steps");

                        mBlackboard.Set("planFailed", false);
                        mBlackboard.Set("planFinished", false);
                        mBlackboard.Set("examDone", false);
                        mBlackboard.Set("unitMoving", false);

                        return Action.Result.SUCCESS;
                    })
                    { Label = "Planning" },

                    new Selector(
                        new BlackboardCondition("planFinished", Operator.IS_EQUAL, true,
                            Stops.IMMEDIATE_RESTART, new WaitUntilStopped()),

                        new BlackboardCondition("planFailed", Operator.IS_EQUAL, true,
                            Stops.IMMEDIATE_RESTART, new WaitUntilStopped()),

                        new Repeater(-1,
                            new Sequence(
                                new Action((bool pickNextAction) =>
                                {
                                    if (mCurrentActionIndex >= mPlan.Count)
                                    {
                                        mBlackboard.Set("planFinished", true);
                                        return Action.Result.FAILED;
                                    }

                                    if (mCurrentActionIndex >= 14)
                                    {
                                        mWorld.mWorldState |= NewWorld.WorldState.WORLD_STATE_CAN_GO_TO_EXAM;
                                    }

                                    // Check preconditions
                                    NewNodePlanning planned = mPlan[mCurrentActionIndex];
                                    if ((mWorld.mWorldState & planned.mAction.mPreconditions) != planned.mAction.mPreconditions)
                                    {
                                        Debug.LogWarning($"{planned.mAction.mName}: preconditions failed. Aborting plan.");
                                        mBlackboard.Set("planFailed", true);
                                        return Action.Result.FAILED;
                                    }

                                    mCurrentActionWaypointIndex = (int)mPlan[mCurrentActionIndex].mAction.mActionType;
                                    mTimeStartAction = Time.time;
                                    return Action.Result.SUCCESS;
                                })
                                { Label = "PickNextAction" },

                                new Selector(
                                    new Action((bool goToExam) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_GO_TO_EXAM)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mWorldState |= mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. GO TO EXAM " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            Debug.Log(mWorld.mKnowledge >= 50 ? "EXAM PASSED" : "EXAM FAILED");

                                            mBlackboard.Set("examDone", true);
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "GoToExam" },

                                    new Action((bool sleep) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_SLEEP)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy + 60, 0, 100);
                                            mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge)
                                                               | mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. SLEEP " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            mCurrentActionIndex++;
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "Sleep" },

                                    new Action((bool study) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_STUDY)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mKnowledge = Mathf.Clamp(mWorld.mKnowledge + 35, 0, 100);
                                            mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy - 15, 0, 100);
                                            mWorld.mCortisol = Mathf.Clamp(mWorld.mCortisol + 20, 0, 100);
                                            mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge)
                                                               | mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. STUDY " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            mCurrentActionIndex++;
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "Study" },

                                    new Action((bool doExercise) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_DO_EXERCISE)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mCortisol = Mathf.Clamp(mWorld.mCortisol - 30, 0, 100);
                                            mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy - 20, 0, 100);
                                            mWorld.mKnowledge = Mathf.Clamp(mWorld.mKnowledge - 5, 0, 100);
                                            mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge)
                                                               | mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. DO EXERCISE " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            mCurrentActionIndex++;
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "DoExercise" },

                                    new Action((bool goToBar) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_GO_TO_BAR)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mCortisol = Mathf.Clamp(mWorld.mCortisol - 35, 0, 100);
                                            mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy - 10, 0, 100);
                                            mWorld.mKnowledge = Mathf.Clamp(mWorld.mKnowledge - 15, 0, 100);
                                            mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge)
                                                               | mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. GO TO BAR " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            mCurrentActionIndex++;
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "GoToBar" },

                                    new Action((bool playVideogames) =>
                                    {
                                        if (mPlan[mCurrentActionIndex].mAction.mActionType != NewAction.ActionType.ACTION_TYPE_PLAY_VIDEOGAMES)
                                        {
                                            return Action.Result.FAILED;
                                        }

                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            mTotalStepsCounter++;
                                            mWorld.mCortisol = Mathf.Clamp(mWorld.mCortisol - 25, 0, 100);
                                            mWorld.mEnergy = Mathf.Clamp(mWorld.mEnergy - 15, 0, 100);
                                            mWorld.mKnowledge = Mathf.Clamp(mWorld.mKnowledge - 5, 0, 100);
                                            mWorld.mWorldState = mWorld.UpdateStates(mWorld.mEnergy, mWorld.mCortisol, mWorld.mKnowledge)
                                                               | mPlan[mCurrentActionIndex].mAction.mEffects;

                                            Debug.Log($"{mTotalStepsCounter}. PLAY VIDEOGAMES " +
                                                      $"(Energy: {mWorld.mEnergy}, Cortisol: {mWorld.mCortisol}, Knowledge: {mWorld.mKnowledge})");
                                            mCurrentActionIndex++;
                                            return Action.Result.SUCCESS;
                                        }
                                        return Action.Result.PROGRESS;
                                    })
                                    { Label = "PlayVideogames" }

                                ), // Selector

                                new Action((bool moveToWaypoint) =>
                                {
                                    if (moveToWaypoint)
                                    {
                                        mBlackboard.Set("unitMoving", false);
                                        mUnitArrived = false;
                                        return Action.Result.SUCCESS;
                                    }
                                    
                                    if (mUnit == null)
                                    {
                                        return Action.Result.SUCCESS;
                                    }

                                    int actionIndex = mCurrentActionWaypointIndex;

                                    if (!mBlackboard.Get<bool>("unitMoving"))
                                    {
                                        mUnitArrived = false;
                                        mBlackboard.Set("unitMoving", true);

                                        Transform waypoint = (actionIndex >= 0 && actionIndex < mActionWaypoints.Length)
                                        ? mActionWaypoints[actionIndex] : null;

                                        if (waypoint != null)
                                        {
                                            mUnit.MoveTo(waypoint.position, () =>
                                            {
                                                mUnitArrived = true;
                                            });
                                        }
                                        else
                                        {
                                            mUnitArrived = true;
                                        }
                                    }

                                    if (!mUnitArrived)
                                    {
                                        return Action.Result.PROGRESS;
                                    }

                                    mBlackboard.Set("unitMoving", false);
                                    return Action.Result.SUCCESS;
                                })
                                { Label = "MoveToWaypoint" }
                                    
                            ) // Sequence
                        ) // Repeater
                    ) // Selector
                ) // Sequence
            ) // Selector
        );

#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = mBehaviorTree;
#endif

        mBehaviorTree.Start();
    }

    /****************************************************************************/

    public void OnDestroy()
    {
        StopBehaviorTree();
    }

    public void StopBehaviorTree()
    {
        if (mBehaviorTree != null && mBehaviorTree.CurrentState == Node.State.ACTIVE)
            mBehaviorTree.Stop();
    }
}