using UnityEngine;
using System.Collections;


public class CharacterLookAt : MonoBehaviour
{
    [System.Serializable]
    // holds a joint as well as data used for look at functionality
    public class JointData
    {
        public Transform joint; // the joint itself

        [Space(5)]
        public Vector3 correctionOffset; // the correction offset to be applied on top of lookat rotation

        [HideInInspector]
        public Quaternion currentOffset; // the current offset rotation used to lookat

        [HideInInspector]
        public Quaternion currentOffsetWeighted; // the current offset rotation used to lookat (with weighting)

        [Range(0, 1)]
        public float weight = 0f; // the joints local (in body section) weight

        [HideInInspector]
        public Vector3 baseRot; // T-pose rotation

        [Space(5)]
        public bool invertX;
        public bool invertZ;


        // reads the joint's local rotation into baseRot
        public void RecordBaseRot()
        {
            if (joint != null)
            {
                baseRot = joint.localRotation.eulerAngles;
            }
        }
    }

    // character's root
    public Transform character;

    // the target to look at
    public Transform target;

    // the source to use when checking for look at direction
    public Transform source;

    [Space(10)]
    public float lookAtSpeed = 1f;

    [Range(0, 1)]
    // designates whether look at rotation is additive ontop of animation clips
    // or overrides all other movement, (0 - additive, 1 - override)
    public float overrideWeight = 0f;


    [Header("EYE JOINTS")]
    [Range(0,1)]
    public float eyeWeight;
    [Space(5)]
    public JointData r_eye;
    public JointData l_eye;

    [Header("HEAD JOINTS")]
    [Range(0, 1)]
    public float headWeight;
    private float lastHeadWeight;
    [Space(5)]
    public JointData head;
    public JointData neck;

    [Header("BODY JOINTS")]
    [Range(0, 1)]
    public float bodyWeight;
    private float lastBodyWeight;

    [Space(5)]
    public JointData chest;
    public JointData[] spine;


    // record base rotations while the joints are still in T pose
    private void Awake()
    {
        // get rig's initial "look forward" rotations
        head.RecordBaseRot();
        neck.RecordBaseRot();
        chest.RecordBaseRot();

        for (int i = 0; i < spine.Length; i++)
        {
            spine[i].RecordBaseRot();
        }

        // init last weights
        lastHeadWeight = headWeight;
        lastBodyWeight = bodyWeight;
    }

    // make sure head/body weighting is accurate before applying the lookat in LateUpdate
    private void Update()
    {
        ReadWeightChange();
    }

    /// <summary>
    /// if weighting for the head/body was altered, adjust the other to compensate
    /// </summary>
    private void ReadWeightChange()
    {
        if (bodyWeight != lastBodyWeight)
        {
            headWeight = 1 - bodyWeight;
        }
        else if (headWeight != lastHeadWeight)
        {
            bodyWeight = 1 - headWeight;
        }

        lastHeadWeight = headWeight;
        lastBodyWeight = bodyWeight;
    }

    // apply the LookAt rotation after physics, object movements, and animations have been applied
    private void LateUpdate()
    {
        ApplyLookAt();
    }

    /// <summary>
    /// Applies a look at rotation to a character rig.
    /// Keeps in mind shared weighting between the character's head and upper body,
    /// as well as local weighting in those two groups of joints.
    /// 
    /// TODO possibly incorporate some local Y rotation?
    /// 
    /// NOTE: All rigs are different, chances are a rig that wasn't set up like mine may not behave properly with these values.
    /// </summary>
    private void ApplyLookAt()
    {
        Vector3 lookDir = -character.InverseTransformDirection(target.position - source.position).normalized;
        Vector3 lookRot = Vector3.zero;

        // trig!    
        lookRot.x = Mathf.Atan2(-lookDir.x, -Mathf.Abs(lookDir.z)) * Mathf.Rad2Deg;

        // the WORLD UP axis (in this case, the joint's local Z) needs to interpolate between a tangent calculated with the CHARACTER's local X and local Z
        float zLerp;
        zLerp = Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg < 90 ? (Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg) % 180f / 180f) * 2 : MapUtility.Map(Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg, 90, 180, 1, 0);
        lookRot.z = Mathf.Lerp(Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.z)), Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.x)), zLerp) * Mathf.Rad2Deg;

        // head
        UpdateCurrentOffset(ref head, lookRot, headWeight);
        ApplyRotationToJoint(head);

        // neck
        UpdateCurrentOffset(ref neck, lookRot, headWeight);
        ApplyRotationToJoint(neck);

        // chest
        UpdateCurrentOffset(ref chest, lookRot, bodyWeight);
        ApplyRotationToJoint(chest);

        // spine
        for (int i = 0; i < spine.Length; i++)
        {
            UpdateCurrentOffset(ref spine[i], lookRot, bodyWeight);
            ApplyRotationToJoint(spine[i]);
        }
    }

    /// <summary>
    /// updates the final offset rotation for a joint
    /// </summary>
    /// <param name="joint"> the JointData to update </param>
    /// <param name="lookRot"> the calculated look rotation </param>
    /// <param name="bodySectionWeight"> the weight setting for this joint's body segment </param>
    /// <returns></returns>
    private void UpdateCurrentOffset(ref JointData joint, Vector3 lookRot, float bodySectionWeight)
    {
        // invert check
        if (joint.invertX) { lookRot.x *= -1; }
        if (joint.invertZ) { lookRot.z *= -1; }

        // get angle values by rotating from T-pose by the lookRotation and offset
        Vector3 currentOffsetEulerAngles = new Vector3(joint.baseRot.x + lookRot.x + joint.correctionOffset.x, 
                                                       joint.baseRot.y + lookRot.y + joint.correctionOffset.y, 
                                                       joint.baseRot.z + lookRot.z + joint.correctionOffset.z);

        // update offset over time by speed
        joint.currentOffset = Quaternion.Slerp(joint.currentOffset, Quaternion.Euler(currentOffsetEulerAngles), Time.deltaTime * lookAtSpeed);

        // weight offset by joint and body section weights
        joint.currentOffsetWeighted = Quaternion.Slerp(Quaternion.identity, joint.currentOffset, joint.weight * bodySectionWeight);
    }

    /// <summary>
    /// applies the calculated offset rotation to a joint
    /// (additive vs overridden based on overrideWeight)
    /// </summary>
    /// <param name="joint"> the joint to apply rotation to </param>
    private void ApplyRotationToJoint(JointData joint)
    {
        Quaternion additiveRot = Quaternion.Euler(joint.joint.localRotation.eulerAngles + joint.currentOffsetWeighted.eulerAngles);
        Quaternion overrideRot = Quaternion.Euler(joint.baseRot + joint.currentOffsetWeighted.eulerAngles);

        joint.joint.localRotation = Quaternion.Slerp(additiveRot, overrideRot, overrideWeight);
    }
}
