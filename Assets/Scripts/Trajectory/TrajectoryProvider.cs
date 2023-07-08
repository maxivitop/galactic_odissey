using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
public class TrajectoryProvider : FutureBehaviour
{
    [NonSerialized]
    public CapacityArray<Vector3> trajectory = new(FuturePhysics.MaxSteps);
    private int trajectoryStartStep;
    
    private CapacityArray<Vector3> referenceFrameTrajectory = new(FuturePhysics.MaxSteps);
    private int referenceFrameTrajectoryStep;

    private FutureTransform futureTransform;
    private ReferenceFrameHost referenceFrameHost;
    public float animationDuration = 0.3f;
    public int minStepsAfterReset = 1000;

    private bool hasReset;
    private bool shouldRenderUnfinished;
    private int minVirtualStepToRecalculateTrajectory;
    private bool updatedThisFrame;

    private void Start()
    {
        referenceFrameHost = GetComponent<ReferenceFrameHost>();
        futureTransform = GetComponent<FutureTransform>();
        StartCoroutine(TriggerUpdate());
        FuturePhysicsRunner.onBgThreadIdle.AddListener(step =>
        {
            hasReset = false;
            shouldRenderUnfinished = false;
            UpdateTrajectoryIfNeeded(step);
        });
    }

    private void UpdateReferenceTrajectoryIfNeeded(int step)
    {
        if (referenceFrameTrajectoryStep == step) return;
        if (referenceFrameHost != ReferenceFrameHost.ReferenceFrame)
        {
            ReferenceFrameHost.ReferenceFrame.trajectoryProvider
                .UpdateReferenceTrajectoryIfNeeded(step);
            return;
        }

        UpdateTrajectoryToArray(step, referenceFrameTrajectory);
        referenceFrameTrajectoryStep = step;
    }

    private void UpdateTrajectoryToArray(int step, CapacityArray<Vector3> array)
    {
        var nextStep = FuturePhysics.currentStep + 1;
        for (var i = nextStep; i <= step; i++)
        {
            array.array[i - nextStep] = futureTransform.GetState(i).position;
        }

        array.size = step - nextStep + 1;
    }

    private void UpdateTrajectory(int step)
    {
        trajectoryStartStep = FuturePhysics.currentStep + 1;
        UpdateTrajectoryToArray(step, trajectory);
        if (trajectory.size == 0) return;
        UpdateReferenceTrajectoryIfNeeded(step);
        var frameOfReferenceTrajectory =
            ReferenceFrameHost.ReferenceFrame.trajectoryProvider.referenceFrameTrajectory;
        if (frameOfReferenceTrajectory.size == 0)
        {
            trajectory.size = 0;
            return;
        }
        var referencePos = ReferenceFrameHost.ReferenceFrame.futureTransform
                .GetState(FuturePhysics.currentStep).position;
        for (var i = 0; i < frameOfReferenceTrajectory.size && i < trajectory.size; i++)
            trajectory.array[i] -= frameOfReferenceTrajectory.array[i] - referencePos;
        for (var i = frameOfReferenceTrajectory.size; i < trajectory.size; i++)
            trajectory.array[i] = trajectory.array[frameOfReferenceTrajectory.size - 1];
    }

    public int TrajectoryStepToPhysicsStep(int trajectoryStep)
    {
        return trajectoryStep + trajectoryStartStep;
    }

    public int PhysicsStepToTrajectoryStep(int physicsStep)
    {
        return physicsStep - trajectoryStartStep;
    }
    
    private void UpdateTrajectoryIfNeeded(int step)
    {
        if (hasReset && step < minVirtualStepToRecalculateTrajectory) return; // reduce flickering
        if (!shouldRenderUnfinished && hasReset) return;
        if (updatedThisFrame) return;
        updatedThisFrame = true;
        UpdateTrajectory(step);
    }
    
    public override void VirtualStep(int step)
    {
        UpdateTrajectoryIfNeeded(step);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        hasReset = true;
        shouldRenderUnfinished = cause == gameObject;
        minVirtualStepToRecalculateTrajectory = minStepsAfterReset + step;
    }

    private IEnumerator TriggerUpdate()
    {
        while (true)
        { 
            updatedThisFrame = false;
            yield return 0;
        }
        // ReSharper disable once IteratorNeverReturns
    }
}