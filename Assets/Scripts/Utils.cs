using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 FindNearestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        var direction = end - start;
        var magnitudeMax = direction.magnitude;
        direction /= magnitudeMax; // normalize

        //Do projection from the point but clamp it
        var dotP = Vector3.Dot(point - start, direction);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return start + direction * dotP;
    }
}