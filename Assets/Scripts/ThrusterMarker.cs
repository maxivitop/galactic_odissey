using System;
using UnityEngine;

public class ThrusterMarker : MonoBehaviour
{
    public GameObject engineEffect;
    private TrajectoryMarker marker;
    private LineRenderer lineRenderer;
    private Thruster.Config config;
    private Thruster thruster;
    private void Awake()
    {
        marker = GetComponentInParent<TrajectoryMarker>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetConfig(Thruster.Config config, Thruster thruster)
    {
        if (this == null)
        {
            return;
        }

        this.config = config;
        this.thruster = thruster;
        var scale = Vector3.one * (config.thrust / thruster.maxAcceleration);
        engineEffect.transform.localScale = scale; // particle system
        marker.FixedRotation = Quaternion.LookRotation(Vector3.forward, config.direction);
    }

    private void Update()
    {
        UpdateTrajectory();
    }

    private void UpdateTrajectory()
    {
        if (config == null)
        {
            return;
        }
        var start = TrajectoryProvider.PhysicsStepToTrajectoryStep(config.initialStep);
        var end = Math.Min(
            TrajectoryProvider.PhysicsStepToTrajectoryStep(config.initialStep + config.steps + 1),
            thruster.trajectoryProvider.trajectory.size);
        if (end <= start)
        {
            return;
        }
        lineRenderer.positionCount = end - start;
        for (var i = start; i < end; i++)
        {
            lineRenderer.SetPosition(i-start, thruster.trajectoryProvider.trajectory[i]);
        } 
    }
}