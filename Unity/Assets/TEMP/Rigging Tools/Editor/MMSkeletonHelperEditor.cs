using MotionMatching;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MMSkeletonHelper))]
public class MMSkeletonHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MMSkeletonHelper component = (MMSkeletonHelper)target;

        if (GUILayout.Button("Create Hierarchy from T-Pose"))
        {
            CreateHierarchyFromTPose(component);
        }
    }

    private void CreateHierarchyFromTPose(MMSkeletonHelper component)
    {
        var data = component.motionMatchingData;
        var animationdata = data.AnimationDataTPose;
        var animation = animationdata.GetAnimation();
        var skeleton = animation.Skeleton;

        var jointToGameObject = new Dictionary<int, GameObject>();

        foreach (var joint in skeleton.Joints)
        {
            var go = new GameObject(joint.Name);
            jointToGameObject.Add(joint.Index, go);

            if (joint.Index == joint.ParentIndex)
            {
                go.transform.SetParent(component.transform);
            }
            else
            {
                var parentGO = jointToGameObject[joint.ParentIndex];
                go.transform.SetParent(parentGO.transform);
            }

            go.transform.localPosition = joint.LocalOffset;
        }
    }
}

