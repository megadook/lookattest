using UnityEngine;
using System.Collections;


public class LookAtTest : MonoBehaviour
{
    [System.Serializable]
    public class JointData
    {
        public Transform joint;
        public Vector3 baseRot;

        public float xOffset, yOffset, zOffset;

        // keep track of current used lookAtOffset, so when target is behind,
        // move back towards no offset at all

        public void RecordBaseRot()
        {
            if (joint != null)
            {
                baseRot = joint.localRotation.eulerAngles;
            }
        }
    }

    public Transform character;
    public Transform lookAtSource;

    public float lookAtWeight;

    public float lookAtSpeed = 1f;

    [Header("WEIGHTS")]
    public float eyeWeight = 0f;

    [Space(5)]
    public float headWeight = 0f;
    public float neckWeight = 0f;

    [Space(5)]
    public float chestWeight = 0f;
    public float[] spineWeight;

    [Header("RIG JOINTS")]
    public JointData r_eye;
    public JointData l_eye;

    [Space(5)]
    public JointData head;
    public JointData neck;

    [Space(5)]
    public JointData chest;
    public JointData[] spine;

    // offsets blended in through lookAtWeight
    private Quaternion currentRightEyeOffset;
    private Quaternion currentLeftEyeOffset;
    private Quaternion currentHeadOffset;
    private Quaternion currentNeckOffset;
    private Quaternion currentChestOffset;
    private Quaternion[] currentSpineOffset;

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
    }

    public void LateUpdate()
    {
        Vector3 lookDir = -character.InverseTransformDirection(transform.position - lookAtSource.position).normalized;
        float tempX, tempY, tempZ;

       // Debug.Log("lookDir = " + lookDir);

        tempX = Mathf.Atan2(-lookDir.x, -Mathf.Abs(lookDir.z)) * Mathf.Rad2Deg + head.xOffset;
        tempY = Mathf.Lerp(Mathf.Atan2(lookDir.y, lookDir.x), Mathf.Atan2(lookDir.y, lookDir.z), (Mathf.Abs(head.joint.localRotation.eulerAngles.y) % 90 / 90f)) * Mathf.Rad2Deg + head.yOffset;


        float zLerp = Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg < 90 ? (Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg) % 180f / 180f) * 2 : MapUtility.Map(Mathf.Abs(Mathf.Atan2(lookDir.x, lookDir.z)) * Mathf.Rad2Deg, 90, 180, 1, 0);
        tempZ = Mathf.Lerp(Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.z)), Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.x)), zLerp) * Mathf.Rad2Deg + head.zOffset;

        currentHeadOffset = Quaternion.Slerp(currentHeadOffset, Quaternion.Euler(new Vector3(tempX, 0, tempZ)), Time.deltaTime * lookAtSpeed);
        //Debug.Log(currentOffset.eulerAngles);

        head.joint.localRotation = Quaternion.Euler(head.joint.localRotation.eulerAngles + currentHeadOffset.eulerAngles);
    }
}
