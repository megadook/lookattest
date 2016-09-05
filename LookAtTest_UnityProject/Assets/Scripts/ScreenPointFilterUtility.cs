using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// checks if a screen point is within a given area
/// </summary>
public class ScreenPointFilterUtility : MonoBehaviour
{
    public Collider col;
    public Collider2D col2D;
    public RectTransform rt;

    // optionally used for collider checking
    public LayerMask layerMask = ~0;

    void Awake()
    {
        col = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
        rt = GetComponent<RectTransform>();
    }

    public bool FilterCheck(Vector2 screenPoint, Camera cam)
    {
        return (((rt != null) ? RectTransformCheck(screenPoint, cam) : true) &&
                ((col != null || col2D != null) ? ColliderCheck(screenPoint, cam) : true));
    }

    public bool ColliderCheck(Vector2 screenPoint, Camera cam)
    {
        Vector3 worldPoint;
        if (col2D != null)
        {
            worldPoint = cam.ScreenToWorldPoint(screenPoint);
            return col2D.OverlapPoint(worldPoint);
        }
        else if (col != null)
        {
            Ray ray = cam.ScreenPointToRay(screenPoint);
            RaycastHit hit;        
            if (Physics.Raycast(ray, out hit, 100f, layerMask))
            {
                return (hit.collider == col);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool RectTransformCheck(Vector2 screenPoint, Camera cam)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, cam);
    }
}