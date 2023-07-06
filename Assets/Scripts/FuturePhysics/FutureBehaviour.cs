using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FutureBehaviour:  MonoBehaviour, IFutureState
{
    public virtual void Reset(int step){}

    public virtual void Step(int step){}

    public virtual void VirtualStep(int step){}

    void Awake()
    {
        FuturePhysics.AddObject(GetType(), this);
    }
    void OnDestroy()
    {
        FuturePhysics.RemoveObject(GetType(), this);
    }
}
