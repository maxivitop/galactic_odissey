using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(TrajectoryRenderer))]
public class TrajectoryUserEventReceiver : MonoBehaviour
{
    public static readonly List<TrajectoryUserEventReceiver> all = new();
    public FutureTransform futureTransform;
    public TrajectoryRenderer trajectoryRenderer;

    private void OnEnable()
    {
        futureTransform = GetComponent<FutureTransform>();
        trajectoryRenderer = GetComponent<TrajectoryRenderer>();
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }
}