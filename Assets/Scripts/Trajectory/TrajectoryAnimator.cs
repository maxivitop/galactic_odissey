using System;
using System.Threading.Tasks;
using UnityEngine;

public class TrajectoryAnimator
{
    private readonly CapacityArray<Vector3> initialValue = new(FuturePhysics.MaxSteps+1);
    private readonly float animationDuration;
    private float animationTime;

    public TrajectoryAnimator(float animationDuration)
    {
        this.animationDuration = animationDuration;
    }

    public void Capture(CapacityArray<Vector3> value)
    {
       initialValue.CopyIntoSelf(value);
       animationTime = 0f;
    }

    public void ForwardTime(float time)
    {
        animationTime += time;
    }

    public bool IsRunning()
    {
        return animationTime > 0f && animationTime < animationDuration;
    }

    public void Animate(CapacityArray<Vector3> targetValue)
    {
        if (!IsRunning() || initialValue.size == 0) return;
        var animationFraction = animationTime / animationDuration;
        targetValue.size = Math.Min(targetValue.size, initialValue.size);
        Parallel.For(fromInclusive: 0, toExclusive: targetValue.size, i =>
        {
            targetValue.array[i] = initialValue.array[i] +
                                   (targetValue.array[i] - initialValue.array[i]) *
                                   animationFraction;
        });
    }

}