using System.Collections;
using System.Collections.Generic;
using UBIK;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Ubiq.MotionMatching
{
    [RequireComponent(typeof(UpperBodyAvatar))]
    [RequireComponent(typeof(LowerBodyAvatar))]
    public class MMVRAvatar : MonoBehaviour
    {
        public MMVRAvatarInput Input;

        private UpperBodyAvatar upperBody;
        private LowerBodyAvatar lowerBody;

        private void Awake()
        {
            upperBody = GetComponent<UpperBodyAvatar>();
            lowerBody = GetComponent<LowerBodyAvatar>();            
        }

        public void SetInput(MMVRAvatarInput input)
        {
            upperBody.LeftTracker = Input.LeftTracker;
            upperBody.RightTracker = Input.RightTracker;
            upperBody.HeadTracker = Input.HeadTracker;
            lowerBody.LowerBodySource = Input.LowerBody;
        }

        // Start is called before the first frame update
        void Start()
        {
            if(!Input)
            {
                Input = MMVRAvatarInput.Local;
            }
            SetInput(Input);
        }
    }
}