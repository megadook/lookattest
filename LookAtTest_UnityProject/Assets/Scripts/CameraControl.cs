using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public Transform pivot;
    public Camera cam;

    public float yawSpeed;
    public float pitchSpeed;

    Vector2 lastMousePos;


    public void Update()
    {
        Vector2 currentMousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(1))
        {
            lastMousePos = currentMousePos;
        }

        if (Input.GetMouseButton(1))
        {
            Vector2 delta = currentMousePos - lastMousePos;
            delta.x = delta.x / Screen.width;
            delta.y = delta.y / Screen.height;

            pivot.Rotate(Vector3.up, delta.x * yawSpeed, Space.World);
            pivot.Rotate(Vector3.right, delta.y * pitchSpeed);
        }

        lastMousePos = currentMousePos;
    }
}
