using UnityEngine;
using System.Collections;

public class LookAtTest : MonoBehaviour
{
    [Header("RIG JOINTS")]
    public Transform j_rightEye;
    public Transform j_leftEye;

    [Space(5)]
    public Transform j_head;
    public Transform j_neck;

    [Space(5)]
    public Transform j_chest;
    public Transform j_spine01;
    public Transform j_spine02;


    public void Awake()
    {
        // get rig's initial "look forward" rotations
    }
}
