using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TrajectoryProvider))]
[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private TrajectoryProvider trajectoryProvider;
    private Gradient initialGradient;
    private readonly Gradient gradient = new();
    private GradientColorKey[] colorKeys = new GradientColorKey[8];
    private GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];
    
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        initialGradient = lineRenderer.colorGradient;
        trajectoryProvider = GetComponent<TrajectoryProvider>();
    }

    private void Update()
    {
        lineRenderer.positionCount = trajectoryProvider.trajectory.size;
        if (trajectoryProvider.trajectory.start != 0)
        {
            Debug.Log("Normalize in update");
            trajectoryProvider.trajectory.Normalize();
        }
        lineRenderer.SetPositions(trajectoryProvider.trajectory.array);
        UpdateTrajectoryColor();
    }


    private void UpdateTrajectoryColor()
    {
        var trajectorySize = trajectoryProvider.trajectory.size;
        if (trajectorySize == 0) return;
        var trajectorySizeFraction = (float) trajectorySize / FuturePhysics.MaxSteps;
        for (var i = 0; i < colorKeys.Length; i++)
        {
            var keyFraction = (float)i / (colorKeys.Length - 1);
            var evaluatedColor = initialGradient.Evaluate(keyFraction * trajectorySizeFraction);
            colorKeys[i].time = keyFraction;
            colorKeys[i].color = evaluatedColor;
            alphaKeys[i].time = keyFraction;
            alphaKeys[i].alpha = evaluatedColor.a;
        }
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }
}