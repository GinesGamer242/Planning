using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public float mSpeed = 10.0f;
    public GameObject AStar;

    private List<NodePathfinding> mPath;
    private int mTargetIndex;

    private System.Action mOnArrived;

    public void MoveTo(Vector3 destination, System.Action onArrived)
    {
        mOnArrived = onArrived;

        StopCoroutine("FollowPath");

        mPath = AStar.GetComponent<Pathfinding>().FindPath(transform.position, destination);

        // If no path found or already at destination, invoke arrival callback
        if (mPath == null || mPath.Count == 0) {
            mOnArrived?.Invoke();
            return;
        }

        mTargetIndex = 0;
        StartCoroutine("FollowPath");
    }

    IEnumerator FollowPath()
    {
        if (mPath == null || mPath.Count == 0)
        {
            mOnArrived?.Invoke();
            yield break;
        }

        Vector3 currentWaypoint = mPath[0].mWorldPosition;

        while(true) {
            if(transform.position == currentWaypoint) {
                mTargetIndex++;
                if(mTargetIndex >= mPath.Count) {
                    // Reached the end of the path
                    mOnArrived?.Invoke();
                    yield break;
                }
                currentWaypoint = mPath[mTargetIndex].mWorldPosition;
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, mSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if(mPath != null) {
            for(int i = mTargetIndex; i < mPath.Count; i++) {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(mPath[i].mWorldPosition, Vector3.one * 0.5f);

                if(i == mTargetIndex) {
                    Gizmos.DrawLine(transform.position, mPath[i].mWorldPosition);
                }
                else {
                    Gizmos.DrawLine(mPath[i - 1].mWorldPosition, mPath[i].mWorldPosition);
                }
            }
        }
    }
}
