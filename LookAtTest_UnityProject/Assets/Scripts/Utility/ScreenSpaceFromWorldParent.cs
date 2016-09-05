using UnityEngine;
using System.Collections;

public class ScreenSpaceFromWorldParent : MonoBehaviour
{
    public Transform worldParent;
    public RectTransform rt;

    public Camera cam;

    public float scaleFactor;


    void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

	void LateUpdate()
    {
        float dist = Vector3.Distance(worldParent.position, cam.transform.position);
        Vector2 newScreenPos = cam.WorldToScreenPoint(worldParent.position);
        rt.localScale = Vector3.one * (1 / dist) * scaleFactor;
        rt.anchoredPosition = newScreenPos + new Vector2(0, rt.rect.height * -rt.localScale.y);
    }
}
