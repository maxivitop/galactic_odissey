using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
[RequireComponent(typeof(TrajectoryProvider))]
public class TrajectoryUserEventReceiver : MonoBehaviour
{
    public static readonly List<TrajectoryUserEventReceiver> all = new();
    public FutureTransform futureTransform;
    public TrajectoryProvider trajectoryProvider;

    private void OnEnable()
    {
        futureTransform = GetComponent<FutureTransform>();
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }
}