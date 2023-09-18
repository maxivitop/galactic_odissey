using System;
using UnityEngine;

public class ThrusterMarker : MonoBehaviour
{
    public GameObject engineEffect;
    private TrajectoryMarker marker;
    private LineRenderer lineRenderer;
    private Thruster.Config config;
    private Thruster thruster;
    public DirectionSelector directionSelector;
    public float minArrowSize = 1;
    public float maxArrowSize = 2;
    private void Awake()
    {
        var lastDirection = Vector2.zero;
        directionSelector.arrow.transform.localScale = new Vector3(2, minArrowSize, 1);
        directionSelector.OnValueChanged.AddListener(direction =>
        {
            direction /= directionSelector.arrow.transform.lossyScale.z;
            if ((lastDirection - direction).sqrMagnitude < 1e-6)
            {
                return;
            }
            lastDirection = direction;
            marker.IsSelected = true; // TODO unselect other markers
            thruster.OnDirectionChanged(config.initialStep, this, direction.normalized);
            var targetTotalThrust = Mathf.Min(thruster.maxDuration, (Mathf.Max(direction.magnitude, minArrowSize) - minArrowSize) * thruster.maxDuration / (maxArrowSize - minArrowSize));
            var duration = Mathf.CeilToInt(targetTotalThrust);
            var thrust = targetTotalThrust / duration * thruster.maxAcceleration;
            if (duration == 0)
            {
                duration = 1;
                thrust = 0;
            }
            thruster.OnDurationChanged(config.initialStep, this, duration);
            thruster.OnThrustChanged(config.initialStep, this, thrust);
            UpdateConfigValues(thrust, duration, direction.normalized);
            directionSelector.arrow.transform.localScale = new Vector3(2, Mathf.Clamp(direction.magnitude, minArrowSize, maxArrowSize), 1);
        });
        marker = GetComponentInParent<TrajectoryMarker>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void UpdateConfigValues(float thrust, int duration, Vector2 direction)
    {
        var scale = Vector3.one * (thrust / thruster.maxAcceleration);
        engineEffect.transform.localScale = scale; // particle system
        marker.FixedRotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    public void SetConfig(Thruster.Config config, Thruster thruster)
    {
        if (this == null)
        {
            return;
        }

        this.config = config;
        this.thruster = thruster;
        UpdateConfigValues(config.thrust, config.steps, config.direction);
        directionSelector.Value = config.direction * config.thrust * config.steps / thruster.maxAcceleration * (maxArrowSize - minArrowSize);
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
            lineRenderer.SetPosition(i - start, thruster.trajectoryProvider.trajectory.array[i]);
        }
    }
}