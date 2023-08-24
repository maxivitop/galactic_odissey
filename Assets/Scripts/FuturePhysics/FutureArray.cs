using System;
using UnityEngine;

public class FutureArray<T>
{
    private CapacityArray<T> capacityArray = new(FuturePhysics.MaxSteps + 1);
    private string name;
    private int startStep;
    private T initialValue;

    public T this[int step]
    {
        get
        {
            if (step <= startStep + 1 && capacityArray.size == 0)
            {
                capacityArray.size = startStep;
                this[startStep] = initialValue;
            }
#if DEBUG
            FuturePhysicsRunner.CheckThread();
            if (capacityArray.size < step)
                Debug.LogError(name + ": Getting state for step " + step + " when max step is " +
                               capacityArray.size +". Virtual step is " + FuturePhysics.lastVirtualStep);
#endif
            if (capacityArray.size == step) this[step] = this[capacityArray.size - 1];
            return capacityArray[step];
        }
        set
        {
            if (capacityArray.size == step)
                capacityArray.Add(value);
            else if (capacityArray.size < step)
                Debug.LogError("Setting state for step " + step + " when max step is " +
                               capacityArray.size);
            else
                capacityArray[step] = value;
        }
    }

    public void Initialize(int startStep, T value, string logName)
    {
        this.startStep = startStep;
        initialValue = value;
        name = logName;
    }
    
    public void ResetToStep(int step)
    {
        FuturePhysicsRunner.CheckThread();
        if (capacityArray.size <= step) return;
        capacityArray.size = step;
    }
}