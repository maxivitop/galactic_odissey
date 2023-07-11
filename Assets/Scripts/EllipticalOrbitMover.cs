using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class EllipticalOrbitMover : FutureBehaviour, IFuturePositionProvider
{
    public GameObject center;

    private IFuturePositionProvider centerPositionProvider;
    private FutureRigidBody2D rigidBody;
    private FutureTransform futureTransform;
    private readonly EllipticalOrbit ellipticalOrbit = new();
   
    private void Start()
    {
        centerPositionProvider = IFuturePositionProvider.SelectFuturePositionProvider(center);
        var centerRigidBody = center.GetComponent<FutureRigidBody2D>(); 
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        var r0 = new Vector3d(transform.position - center.transform.position);
        var v0 = new Vector3d( rigidBody.initialVelocity);
        var mu = FuturePhysics.G * (rigidBody.initialMass + 
                                    center.GetComponent<FutureRigidBody2D>().initialMass);
        ellipticalOrbit.InitializeFromRv(r0,v0, 0, mu, centerPositionProvider, centerRigidBody);
    }

    public override void VirtualStep(int step)
    {
        futureTransform.GetState(step).position = GetFuturePosition(step, 0f);
    }

    public Vector3d GetFuturePosition(int step, double dt)
    {
        ellipticalOrbit.Evolve(step, dt, out var position, out var velocity);
        return position;
    }


    public int GetPriority()
    {
        return 1010;
    }
}