using System;
using Ubiq;
using Ubiq.Avatars;
using UnityEngine;

namespace Ubiq.MotionMatching
{
    [DefaultExecutionOrder(9)]
    public class MMVRAvatarInput : MonoBehaviour
    {
        [SerializeField] private Transform left;
        [SerializeField] private Transform right;
        [SerializeField] private Transform head;
        [SerializeField] private Transform neck;
        [SerializeField] private Transform leftFallback;
        [SerializeField] private Transform rightFallback;
        [SerializeField] private MMVRLowerBody lowerBodySource;

        private Pose leftPose;
        private Pose rightPose;
        private Pose headPose;

        public Transform LeftTracker => left;
        public Transform RightTracker => right;
        public Transform HeadTracker => neck;
        public MMVRLowerBody LowerBody => lowerBodySource;

        private AvatarInput input;

        [Tooltip("Optional source for Head and Hand Tracking. If not null, will be used instead of the AvatarManager Input")]
        public HeadAndHandsAvatar headAndHandsAvatar;

        /// <summary>
        /// Points to the shared local instance that represents the users XR system, if any.
        /// </summary>
        public static MMVRAvatarInput Local { get; private set; }

        private void Awake()
        {
            if (transform.parent == null)
            {
                Local = this;
            }
            else
            {
                //transform.parent = null;
            }
        }

        void Start()
        {
            if (headAndHandsAvatar)
            {
                headAndHandsAvatar.OnHeadUpdate.AddListener(HeadAndHandsAvatar_OnHead);
                headAndHandsAvatar.OnLeftHandUpdate.AddListener(HeadAndHandsAvatar_OnLeftHand);
                headAndHandsAvatar.OnRightHandUpdate.AddListener(HeadAndHandsAvatar_OnRightHand);
            }
            else
            {
                var avatarManager = FindAnyObjectByType<AvatarManager>();
                SetInput(avatarManager.input);
            }
        }

        public void SetInput(AvatarInput input)
        {
            this.input = input;
        }

        void Update()
        {
            if (input != null && input.TryGet(out IHeadAndHandsInput src))
            {
                HeadAndHandsAvatar_OnHead(src.head);
                HeadAndHandsAvatar_OnLeftHand(src.leftHand);
                HeadAndHandsAvatar_OnRightHand(src.rightHand);
            }

            // Update the left and right fallback gameobjects

            transform.position = new Vector3(neck.position.x, 0, neck.position.z);
            var headForward = head.forward;
            headForward.y = 0;
            headForward.Normalize();
            transform.rotation = Quaternion.LookRotation(headForward, Vector3.up);

            head.SetPositionAndRotation(headPose.position, headPose.rotation);
            left.SetPositionAndRotation(leftPose.position, leftPose.rotation);
            right.SetPositionAndRotation(rightPose.position, rightPose.rotation);
        }

        private void HeadAndHandsAvatar_OnHead(InputVar<Pose> headPose)
        {
            if(headPose.valid)
            {
                this.headPose = headPose.value;
            }
        }

        private void HeadAndHandsAvatar_OnLeftHand(InputVar<Pose> leftHandPose)
        {
            if (leftHandPose.valid)
            {
                this.leftPose = leftHandPose.value;
            }
            else
            {
                this.leftPose.position = leftFallback.position;
                this.leftPose.rotation = leftFallback.rotation;
            }
        }

        private void HeadAndHandsAvatar_OnRightHand(InputVar<Pose> rightHandPose)
        {
            if (rightHandPose.valid)
            {
                this.rightPose = rightHandPose.value;
            }
            else
            {
                this.rightPose.position = rightFallback.position;
                this.rightPose.rotation = rightFallback.rotation;
            }
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
    }
}