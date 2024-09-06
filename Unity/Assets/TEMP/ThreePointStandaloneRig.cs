using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Ubiq.XRI;
using Ubiq;

public class ThreePointStandaloneRig : MonoBehaviour
{
    public Transform left;
    public Transform right;
    public Transform head;
    
    [SerializeField]private Transform leftFallback;
    [SerializeField]private Transform rightFallback;

    private HeadAndHandsAvatar headAndHandsAvatar;

    private void Start()
    {
        headAndHandsAvatar = GetComponent<HeadAndHandsAvatar>();
        headAndHandsAvatar.OnHeadUpdate.AddListener(HeadAndHandsAvatar_OnHead);
        headAndHandsAvatar.OnLeftHandUpdate.AddListener(HeadAndHandsAvatar_OnLeftHand);
        headAndHandsAvatar.OnRightHandUpdate.AddListener(HeadAndHandsAvatar_OnRightHand);
    }

    private void OnDestroy()
    {
        if (!headAndHandsAvatar)
        {
            return;
        }
        
        headAndHandsAvatar.OnHeadUpdate.RemoveListener(HeadAndHandsAvatar_OnHead);
        headAndHandsAvatar.OnLeftHandUpdate.RemoveListener(HeadAndHandsAvatar_OnLeftHand);
        headAndHandsAvatar.OnRightHandUpdate.RemoveListener(HeadAndHandsAvatar_OnRightHand);
        headAndHandsAvatar = null;
    }

    private void HeadAndHandsAvatar_OnHead(InputVar<Pose> headPose)
    {
        if (!headPose.valid)
        {
            return;
        }
        
        head.SetPositionAndRotation(headPose.value.position,headPose.value.rotation);
    }
    
    private void HeadAndHandsAvatar_OnLeftHand(InputVar<Pose> leftHandPose)
    {
        left.SetPositionAndRotation(
            leftHandPose.valid ? leftHandPose.value.position : leftFallback.position,
            leftHandPose.valid ? leftHandPose.value.rotation : leftFallback.rotation);
    }
    
    private void HeadAndHandsAvatar_OnRightHand(InputVar<Pose> rightHandPose)
    {
        right.SetPositionAndRotation(
            rightHandPose.valid ? rightHandPose.value.position : rightFallback.position, 
            rightHandPose.valid ? rightHandPose.value.rotation : rightFallback.rotation);
    }
}
