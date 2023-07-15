using System;
using UnityEngine;


public class FutureBehaviour : MonoBehaviour, IFutureObject
{
    private const int NotDisabled = -1;
    private int disablingSourceStep;
    private int disabledFromStep = NotDisabled;
    [NonSerialized]
    public new string name;
    
    public virtual void ResetToStep(int step, GameObject cause)
    {
        if (disablingSourceStep < step) return;
        disablingSourceStep = 0;
        disabledFromStep = NotDisabled;
    }

    public virtual void Step(int step)
    {
    }

    public virtual void VirtualStep(int step)
    {
    }

    protected virtual void OnEnable()
    {
        Register();
        name = ToString();
    }

    protected virtual void OnDisable()
    {
        FuturePhysicsRunner.ExecuteOnUpdate(Unregister);
    }

    public virtual void Register()
    {
        FuturePhysics.AddObject(GetType(), this);
    }

    public virtual void Unregister()
    {
        FuturePhysics.RemoveObject(this);
    }

    public bool IsAlive(int step)
    {
        return !IsObsolete(step);
    }

    public bool IsObsolete(int step)
    {
        return disabledFromStep != NotDisabled && disabledFromStep <= step;
    }

    public virtual void Disable(int disablingSourceStep, int disabledFromStep)
    {
        this.disablingSourceStep = disablingSourceStep;
        this.disabledFromStep = disabledFromStep;
    }
}