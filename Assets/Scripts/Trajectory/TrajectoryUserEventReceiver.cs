using System;
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
    [NonSerialized]
    public Vector3? closestToMouseTrajectoryPosition;
    public int? closestToMouseTrajectoryStep;

    private void OnEnable()
    {
        futureTransform = GetComponent<FutureTransform>();
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        trajectoryProvider.onTrajectoryUpdated.AddListener(UpdateWithTrajectory);
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void UpdateWithTrajectory(CapacityArray<Vector3> trajectory)
    {
        var worldMousePosition = MouseHandler.WorldMousePosition;
        var minDistance = float.MaxValue;
        closestToMouseTrajectoryPosition = null;
        closestToMouseTrajectoryStep = null;
        for (var i = 0; i < trajectory.size; i++)
        {
            var position = trajectory.array[i];
            var distance = (position - worldMousePosition).sqrMagnitude;
            if (distance + 2 * Mathf.Epsilon < minDistance)
            {
                closestToMouseTrajectoryPosition = position;
                closestToMouseTrajectoryStep = i;
                minDistance = distance;
            }
            i++;
        }

        if (!closestToMouseTrajectoryStep.HasValue) return;
        var step = closestToMouseTrajectoryStep.Value;
        var pos = closestToMouseTrajectoryPosition!.Value;
        var prev = trajectory.GetOrElse(step - 1, pos);
        var next = trajectory.GetOrElse(step + 1, pos);
        closestToMouseTrajectoryPosition = Utils.FindNearestPointOnSegment(
            start: prev,
            end: next,
            point: worldMousePosition
        );
    }
}