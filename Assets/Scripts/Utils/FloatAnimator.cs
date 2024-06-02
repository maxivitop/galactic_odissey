using UnityEngine;

public class FloatAnimator
{
    private float initialValue;
    private readonly float animationDuration;
    private float animationTime;

    public FloatAnimator(float animationDuration)
    {
        this.animationDuration = animationDuration;
    }

    public void Capture(float value)
    {
       initialValue = value;
       animationTime = 0f;
    }

    public void ForwardTime(float time)
    {
        animationTime += time;
    }

    public bool HasFinished()
    {
        return animationTime >= animationDuration;
    }

    public float AnimateTowards(float targetValue)
    {
        if (animationTime > animationDuration) return targetValue;
        var animationFraction = animationTime / animationDuration;
        return initialValue + (targetValue - initialValue) * animationFraction;
    }

}