using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class FutureStateBehaviour<TState> : FutureBehaviour where TState : IEvolving<TState>
{
    private readonly CyclicArray<TState> states = new(FuturePhysics.MaxSteps+1);

    protected abstract TState GetInitialState();

    private void SetState(int step, TState state)
    {
        if (states.Count == step)
            states.Add(state);
        else if (states.Count < step)
            Debug.LogError("Setting state for step " + step + " when max step is " +
                           states.Count);
        else
            states[step] = state;
    }

    public TState GetState(int step)
    {
#if DEBUG
        FuturePhysicsRunner.CheckThread();
        if (states.Count < step)
            Debug.LogError("Getting state for step " + step + " when max step is " +
                           states.Count +". Virtual step is " + FuturePhysics.lastVirtualStep);
#endif
        if (step == 0) SetState(0, GetInitialState());
        if (states.Count == step) SetState(step, states[states.Count - 1].Next());
        return states[step];
    }

    public override void ResetToStep(int step, GameObject cause)
    {
        FuturePhysicsRunner.CheckThread();
        base.ResetToStep(step, cause);

        if (states.Count <= step) return;
        states.ReduceSizeTo(step);
    }
}