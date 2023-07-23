using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(FutureTransform))]
public class TrajectoryProvider : FutureBehaviour
{
    [NonSerialized] public CapacityArray<Vector3> trajectory = new(FuturePhysics.MaxSteps);

    public readonly SingleEvent<CapacityArray<Vector3>> onTrajectoryUpdated = new();

    private static int trajectoryStartStep;

    private CapacityArray<Vector3> absoluteTrajectory = new(FuturePhysics.MaxSteps);
    private FutureTransform futureTransform;
    private ReferenceFrameHost referenceFrameHost;
    public float animationDuration = 0.3f;
    public int minStepsAfterReset = 1000;

    private bool hasReset;
    private bool shouldRenderUnfinished;
    private int minVirtualStepToRecalculateTrajectory;
    private bool updatedThisFrame;
    private TrajectoryAnimator animator;
    private int lastValidAbsoluteStep;
    private int lastValidStep;
    private bool movedAbsTrajThisFrame = false;

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
        lastValidStep = 0;
    }

    private void UpdateAbsoluteTrajectory(int step)
    {
        var lastStep = Math.Max(trajectoryStartStep, lastValidAbsoluteStep) - 1;
        for (var i = lastStep + 1; i < step && IsAlive(i); i++)
        {
            futureTransform.GetState(i).position.SetToVector3(
                ref absoluteTrajectory.array[i - trajectoryStartStep]
            );
            lastStep = i;
        }

        absoluteTrajectory.size = lastStep - trajectoryStartStep + 1;
        lastValidAbsoluteStep = lastStep;
    }

    private void UpdateTrajectory(int step)
    {
        trajectoryStartStep = FuturePhysics.currentStep + FuturePhysicsRunner.stepsNextFrame;
        UpdateAbsoluteTrajectory(step);
        if (referenceFrameHost != ReferenceFrameHost.ReferenceFrame)
        {
            ReferenceFrameHost.ReferenceFrame.trajectoryProvider.UpdateTrajectoryIfNeeded(step);
        }
        
        var frameOfReferenceTrajectory =
            ReferenceFrameHost.ReferenceFrame.trajectoryProvider.absoluteTrajectory;
        trajectory.size = Math.Min(absoluteTrajectory.size, frameOfReferenceTrajectory.size);
        
        var referencePos = ReferenceFrameHost.ReferenceFrame.futureTransform
            .GetState(trajectoryStartStep).position.ToVector3();
        var updateStartTrajStep = Math.Max(0, PhysicsStepToTrajectoryStep(lastValidStep));
        for (var i = updateStartTrajStep; i < trajectory.size; i++)
        {
            trajectory.array[i] = absoluteTrajectory.array[i] -
                frameOfReferenceTrajectory.array[i] + referencePos;
        }

        if (animator.Animate(trajectory))
        {
            lastValidStep = 0;
        }
        else
        {
            lastValidStep = trajectoryStartStep + trajectory.size;
        }
    }

    private void UpdateTrajectoryIfNeeded(int step)
    {
        if (updatedThisFrame) return;
        if (FuturePhysicsRunner.stepsNextFrame > 0 && !movedAbsTrajThisFrame)
        {
            lastValidStep = 0; // reference frame probably moved
            absoluteTrajectory.MoveStart(FuturePhysicsRunner.stepsNextFrame);
            absoluteTrajectory.Normalize();
            movedAbsTrajThisFrame = true;
        }
        if (hasReset && step < minVirtualStepToRecalculateTrajectory)
            return; // reduce flickering
        if (!shouldRenderUnfinished && hasReset) return;
        updatedThisFrame = true;

        UpdateTrajectory(step);
        onTrajectoryUpdated.Invoke(trajectory);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        hasReset = true;
        if (cause == gameObject)
        {
            lastValidStep = 0;
            lastValidAbsoluteStep = 0;
        }

        shouldRenderUnfinished = cause == gameObject;
        minVirtualStepToRecalculateTrajectory = minStepsAfterReset + step;
    }

    private void Update()
    {
        animator.ForwardTime(Time.deltaTime);
        updatedThisFrame = false;
        movedAbsTrajThisFrame = false;
    }

    private void OnDestroy()
    {
        FuturePhysicsRunner.onBgThreadIdle.AddListener(OnBgThreadIdle);
        FuturePhysics.afterVirtualStep.AddListener(UpdateTrajectoryIfNeeded);
        ReferenceFrameHost.referenceFrameChangeOld.AddListener(OnReferenceFrameChange);
    }
}