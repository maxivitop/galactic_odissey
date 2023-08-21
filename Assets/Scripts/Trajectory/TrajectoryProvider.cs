using System;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
public class TrajectoryProvider : FutureBehaviour
{
    [NonSerialized] public CapacityArray<Vector3> trajectory = new(FuturePhysics.MaxSteps+1);

    public readonly SingleEvent<CapacityArray<Vector3>> onTrajectoryUpdated = new();

    public static int trajectoryStartStep;

    public CapacityArray<Vector3> absoluteTrajectory = new(FuturePhysics.MaxSteps+1);
    private FutureTransform futureTransform;
    private ReferenceFrameHost referenceFrameHost;
    public float animationDuration = 0.3f;
    public int minStepsAfterReset = 1000;

    private int minVirtualStepToRecalculateTrajectory;
    private bool updatedThisFrame;
    private TrajectoryAnimator animator;
    private int lastValidAbsoluteStep = -1;
    private int lastValidStep = -1;
    private bool movedAbsTrajThisFrame;
    private bool hasNotFinishedTraj = true;

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

        ReferenceFrameHost.referenceFrameChangeOld.AddListener(OnReferenceFrameChange);
    }

    private void OnReferenceFrameChange(ReferenceFrameHost old)
    {
        animator.Capture(trajectory);
        lastValidStep = -1;
    }

    private void UpdateAbsoluteTrajectory(int step)
    {
        var lastStep = trajectoryStartStep - 1;
        for (var i = Math.Max(trajectoryStartStep, lastValidAbsoluteStep);
             i < step && IsAlive(i);
             i++)
        {
            futureTransform.GetFuturePosition(i).SetToVector3(
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
            .GetFuturePosition(trajectoryStartStep).ToVector3();
        var updateStartTrajStep = Math.Max(0, PhysicsStepToTrajectoryStep(lastValidStep));
        for (var i = updateStartTrajStep; i < trajectory.size; i++)
        {
            trajectory.array[i] = absoluteTrajectory.array[i] -
                frameOfReferenceTrajectory.array[i] + referencePos;
        }

        if (animator.Animate(trajectory))
        {
            lastValidStep = -1;
        }
        else
        {
            lastValidStep = trajectoryStartStep + trajectory.size;
        }
    }

    private void UpdateTrajectoryIfNeeded(int step)
    {
        if (updatedThisFrame) return;
        if (step < minVirtualStepToRecalculateTrajectory && hasNotFinishedTraj) // reduce flickering
        {
            if (movedAbsTrajThisFrame) return;
            trajectory.MoveStart(FuturePhysicsRunner.stepsNextFrame);
            trajectory.Normalize();
            absoluteTrajectory.MoveStart(FuturePhysicsRunner.stepsNextFrame);
            absoluteTrajectory.Normalize();
            movedAbsTrajThisFrame = true;
            return;
        }

        updatedThisFrame = true;
        if (FuturePhysicsRunner.stepsNextFrame > 0 && !movedAbsTrajThisFrame)
        {
            lastValidStep = -1; // reference frame probably moved
            absoluteTrajectory.MoveStart(FuturePhysicsRunner.stepsNextFrame);
            absoluteTrajectory.Normalize();
            movedAbsTrajThisFrame = true;
        }

        UpdateTrajectory(step);
        onTrajectoryUpdated.Invoke(trajectory);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        base.ResetToStep(step, cause);
        if (cause == gameObject)
        {
            lastValidStep = -1;
            lastValidAbsoluteStep = -1;
            hasNotFinishedTraj = true;
        }

        minVirtualStepToRecalculateTrajectory = minStepsAfterReset + step;
    }

    public override bool CatchUpWithVirtualStep(int virtualStep)
    {
        if (virtualStep - FuturePhysics.currentStep >= FuturePhysics.MaxSteps && FuturePhysics.upToDateWithLastStep)
        {
            hasNotFinishedTraj = false;
        }
        UpdateTrajectoryIfNeeded(myLastVirtualStep);
        if (myLastVirtualStep >= virtualStep - 1) return true;
        myLastVirtualStep++;
        return true;
    }

    private void Update()
    {
        animator.ForwardTime(Time.deltaTime);
        updatedThisFrame = false;
        movedAbsTrajThisFrame = false;
    }

    private void OnDestroy()
    {
        ReferenceFrameHost.referenceFrameChangeOld.RemoveListener(OnReferenceFrameChange);
    }
}