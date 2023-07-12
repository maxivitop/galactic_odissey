using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FutureBehaviour : MonoBehaviour, IFutureState
{
    // ReSharper disable once Unity.IncorrectMethodSignature
    public virtual void ResetToStep(int step, GameObject cause)
    {
    }

    public virtual void Step(int step)
    {
    }

    public virtual void VirtualStep(int step)
    {
    }

    protected virtual void OnEnable()
    {
        FuturePhysics.AddObject(GetType(), this);
    }

    protected virtual void OnDisable()
    {
        FuturePhysicsRunner.ExecuteOnUpdate(() =>
        {
            FuturePhysics.RemoveObject(GetType(), this);
        });
    }
}