using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ThrusterMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject engineEffect;
    private TrajectoryMarker marker;
    private LineRenderer lineRenderer;
    private Thruster.Config config;
    private Thruster thruster;
    public DirectionSelector directionSelector;
    public Color selectedColor;
    public Color highlightedColor;
    public Color usualColor;
    private void Awake()
    {
        directionSelector.OnValueChanged.AddListener(() =>
        {
            if (Vector2.Distance(directionSelector.Direction,config.direction) > 1e-6)
            {
                thruster.OnDirectionChanged(config.initialStep, this, directionSelector.Direction);
            }
            var targetTotalThrust = directionSelector.Magnitude * thruster.maxDuration;
            var duration = Mathf.CeilToInt(targetTotalThrust);
            var thrust = targetTotalThrust / duration * thruster.maxAcceleration;
            if (duration == 0)
            {
                duration = 1;
                thrust = 0;
            }
            if (duration != config.steps)
            { 
                thruster.OnDurationChanged(config.initialStep, this, duration);
            }
            if (Math.Abs(thrust - config.thrust) > 1e-6)
            {
                thruster.OnThrustChanged(config.initialStep, this, thrust);
            }
            UpdateConfigValues(thrust, duration, directionSelector.Direction);
            TrajectoryUserEventCreator.Instance.SelectMarker(marker);
        });
        marker = GetComponentInParent<TrajectoryMarker>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void UpdateConfigValues(float thrust, int duration, Vector2 direction)
    {
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
        directionSelector.Direction = config.direction;
        directionSelector.Magnitude = config.thrust * config.steps / thruster.maxAcceleration / thruster.maxDuration;
    }

    private void Update()
    {
        if (marker.IsSelected)
        {
            directionSelector.Color = selectedColor;
        }
        else if (marker.IsHighlighted)
        {
            directionSelector.Color = highlightedColor;
        }
        else
        {
            directionSelector.Color = usualColor;
        }
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
            lineRenderer.SetPosition(i - start, thruster.trajectoryProvider.trajectory[i]);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TrajectoryUserEventCreator.Instance.forceHighlightMarker = marker;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TrajectoryUserEventCreator.Instance.forceHighlightMarker == marker)
        {
            TrajectoryUserEventCreator.Instance.forceHighlightMarker = null;
        }
    }
}