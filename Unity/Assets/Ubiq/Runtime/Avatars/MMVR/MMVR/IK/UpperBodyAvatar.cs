using MotionMatching;
using Unity.Mathematics;
using UnityEngine;

namespace UBIK
{
    public class UpperBodyAvatar : MonoBehaviour
    {
        public bool OnlyHMD;
        [Tooltip("Time in seconds to reach 50% of the target value provided by the IK.")]
        [Range(0.0f, 1.0f)] public float SmoothingHalfLife = 0.1f;

        public Transform HeadTracker;
        public Transform LeftTracker;
        public Transform RightTracker;

        public Joint[] Joints;

        [HideInInspector]
        [SerializeField]
        private quaternion[] DefaultPose;

        private MotionMatchingSkinnedMeshRenderer MMRenderer;
        private AvatarMaskData LowerBodyMask;
        private UBIK UpperBodyIK;
        private quaternion ForwardLocalLHand;
        private quaternion ForwardLocalRHand;
        private quaternion[] PreviousLocalRotations;
        private float3[] JointVelocities;

        private void Awake()
        {
            UpperBodyIK = new UBIK();
            UpperBodyIK.Init(GetSkeleton(), GetDefaultPose());
            if (TryGetComponent(out MotionMatchingSkinnedMeshRenderer renderer))
            {
                MMRenderer = renderer;
                LowerBodyMask = renderer.AvatarMask;
            }
            // Compute Forward vectors of the hands in local space
            if (TryGetComponent<Animator>(out var animator))
            {
                quaternion getForwardUpHand(HumanBodyBones handBone, HumanBodyBones indexDistalBone, HumanBodyBones thumbProximalBone)
                {
                    float3 forward = math.forward();
                    float3 up = math.up();
                    Transform hand = animator.GetBoneTransform(handBone);
                    Transform mid = animator.GetBoneTransform(indexDistalBone);
                    Transform thumb = animator.GetBoneTransform(thumbProximalBone);
                    if (hand != null && mid != null && thumb != null)
                    {
                        forward = math.normalize(mid.position - hand.position);
                        float3 right = math.normalize(math.cross((thumb.position - mid.position).normalized, forward));
                        up = math.normalize(math.cross(forward, right));
                    }
                    forward = hand.InverseTransformDirection(forward);
                    up = hand.InverseTransformDirection(up);
                    return quaternion.LookRotationSafe(forward, up);
                }
                ForwardLocalLHand = getForwardUpHand(HumanBodyBones.LeftHand, HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftThumbProximal);
                ForwardLocalRHand = getForwardUpHand(HumanBodyBones.RightHand, HumanBodyBones.RightIndexDistal, HumanBodyBones.RightThumbProximal);
            }
            PreviousLocalRotations = new quaternion[Joints.Length];
            JointVelocities = new float3[Joints.Length];
        }

        private void OnEnable()
        {
            if (MMRenderer != null)
            {
                MMRenderer.OnSkeletonUpdated += AfterSkeletonUpdated;
            }
        }

        private void OnDisable()
        {
            if (MMRenderer != null)
            {
                MMRenderer.OnSkeletonUpdated -= AfterSkeletonUpdated;
            }
        }

        public void AfterSkeletonUpdated()
        {
            if (MMRenderer != null)
            {
                MMRenderer.AvatarMask = OnlyHMD ? null : LowerBodyMask;
            }

            Transform hips = GetHipsTransform();

            quaternion getWorldHandTarget(int index, Transform tracker, quaternion forwardLocalHand)
            {
                Transform hand = GetJointTransform(index);
                quaternion targetRot = quaternion.LookRotation(tracker.forward, tracker.up);
                quaternion currentRot = math.mul(hand.rotation, forwardLocalHand);
                quaternion delta = math.mul(targetRot, math.inverse(currentRot)); // delta rotation from current to target
                return math.mul(delta, hand.rotation);
            }
            quaternion lhandRot = getWorldHandTarget(17, LeftTracker, ForwardLocalLHand);
            quaternion rhandRot = getWorldHandTarget(21, RightTracker, ForwardLocalRHand);

            for (int i = 0; i < Joints.Length; ++i)
            {
                if (Joints[i].Transform != null)
                {
                    PreviousLocalRotations[i] = Joints[i].Transform.localRotation;
                }
            }


            UpperBodyIK.Solve(new UBIK.Target
            {
                Position = HeadTracker.position,
                Rotation = HeadTracker.rotation
            }, new UBIK.Target
            {
                Position = hips.position,
                Rotation = hips.rotation
            }, new UBIK.Target
            {
                Position = LeftTracker.position,
                Rotation = lhandRot
            }, new UBIK.Target
            {
                Position = RightTracker.position,
                Rotation = rhandRot
            });

            for (int i = 0; i < Joints.Length; ++i)
            {
                if (Joints[i].Transform != null)
                {
                    quaternion currentLocalRotation = PreviousLocalRotations[i];
                    quaternion targetLocalRotation = Joints[i].Transform.localRotation;
                    Spring.SimpleSpringDamperImplicit(ref currentLocalRotation, ref JointVelocities[i], targetLocalRotation, SmoothingHalfLife, Time.deltaTime);
                    Joints[i].Transform.localRotation = currentLocalRotation;
                }
            }
        }

        public quaternion[] GetDefaultPose()
        {
            return DefaultPose;
        }

        public Transform[] GetSkeleton()
        {
            Transform[] skeleton = new Transform[Joints.Length];
            for (int i = 0; i < Joints.Length; i++)
            {
                skeleton[i] = Joints[i].Transform;
            }
            return skeleton;
        }

        private void Reset()
        {
            Joints = new Joint[]
            {
                new(HumanBodyBones.Hips), // 0
                new(HumanBodyBones.LeftUpperLeg), // 1
                new(HumanBodyBones.LeftLowerLeg), // 2
                new(HumanBodyBones.LeftFoot), // 3
                new(HumanBodyBones.LeftToes), // 4
                new(HumanBodyBones.RightUpperLeg), // 5
                new(HumanBodyBones.RightLowerLeg), // 6
                new(HumanBodyBones.RightFoot), // 7
                new(HumanBodyBones.RightToes), // 8
                new(HumanBodyBones.Spine), // 9
                new(HumanBodyBones.Chest), // 10
                new(HumanBodyBones.UpperChest), // 11
                new(HumanBodyBones.Neck), // 12
                new(HumanBodyBones.Head), // 13
                new(HumanBodyBones.LeftShoulder), // 14
                new(HumanBodyBones.LeftUpperArm), // 15
                new(HumanBodyBones.LeftLowerArm), // 16
                new(HumanBodyBones.LeftHand), // 17
                new(HumanBodyBones.RightShoulder), // 18
                new(HumanBodyBones.RightUpperArm), // 19
                new(HumanBodyBones.RightLowerArm), // 20
                new(HumanBodyBones.RightHand), // 21
            };
            if (TryGetComponent<Animator>(out var animator))
            {
                for (int i = 0; i < Joints.Length; ++i)
                {
                    Joints[i].Transform = animator.GetBoneTransform(Joints[i].HumanBodyBone);
                }
            }
        }

        public bool IsDefaultPoseSet()
        {
            return DefaultPose != null && DefaultPose.Length == Joints.Length;
        }

        public Transform GetJointTransform(int index)
        {
            return Joints[index].Transform;
        }
        public Transform GetHipsTransform()
        {
            return Joints[0].Transform;
        }
        public Transform GetHeadTransform()
        {
            return Joints[13].Transform;
        }
        [ContextMenu("Compute Local Axes")]
        public void ComputeLocalAxes()
        {
            DefaultPose = new quaternion[Joints.Length];
            for (int i = 0; i < Joints.Length; ++i)
            {
                if (Joints[i].Transform != null)
                {
                    DefaultPose[i] = Joints[i].Transform.rotation;
                }
            }
        }

        public void GetJointWorldAxes(int index, out float3 forward, out float3 up, out float3 right)
        {
            Debug.Assert(Joints[index].Transform != null, "Joint " + index + " is not set");
            forward = math.mul(math.mul(Joints[index].Transform.rotation, math.inverse(DefaultPose[index])), math.forward());
            up = math.mul(math.mul(Joints[index].Transform.rotation, math.inverse(DefaultPose[index])), math.up());
            right = math.mul(math.mul(Joints[index].Transform.rotation, math.inverse(DefaultPose[index])), math.right());
        }

        [System.Serializable]
        public struct Joint
        {
            public HumanBodyBones HumanBodyBone;
            public Transform Transform;

            public Joint(HumanBodyBones humanBodyBones) : this()
            {
                HumanBodyBone = humanBodyBones;
            }
        }
    }
}