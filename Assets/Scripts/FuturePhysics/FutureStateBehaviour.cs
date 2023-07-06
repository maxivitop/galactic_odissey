using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract class FutureStateBehaviour<State>: FutureBehaviour where State: Evolving<State>
{

    private List<State> states = new();
    private object LOCK = new object();

    protected abstract State GetInitialState();

    public void SetState(int step, State state)
    {
        lock(LOCK)
        {
            if (states.Count == step)
            {
                states.Add(state);
            }
            else if (states.Count < step)
            {
                Debug.LogError("Setting state for step " + step + " when max step is " + states.Count);
            }
            else
            {
                states[step] = state;
            }
        }
    }
    public State GetState(int step)
    {
        lock (LOCK)
        {
            if (step == 0)
            {
                SetState(0, GetInitialState());
            }
            if (states.Count < step)
            {
                Debug.LogError("Getting state for step " + step + " when max step is " + states.Count);
            }
            else if (states.Count == step)
            {
                SetState(step, states[states.Count - 1].next());
            }
            return states[step];
        }
    }

    public IEnumerable<State> GetFutureStates()
    {
        lock (LOCK)
        {
            return states.Skip(FuturePhysics.currentStep).ToArray();
        }
    }
    public override void Reset(int step)
    {
        lock(LOCK)
        {
            if (states.Count <= step)
            {
                return;
            }
            states.RemoveRange(step, states.Count - step);
        }
    }
}
