
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
    
    protected override void OnEnable()
    {
        TryGetComponent<FutureRigidBody2D>(out var futureRigidBody2D);
        collisionEnterHandlers = GetComponents<ICollisionEnterHandler>();
        if (futureRigidBody2D.processCollisions)
        {
            isPassive = false;
        }
        futureTransform = GetComponent<FutureTransform>();
        FuturePhysics.AddObject(typeof(FutureCollider), this);
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        FuturePhysicsRunner.ExecuteOnUpdate(() =>
        {
            FuturePhysics.RemoveObject(typeof(FutureCollider), this);
        });
        base.OnDisable();
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        SwapCollidersIfNeeded(collision);
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.StepCollisionEnter(step, collision);
        }
    }
    
    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        SwapCollidersIfNeeded(collision);
        foreach (var collisionEnterHandler in collisionEnterHandlers)
        {
            collisionEnterHandler.VirtualStepCollisionEnter(step, collision);
        }
    }

    private void SwapCollidersIfNeeded(FutureCollision collision)
    {
        if (collision.other != this) return;
        collision.other = collision.my;
        collision.my = this;
    }
}
