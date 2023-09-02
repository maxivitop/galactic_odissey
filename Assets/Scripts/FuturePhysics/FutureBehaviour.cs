using System;
using UnityEngine;


public class FutureBehaviour : MonoBehaviour, IFutureObject
{
    private const int NotDisabled = int.MaxValue;
    [NonSerialized] public int disabledFromStep = NotDisabled;
    [NonSerialized] protected int startStep;
    public virtual int StartStep
    {
        get => startStep;
        set
        {
            startStep = value;
            myLastVirtualStep = value;
        }
    }

    [NonSerialized] public string myName;
    [NonSerialized] public int myLastVirtualStep;
    [NonSerialized] protected bool isMyLastVirtualStepFinished;
    [NonSerialized] public GameObject myGameObject;
    
    public virtual void ResetToStep(int step, GameObject cause)
    {
        if (cause != myGameObject) return;
        myLastVirtualStep = Mathf.Max(step, startStep);
        isMyLastVirtualStepFinished = false;
        if (disabledFromStep < step) return;
        disabledFromStep = NotDisabled;
    }

    public virtual void Step(int step)
    {
    }

    public virtual bool CatchUpWithVirtualStep(int virtualStep)
    {
        if (myLastVirtualStep > virtualStep) return true;
        if (startStep > virtualStep) return true;
        if (myLastVirtualStep == virtualStep && isMyLastVirtualStepFinished) return true;
        if (isMyLastVirtualStepFinished)
        {
            myLastVirtualStep++;
        }
        if (!IsAlive(myLastVirtualStep)) return true;
        isMyLastVirtualStepFinished = true;
        VirtualStep(myLastVirtualStep);
        return myLastVirtualStep == virtualStep && isMyLastVirtualStepFinished;
    }

    protected virtual void VirtualStep(int step)
    {
    }

    protected virtual void OnEnable()
    {
        myLastVirtualStep = startStep;
        isMyLastVirtualStepFinished = false;
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
        return startStep <= step && (disabledFromStep == NotDisabled || disabledFromStep > step);
    }
 
    public virtual void Disable(int disabledFromStep)
    {
        this.disabledFromStep = disabledFromStep;
    }

    public virtual int RequiredVirtualStepForStep(int step)
    {
        return step;
    }
}