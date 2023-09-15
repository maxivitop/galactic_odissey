using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
public class TrajectoryProvider : FutureBehaviour
{
    [NonSerialized] public readonly CapacityArray<Vector3> trajectory = new(FuturePhysics.MaxSteps+1);
    private static int trajectoryStartStep;
    private FutureTransform futureTransform;
    public float animationDuration = 0.3f;

    private TrajectoryAnimator animator;
    private int validAtStep=-1;

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
        futureTransform = GetComponent<FutureTransform>();
        animator = new TrajectoryAnimator(animationDuration);

        ReferenceFrameHost.referenceFrameChangeOld.AddListener(OnReferenceFrameChange);
    }

    private void OnReferenceFrameChange(ReferenceFrameHost old)
    {
        animator.Capture(trajectory);
        validAtStep = -1;
    }

    private void UpdateTrajectory(int step)
    {
        trajectoryStartStep = FuturePhysicsRunner.renderFramePrevStep;
        var frameOfReferenceTransform 
            = ReferenceFrameHost.ReferenceFrame.trajectoryProvider.futureTransform;
        trajectory.size = Mathf.Min(
            step - trajectoryStartStep,
            futureTransform.position.capacityArray.size - trajectoryStartStep,
            frameOfReferenceTransform.position.capacityArray.size - trajectoryStartStep,
            futureTransform.disabledFromStep
        );
        var referencePos = frameOfReferenceTransform.GetFuturePosition(
            FuturePhysicsRunner.renderFramePrevStep, FuturePhysicsRunner.renderFrameStepPart);
        Parallel.For(fromInclusive: 0, toExclusive: trajectory.size, i =>
        {
            var s = trajectoryStartStep + i;
            trajectory[i] = 
                futureTransform.position[s] - frameOfReferenceTransform.position[s] + referencePos;
        });

        animator.Animate(trajectory);
        validAtStep = FuturePhysics.lastVirtualStep;
    }

    private void Update()
    {
        if (animator.IsRunning()) validAtStep = -1;
        animator.ForwardTime(Time.unscaledDeltaTime);
        if (animator.IsRunning()) validAtStep = -1;

        if (validAtStep != FuturePhysics.lastVirtualStep)
        {
            UpdateTrajectory(FuturePhysics.lastVirtualStep);
        }
    }

    private void OnDestroy()
    {
        ReferenceFrameHost.referenceFrameChangeOld.RemoveListener(OnReferenceFrameChange);
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        if (cause == myGameObject)
        {
            validAtStep = -1;
        }
    }
}