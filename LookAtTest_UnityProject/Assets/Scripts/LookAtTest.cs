using UnityEngine;
using System.Collections;

public class LookAtTest : MonoBehaviour
{
    [System.Serializable]
    public class JointData
    {
        public Transform joint;
        public Vector3 baseRot;
    }

    [Header("RIG JOINTS")]
    public JointData j_rightEye;
    public JointData j_leftEye;

    [Space(5)]
    public JointData j_head;
    public JointData j_neck;

    [Space(5)]
    public JointData j_chest;
    public JointData j_spine01;
    public JointData j_spine02;


    public void Awake()
    {
        // get rig's initial "look forward" rotations
        //j_rightEye.baseRot = j_rightEye.joint.rotation.eulerAngles;
        //j_leftEye.baseRot = j_leftEye.joint.rotation.eulerAngles;
        j_head.baseRot = j_head.joint.localRotation.eulerAngles;
        j_neck.baseRot = j_neck.joint.localRotation.eulerAngles;
        //j_chest.baseRot = j_chest.joint.rotation.eulerAngles;
        //j_spine01.baseRot = j_spine01.joint.rotation.eulerAngles;
        //j_spine02.baseRot = j_spine02.joint.rotation.eulerAngles;
    }
    public float xOffset, yOffset, zOffset;
    public void LateUpdate()
    {
        Vector3 lookDir = (transform.position - j_head.joint.position).normalized;
        float tempX, tempY, tempZ;

        tempX = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg + xOffset;
        tempY = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + yOffset;
        tempZ = Mathf.Atan2(-lookDir.y, lookDir.z) * Mathf.Rad2Deg + zOffset;
        j_head.joint.localRotation = Quaternion.Euler(j_head.baseRot + new Vector3(tempX, 0, tempZ));
    }
}
