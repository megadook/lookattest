using UnityEngine;
using System.Collections;

public class VirtualParent : MonoBehaviour
{
	public bool active = false;

	[Header("~ POSITION ~")]
	public bool positionActive = false;
	public Transform positionParent;
	public Vector3 positionOffset;
    public bool x = true;
    public bool y = true;
    public bool z = true;

	[Header("~ ROTATION ~")]
	public bool rotationActive = false;
	public Transform rotationParent;

	void LateUpdate()
	{
		if (!active) { return; }

		if(positionActive && positionParent != null)
		{
            float newX = (x ? positionParent.position.x + positionOffset.x : transform.position.x);
            float newY = (y ? positionParent.position.y + positionOffset.y : transform.position.y);
            float newZ = (z ? positionParent.position.z + positionOffset.z : transform.position.z);
            transform.position = new Vector3(newX, newY, newZ);
		}

		if(rotationActive && rotationParent != null)
		{
			transform.rotation = rotationParent.rotation;
		}
	}
}
