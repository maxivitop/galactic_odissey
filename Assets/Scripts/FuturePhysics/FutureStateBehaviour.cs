using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class FutureStateBehaviour<TState> : FutureBehaviour where TState : IEvolving<TState>
{
    private List<TState> states = new();
    private object @lock = new();

    protected abstract TState GetInitialState();

    public void SetState(int step, TState state)
    {
        lock (@lock)
        {
            if (states.Count == step)
                states.Add(state);
            else if (states.Count < step)
                Debug.LogError("Setting state for step " + step + " when max step is " +
                               states.Count);
            else
                states[step] = state;
        }
    }

    public TState GetState(int step)
    {
        lock (@lock)
        {
            if (step == 0) SetState(0, GetInitialState());
            if (states.Count < step)
                Debug.LogError("Getting state for step " + step + " when max step is " +
                               states.Count);
            else if (states.Count == step) SetState(step, states[states.Count - 1].Next());
            return states[step];
        }
    }

    public IEnumerable<TState> GetFutureStates()
    {
        lock (@lock)
        {
            return states.Skip(FuturePhysics.currentStep).ToArray();
        }
    }

    public override void Reset(int step)
    {
        lock (@lock)
        {
            if (states.Count <= step) return;
            states.RemoveRange(step, states.Count - step);
        }
    }
}