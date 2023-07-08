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

    private void Awake()
    {
        FuturePhysics.AddObject(GetType(), this);
    }

    private void OnDestroy()
    {
        FuturePhysicsRunner.ExecuteOnUpdate(() =>
        {
            FuturePhysics.RemoveObject(GetType(), this);
        });
    }
}