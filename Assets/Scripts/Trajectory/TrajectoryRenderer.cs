using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(FutureTransform))]
public class TrajectoryRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private FutureTransform futureTransform;
    public float animtionDuration = 0.3f;
    public int minStepsDuringAnimation = 1000;

    private bool hasReset;
    private bool shouldRenderUnfinished = false;
    public Vector3[] trajectory = Array.Empty<Vector3>();
    private Gradient initialGradient;
    private readonly Gradient gradient = new();
    private int minTrajectoryStepsToRender;
    private int lastResetStep;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        initialGradient = lineRenderer.colorGradient;
        futureTransform = GetComponent<FutureTransform>();
        FuturePhysics.beforeReset.AddListener(resetParams =>
        {
            lastResetStep = resetParams.step;
            hasReset = true;
            shouldRenderUnfinished = resetParams.cause == gameObject;
            minTrajectoryStepsToRender = minStepsDuringAnimation + resetParams.step;
        });
    }

    private void Update()
    {
        trajectory = GetRenderedTrajectory();
        lineRenderer.positionCount = trajectory.Length;
        lineRenderer.SetPositions(trajectory);
        UpdateTrajectoryColor();
    }


    private Vector3[] GetRenderedTrajectory()
    {
        var actualTrajectory = GetTrajectory();
        if (actualTrajectory.Length >= FuturePhysics.MaxSteps - 1)
        {
            hasReset = false;
            shouldRenderUnfinished = false;
        }

        if (hasReset &&
            actualTrajectory.Length <
            minTrajectoryStepsToRender) return trajectory; // reduce flickering
        if (!shouldRenderUnfinished && hasReset) return trajectory;
        return actualTrajectory;
    }

    private Vector3[] GetTrajectory()
    {
        var myTrajectory = futureTransform.GetFutureStates().Select(
            state => state.position).ToArray();
        var frameOfReferenceTrajectory = ReferenceFrameHost.ReferenceFrame.futureTransform
            .GetFutureStates().Select(
                state => state.position).ToArray();
        if (myTrajectory.Length == 0) return myTrajectory;
        if (frameOfReferenceTrajectory.Length == 0)
            return frameOfReferenceTrajectory; // no trajectory available
        var referencePos = ReferenceFrameHost.ReferenceFrame.transform.position;
        for (var i = 0; i < frameOfReferenceTrajectory.Length && i < myTrajectory.Length; i++)
            myTrajectory[i] -= frameOfReferenceTrajectory[i] - referencePos;
        for (var i = frameOfReferenceTrajectory.Length; i < myTrajectory.Length; i++)
            myTrajectory[i] = myTrajectory[frameOfReferenceTrajectory.Length - 1];
        return myTrajectory;
    }


    private void UpdateTrajectoryColor()
    {
        if (trajectory.Length == 0) return;
        var colorKeys = (from colorKey in initialGradient.colorKeys
            let trajectoryPercent = CalculateTrajectoryPercent(colorKey.time)
            where trajectoryPercent.HasValue
            select new GradientColorKey(colorKey.color, trajectoryPercent.Value)).ToList();

        var alphaKeys = (from alphaKey in initialGradient.alphaKeys
            let trajectoryPercent = CalculateTrajectoryPercent(alphaKey.time)
            where trajectoryPercent.HasValue
            select new GradientAlphaKey(alphaKey.alpha, trajectoryPercent.Value)).ToList();

        var lastPosition = (float)trajectory.Length / FuturePhysics.MaxSteps;
        var lastColor = initialGradient.Evaluate(lastPosition);
        colorKeys.Add(new GradientColorKey(lastColor, 1f));

        var resetTrajectoryPercent = (float)lastResetStep / trajectory.Length;
        var resetColor = initialGradient.Evaluate((float)lastResetStep / FuturePhysics.MaxSteps);
        colorKeys.Add(new GradientColorKey(resetColor, resetTrajectoryPercent));
        alphaKeys.Add(new GradientAlphaKey(resetColor.a, resetTrajectoryPercent));

        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        lineRenderer.colorGradient = gradient;
    }

    private float? CalculateTrajectoryPercent(float fullTrajectoryPercent)
    {
        var stepForKey = Mathf.RoundToInt(FuturePhysics.MaxSteps * fullTrajectoryPercent);
        if (stepForKey > trajectory.Length) return null;
        return (float)stepForKey / trajectory.Length;
    }
}