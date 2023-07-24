using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class FutureStateBehaviour<TState> : FutureBehaviour where TState : IEvolving<TState>
{
    private readonly CapacityArray<TState> states = new(FuturePhysics.MaxSteps+1);

    protected abstract TState GetInitialState();

    private void SetState(int step, TState state)
    {
        if (states.size == step)
            states.Add(state);
        else if (states.size < step)
            Debug.LogError("Setting state for step " + step + " when max step is " +
                           states.size);
        else
            states[step] = state;
    }

    public TState GetState(int step)
    {
        if (step <= 1) SetState(0, GetInitialState());
#if DEBUG
        FuturePhysicsRunner.CheckThread();
        if (states.size < step)
            Debug.LogError("Getting state for step " + step + " when max step is " +
                           states.size +". Virtual step is " + FuturePhysics.lastVirtualStep + " " + myName);
#endif
        if (states.size == step) SetState(step, states[states.size - 1].Next());
        return states[step];
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        FuturePhysicsRunner.CheckThread();
        base.ResetToStep(step, cause);
        if (cause != gameObject) return;
        if (states.size <= step) return;
        states.size = step;
    }
}