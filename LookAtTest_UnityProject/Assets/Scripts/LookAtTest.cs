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

    public float lookAtWeight; // set in inspector
    private float _lookAtWeight;

    [Header("RIG JOINTS")]
    public JointData r_eye;
    public JointData l_eye;

    [Space(5)]
    public JointData head;
    public JointData neck;

    [Space(5)]
    public JointData chest;
    public JointData[] spine;


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
        Vector3 lookDir = -character.InverseTransformDirection(transform.position - head.joint.position).normalized;
        float tempX, tempY, tempZ;

        Debug.Log("lookDir = " + lookDir);

        tempX = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg + head.xOffset;
        tempY = (Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + head.yOffset);
        tempZ = (Mathf.Atan2(lookDir.y, -Mathf.Abs(lookDir.z)) * Mathf.Rad2Deg) + head.zOffset;

        head.joint.localRotation = Quaternion.Euler(head.joint.localRotation.eulerAngles + new Vector3(tempX, 0, tempZ));
    }
}
