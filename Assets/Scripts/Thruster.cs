using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TrajectoryUserEventReceiver))]
public class Thruster : FutureBehaviour, ITrajectoryUserEventProvider
{
    public class Config
    {
        public readonly int initialStep;
        public int steps = 1;
        public Vector2 direction = Vector2.up;
        public float thrust;
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
    public GameObject engineEffect;

    private FutureRigidBody2D futureRigidBody2D;
    public TrajectoryProvider trajectoryProvider;

    private void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        hasVirtualStep = true;
    }

    public GameObject CreateUI(int step, TrajectoryMarker marker)
    {
        var isNewConfig = !stepsConfig.ContainsKey(step);
        var stepConfig = GetOrCreateStepConfig(step, marker);
        if (isNewConfig)
        {
            stepConfig.direction = marker.transform.rotation * Vector3.up;
        }

        var ui = Instantiate(UIPrefab);
        var thrusterMarker = GetOrCreateMarkerCustomizer(marker);
        thrusterMarker.SetConfig(stepsConfig[step], this);
        stepsConfig[step] = stepConfig;
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
        if (stepConfig.marker != null)
        {
            GetOrCreateMarkerCustomizer(stepConfig.marker).SetConfig(stepConfig, this);
        }
        stepConfig = newConfig;

        return stepConfig;
    }

    private ThrusterMarker GetOrCreateMarkerCustomizer(TrajectoryMarker marker)
    {
        var tm = marker.GetComponentInChildren<ThrusterMarker>();
        if (tm != null) return tm;
        var instantiated = Instantiate(markerCustomizer.gameObject, marker.transform);
        
        instantiated.transform.localPosition = Vector3.zero;
        instantiated.transform.localRotation = Quaternion.identity;
        instantiated.transform.localScale = Vector3.one;
        var thrusterMarker = instantiated.GetComponent<ThrusterMarker>();

        return thrusterMarker;
    }

    public void OnThrustChanged(int step, ThrusterMarker marker, float value)
    {
        stepsConfig[step].thrust = value;
        FuturePhysics.Reset(step - 1, gameObject);
    }

    public void OnDurationChanged(int step, ThrusterMarker marker, int value)
    {
        ChangeDuration(step, marker, value);
        if (stepsConfig[step].thrust != 0)
        {
            FuturePhysics.Reset(step - 1, gameObject);
        }
    }

    private void ChangeDuration(int step, ThrusterMarker marker, int value)
    {
        for (var i = step + value; i < stepsConfig[step].steps + step; i++) stepsConfig.Remove(i);

        for (var i = stepsConfig[step].steps; i < value; i++)
            stepsConfig[step + i] = stepsConfig[step];
        stepsConfig[step].steps = value;
    }

    public void OnDirectionChanged(int step, ThrusterMarker marker, Vector2 direction)
    {
        ChangeDirection(step, marker, direction);
        if (stepsConfig[step].thrust != 0)
        {
            FuturePhysics.Reset(step - 1, gameObject);
        }
    }
    
    private void ChangeDirection(int step, ThrusterMarker marker, Vector2 direction)
    {
        stepsConfig[step].direction = direction;
    }

    protected override void VirtualStep(int step)
    {
        if (!stepsConfig.ContainsKey(step)) return;
        var currentConfig = stepsConfig[step];
        futureRigidBody2D.AddForce(step, currentConfig.direction * currentConfig.thrust);
    }

    private void Update()
    { 
        stepsConfig.TryGetValue(FuturePhysics.currentStep, out var config);
        if (config  == null)
        {
            engineEffect.SetActive(false);
            return;
        }
        engineEffect.SetActive(true);
        engineEffect.transform.localScale = Vector3.one * config.thrust / maxAcceleration;
    }

    public void DestroyMarker(TrajectoryMarker marker)
    {
        var step = marker.step;
        if (!stepsConfig.ContainsKey(step))
        {
            return;
        }
        var maxStep = stepsConfig[step].steps + step;
        for (var i = step; i < maxStep; i++) stepsConfig.Remove(i);
        
        if (FuturePhysics.currentStep < step)
        {
            FuturePhysics.Reset(step - 1, gameObject);
        }
        Destroy(marker.gameObject);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        if (cause != myGameObject)
            return;
        foreach (var config in stepsConfig.Values.Where(config => config.initialStep > step+1).Distinct().ToArray())
        {
            DestroyMarker(config.marker);
        }
    }
}