using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(FutureTransform))]
public class TrajectoryRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    FutureTransform futureTransform;
    public float animtionDuration = 0.3f;
    public int minStepsDuringAnimation = 1000;

    private bool hasReset;
    private bool shouldRenderUnfinished = false;
    public Vector3[] trajectory = new Vector3[0];
    private Gradient initialGradient;
    private Gradient gradient = new Gradient();
    private int minTrajectoryStepsToRender;
    private int lastResetStep = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        initialGradient = lineRenderer.colorGradient;
        futureTransform = GetComponent<FutureTransform>();
        FuturePhysics.beforeReset.AddListener((FuturePhysics.ResetParams resetParams) =>
        {
            lastResetStep = resetParams.step;
            hasReset = true;
            shouldRenderUnfinished = resetParams.cause == gameObject;
            minTrajectoryStepsToRender = minStepsDuringAnimation + resetParams.step;
        });
    }

    void Update()
    {
        trajectory = GetRenderedTrajectory();
        lineRenderer.positionCount = trajectory.Length;
        lineRenderer.SetPositions(trajectory);
        UpdateTrajectoryColor();
    }


    Vector3[] GetRenderedTrajectory()
    {
        var actualTrajectory = GetTrajectory();
        if (actualTrajectory.Length >= FuturePhysics.maxSteps - 1)
        {
            hasReset = false;
            shouldRenderUnfinished = false;
        }
        if (hasReset && actualTrajectory.Length < minTrajectoryStepsToRender)
        {
            return trajectory; // reduce flickering
        }
        if (!shouldRenderUnfinished && hasReset)
        {
            return trajectory;
        }
        return actualTrajectory;
    }

    Vector3[] GetTrajectory()
    {
        Vector3[] myTrajectory = futureTransform.GetFutureStates().Select(
                       state => state.position).ToArray();
        Vector3[] frameOfReferenceTrajectory = ReferenceFrameHost.ReferenceFrame.FutureTransform.GetFutureStates().Select(
                       state => state.position).ToArray();
        if (myTrajectory.Length == 0)
        {
            return myTrajectory;
        }
        if (frameOfReferenceTrajectory.Length == 0)
        {
            return frameOfReferenceTrajectory; // no trajectory available
        }
        Vector3 referencePos = ReferenceFrameHost.ReferenceFrame.transform.position;
        for (int i = 0; i < frameOfReferenceTrajectory.Length && i < myTrajectory.Length; i++)
        {
            myTrajectory[i] -= frameOfReferenceTrajectory[i] - referencePos;
        }
        for (int i = frameOfReferenceTrajectory.Length; i < myTrajectory.Length; i++)
        {
            myTrajectory[i] = myTrajectory[frameOfReferenceTrajectory.Length - 1];
        }
        return myTrajectory;
    }


    private void UpdateTrajectoryColor()
    {
        if (trajectory.Length == 0)
        {
            return;
        }
        List<GradientColorKey> colorKeys = new();
        List<GradientAlphaKey> alphaKeys = new();
        foreach (GradientColorKey colorKey in initialGradient.colorKeys)
        {
            float? trajectoryPercent = CalculateTrajectoryPercent(colorKey.time);
            if (trajectoryPercent.HasValue)
            {
                colorKeys.Add(new GradientColorKey(colorKey.color, trajectoryPercent.Value));
            }
        }
        foreach (GradientAlphaKey alphaKey in initialGradient.alphaKeys)
        {
            float? trajectoryPercent = CalculateTrajectoryPercent(alphaKey.time);
            if (trajectoryPercent.HasValue)
            {
                alphaKeys.Add(new GradientAlphaKey(alphaKey.alpha, trajectoryPercent.Value));
            }
        }
        float lastPosition = (float)trajectory.Length / FuturePhysics.maxSteps;
        Color lastColor = initialGradient.Evaluate(lastPosition);
        colorKeys.Add(new GradientColorKey(lastColor, 1f));

        float resetTrajectoryPercent = (float) lastResetStep / trajectory.Length;
        var resetColor = initialGradient.Evaluate((float) lastResetStep / FuturePhysics.maxSteps);
        colorKeys.Add(new GradientColorKey(resetColor, resetTrajectoryPercent));
        alphaKeys.Add(new GradientAlphaKey(resetColor.a, resetTrajectoryPercent));

        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        lineRenderer.colorGradient = gradient;
    }

    private float? CalculateTrajectoryPercent(float fullTrajectoryPercent)
    {
        int stepForKey = Mathf.RoundToInt(FuturePhysics.maxSteps * fullTrajectoryPercent);
        if (stepForKey > trajectory.Length)
        {
            return null;
        }
        return (float)stepForKey / trajectory.Length;
    }
}
