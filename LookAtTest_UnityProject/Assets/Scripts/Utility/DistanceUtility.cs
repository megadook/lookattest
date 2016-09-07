using UnityEngine;
using System.Collections;

public class DistanceUtility
{ 
    /// <summary>
    /// flattens the toFlatten Vector to the normal, aligned with toCompare
    /// </summary>
    public static Vector3 GetFlatVector(Vector3 toFlatten, Vector3 toCompare, Vector3 normal)
    {
        Vector3 returnVector = toFlatten;

        if (normal == Vector3.up || normal == -Vector3.up)
        {
            returnVector.y = toCompare.y;
        }
        else if (normal == Vector3.right || normal == -Vector3.right)
        {
            returnVector.x = toCompare.x;
        }
        else if (normal == Vector3.forward || normal == -Vector3.forward)
        {
            returnVector.z = toCompare.z;
        }
        else
        {
            Plane p = new Plane(normal, toCompare);
            float rayDistance;

            // try normal direction
            Ray ray = new Ray(toFlatten, -normal);
            if (p.Raycast(ray, out rayDistance))
            {
                returnVector = ray.GetPoint(rayDistance);
            }
            else
            {
                // try inverse normal direction
                ray = new Ray(toFlatten, normal);
                if (p.Raycast(ray, out rayDistance))
                {
                    returnVector = ray.GetPoint(rayDistance);
                }
            }
        }

        return returnVector;
    }

    /// <summary>
    /// returns the (2D) distance of two vectors, as if they were flattened onto a normal plane
    /// </summary>
    public static float FlatDistance(Vector3 a, Vector3 b, Vector3 normal)
    {
        Vector3 flatB = GetFlatVector(b, a, normal);

        return Vector3.Distance(a, flatB);
    }

    /// <summary>
    /// gets the (3D) manhattan distance between two vectors
    /// </summary>
    public static float ManhattanDistance(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z));
    }

    public static float FlatManhattanDistance(Vector3 a, Vector3 b, Vector3 normal)
    {
        Vector3 flatB = GetFlatVector(b, a, normal);

        return ManhattanDistance(a, flatB);
    }
}
