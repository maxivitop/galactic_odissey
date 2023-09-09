using UnityEngine;

public class FutureArray<T>
{
    public readonly CapacityArray<T> capacityArray = new(FuturePhysics.MaxSteps * 2);
    private string name;
    private int startStep;
    private T initialValue;
    public T defaultValue;
    public bool useDefaultValue;

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
            if (capacityArray.size < step)
                Debug.LogError(name + ": Getting state for step " + step + " when max step is " +
                               capacityArray.size + ". Virtual step is " +
                               FuturePhysics.lastVirtualStep +
                    ", start step = " +startStep);
#endif
            if (capacityArray.size == step)
                this[step] = useDefaultValue ? defaultValue : this[capacityArray.size - 1];
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

    public void Initialize(int startStep, T[] values, int trajectoryLength, string logName)
    {
        this.startStep = startStep;
        initialValue = values[0];
        capacityArray.InitializeFrom(startStep, values, trajectoryLength);
        name = logName;
    }

    public void ResetToStep(int step)
    {
        FuturePhysicsRunner.CheckThread();
        if (capacityArray.size <= step) return;
        capacityArray.size = step;
    }
}