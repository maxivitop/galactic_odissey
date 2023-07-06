using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TrajectoryUserEventReceiver))]
public class Thruster : FutureBehaviour, ITrajectoryUserEventProvider
{
    public class Config
    {
        public int initialStep;
        public int steps = 1;
        public int maxSteps = -1;
        public Vector2 direction = Vector2.up;
        public float thrust = 0;
        public TrajectoryMarker marker;

        public Config(int initialStep, TrajectoryMarker marker)
        {
            this.initialStep = initialStep;
            this.marker = marker;
        }
    }
    public GameObject UIPrefab;
    public ThrusterMarker markerCustomizer;
    public float maxAcceleration;
    public int maxDuration;
    Dictionary<int, Config> StepsConfig = new();

    FutureRigidBody2D futureRigidBody2D;

    void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
    }

    public GameObject CreateUI(int step, TrajectoryMarker marker)
    {
        var isNewConfig = !StepsConfig.ContainsKey(step);
        Config stepConfig = GetOrCreateStepConfig(step, marker);
        var maxSteps = GetMaxSteps(step, stepConfig);
        stepConfig.maxSteps = maxSteps;

        GameObject UI = Instantiate(UIPrefab);
        ThrusterMarker thrusterMarker = GetOrCreateMarkerCustomizer(marker);
        thrusterMarker.SetConfig(StepsConfig[step], this);

        var thrustSlider = UI.transform.Find("Thrust slider").GetComponent<Slider>();
        thrustSlider.maxValue = maxAcceleration;
        thrustSlider.value = stepConfig.thrust;
        thrustSlider.onValueChanged.AddListener((float value) =>
        {
            OnThrustSliderChanged(step, thrusterMarker, value);
        });

        var directionSelector = UI.transform.Find("Direction Selector").GetComponent<DirectionSelector>();
        directionSelector.Value = stepConfig.direction;
        directionSelector.OnValueChanged.AddListener((Vector2 value) =>
        {
            OnDirectionChanged(step, thrusterMarker, value);
        });

        var durationSlider = UI.transform.Find("Duration slider").GetComponent<Slider>();
        durationSlider.maxValue = maxSteps;
        durationSlider.value = isNewConfig ? (maxSteps + 1) / 2 : stepConfig.steps;
        ChangeDuration(step, thrusterMarker, (int)durationSlider.value);
        durationSlider.onValueChanged.AddListener((float value) =>
        {
            OnDurationSliderChanged(step, thrusterMarker, (int) value);
        });
        return UI;
    }

    public bool isEnabled(int step)
    {
        return true;
    }

    private Config GetOrCreateStepConfig(int step, TrajectoryMarker marker)
    {
        if (!StepsConfig.ContainsKey(step))
        {
            StepsConfig[step] = new Config(step, marker);
        }
        Config stepConfig = StepsConfig[step];
        if (stepConfig.initialStep != step)
        {
            Config newConfig = new Config(step, marker);
            newConfig.steps = stepConfig.steps + stepConfig.initialStep - step;
            newConfig.thrust = stepConfig.thrust;
            newConfig.direction = stepConfig.direction;
            for (int i = 0; i < newConfig.steps; i++)
            {
                StepsConfig[step + i] = newConfig;
            }
            stepConfig.maxSteps = step - stepConfig.initialStep;
            GetOrCreateMarkerCustomizer(stepConfig.marker).SetConfig(stepConfig, this);
            stepConfig = newConfig;
        }
        return stepConfig;
    } 

    private int GetMaxSteps(int step, Config stepConfig)
    {
        var maxSteps = maxDuration;
        for (int i = 0; i < maxDuration; i++)
        {
            Config config;
            StepsConfig.TryGetValue(step + i, out config);
            if (config == null || config == stepConfig)
            {
                continue;
            }
            maxSteps = i-1;
            break;
        }
        return maxSteps;
    }
    private ThrusterMarker GetOrCreateMarkerCustomizer(TrajectoryMarker marker)
    {
        var markerCustomizer = marker.GetComponentInChildren<ThrusterMarker>();
        if (markerCustomizer == null)
        {
            GameObject instantiated = Instantiate(this.markerCustomizer.gameObject);
            instantiated.transform.parent = marker.transform;
            instantiated.transform.localPosition = Vector3.zero;
            instantiated.transform.localRotation = Quaternion.identity;
            markerCustomizer = instantiated.GetComponent<ThrusterMarker>();
        }
        return markerCustomizer;
    } 

    private void OnThrustSliderChanged(int step, ThrusterMarker marker, float value)
    {
        StepsConfig[step].thrust = value;
        marker.SetConfig(StepsConfig[step], this);
        FuturePhysics.Reset(step-1, gameObject);
    }
    private void OnDurationSliderChanged(int step, ThrusterMarker marker, int value)
    {
        ChangeDuration(step, marker, value);
        FuturePhysics.Reset(step - 1, gameObject);
    }

    private void ChangeDuration(int step, ThrusterMarker marker, int value)
    {
        for (int i = step + value; i < StepsConfig[step].steps + step; i++)
        {
            StepsConfig.Remove(i);
        }

        for (int i = StepsConfig[step].steps; i < value; i++)
        {
            StepsConfig[step + i] = StepsConfig[step];
        }
        StepsConfig[step].steps = value;
        marker.SetConfig(StepsConfig[step], this);
    }

    private void OnDirectionChanged(int step, ThrusterMarker marker, Vector2 direction)
    {
        StepsConfig[step].direction = direction;
        marker.SetConfig(StepsConfig[step], this);
        FuturePhysics.Reset(step - 1, gameObject);
    }
    public override void VirtualStep(int step)
    {
        if (!StepsConfig.ContainsKey(step))
        {
            return;
        }
        Config currentConfig = StepsConfig[step];
        futureRigidBody2D.GetState(step).AddForce(currentConfig.direction * currentConfig.thrust);
    }
}
