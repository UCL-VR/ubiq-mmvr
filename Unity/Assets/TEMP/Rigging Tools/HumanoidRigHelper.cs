using MotionMatching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMSkeletonHelper : MonoBehaviour
{
    public MotionMatchingData motionMatchingData;

    private void OnDrawGizmos()
    {
        DrawGizmos(transform.GetChild(0));
    }

    private void DrawGizmos(Transform node)
    {
        foreach (Transform child in node)
        {
            Gizmos.DrawLine(child.position, child.parent.position);
            DrawGizmos(child);
        }
    }
}

