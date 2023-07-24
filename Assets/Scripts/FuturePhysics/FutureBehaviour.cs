using System;
using UnityEngine;


public class FutureBehaviour : MonoBehaviour, IFutureObject
{
    private const int NotDisabled = -1;
    private int disablingSourceStep;
    private int disabledFromStep = NotDisabled;
    [NonSerialized]
    public string myName;

    protected int myLastVirtualStep = -1;
    private GameObject myGameObject;
    
    public virtual void ResetToStep(int step, GameObject cause)
    {
        if (cause != myGameObject) return;
        myLastVirtualStep = step-1;
        if (disablingSourceStep < step) return;
        disablingSourceStep = 0;
        disabledFromStep = NotDisabled;
    }

    public virtual void Step(int step)
    {
    }

    public virtual bool CatchUpWithVirtualStep(int virtualStep)
    {
        if (myLastVirtualStep >= virtualStep) return true;
        if (!IsAlive(myLastVirtualStep)) return true;
        myLastVirtualStep++;
        VirtualStep(myLastVirtualStep);
        return myLastVirtualStep >= virtualStep;
    }

    public virtual void VirtualStep(int step)
    {
    }

    protected virtual void OnEnable()
    {
        Register();
        myGameObject = gameObject;
        myName = ToString();
    }

    protected virtual void OnDisable()
    {
        FuturePhysicsRunner.ExecuteOnUpdate(Unregister);
    }

    protected virtual void Register()
    {
        FuturePhysics.AddObject(GetType(), this);
    }

    protected virtual void Unregister()
    {
        FuturePhysics.RemoveObject(this);
    }

    public virtual bool IsAlive(int step)
    {
        return disabledFromStep == NotDisabled || disabledFromStep > step;
    }

    public virtual void Disable(int disablingSourceStep, int disabledFromStep)
    {
        if (this.disabledFromStep != NotDisabled && this.disabledFromStep < disabledFromStep)
            return;
        this.disablingSourceStep = disablingSourceStep;
        this.disabledFromStep = disabledFromStep;
    }
}