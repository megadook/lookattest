using UnityEngine;
using System.Collections;

public class ThirdPersonDemoManager : MonoBehaviour
{
    public CharacterLookAt characterLookAt;
    public Transform defaultLookAtTarget;

    [Space(10)]
    public float lookRadius = 5f;
    public Transform[] lookAtTargets;


    public void Update()
    {
        if (!CheckTargetDistance())
        {
            characterLookAt.target = defaultLookAtTarget;
        }
    }

    private bool CheckTargetDistance()
    {
        float dist;

        for (int i = 0; i < lookAtTargets.Length; i++)
        {
            dist = DistanceUtility.FlatDistance(lookAtTargets[i].position, characterLookAt.transform.position, Vector3.up);
            if (dist < lookRadius)
            {
                characterLookAt.target = lookAtTargets[i];
                return true;
            }
        }

        return false;
    }
}
