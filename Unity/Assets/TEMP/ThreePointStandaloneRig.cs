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

    private Ubiq.Avatars.AvatarManager avatarManager;

    void Start()
    {
        avatarManager = FindAnyObjectByType<AvatarManager>();
    }

    void Update()
    {
        if(avatarManager.input.TryGet(out IHeadAndHandsInput src))
        {
            left.position = src.leftHand.value.position;
            left.rotation = src.leftHand.value.rotation;
            right.position = src.rightHand.value.position;
            right.rotation = src.rightHand.value.rotation;
            head.position = src.head.value.position;
            head.rotation = src.head.value.rotation;
        }
    }
}
