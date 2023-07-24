using System;
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

    public bool Animate(CapacityArray<Vector3> targetValue)
    {
        if (animationTime <= 0f || animationTime > animationDuration) return false;
        if (initialValue.size == 0) return false;
        var animationFraction = animationTime / animationDuration;
        targetValue.size = Math.Min(targetValue.size, initialValue.size);
        for (var i = 0; i < targetValue.size; i++)
        {
            targetValue.array[i] = initialValue.array[i] +
                                   (targetValue.array[i] - initialValue.array[i]) *
                                   animationFraction;
        }

        return true;
    }

}