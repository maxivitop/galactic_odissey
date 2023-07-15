using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(FutureTransform))]
public class TrajectoryProvider : FutureBehaviour
{
    [NonSerialized]
    public CapacityArray<Vector3> trajectory = new(FuturePhysics.MaxSteps);

    public readonly SingleEvent<CapacityArray<Vector3>> onTrajectoryUpdated = new();
    
    private static int trajectoryStartStep;
    
    private CapacityArray<Vector3> referenceFrameTrajectory = new(FuturePhysics.MaxSteps);
    private FutureTransform futureTransform;
    private ReferenceFrameHost referenceFrameHost;
    public float animationDuration = 0.3f;
    public int minStepsAfterReset = 1000;

    private bool hasReset;
    private bool shouldRenderUnfinished;
    private int minVirtualStepToRecalculateTrajectory;
    private bool updatedThisFrame;
    private bool updatedReferenceThisFrame;
    private TrajectoryAnimator animator;

    public static int TrajectoryStepToPhysicsStep(int trajectoryStep)
    {
        return trajectoryStep + trajectoryStartStep;
    }

    public static int PhysicsStepToTrajectoryStep(int physicsStep)
    {
        return physicsStep - trajectoryStartStep;
    }

    private void Start()
    {
        referenceFrameHost = GetComponent<ReferenceFrameHost>();
        futureTransform = GetComponent<FutureTransform>();
        animator = new TrajectoryAnimator(animationDuration);

        FuturePhysicsRunner.onBgThreadIdle.AddListener(OnBgThreadIdle);
        FuturePhysics.afterVirtualStep.AddListener(UpdateTrajectoryIfNeeded);
        ReferenceFrameHost.referenceFrameChangeOld.AddListener(OnReferenceFrameChange);
    }

    private void OnBgThreadIdle(int step)
    {
        hasReset = false;
        shouldRenderUnfinished = false;
        UpdateTrajectoryIfNeeded(step);
    }

    private void OnReferenceFrameChange(ReferenceFrameHost old)
    {
        animator.Capture(trajectory);
    }

    private void UpdateReferenceTrajectoryIfNeeded(int step)
    {
        if (updatedReferenceThisFrame) return;
        if (referenceFrameHost != ReferenceFrameHost.ReferenceFrame)
        {
            ReferenceFrameHost.ReferenceFrame.trajectoryProvider
                .UpdateReferenceTrajectoryIfNeeded(step);
            return;
        }

        UpdateTrajectoryToArray(step, referenceFrameTrajectory);
        updatedReferenceThisFrame = true;
    }

    private void UpdateTrajectoryToArray(int step, CapacityArray<Vector3> array)
    {
        var nextStep = trajectoryStartStep;
        var lastStep = nextStep-1;
        for (var i = nextStep; i < step && IsAlive(i); i++)
        {
            array.array[i - nextStep] = futureTransform.GetState(i).position.ToVector3();
            lastStep = i;
        }

        array.size = lastStep - nextStep + 1;
    }

    private void UpdateTrajectory(int step)
    {
        trajectoryStartStep = FuturePhysics.currentStep + FuturePhysicsRunner.stepsNextFrame;
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
                .GetState(trajectoryStartStep).position.ToVector3();
        for (var i = 0; i < frameOfReferenceTrajectory.size && i < trajectory.size; i++)
            trajectory.array[i] -= frameOfReferenceTrajectory.array[i] - referencePos;
        for (var i = frameOfReferenceTrajectory.size; i < trajectory.size; i++)
            trajectory.array[i] = trajectory.array[frameOfReferenceTrajectory.size - 1];
        animator.Animate(trajectory);
    }

    private void UpdateTrajectoryIfNeeded(int step)
    {
        if (hasReset && step < minVirtualStepToRecalculateTrajectory) return; // reduce flickering
        if (!shouldRenderUnfinished && hasReset) return;
        if (updatedThisFrame) return;
        updatedThisFrame = true;
        UpdateTrajectory(step);
        onTrajectoryUpdated.Invoke(trajectory);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        hasReset = true;
        shouldRenderUnfinished = cause == gameObject;
        minVirtualStepToRecalculateTrajectory = minStepsAfterReset + step;
    }

    private void Update()
    {
        animator.ForwardTime(Time.deltaTime);
        updatedThisFrame = false;
        updatedReferenceThisFrame = false;
    }

    private void OnDestroy()
    {
        FuturePhysicsRunner.onBgThreadIdle.AddListener(OnBgThreadIdle);
        FuturePhysics.afterVirtualStep.AddListener(UpdateTrajectoryIfNeeded);
        ReferenceFrameHost.referenceFrameChangeOld.AddListener(OnReferenceFrameChange);
    }
}