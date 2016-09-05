using UnityEngine;
using System.Collections;

public class LookAtTargetUndo : MonoBehaviour
{
    public Transform dragObj;
    public ClickNDrag[] clickNDrags;

    Vector3 currentPos;
    Vector3 lastPos;
    Vector3 initialPos;

    bool lastDragging = false;
    bool canUndo = true;


    void Awake()
    {
        currentPos = dragObj.position;
        lastPos = currentPos;

        initialPos = currentPos;
    }

	void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 temp = currentPos;
            currentPos = initialPos;
            lastPos = temp;

            dragObj.position = currentPos;
            return;
        }

        if ((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) || !Input.GetKey(KeyCode.Z))
        {
            canUndo = true;
        }

        bool allOff = true;

        for (int i = 0; i < clickNDrags.Length; i++)
        {
            if (clickNDrags[i].dragging)
            {
                allOff = false;
                canUndo = false;
                break;
            }
        }

        if (lastDragging && allOff)
        {
            lastDragging = false;
            lastPos = currentPos;
            currentPos = dragObj.position;
            canUndo = true;
        }

        lastDragging = !allOff;

        if (canUndo && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Z))
        {
            Undo();
        }
    }

    void Undo()
    {
        Vector3 temp = currentPos;
        currentPos = lastPos;
        lastPos = temp;

        dragObj.position = currentPos;
        canUndo = false;
    }
}
