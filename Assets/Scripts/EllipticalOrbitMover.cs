using System;
using UnityEngine;

[RequireComponent(typeof(FutureRigidBody2D))]
public class EllipticalOrbitMover : FutureBehaviour, IFuturePositionProvider
{
    public GravitySource center;

    private IFuturePositionProvider centerPositionProvider;
    private FutureRigidBody2D rigidBody;
    private FutureTransform futureTransform;
    private EllipticalOrbit ellipticalOrbit;
   
    private void Start()
    {
        futureTransform = GetComponent<FutureTransform>();
        rigidBody = GetComponent<FutureRigidBody2D>();
        if (center == null)
        {
            center = OrbitUtils.FindBiggestGravitySource(new Vector3d(transform.position));
        }
        var r0 = new Vector3d(transform.position - center.transform.position);
        var v0 = new Vector3d( rigidBody.initialVelocity);
        ellipticalOrbit = new EllipticalOrbit(center, rigidBody.initialMass, 0, r0, v0); 
    }

    public override void VirtualStep(int step)
    {
        futureTransform.GetState(step).position = GetFuturePosition(step, 0f);
    }

    public Vector3d GetFuturePosition(int step, double dt)
    {
        if (ellipticalOrbit == null) // not initialized yet
        {
            return new Vector3d(transform.position);
        }
        ellipticalOrbit.Evolve(step, dt, out var position, out var velocity);
        return position;
    }


    public int GetPriority()
    {
        return 1010;
    }
}