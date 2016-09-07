using UnityEngine;
using System.Collections;


public class LookAtTest : MonoBehaviour
{
    [System.Serializable]
    // holds a joint as well as data used for look at functionality
    public class JointData
    {
        public Transform joint; // the joint itself

        [Space(5)]
        public Vector3 offset; // the correction offset to be applied on top of lookat rotation

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

    // the source to use when checking for look at direction
    public Transform lookAtSource;

    [Space(5)]
    public float lookAtSpeed = 1f;
    [Range(0, 1)]
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
    public void Awake()
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
    public void Update()
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
    public void LateUpdate()
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
    public void ApplyLookAt()
    {
        Vector3 lookDir = -character.InverseTransformDirection(transform.position - lookAtSource.position).normalized;
        Vector3 lookRot = Vector3.zero;

        // the WORLD UP axis (in this case, the joint's local Z) needs to interpolate between a tangent calculated with the CHARACTER's local X and local Z
        float zLerp;

        // trig!    
        lookRot.x = Mathf.Atan2(-lookDir.x, -Mathf.Abs(lookDir.z)) * Mathf.Rad2Deg;

        zLerp = Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg < 90 ? (Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg) % 180f / 180f) * 2 : MapUtility.Map(Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg, 90, 180, 1, 0);
        lookRot.z = Mathf.Lerp(Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.z)), Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.x)), zLerp) * Mathf.Rad2Deg;

        #region head
        // head
        UpdateCurrentOffset(ref head, lookRot, headWeight);

        head.joint.localRotation = Quaternion.Slerp(Quaternion.Euler(head.joint.localRotation.eulerAngles + head.currentOffsetWeighted.eulerAngles), Quaternion.Euler(head.baseRot + head.currentOffsetWeighted.eulerAngles), overrideWeight);

        // neck
        UpdateCurrentOffset(ref neck, lookRot, headWeight);

        neck.joint.localRotation = Quaternion.Slerp(Quaternion.Euler(neck.joint.localRotation.eulerAngles + neck.currentOffsetWeighted.eulerAngles), Quaternion.Euler(neck.baseRot + neck.currentOffsetWeighted.eulerAngles), overrideWeight);
        #endregion


        #region body
        // chest
        UpdateCurrentOffset(ref chest, lookRot, bodyWeight);

        chest.joint.localRotation = Quaternion.Slerp(Quaternion.Euler(chest.joint.localRotation.eulerAngles + chest.currentOffsetWeighted.eulerAngles), Quaternion.Euler(chest.baseRot + chest.currentOffsetWeighted.eulerAngles), overrideWeight);

        // spine
        for (int i = 0; i < spine.Length; i++)
        {
            UpdateCurrentOffset(ref spine[i], lookRot, bodyWeight);

            spine[i].joint.localRotation = Quaternion.Slerp(Quaternion.Euler(spine[i].joint.localRotation.eulerAngles + spine[i].currentOffsetWeighted.eulerAngles), Quaternion.Euler(spine[i].baseRot + spine[i].currentOffsetWeighted.eulerAngles), overrideWeight);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentOffset"> the current offset </param>
    /// <param name="lookRot"></param>
    /// <param name="joint"></param>
    /// <param name="bodySectionWeight"></param>
    /// <returns></returns>
    void UpdateCurrentOffset(ref JointData joint, Vector3 lookRot, float bodySectionWeight)
    {
        // invert check
        if (joint.invertX) { lookRot.x *= -1; }
        if (joint.invertZ) { lookRot.z *= -1; }

        // get angle values by rotating from T-pose by the lookRotation and offset
        Vector3 currentOffsetEulerAngles = new Vector3(joint.baseRot.x + lookRot.x + joint.offset.x, 
                                                       joint.baseRot.y + lookRot.y + joint.offset.y, 
                                                       joint.baseRot.z + lookRot.z + joint.offset.z);

        // update offset over time by speed
        joint.currentOffset = Quaternion.Slerp(joint.currentOffset, Quaternion.Euler(currentOffsetEulerAngles), Time.deltaTime * lookAtSpeed);

        // weight offset by joint and body section weights
        joint.currentOffsetWeighted = Quaternion.Slerp(Quaternion.identity, joint.currentOffset, joint.weight * bodySectionWeight);
    }
}
