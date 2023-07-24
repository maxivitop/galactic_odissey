
using System;
using UnityEngine;

[RequireComponent(typeof(FutureTransform))]
public class FutureCollider : FutureBehaviour
{
    [NonSerialized]
    public FutureTransform futureTransform;
    [NonSerialized] 
    public bool isPassive = true;

    private ICollisionEnterHandler[] collisionEnterHandlers;

    protected override void Register()
    {
        base.Register();
        FuturePhysics.AddObject(typeof(FutureCollider), this);
        collisionEnterHandlers = GetComponents<ICollisionEnterHandler>();
        futureTransform = GetComponent<FutureTransform>();
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.StepCollisionEnter(step, collision);
        }
    }
    
    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.VirtualStepCollisionEnter(step, collision);
        }
    }
}
