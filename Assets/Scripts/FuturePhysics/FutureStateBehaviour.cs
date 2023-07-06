using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class FutureStateBehaviour<TState> : FutureBehaviour where TState : IEvolving<TState>
{
    private readonly List<TState> states = new();

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
#endif
        if (step == 0) SetState(0, GetInitialState());
        if (states.Count < step)
            Debug.LogError("Getting state for step " + step + " when max step is " +
                           states.Count);
        else if (states.Count == step) SetState(step, states[^1].Next());
        return states[step];
    }

    public IEnumerable<TState> GetFutureStates()
    {
        FuturePhysicsRunner.CheckThread();
        return states.Skip(FuturePhysics.currentStep).ToArray();
    }

    public override void Reset(int step)
    {
        FuturePhysicsRunner.CheckThread();

        if (states.Count <= step) return;
        states.RemoveRange(step, states.Count - step);
    }
}