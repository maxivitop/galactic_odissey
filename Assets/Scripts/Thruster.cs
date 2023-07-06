using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TrajectoryUserEventReceiver))]
public class Thruster : FutureBehaviour, ITrajectoryUserEventProvider
{
    public class Config
    {
        public readonly int initialStep;
        public int steps = 1;
        public int maxSteps = -1;
        public Vector2 direction = Vector2.up;
        public float thrust = 0;
        public readonly TrajectoryMarker marker;

        public Config(int initialStep, TrajectoryMarker marker)
        {
            this.initialStep = initialStep;
            this.marker = marker;
        }
    }

    // ReSharper disable once InconsistentNaming
    public GameObject UIPrefab;
    public ThrusterMarker markerCustomizer;
    public float maxAcceleration;
    public int maxDuration;
    private readonly Dictionary<int, Config> stepsConfig = new();

    private FutureRigidBody2D futureRigidBody2D;

    private void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
    }

    public GameObject CreateUI(int step, TrajectoryMarker marker)
    {
        var isNewConfig = !stepsConfig.ContainsKey(step);
        var stepConfig = GetOrCreateStepConfig(step, marker);
        var maxSteps = GetMaxSteps(step, stepConfig);
        stepConfig.maxSteps = maxSteps;

        var ui = Instantiate(UIPrefab);
        var thrusterMarker = GetOrCreateMarkerCustomizer(marker);
        thrusterMarker.SetConfig(stepsConfig[step], this);

        var thrustSlider = ui.transform.Find("Thrust slider").GetComponent<Slider>();
        thrustSlider.maxValue = maxAcceleration;
        thrustSlider.value = stepConfig.thrust;
        thrustSlider.onValueChanged.AddListener((float value) =>
        {
            OnThrustSliderChanged(step, thrusterMarker, value);
        });

        var directionSelector =
            ui.transform.Find("Direction Selector").GetComponent<DirectionSelector>();
        directionSelector.Value = stepConfig.direction;
        directionSelector.OnValueChanged.AddListener((Vector2 value) =>
        {
            OnDirectionChanged(step, thrusterMarker, value);
        });

        var durationSlider = ui.transform.Find("Duration slider").GetComponent<Slider>();
        durationSlider.maxValue = maxSteps;
        durationSlider.value = isNewConfig ? (maxSteps + 1) / 2 : stepConfig.steps;
        ChangeDuration(step, thrusterMarker, (int)durationSlider.value);
        durationSlider.onValueChanged.AddListener((float value) =>
        {
            OnDurationSliderChanged(step, thrusterMarker, (int)value);
        });
        return ui;
    }

    public bool IsEnabled(int step)
    {
        return true;
    }

    private Config GetOrCreateStepConfig(int step, TrajectoryMarker marker)
    {
        if (!stepsConfig.ContainsKey(step)) stepsConfig[step] = new Config(step, marker);
        var stepConfig = stepsConfig[step];
        if (stepConfig.initialStep == step) return stepConfig;
        
        var newConfig = new Config(step, marker)
        {
            steps = stepConfig.steps + stepConfig.initialStep - step,
            thrust = stepConfig.thrust,
            direction = stepConfig.direction
        };
        for (var i = 0; i < newConfig.steps; i++) stepsConfig[step + i] = newConfig;
        stepConfig.maxSteps = step - stepConfig.initialStep;
        GetOrCreateMarkerCustomizer(stepConfig.marker).SetConfig(stepConfig, this);
        stepConfig = newConfig;

        return stepConfig;
    }

    private int GetMaxSteps(int step, Config stepConfig)
    {
        var maxSteps = maxDuration;
        for (var i = 0; i < maxDuration; i++)
        {
            stepsConfig.TryGetValue(step + i, out var config);
            if (config == null || config == stepConfig) continue;
            maxSteps = i - 1;
            break;
        }

        return maxSteps;
    }

    private ThrusterMarker GetOrCreateMarkerCustomizer(TrajectoryMarker marker)
    {
        var thrusterMarker = marker.GetComponentInChildren<ThrusterMarker>();
        if (thrusterMarker != null) return thrusterMarker;
        
        var instantiated = Instantiate(
            markerCustomizer.gameObject, marker.transform, true);
        
        instantiated.transform.localPosition = Vector3.zero;
        instantiated.transform.localRotation = Quaternion.identity;
        thrusterMarker = instantiated.GetComponent<ThrusterMarker>();

        return thrusterMarker;
    }

    private void OnThrustSliderChanged(int step, ThrusterMarker marker, float value)
    {
        stepsConfig[step].thrust = value;
        marker.SetConfig(stepsConfig[step], this);
        FuturePhysics.Reset(step - 1, gameObject);
    }

    private void OnDurationSliderChanged(int step, ThrusterMarker marker, int value)
    {
        ChangeDuration(step, marker, value);
        FuturePhysics.Reset(step - 1, gameObject);
    }

    private void ChangeDuration(int step, ThrusterMarker marker, int value)
    {
        for (var i = step + value; i < stepsConfig[step].steps + step; i++) stepsConfig.Remove(i);

        for (var i = stepsConfig[step].steps; i < value; i++)
            stepsConfig[step + i] = stepsConfig[step];
        stepsConfig[step].steps = value;
        marker.SetConfig(stepsConfig[step], this);
    }

    private void OnDirectionChanged(int step, ThrusterMarker marker, Vector2 direction)
    {
        stepsConfig[step].direction = direction;
        marker.SetConfig(stepsConfig[step], this);
        FuturePhysics.Reset(step - 1, gameObject);
    }

    public override void VirtualStep(int step)
    {
        if (!stepsConfig.ContainsKey(step)) return;
        var currentConfig = stepsConfig[step];
        futureRigidBody2D.GetState(step).AddForce(currentConfig.direction * currentConfig.thrust);
    }
}