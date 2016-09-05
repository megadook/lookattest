using UnityEngine;
using System.Collections;

public class MapUtility
{
    /// <summary>
    /// maps a value in a range of numbers, to a value in a range of other numbers
    /// 
    ///    eg: .5 in a range from 0 to 1, converted to a range from -1 to 1, will return 0
    /// </summary>
    public static float Map(float x, float inMin, float inMax, float outMin, float outMax)
    {
        if (inMax == inMin && outMin == 0)
        {
            return outMax;
        }
        else
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}