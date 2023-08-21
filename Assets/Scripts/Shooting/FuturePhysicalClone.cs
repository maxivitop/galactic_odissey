using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(FutureRigidBody2D))]
[RequireComponent(typeof(FutureTransform))]
public class FuturePhysicalClone : FutureBehaviour
{
    public FutureRigidBody2D targetBody;
    private FutureTransform targetTransform;
    private FutureTransform futureTransform;
    private FutureRigidBody2D futureRigidBody2D;
    private const double Precision = 0.1f;
    private FutureBehaviour[] myFutureComponents;


    private void Start()
    {
        futureRigidBody2D = GetComponent<FutureRigidBody2D>();
        futureTransform = GetComponent<FutureTransform>();
        targetTransform = targetBody.GetComponent<FutureTransform>();
        myFutureComponents = GetComponents<FutureBehaviour>();
    }

    private void Update()
    {
        if (!targetTransform.IsAlive(FuturePhysics.currentStep))
        {
            Destroy(gameObject);
            return;
        }
        foreach (var myFutureComponent in myFutureComponents)
        {
            myFutureComponent.Disable(targetBody.disabledFromStep);
        }
        var step = FuturePhysics.currentStep;
        if (Vector3d.SqrMagnitude(futureTransform.GetFuturePosition(step) -
                                  targetTransform.GetFuturePosition(step)) < Precision &&
            Vector2d.SqrMagnitude(futureRigidBody2D.GetState(step).acceleration -
                                  targetBody.GetState(step).acceleration) < Precision &&
            Vector2d.SqrMagnitude(futureRigidBody2D.GetState(step).velocity -
                                  targetBody.GetState(step).velocity) < Precision) return;
        FuturePhysics.Reset(step + 1, gameObject);
        CloneParams(step);
        CloneParams(step + 1);
    }

    protected override void VirtualStep(int step)
    {
        if (step > 0)
        {
            futureRigidBody2D.GetState(step).acceleration =
                futureRigidBody2D.GetState(step - 1).acceleration;
        }
    }

    private void CloneParams(int step)
    {
        futureTransform.SetFuturePosition(step, targetTransform.GetFuturePosition(step));
        futureRigidBody2D.GetState(step).acceleration = targetBody.GetState(step).acceleration;
        futureRigidBody2D.GetState(step).velocity = targetBody.GetState(step).velocity;
        futureRigidBody2D.GetState(step).orbit = targetBody.GetState(step).orbit;
    }
}