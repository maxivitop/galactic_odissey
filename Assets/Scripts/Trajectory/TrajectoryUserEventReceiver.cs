using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int? closestToMousePhysicsStep;

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

    private void Update()
    {
        UpdateWithTrajectory(trajectoryProvider.trajectory);
    }

    private void UpdateWithTrajectory(CapacityArray<Vector3> trajectory)
    {
        var worldMousePosition = MouseHandler.WorldMousePosition;
        var minDistance = float.MaxValue;
        closestToMouseTrajectoryPosition = null;
        closestToMouseTrajectoryStep = null;
        closestToMousePhysicsStep = null;

        var closestSegmentPos = Vector3.zero;
        for (var i = 0; i < trajectory.size; i++)
        {
            var position = trajectory[i];
            var distance = (position - worldMousePosition).sqrMagnitude;
            if (!(distance + Mathf.Epsilon < minDistance)) continue;
            if (closestToMouseTrajectoryStep.HasValue)
            {
                var prevCurr = trajectory.GetOrElse(i - 1, position);
                var nextCurr = trajectory.GetOrElse(i + 1, position);
                var closestCurr = Utils.FindNearestPointOnSegment(
                    start: prevCurr,
                    end: nextCurr,
                    point: worldMousePosition
                );
                if (i - closestToMouseTrajectoryStep.Value > 10 &&
                    (closestCurr - closestSegmentPos).sqrMagnitude < 1e-2)
                {
                    continue; // we are in the loop
                }

                closestSegmentPos = closestCurr;
            }

            closestToMouseTrajectoryPosition = position;
            closestToMouseTrajectoryStep = i;
            minDistance = distance;
        }
        
        if (!closestToMouseTrajectoryStep.HasValue) return;
        var step = closestToMouseTrajectoryStep.Value;
        closestToMousePhysicsStep = TrajectoryProvider.TrajectoryStepToPhysicsStep(step);
        closestToMouseTrajectoryPosition = closestSegmentPos;
    }
}