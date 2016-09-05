using UnityEngine;
using System.Collections;

public class ClickNDrag : MonoBehaviour
{
    public Transform dragObj;
    public float dragSpeed;

    public Transform cameraRig;

    public enum Axis
    {
        X, Y, Z
    }
    public Axis axis;

    private ScreenPointFilterUtility clickArea;
    public bool dragging = false;

    private Vector2 lastScreenPos;
    private Vector2 delta;

    void Start()
    {
        clickArea = gameObject.GetComponent<ScreenPointFilterUtility>();
        if (clickArea == null)
        {
            clickArea = gameObject.AddComponent<ScreenPointFilterUtility>();
        }

        cameraRig = GameObject.Find("Camera_Rig").transform;
    }

    void Update()
    {
        if (!dragging)
        {
            ReadStart();
        }
        else
        {
            if (!ReadEnd())
            {
                Vector2 currentScreenPos = Input.mousePosition;
                delta = currentScreenPos - lastScreenPos;
                delta.x = delta.x / Screen.width;
                delta.y = delta.y / Screen.width;

                if (axis == Axis.X)
                {
                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.right).y < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.right).y)
                    {
                        delta.y = -delta.y;
                    }

                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.right).x < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.right).x)
                    {
                        delta.x = -delta.x;
                    }

                    dragObj.position += new Vector3(1, 0, 0) * delta.x * dragSpeed;
                    dragObj.position += new Vector3(1, 0, 0) * delta.y * dragSpeed;
                }
                if (axis == Axis.Y)
                {
                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.up).y < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.up).y)
                    {
                        delta.y = -delta.y;
                    }

                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.up).x < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.up).x)
                    {
                        delta.x = -delta.x;
                    }

                    dragObj.position += new Vector3(0, 1, 0) * delta.y * dragSpeed;
                    dragObj.position += new Vector3(0, 1, 0) * delta.x * dragSpeed;

                }
                if (axis == Axis.Z)
                {
                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.forward).x < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.forward).x)
                    {
                        delta.x = -delta.x;
                    }

                    if (Camera.main.WorldToScreenPoint(dragObj.position + Vector3.forward).y < Camera.main.WorldToScreenPoint(dragObj.position - Vector3.forward).y)
                    {
                        delta.y = -delta.y;
                    }

                    dragObj.position += new Vector3(0, 0, 1) * delta.x * dragSpeed;
                    dragObj.position += new Vector3(0, 0, 1) * delta.y * dragSpeed;
                }

                lastScreenPos = currentScreenPos;
            }
        }
    }

    private void ReadStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (clickArea.FilterCheck(Input.mousePosition, Camera.main))
            {
                dragging = true;
                lastScreenPos = Input.mousePosition;
            }
        }
    }

    private bool ReadEnd()
    {
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            return true;
        }

        return false;
    }
}
