using UnityEngine;

public class Vector3Animator
{
    private Vector3 initialValue;
    private readonly float animationDuration;
    private float animationTime;

    public Vector3Animator(float animationDuration)
    {
        this.animationDuration = animationDuration;
    }

    public void Capture(Vector3 value)
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

    public Vector3 AnimateTowards(Vector3 targetValue)
    {
        if (animationTime > animationDuration) return targetValue;
        var animationFraction = animationTime / animationDuration;
        return initialValue + (targetValue - initialValue) * animationFraction;
    }

}