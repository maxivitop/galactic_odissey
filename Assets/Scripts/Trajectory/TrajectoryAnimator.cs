using UnityEngine;

public class TrajectoryAnimator
{
    private readonly CapacityArray<Vector3> initialValue = new(FuturePhysics.MaxSteps);
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

    public void Animate(CapacityArray<Vector3> targetValue)
    {
        if (animationTime > animationDuration) return;
        if (initialValue.size == 0) return;
        var animationFraction = animationTime / animationDuration;
        for (var i = 0; i < targetValue.size && i < initialValue.size; i++)
        {
            
            targetValue.array[i] = initialValue.array[i] +
                                   (targetValue.array[i] - initialValue.array[i]) *
                                   animationFraction;
        }

        var lastInitialValue = initialValue.array[initialValue.size - 1];
        for (var i = initialValue.size; i < targetValue.size; i++)
        {
            targetValue.array[i] = lastInitialValue +
                                   (targetValue.array[i] - lastInitialValue) * animationFraction;
        }
    }

}