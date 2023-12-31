using UnityEngine;

public interface IFutureObject
{
    public void Step(int step);

    /**
     * Returns true if caught up.
     */
    public bool CatchUpWithVirtualStep(int virtualStep);

    public void ResetToStep(int step, GameObject cause);

    public bool IsAlive(int step);
}